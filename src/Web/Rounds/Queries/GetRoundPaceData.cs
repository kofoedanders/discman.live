using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;

namespace Web.Rounds.Queries
{
    /// <summary>
    /// Query to get pace data for a specific round
    /// </summary>
    public class GetRoundPaceDataQuery : IRequest<RoundPaceData>
    {
        public Guid RoundId { get; set; }
    }

    /// <summary>
    /// Response model for pace data
    /// </summary>
    public class RoundPaceData
    {
        public double AverageCourseDurationMinutes { get; set; }
        public double AdjustedDurationMinutes { get; set; }

        /// <summary>Baseline: 3 players = 1.0</summary>
        public double PlayerCountFactor { get; set; }

        /// <summary>1.0 = average speed, >1.0 = slower, &lt;1.0 = faster</summary>
        public double CardSpeedFactor { get; set; }

        public int SampleCount { get; set; }
        public int TotalHoles { get; set; }

        /// <summary>Per-player factors: >1.0 = slower than course avg, &lt;1.0 = faster</summary>
        public Dictionary<string, double> PlayerFactors { get; set; } = new();
    }

    /// <summary>
    /// Handler for GetRoundPaceDataQuery
    /// </summary>
    public class GetRoundPaceDataQueryHandler : IRequestHandler<GetRoundPaceDataQuery, RoundPaceData>
    {
        private readonly DiscmanDbContext _dbContext;

        private const int MinSampleSize = 3;

        // Player count pace factors (data-driven from analysis of all completed rounds)
        private static readonly Dictionary<int, double> PlayerCountFactors = new()
        {
            { 1, 0.85 },
            { 2, 0.92 },
            { 3, 1.0 },   // baseline - 3 players
            { 4, 1.07 },
            { 5, 1.12 },
            { 6, 1.18 },
        };

        public GetRoundPaceDataQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RoundPaceData> Handle(GetRoundPaceDataQuery request, CancellationToken cancellationToken)
        {
            var round = await _dbContext.Rounds
                .SingleOrDefaultAsync(r => r.Id == request.RoundId, cancellationToken);

            if (round == null) return null;

            var totalHoles = round.PlayerScores.FirstOrDefault()?.Scores.Count ?? 18;

            var historicalRounds = await _dbContext.Rounds
                .Where(r => r.CourseId == round.CourseId)
                .Where(r => r.CourseLayout == round.CourseLayout)
                .Where(r => r.IsCompleted)
                .Where(r => r.CompletedAt != default)
                .Where(r => r.StartTime != default)
                .ToListAsync(cancellationToken);

            var minDurationMinutes = totalHoles * 1.5;
            var maxDurationMinutes = totalHoles * 15.0;

            var validRounds = historicalRounds
                .Where(r => (r.CompletedAt - r.StartTime).TotalMinutes >= minDurationMinutes)
                .Where(r => (r.CompletedAt - r.StartTime).TotalMinutes < maxDurationMinutes)
                .OrderByDescending(r => r.CompletedAt)
                .Take(50)
                .ToList();

            var averageCourseDuration = validRounds.Count >= MinSampleSize
                ? validRounds.Average(r => (r.CompletedAt - r.StartTime).TotalMinutes)
                : 0;

            var playerCount = round.PlayerScores.Count;
            var playerCountFactor = GetPlayerCountFactor(playerCount);

            var playerNames = round.PlayerScores.Select(p => p.PlayerName).Distinct().ToList();
            var playerFactors = CalculatePlayerFactors(validRounds, playerNames);

            var cardSpeedFactor = playerFactors.Count > 0
                ? playerFactors.Values.Average()
                : 1.0;

            // adjustedDuration = base * playerCountFactor * cardSpeedFactor
            var adjustedDuration = averageCourseDuration > 0
                ? averageCourseDuration * playerCountFactor * cardSpeedFactor
                : 0;

            return new RoundPaceData
            {
                AverageCourseDurationMinutes = Math.Round(averageCourseDuration, 1),
                AdjustedDurationMinutes = Math.Round(adjustedDuration, 1),
                PlayerCountFactor = Math.Round(playerCountFactor, 2),
                CardSpeedFactor = Math.Round(cardSpeedFactor, 2),
                SampleCount = validRounds.Count,
                TotalHoles = totalHoles,
                PlayerFactors = playerFactors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Math.Round(kvp.Value, 2)
                )
            };
        }

        private static double GetPlayerCountFactor(int playerCount)
        {
            if (PlayerCountFactors.TryGetValue(playerCount, out var factor))
                return factor;

            // Extrapolate for >6 players: +0.06 per additional player beyond 6
            if (playerCount > 6)
                return 1.18 + (playerCount - 6) * 0.06;

            return 1.0;
        }

        private static Dictionary<string, double> CalculatePlayerFactors(
            IReadOnlyList<Round> validRounds,
            IReadOnlyList<string> playerNames)
        {
            if (validRounds.Count < MinSampleSize)
                return playerNames.ToDictionary(p => p, _ => 1.0);

            var courseAvgDuration = validRounds.Average(r => (r.CompletedAt - r.StartTime).TotalMinutes);

            if (courseAvgDuration <= 0)
                return playerNames.ToDictionary(p => p, _ => 1.0);

            var result = new Dictionary<string, double>();

            foreach (var player in playerNames)
            {
                var playerRounds = validRounds
                    .Where(r => r.PlayerScores.Any(ps => ps.PlayerName == player))
                    .ToList();

                if (playerRounds.Count >= MinSampleSize)
                {
                    var playerAvgDuration = playerRounds
                        .Average(r => (r.CompletedAt - r.StartTime).TotalMinutes);

                    result[player] = playerAvgDuration / courseAvgDuration;
                }
                else
                {
                    result[player] = 1.0;
                }
            }

            return result;
        }
    }
}
