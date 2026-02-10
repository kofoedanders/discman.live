using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.Reflection;
using Web.Courses;
using Web.Users;

namespace Web.Rounds
{
    public class Round
    {
        public Round()
        {
        }

        public Round(Course course, List<User> players, string createdBy, string roundName, ScoreMode scoreMode)
        {
            Id = Guid.NewGuid();
            CourseName = course.Name;
            CourseLayout = course.Layout;
            CourseId = course.Id;
            StartTime = DateTime.Now;
            PlayerScores = GenerateEmptyScoreCard(course.Holes, players);
            CreatedBy = createdBy;
            RoundName = roundName;
            ScoreMode = scoreMode;
        }

        public Round(List<User> players, string createdBy, string roundName, ScoreMode scoreMode)
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
            PlayerScores = players
                .Select(p => new PlayerScore
                {
                    PlayerName = p.Username,
                    PlayerEmoji = p.Emoji,
                    Scores = new List<HoleScore>()
                }).ToList();
            RoundName = roundName;
            CreatedBy = createdBy;
            ScoreMode = scoreMode;
        }

        public Guid Id { get; set; }

        public List<string> Spectators { get; set; } = new List<string>();

        public ScoreMode ScoreMode { get; set; }

        /// <summary>
        /// Optional property usually only used when no course is used 
        /// </summary>
        public string RoundName { get; set; }
        public string CourseName { get; set; }
        public string CourseLayout { get; set; }
        public Guid CourseId { get; set; }
        public DateTime StartTime { get; set; }
        public List<PlayerSignature> Signatures { get; set; } = new List<PlayerSignature>();
        public bool IsCompleted { get; set; }
        public DateTime CompletedAt { get; set; }
        public string CreatedBy { get; set; }
        public List<Achievement> Achievements { get; set; }

        public List<PlayerScore> PlayerScores { get; set; }

        public bool Deleted { get; set; }
        public List<RatingChange> RatingChanges { get; set; } = new List<RatingChange>();

        public double DurationMinutes => IsCompleted ? Math.Round((CompletedAt - StartTime).TotalMinutes) : Math.Round((DateTime.Now - StartTime).TotalMinutes);

        private static List<PlayerScore> GenerateEmptyScoreCard(List<Hole> courseHoles, List<User> players)
        {
            return players
                .Select(p => new PlayerScore
                {
                    PlayerName = p.Username,
                    PlayerEmoji = p.Emoji,
                    Scores = courseHoles.Select(h => new HoleScore { Hole = new Hole(h.Number, h.Par, h.Distance, h.Rating, h.Average) }).ToList()
                }).ToList();
        }

        public void AddHole(int holeNumber, int par, int length)
        {
            foreach (var playerScore in PlayerScores)
            {
                playerScore.Scores.Add(new HoleScore { Hole = new Hole(holeNumber, par, length) });
            }
        }

        public int PlayerScore(string player)
        {
            return PlayerScores
                .Where(s => s.PlayerName == player)
                .SelectMany(s => s.Scores)
                .Sum(q => q.RelativeToPar);
        }

        public int PlayerHcpScore(string player)
        {
            var playerScores = PlayerScores
                .Where(s => s.PlayerName == player)
                .SelectMany(s => s.Scores);

            var playerHcpEntry = PlayerScores.SingleOrDefault(p => p.PlayerName == player);
            var playerHcpStrokes = playerHcpEntry?.NumberOfHcpStrokes ?? 0;

            return playerScores.Sum(q => q.RelativeToPar) - playerHcpStrokes;
        }

        public double RoundAverageScore()
        {
            if (PlayerScores == null || PlayerScores.Count == 0) return 0;
            return PlayerScores.Sum(ps => ps.Scores.Sum(s => s.RelativeToPar)) / (double)PlayerScores.Count;
        }

        public void SignRound(string username, string base64Signature)
        {
            Signatures.Add(new PlayerSignature
            {
                Username = username,
                Base64Signature = base64Signature,
                SignedAt = DateTime.Now
            });
            var playersInRound = PlayerScores.Select(s => s.PlayerName);
            if (playersInRound.All(pr => Signatures.Any(s => s.Username == pr)))
            {

                IsCompleted = true;
                CompletedAt = DateTime.Now;
            }
        }

        public bool IsPartOfRound(string username)
        {
            return PlayerScores.Any(p => p.PlayerName == username);
        }

        public void OrderByTeeHonours()
        {
            PlayerScores = PlayerScores.OrderBy(ps => ps.Scores.LastOrDefault(s => s.Strokes != 0)?.RelativeToPar).ToList();
        }

        public int PlayerStanding(string username)
        {
            var sortedPlayers = PlayerScores
                .GroupBy(s => s.PlayerName)
                .OrderBy(_ => _.SelectMany(_ => _.Scores).Sum(_ => _.RelativeToPar))
                .Select(_ => _.Key)
                .ToArray();

            return Array.IndexOf(sortedPlayers, username) + 1;
        }
    }

    public class RatingChange
    {
        public double Change { get; set; }
        public string Username { get; set; }
    }

    public enum ScoreMode
    {
        DetailedLive = 0,
        StrokesLive = 1,
        OneForAll = 2
    }

    public class PlayerSignature
    {
        public string Username { get; set; }
        public string Base64Signature { get; set; }
        public DateTime SignedAt { get; set; }
    }

    public class PlayerScore
    {
        public string PlayerName { get; set; }
        public string PlayerEmoji { get; set; }
        public string PlayerRoundStatusEmoji { get; set; }
        public double CourseAverageAtTheTime { get; set; }
        public int NumberOfHcpStrokes { get; set; }
        public List<HoleScore> Scores { get; set; }
    }

    public class HoleScore
    {
        public int UpdateScore(int strokes, string[] strokeOutcomes, int? putDistance = null)
        {
            Strokes = strokes;
            var relativeToPar = strokes - Hole.Par;
            RelativeToPar = relativeToPar;
            StrokeSpecs = strokeOutcomes?.Select(outcome => new StrokeSpec { Outcome = Enum.Parse<StrokeSpec.StrokeOutcome>(outcome, true) }).ToList();
            if (StrokeSpecs != null && StrokeSpecs.Any()) StrokeSpecs.Last().PutDistance = putDistance;
            RegisteredAt = DateTime.Now;
            return relativeToPar;
        }

        public Hole Hole { get; set; }
        public int Strokes { get; set; }
        public int RelativeToPar { get; set; }
        public DateTime RegisteredAt { get; set; }

        public List<StrokeSpec> StrokeSpecs { get; set; }
    }

    public class StrokeSpec
    {
        public StrokeOutcome Outcome { get; set; }
        public int? PutDistance { get; set; }

        public enum StrokeOutcome
        {
            Fairway,
            Rough,
            OB,
            Circle2,
            Circle1,
            Basket
        }
    }
}
