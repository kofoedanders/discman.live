using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Marten;
using Web.Rounds;

namespace Web
{
    /// <summary>
    /// Analyzes round pace data to calculate player count factors
    /// </summary>
    public static class PaceAnalysis
    {
        public static async Task AnalyzePaceFactors(IDocumentSession session)
        {
            Console.WriteLine("Starting player count pace factor analysis...");
            
            // Get all completed rounds with valid timing data
            var completedRounds = await session.Query<Round>()
                .Where(r => r.IsCompleted && r.StartTime != default && r.CompletedAt != default)
                .ToListAsync();
            
            Console.WriteLine($"Found {completedRounds.Count} completed rounds to analyze");
            
            // Group rounds by player count
            var roundsByPlayerCount = completedRounds
                .Where(r => r.PlayerScores.Count > 0 && 
                           (r.CompletedAt - r.StartTime).TotalMinutes > 60 &&
                           (r.CompletedAt - r.StartTime).TotalHours < 5) // Filter out invalid data
                .GroupBy(r => r.PlayerScores.Count)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Calculate base metrics
            Dictionary<int, double> averageMinutesPerHole = new Dictionary<int, double>();
            Dictionary<int, double> averageMinutesPerRound = new Dictionary<int, double>();
            Dictionary<int, int> roundCounts = new Dictionary<int, int>();
            
            // Calculate average pace for each player count group
            foreach (var entry in roundsByPlayerCount)
            {
                int playerCount = entry.Key;
                List<Round> rounds = entry.Value;
                
                // Calculate total minutes and holes across all rounds with this player count
                double totalMinutes = 0;
                int totalRounds = 0;
                
                foreach (var round in rounds)
                {
                    var duration = (round.CompletedAt - round.StartTime).TotalMinutes;
                    totalMinutes += duration;
                    totalRounds++;
                }
                
                double avgMinutesPerRound = totalMinutes / totalRounds;
                double avgMinutesPerHole = avgMinutesPerRound / 18;
                
                averageMinutesPerHole[playerCount] = avgMinutesPerHole;
                averageMinutesPerRound[playerCount] = avgMinutesPerRound;
                roundCounts[playerCount] = totalRounds;
                
                Console.WriteLine($"{playerCount} players: {rounds.Count} rounds, {avgMinutesPerRound:F1} min/round, {avgMinutesPerHole:F1} min/hole");
            }
            
            // Find the baseline (2 players is common and a good reference)
            double baselineMinutesPerHole = averageMinutesPerHole.ContainsKey(3) ? 
                averageMinutesPerHole[3] : 
                averageMinutesPerHole.Values.Min();
            
            int baselinePlayerCount = averageMinutesPerHole.ContainsKey(3) ? 
                3 : 
                averageMinutesPerHole.First(x => x.Value == averageMinutesPerHole.Values.Min()).Key;
            
            // Calculate factors relative to the baseline
            Dictionary<int, double> playerCountFactors = new Dictionary<int, double>();
            
            foreach (var playerCount in averageMinutesPerHole.Keys)
            {
                double factor = averageMinutesPerHole[playerCount] / baselineMinutesPerHole;
                playerCountFactors[playerCount] = factor;
            }
            
            // Calculate incremental factors
            Dictionary<int, double> incrementalFactors = new Dictionary<int, double>();
            
            for (int i = baselinePlayerCount + 1; i <= playerCountFactors.Keys.Max(); i++)
            {
                if (playerCountFactors.ContainsKey(i) && playerCountFactors.ContainsKey(i - 1))
                {
                    double incrementalFactor = playerCountFactors[i] / playerCountFactors[i - 1] - 1.0;
                    incrementalFactors[i] = incrementalFactor;
                }
            }
            
            // Output recommended factors for code
            Console.WriteLine();
            Console.WriteLine("PACE FACTOR ANALYSIS RESULTS");
            Console.WriteLine("=============================");
            Console.WriteLine($"Baseline: {baselinePlayerCount} players = {baselineMinutesPerHole:F2} minutes per hole");
            Console.WriteLine();
            Console.WriteLine("Player Count Factors (relative to baseline):");
            
            foreach (var entry in playerCountFactors.OrderBy(e => e.Key))
            {
                Console.WriteLine($"{entry.Key} players: {entry.Value:F2}x ({entry.Value * 100 - 100:F1}% {(entry.Value > 1 ? "slower" : "faster")})");
            }
            
            Console.WriteLine();
            Console.WriteLine("Incremental Slowdown Per Additional Player:");
            
            foreach (var entry in incrementalFactors.OrderBy(e => e.Key))
            {
                Console.WriteLine($"{entry.Key - 1} → {entry.Key} players: +{entry.Value * 100:F1}%");
            }
            
            double averageIncrementalSlowdown = incrementalFactors.Values.Any() ? 
                incrementalFactors.Values.Average() * 100 : 15.0;
            
            Console.WriteLine();
            Console.WriteLine($"Average incremental slowdown: +{averageIncrementalSlowdown:F1}% per additional player");
            
            // Generate code for GetRoundPaceData handler
            Console.WriteLine();
            Console.WriteLine("======== CODE FOR GetRoundPaceData.cs ========");
            Console.WriteLine();
            Console.WriteLine("// Player count pace factors (automatically generated)");
            Console.WriteLine("private static readonly Dictionary<int, double> PlayerCountFactors = new Dictionary<int, double>");
            Console.WriteLine("{");
            
            foreach (var entry in playerCountFactors.OrderBy(e => e.Key))
            {
                Console.WriteLine($"    {{ {entry.Key}, {entry.Value:F3} }},");
            }
            
            Console.WriteLine("};");
            Console.WriteLine();
        }
    }
}
