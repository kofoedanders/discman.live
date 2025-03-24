using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Web.Rounds.Queries
{
    /// <summary>
    /// Query to get time projection for the active round of the authenticated user
    /// </summary>
    public class GetActiveRoundTimeProjectionQuery : IRequest<RoundTimeProjection>
    {
    }

    /// <summary>
    /// Handler for GetActiveRoundTimeProjectionQuery
    /// </summary>
    public class GetActiveRoundTimeProjectionQueryHandler : IRequestHandler<GetActiveRoundTimeProjectionQuery, RoundTimeProjection>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetActiveRoundTimeProjectionQueryHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RoundTimeProjection> Handle(GetActiveRoundTimeProjectionQuery request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            // Find the active round for the user
            var activeRound = _documentSession
                .Query<Round>()
                .Where(r => !r.Deleted)
                .Where(r => r.PlayerScores.Any(ps => ps.PlayerName == authenticatedUsername) && !r.IsCompleted)
                .OrderByDescending(r => r.StartTime)
                .FirstOrDefault();

            if (activeRound == null)
            {
                return null;
            }

            // Get the course and layout information
            var courseName = activeRound.CourseName;
            var courseLayout = activeRound.CourseLayout;
            
            // Get all players in the round
            var players = activeRound.PlayerScores.Select(ps => ps.PlayerName).ToList();
            
            // Get historical rounds on this course/layout
            var historicalRounds = _documentSession
                .Query<Round>()
                .Where(r => !r.Deleted)
                .Where(r => r.CourseName == courseName && 
                           r.CourseLayout == courseLayout && 
                           r.IsCompleted &&
                           r.StartTime < activeRound.StartTime)
                .ToList();

            // Calculate average time per hole for each player in the card
            var playerAverages = new Dictionary<string, Dictionary<int, List<double>>>();
            
            // Initialize dictionary for each player
            foreach (var player in players)
            {
                playerAverages[player] = new Dictionary<int, List<double>>();
            }
            
            // Process historical rounds to get time data
            foreach (var round in historicalRounds)
            {
                foreach (var playerScore in round.PlayerScores)
                {
                    // Only consider players who are in the current round
                    if (!players.Contains(playerScore.PlayerName))
                        continue;
                        
                    var scores = playerScore.Scores
                        .Where(s => s.Strokes > 0 && s.RegisteredAt != default)
                        .OrderBy(s => s.Hole.Number)
                        .ToList();
                        
                    // Calculate time between holes
                    for (int i = 1; i < scores.Count; i++)
                    {
                        var currentScore = scores[i];
                        var previousScore = scores[i-1];
                        var holeNumber = currentScore.Hole.Number;
                        
                        var timeDiff = (currentScore.RegisteredAt - previousScore.RegisteredAt).TotalMinutes;
                        
                        // Only consider reasonable time differences (1-20 minutes)
                        if (timeDiff >= 1 && timeDiff <= 20)
                        {
                            if (!playerAverages[playerScore.PlayerName].ContainsKey(holeNumber))
                            {
                                playerAverages[playerScore.PlayerName][holeNumber] = new List<double>();
                            }
                            
                            playerAverages[playerScore.PlayerName][holeNumber].Add(timeDiff);
                        }
                    }
                }
            }
            
            // Calculate course-wide averages for holes with no player data
            var courseAverages = new Dictionary<int, double>();
            foreach (var player in playerAverages.Keys)
            {
                foreach (var holeData in playerAverages[player])
                {
                    var holeNumber = holeData.Key;
                    var times = holeData.Value;
                    
                    if (times.Count > 0)
                    {
                        if (!courseAverages.ContainsKey(holeNumber))
                        {
                            courseAverages[holeNumber] = 0;
                        }
                        
                        courseAverages[holeNumber] += times.Average() / players.Count;
                    }
                }
            }
            
            // Create time estimates for each hole
            var holeTimeEstimates = new List<HoleTimeEstimate>();
            var totalHoles = activeRound.PlayerScores.FirstOrDefault()?.Scores.Count ?? 0;
            
            // Default average if no historical data is available
            var defaultAverageMinutes = 4.0;
            
            for (int holeNumber = 1; holeNumber <= totalHoles; holeNumber++)
            {
                double averageTime;
                
                // Try to get player-specific average for this hole
                var playersWithData = players
                    .Where(p => playerAverages.ContainsKey(p) && 
                               playerAverages[p].ContainsKey(holeNumber) && 
                               playerAverages[p][holeNumber].Count > 0)
                    .ToList();
                
                if (playersWithData.Any())
                {
                    // Calculate weighted average based on players in the card
                    averageTime = playersWithData
                        .Select(p => playerAverages[p][holeNumber].Average())
                        .Average();
                }
                else if (courseAverages.ContainsKey(holeNumber))
                {
                    // Use course-wide average if available
                    averageTime = courseAverages[holeNumber];
                }
                else
                {
                    // Fall back to default average
                    averageTime = defaultAverageMinutes;
                }
                
                // Adjust for group size
                averageTime = AdjustTimeForGroupSize(averageTime, players.Count);
                
                holeTimeEstimates.Add(new HoleTimeEstimate
                {
                    HoleNumber = holeNumber,
                    AverageMinutesToComplete = averageTime
                });
            }
            
            // Find the last scored hole for the user
            var userScores = activeRound.PlayerScores
                .FirstOrDefault(ps => ps.PlayerName == authenticatedUsername)?.Scores
                .OrderBy(s => s.Hole.Number)
                .ToList();
                
            int lastScoredHoleIndex = -1;
            
            if (userScores != null)
            {
                for (int i = 0; i < userScores.Count; i++)
                {
                    if (userScores[i].Strokes == 0)
                    {
                        lastScoredHoleIndex = i - 1;
                        break;
                    }
                }
                
                // If all holes are scored, the last hole is the last scored hole
                if (lastScoredHoleIndex == -1 && userScores.All(s => s.Strokes > 0))
                {
                    lastScoredHoleIndex = userScores.Count - 1;
                }
            }
            
            // Calculate current pace based on completed holes
            double currentAverageMinutesPerHole = CalculateCurrentPace(userScores, lastScoredHoleIndex);
            
            // Calculate historical average minutes per hole
            double historicalAverageMinutesPerHole = holeTimeEstimates.Average(h => h.AverageMinutesToComplete);
            
            // Calculate remaining time
            int remainingHoles = totalHoles - (lastScoredHoleIndex + 1);
            double minutesRemaining = 0;
            
            for (int i = lastScoredHoleIndex + 1; i < totalHoles; i++)
            {
                minutesRemaining += holeTimeEstimates[i].AverageMinutesToComplete;
            }
            
            // Use current pace if available, otherwise use historical
            if (currentAverageMinutesPerHole > 0)
            {
                minutesRemaining = remainingHoles * currentAverageMinutesPerHole;
            }
            
            // Calculate estimated finish time
            var now = DateTime.Now;
            var estimatedFinishTime = now.AddMinutes(minutesRemaining);
            
            // Calculate total round time
            var elapsedMinutes = (now - activeRound.StartTime).TotalMinutes;
            var totalEstimatedMinutes = (int)(elapsedMinutes + minutesRemaining);
            
            return new RoundTimeProjection
            {
                RoundId = activeRound.Id,
                HoleTimeEstimates = holeTimeEstimates,
                EstimatedFinishTime = estimatedFinishTime,
                TotalEstimatedMinutes = totalEstimatedMinutes,
                EstimatedMinutesRemaining = (int)minutesRemaining,
                CurrentAverageMinutesPerHole = currentAverageMinutesPerHole,
                HistoricalAverageMinutesPerHole = historicalAverageMinutesPerHole,
                IsAheadOfHistoricalPace = currentAverageMinutesPerHole > 0 && currentAverageMinutesPerHole < historicalAverageMinutesPerHole
            };
        }
        
        /// <summary>
        /// Calculate current pace based on completed holes
        /// </summary>
        private double CalculateCurrentPace(List<HoleScore> scores, int lastScoredHoleIndex)
        {
            if (scores == null || lastScoredHoleIndex < 1)
                return 0;
                
            var scoredHoles = scores
                .Take(lastScoredHoleIndex + 1)
                .Where(s => s.Strokes > 0 && s.RegisteredAt != default)
                .OrderBy(s => s.Hole.Number)
                .ToList();
                
            if (scoredHoles.Count < 2)
                return 0;
                
            double totalMinutes = 0;
            int validIntervals = 0;
            
            for (int i = 1; i < scoredHoles.Count; i++)
            {
                var timeDiff = (scoredHoles[i].RegisteredAt - scoredHoles[i-1].RegisteredAt).TotalMinutes;
                
                // Only count reasonable intervals (1-20 minutes)
                if (timeDiff >= 1 && timeDiff <= 20)
                {
                    totalMinutes += timeDiff;
                    validIntervals++;
                }
            }
            
            return validIntervals > 0 ? totalMinutes / validIntervals : 0;
        }
        
        /// <summary>
        /// Adjust time based on group size
        /// </summary>
        private double AdjustTimeForGroupSize(double baseTimePerHole, int numberOfPlayers)
        {
            // Solo player: 0.8x, 2 players: 1x, 3 players: 1.2x, 4+ players: 1.3x + 0.1x per additional player
            if (numberOfPlayers == 1) return baseTimePerHole * 0.8;
            if (numberOfPlayers == 2) return baseTimePerHole;
            if (numberOfPlayers == 3) return baseTimePerHole * 1.2;
            return baseTimePerHole * (1.3 + (numberOfPlayers - 4) * 0.1);
        }
    }
}
