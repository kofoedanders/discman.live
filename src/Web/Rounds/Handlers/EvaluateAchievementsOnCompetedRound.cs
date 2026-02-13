using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Web.Infrastructure;
using Web.Users;
using NServiceBus;
using Web.Rounds.NSBEvents;
using Microsoft.EntityFrameworkCore;

namespace Web.Rounds.Notifications
{
    public class EvaluateAchievementsOnCompetedRound : IHandleMessages<RoundWasCompleted>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public EvaluateAchievementsOnCompetedRound(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(RoundWasCompleted notification, IMessageHandlerContext context)
        {
            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == notification.RoundId, context.CancellationToken);

            var newUserAchievements = await EvaluateAchievements(round, context);
            if (round.Achievements is null) round.Achievements = new List<Achievement>();
            round.Achievements.AddRange(newUserAchievements);

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound("", round);
        }

        private async Task<IEnumerable<Achievement>> EvaluateAchievements(Round round, IMessageHandlerContext context)
        {
            var userNames = round.PlayerScores.Select(s => s.PlayerName).ToArray();

            var users = await _dbContext.Users
                .Where(u => userNames.Contains(u.Username))
                .ToListAsync(context.CancellationToken);


            var newUserAchievements = new List<Achievement>();
            foreach (var userInRound in users)
            {
                if (userInRound.Achievements is null) userInRound.Achievements = new Achievements();
                var roundAchievements = userInRound.Achievements.EvaluatePlayerRound(round.Id, userInRound.Username, round);

                var now = DateTime.UtcNow;
                var rounds = await _dbContext.Rounds
                    .Where(r => !r.Deleted)
                    .Where(r => r.PlayerScores.Any(p => p.PlayerName == userInRound.Username))
                    .Where(r => r.PlayerScores.Count > 1)
                    .Where(r => r.IsCompleted)
                    // .Where(r => r.CompletedAt > new DateTime(now.Year, 1, 1))
                    .ToListAsync(context.CancellationToken);

                var userRounds = rounds.Concat(new List<Round> { round }).ToList();

                var userAchievements = userInRound.Achievements.EvaluateUserRounds(userRounds, userInRound.Username);

                var newAchievements = roundAchievements.Concat(userAchievements).ToList();

                if (!newAchievements.Any()) continue;
                _dbContext.Users.Update(userInRound);
                newUserAchievements.AddRange(newAchievements);
            }

            foreach (var achievement in newUserAchievements)
            {
                await context.Publish(new UserEarnedAchievement
                {
                    RoundId = achievement.RoundId,
                    Username = achievement.Username,
                    AchievementName = achievement.AchievementName,
                    AchievedAt = achievement.AchievedAt
                }
                );
            }

            return newUserAchievements;
        }
    }
}
