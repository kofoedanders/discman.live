using System;
using System.Collections.Generic;
using Marten.Util;
using Web.Matches;

namespace Web.Leaderboard
{
    public class HallOfFame
    {
        public Guid Id { get; set; }
        public DateTime UpdatedAt { get; set; }

        public MostBirdies MostBirdies { get; set; }
        public MostBogies MostBogies { get; set; }
        public MostRounds MostRounds { get; set; }
        public BestRoundAverage BestRoundAverage { get; set; }

        public void UpdateHallOfFame(MonthHallOfFame monthHallOfFame)
        {
            if (MostBirdies is null)
            {
                MostBirdies = monthHallOfFame.MostBirdies;
                MostBogies = monthHallOfFame.MostBogies;
                MostRounds = monthHallOfFame.MostRounds;
                BestRoundAverage = monthHallOfFame.BestRoundAverage;
                UpdatedAt = DateTime.Now;
                return;
            }

            MostBirdies.NewThisMonth = false;
            MostBogies.NewThisMonth = false;
            MostRounds.NewThisMonth = false;
            BestRoundAverage.NewThisMonth = false;

            if (monthHallOfFame.MostBirdies.Count > MostBirdies.Count) MostBirdies = monthHallOfFame.MostBirdies;
            if (monthHallOfFame.MostBogies.Count > MostBogies.Count) MostBogies = monthHallOfFame.MostBogies;
            if (monthHallOfFame.MostRounds.Count > MostRounds.Count) MostRounds = monthHallOfFame.MostRounds;
            if (monthHallOfFame.BestRoundAverage.RoundAverage < BestRoundAverage.RoundAverage) BestRoundAverage = monthHallOfFame.BestRoundAverage;
            UpdatedAt = DateTime.Now;
        }
    }

    public class MonthHallOfFame : HallOfFame
    {
        public new Guid Id { get; set; }

        // Parameterless constructor for EF Core
        private MonthHallOfFame() { }

        public MonthHallOfFame(PlayerStats mostBirdies, PlayerStats mostBogies, PlayerStats mostRounds, PlayerStats bestRoundAverage)
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            Month = month - 1;
            Year = year;
            UpdatedAt = DateTime.Now;
            MostBirdies = new MostBirdies
            {
                Count = mostBirdies.BirdieCount,
                Username = mostBirdies.Username,
                PerRound = mostBirdies.BirdieCount / (double) mostBirdies.RoundCount,
                TimeOfEntry = DateTime.Now,
                NewThisMonth = true
            };
            MostBogies = new MostBogies
            {
                Count = mostBogies.BogeyCount,
                Username = mostBogies.Username,
                PerRound = mostBogies.BogeyCount / (double) mostBogies.RoundCount,
                TimeOfEntry = DateTime.Now,
                NewThisMonth = true
            };
            MostRounds = new MostRounds
            {
                Count = mostRounds.RoundCount,
                Username = mostRounds.Username,
                TimeOfEntry = DateTime.Now,
                NewThisMonth = true
            };
            BestRoundAverage = new BestRoundAverage
            {
                RoundAverage = bestRoundAverage.AverageHoleScore,
                Username = bestRoundAverage.Username,
                TimeOfEntry = DateTime.Now,
                NewThisMonth = true
            };
        }

        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BestRoundAverage : HallOfFameEntry
    {
        public double RoundAverage { get; set; }
    }

    public class MostBirdies : HallOfFameEntry
    {
        public int Count { get; set; }
        public double PerRound { get; set; }
    }

    public class MostBogies : HallOfFameEntry
    {
        public int Count { get; set; }
        public double PerRound { get; set; }
    }

    public class MostRounds : HallOfFameEntry
    {
        public int Count { get; set; }
    }

    public abstract class HallOfFameEntry
    {
        public string Username { get; set; }
        public DateTime TimeOfEntry { get; set; }
        public int DaysInHallOfFame => (DateTime.Now - TimeOfEntry.AddDays(-1)).Days;
        public bool NewThisMonth { get; set; }
    }
}