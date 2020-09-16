using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Web.Matches;
using Web.Rounds;

namespace Web.Users.Queries
{
    public class GetUserStatsQuery : IRequest<UserStats>
    {
        public string Username { get; set; }
        public DateTime Since { get; set; }
        public int IncludeMonths { get; set; }
    }

    public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStats>
    {
        private readonly IDocumentSession _documentSession;
        private readonly UserStatsCache _userStatsCache;

        public GetUserStatsQueryHandler(IDocumentSession documentSession, UserStatsCache userStatsCache)
        {
            _documentSession = documentSession;
            _userStatsCache = userStatsCache;
        }

        public async Task<UserStats> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
        {
            var userStats = await _userStatsCache.GetOrCreate($"{request.Username}{request.Since}{request.IncludeMonths}",
                async () => await CalculateUserStats(request.Username, request.Since, request.IncludeMonths));

            return userStats;
        }

        private async Task<UserStats> CalculateUserStats(string username, DateTime since, int includeMonths)
        {
            var rounds = await _documentSession
                .Query<Round>()
                .Where(r => !r.Deleted)
                .Where(r => r.PlayerScores.Any(s => s.PlayerName == username))
                .Where(r => r.StartTime > since)
                .Where(r => includeMonths == default || r.StartTime > DateTime.Today.AddMonths(-includeMonths))
                .ToListAsync();

            var holesWithDetails = rounds.PlayerHolesWithDetails(username);

            var roundsPlayed = rounds.Count;
            var holesPlayed = rounds.Sum(r => r.PlayerScores[0].Scores.Count);
            if (roundsPlayed == 0 || holesWithDetails.Count == 0)
            {
                return new UserStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            var playerRounds = rounds.Where(r => r.PlayerScores.Any(p => p.PlayerName == username)).ToList();
            var totalScore = playerRounds.Sum(r => r.PlayerScore(username));


            var totalAverageAllPlayers = playerRounds.Sum(r => r.RoundAverageScore()) / playerRounds.Count;
            var playerRoundAverage = totalScore / (double) playerRounds.Count;
            var strokesGained = playerRoundAverage - totalAverageAllPlayers;

            var circle1Rate = holesWithDetails.Circle1Rate();
            var circle2Rate = holesWithDetails.Circle2Rate();
            var scrambleRate = holesWithDetails.ScrambleRate();
            var fairwayHitRate = holesWithDetails.FairwayRate();
            var birdieRate = holesWithDetails.BirdieRate();
            var obRate = holesWithDetails.ObRate();

            return new UserStats(roundsPlayed, holesPlayed, circle1Rate, circle2Rate, fairwayHitRate, scrambleRate, playerRoundAverage,
                strokesGained, birdieRate, obRate);
        }
    }
}