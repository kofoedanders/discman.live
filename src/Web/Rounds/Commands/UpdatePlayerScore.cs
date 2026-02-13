using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;

namespace Web.Rounds.Commands
{
    public class UpdatePlayerScoreCommand : IRequest<Round>
    {
        public Guid RoundId { get; set; }
        public int HoleIndex { get; set; }
        public int Strokes { get; set; }
        public string Username { get; set; }

        public string[] StrokeOutcomes { get; set; }
        public int? PutDistance { get; set; }
    }

    public class UpdatePlayerScoreCommandHandler : IRequestHandler<UpdatePlayerScoreCommand, Round>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;
        private readonly IMessageSession _messageSession;

        public UpdatePlayerScoreCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub, IMessageSession messageSession)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
            _messageSession = messageSession;
        }

        public async Task<Round> Handle(UpdatePlayerScoreCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var round = await _dbContext.Rounds
                .SingleOrDefaultAsync(x => x.Id == request.RoundId, cancellationToken);

            if (!round.IsPartOfRound(authenticatedUsername)) throw new UnauthorizedAccessException($"Cannot update round you are not part of");
            if (request.Username != authenticatedUsername) throw new UnauthorizedAccessException($"You can only update scores for yourself");

            var holeScore = round.PlayerScores.Single(p => p.PlayerName == authenticatedUsername).Scores[request.HoleIndex];

            var holeAlreadyRegistered = holeScore.Strokes != 0;

            var relativeScore = holeScore.UpdateScore(request.Strokes, request.StrokeOutcomes, request.PutDistance);

            round.OrderByTeeHonours();

            CalculateNewStartingHole(request.HoleIndex, round);

            CalculateStatusEmoji(request.Username, round);

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(authenticatedUsername, round);

            await _messageSession.Publish(new ScoreWasUpdated
            {
                RoundId = round.Id,
                Username = authenticatedUsername,
                CourseName = round.CourseName,
                HoleNumber = holeScore.Hole.Number,
                RelativeScore = relativeScore,
                ScoreWasChanged = holeAlreadyRegistered
            });

            return round;
        }

        private static void CalculateStatusEmoji(string username, Round round)
        {
            //Calculate emojies based on the holes played for the active user
            var playerScore = round.PlayerScores.Single(x => x.PlayerName == username);
            var holesPlayed = playerScore.Scores.Where(x => x.Strokes != 0);

            // Default emoji if not enough holes played
            if (holesPlayed.Count() < 5) {
                playerScore.PlayerRoundStatusEmoji = "🏌️";
                return;
            }

            var prevHole = holesPlayed.LastOrDefault();
            var lastFiveHoles = holesPlayed.TakeLast(5);
            var lastThreeHoles = holesPlayed.TakeLast(3);
            var totalRelativeToPar = holesPlayed.Sum(x => x.RelativeToPar);
            var lastFiveRelativeToPar = lastFiveHoles.Sum(x => x.RelativeToPar);

            // Great performance - birdie streak
            if (lastThreeHoles.All(x => x.RelativeToPar < 0)) {
                playerScore.PlayerRoundStatusEmoji = "🦃";
                return;
            }

            // Hot streak - playing well recently
            if (lastFiveRelativeToPar <= -1) {
                playerScore.PlayerRoundStatusEmoji = "🔥";
                return;
            }

            // Making a comeback - overall not great but improving
            if (totalRelativeToPar > holesPlayed.Count() / 3 && lastFiveRelativeToPar <= 0) {
                playerScore.PlayerRoundStatusEmoji = "🚀";
                return;
            }

            // Just had a great hole
            if (prevHole.RelativeToPar < 0) {
                playerScore.PlayerRoundStatusEmoji = "👏";
                return;
            }

            // Playing at par
            if (Math.Abs(totalRelativeToPar) <= holesPlayed.Count() / 3) {
                playerScore.PlayerRoundStatusEmoji = "👌";
                return;
            }

            // Steady progress - not too many big mistakes
            if (totalRelativeToPar <= holesPlayed.Count() && !holesPlayed.Any(x => x.RelativeToPar > 2)) {
                playerScore.PlayerRoundStatusEmoji = "🐢";
                return;
            }

            // Just had a bad hole, but encouraging
            if (prevHole.RelativeToPar > 2) {
                playerScore.PlayerRoundStatusEmoji = "💪";
                return;
            }

            // Recent rough patch but not too bad
            if (lastFiveRelativeToPar > 5 && lastFiveRelativeToPar <= 8) {
                playerScore.PlayerRoundStatusEmoji = "😓";
                return;
            }

            // Really struggling recently
            if (lastFiveRelativeToPar > 8) {
                playerScore.PlayerRoundStatusEmoji = "🤮";
                return;
            }

            // Consistently over par but not terrible
            if (lastFiveHoles.All(x => x.RelativeToPar > 0) && lastFiveRelativeToPar <= 10) {
                playerScore.PlayerRoundStatusEmoji = "🤔";
                return;
            }

            // Consistently way over par
            if (lastFiveHoles.All(x => x.RelativeToPar > 0) && lastFiveRelativeToPar > 10) {
                playerScore.PlayerRoundStatusEmoji = "🗑️";
                return;
            }

            // Exceptional round overall
            if (totalRelativeToPar <= -3) {
                playerScore.PlayerRoundStatusEmoji = "🏆";
                return;
            }

            // Default - neutral emoji if nothing else applies
            playerScore.PlayerRoundStatusEmoji = "🤷";
        }

        private static void CalculateNewStartingHole(int holeIndex, Round round)
        {
            if (round.PlayerScores.Sum(p => p.Scores.Count(s => s.Strokes != 0)) == 1)
            {
                foreach (var playerScore in round.PlayerScores)
                {
                    // var firstHoleIndex = playerScore.Scores.FindIndex(x => x.Hole.Number == request.Hole);
                    playerScore.Scores = playerScore.Scores
                        .Skip(holeIndex)
                        .Concat(playerScore.Scores.Take(holeIndex))
                        .ToList();
                }
            }
        }
    }
}
