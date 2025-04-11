using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;

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
        public Dictionary<string, double[]> PlayerAverages { get; set; }
        public double[] CourseAverage { get; set; }
        public double[] GroupAdjustedPace { get; set; }
    }

    /// <summary>
    /// Handler for GetRoundPaceDataQuery
    /// </summary>
    public class GetRoundPaceDataQueryHandler : IRequestHandler<GetRoundPaceDataQuery, RoundPaceData>
    {
        private readonly IDocumentSession _session;
        

        // Player count pace factors (data-driven from analysis of all completed rounds)
        private static readonly Dictionary<int, double> PlayerCountFactors = new Dictionary<int, double>
        {
            { 1, 0.85 },
            { 2, 0.92 },
            { 3, 1.0 },   // baseline - 3 players
            { 4, 1.07 },
            { 5, 1.12 },
            { 6, 1.18 },
        };

        public GetRoundPaceDataQueryHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<RoundPaceData> Handle(GetRoundPaceDataQuery request, CancellationToken cancellationToken)
        {
            var round = await _session.Query<Round>()
                .SingleOrDefaultAsync(r => r.Id == request.RoundId, token: cancellationToken);
            
            if (round == null) return null;

            var historicalRounds = await _session.Query<Round>()
                .Where(r => r.CourseId == round.CourseId)
                .Where(r => r.IsCompleted)
                .OrderByDescending(r => r.CompletedAt)
                .Take(20)
                .ToListAsync(cancellationToken);

            var playerAverages = round.PlayerScores
                .Select(p => p.PlayerName)
                .Distinct()
                .ToDictionary(
                    player => player,
                    player => CalculatePlayerAverages(historicalRounds, player)
                );

            var courseAverages = CalculateCourseAverages(historicalRounds);

            var groupAdjustedPace = CalculateGroupAdjustedPace(courseAverages, round.PlayerScores.Count);

            return new RoundPaceData
            {
                PlayerAverages = playerAverages,
                CourseAverage = courseAverages,
                GroupAdjustedPace = groupAdjustedPace
            };
        }

        private static double[] CalculatePlayerAverages(IReadOnlyList<Round> rounds, string playerName)
        {
            var holeData = new (double Sum, int Count)[18];
            
            foreach (var round in rounds)
            {
                var scores = round.PlayerScores
                    .FirstOrDefault(p => p.PlayerName == playerName)?
                    .Scores
                    .Where(s => s.RegisteredAt != default)
                    .OrderBy(s => s.Hole.Number)
                    .ToList();

                if (scores?.Count > 1)
                {
                    for (var i = 1; i < scores.Count; i++)
                    {
                        var holeIndex = scores[i].Hole.Number - 1;
                        var minutes = (scores[i].RegisteredAt - scores[i-1].RegisteredAt).TotalMinutes;
                        
                        if (minutes is >= 1 and <= 20)
                        {
                            holeData[holeIndex].Sum += minutes;
                            holeData[holeIndex].Count++;
                        }
                    }
                }
            }

            return holeData.Select(x => x.Count > 0 ? x.Sum / x.Count : 4.0).ToArray();
        }

        private static double[] CalculateCourseAverages(IReadOnlyList<Round> rounds)
        {
            var holeData = new (double Sum, int Count)[18];
            
            foreach (var round in rounds)
            {
                var allScores = round.PlayerScores
                    .SelectMany(p => p.Scores)
                    .Where(s => s.RegisteredAt != default)
                    .OrderBy(s => s.Hole.Number)
                    .ToList();

                if (allScores.Count > 1)
                {
                    for (var i = 1; i < allScores.Count; i++)
                    {
                        var holeIndex = allScores[i].Hole.Number - 1;
                        var minutes = (allScores[i].RegisteredAt - allScores[i-1].RegisteredAt).TotalMinutes;
                        
                        if (minutes is >= 1 and <= 20)
                        {
                            holeData[holeIndex].Sum += minutes;
                            holeData[holeIndex].Count++;
                        }
                    }
                }
            }

            return holeData.Select(x => x.Count > 0 ? x.Sum / x.Count : 4.0).ToArray();
        }

        private static double[] CalculateGroupAdjustedPace(double[] courseAverages, int playerCount)
        {
            if (!PlayerCountFactors.TryGetValue(playerCount, out var factor))
            {
                factor = 1.0; // default to baseline factor
            }

            return courseAverages.Select(avg => avg * factor).ToArray();
        }
    }
}
