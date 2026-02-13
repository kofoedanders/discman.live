using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Queries
{
    public class GetPlayersCourseStatsQuery : IRequest<List<PlayerCourseStats>>
    {
        public Guid RoundId { get; set; }
    }

    public class GetPlayersCourseStatsQueryHandler : IRequestHandler<GetPlayersCourseStatsQuery, List<PlayerCourseStats>>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly PlayerCourseStatsCache _playerCourseStatsCache;

        public GetPlayersCourseStatsQueryHandler(DiscmanDbContext dbContext, PlayerCourseStatsCache playerCourseStatsCache)
        {
            _dbContext = dbContext;
            _playerCourseStatsCache = playerCourseStatsCache;
        }

        public async Task<List<PlayerCourseStats>> Handle(GetPlayersCourseStatsQuery request, CancellationToken cancellationToken)
        {
            return await _playerCourseStatsCache.GetOrCreate(request.RoundId, () => CalculatePlayersCourseStats(request, cancellationToken));
        }

        private async Task<List<PlayerCourseStats>> CalculatePlayersCourseStats(GetPlayersCourseStatsQuery request, CancellationToken cancellationToken)
        {
            var activeRound = await _dbContext.Rounds
                .SingleOrDefaultAsync(r => r.Id == request.RoundId, cancellationToken);

            if (activeRound is null)
            {
                return new List<PlayerCourseStats>();
            }

            var courseName = activeRound.CourseName;
            var layoutName = activeRound.CourseLayout;
            var players = activeRound.PlayerScores.Select(p => p.PlayerName);


            var playersStats = new List<PlayerCourseStats>();
            if (courseName is null) return playersStats;
            foreach (var player in players)
            {
                var playerRounds = _dbContext.Rounds
                    .Where(r => !r.Deleted)
                    .Where(r => r.IsCompleted)
                    .Where(r => r.PlayerScores.Count > 1)
                    .Where(r => r.CourseName == courseName && r.CourseLayout == layoutName)
                    .Where(r => r.PlayerScores.Any(s => s.PlayerName == player))
                    .ToList();

                var playerHoleScores = playerRounds.SelectMany(r => r.PlayerScores.Single(p => p.PlayerName == player).Scores).ToList();

                var holeStats = playerHoleScores.Any()
                    ? playerHoleScores.GroupBy(x => x.Hole.Number).Select(x => new HoleStats
                    {
                        HoleNumber = x.Key,
                        AverageScore = x.Average(s => s.Strokes),
                        BestScore = x.MinBy(s => s.RelativeToPar),
                        Birdie = x.Any(s => s.RelativeToPar == -1),
                        Birdies = x.Count(s => s.RelativeToPar == -1),
                        Pars = x.Count(s => s.RelativeToPar == 0),
                        WorseThanPar = x.Count(s => s.RelativeToPar > 0),
                        Last10Scores = x.OrderByDescending(x => x.RegisteredAt).Take(10).ToArray()
                    }).ToList()
                    : new List<HoleStats>();

                var playerCourseRecord = playerRounds.Any() ? playerRounds.Select(r => r.PlayerScore(player)).Min() : (int?)null;

                var fivePreviousRounds = playerRounds
                    .Where(r => r.StartTime < activeRound.StartTime)
                    .OrderByDescending(r => r.StartTime)
                    .Take(5)
                    .ToList();

                if (fivePreviousRounds.Count == 0) continue;

                var courseScores = fivePreviousRounds.Select(r => r.PlayerScore(player));
                var currentCourseAverage = courseScores.Average();
                var thisRound = activeRound
                    .PlayerScores
                    .Single(s => s.PlayerName == player)
                    .Scores.Sum(s => s.RelativeToPar);

                var playerHoleAverages = fivePreviousRounds
                    .SelectMany(r => r.PlayerScores.Where(s => s.PlayerName == player))
                    .SelectMany(s => s.Scores)
                    .GroupBy(s => s.Hole.Number)
                    .ToDictionary(x => x.Key.ToString(), x => x.Average(y => y.RelativeToPar));

                // Total score for each hole, as a list of totals, one per hole
                var progressionDataPoints = playerHoleAverages.Scan((state, item) => state + item.Value, 0.0).Skip(1).ToList();

                playersStats.Add(new PlayerCourseStats
                {
                    PlayerName = player,
                    CourseName = courseName,
                    LayoutName = layoutName,
                    CourseAverage = currentCourseAverage,
                    PlayerCourseRecord = playerCourseRecord,
                    ThisRoundVsAverage = thisRound - currentCourseAverage,
                    HoleAverages = playerHoleAverages.Select(x => x.Value).ToList(),
                    AveragePrediction = progressionDataPoints,
                    RoundsPlayed = playerRounds.Count,
                    HoleStats = holeStats,
                });
            }

            return playersStats.OrderBy(s => s.ThisRoundVsAverage).ToList();
        }
    }

    public class HoleStats
    {
        public int HoleNumber { get; set; }
        public HoleScore BestScore { get; set; }
        public double AverageScore { get; set; }
        public bool Birdie { get; set; }
        public int Birdies { get; set; }
        public int Pars { get; set; }
        public int WorseThanPar { get; set; }
        public HoleScore[] Last10Scores { get; set; }
    }
}
