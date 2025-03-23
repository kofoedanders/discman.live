using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Web.Rounds;

namespace Web.Users.Queries
{
    public class GetYearSummaryQuery : IRequest<UserYearSummary>
    {
        public string Username { get; set; }
        public int Year { get; set; }
    }

    public class GetYearSummaryQueryHandler : IRequestHandler<GetYearSummaryQuery, UserYearSummary>
    {
        private readonly IDocumentSession _documentSession;

        public GetYearSummaryQueryHandler(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public async Task<UserYearSummary> Handle(GetYearSummaryQuery request, CancellationToken cancellationToken)
        {

            var rounds = await _documentSession
                .Query<Round>()
                .Where(r => !r.Deleted)
                .Where(r => r.PlayerScores.Any(s => s.PlayerName == request.Username))
                .Where(r => r.StartTime > new DateTime(request.Year, 1, 1) && r.StartTime < new DateTime(request.Year + 1, 1, 1))
                .ToListAsync(token: cancellationToken);

            var holesWithDetails = rounds.PlayerHolesWithDetails(request.Username);
            var playerRounds = rounds.Where(r => r.PlayerScores.Any(p => p.PlayerName == request.Username)).ToList();
            var totalScore = playerRounds.Sum(r => r.PlayerScore(request.Username));

            var cardmateAverages = CalculateCardmateAverages(playerRounds, request.Username);

            var bestCardmate = cardmateAverages.OrderBy(c => c.Value).First().Key;
            var bestCardmateAverageScore = cardmateAverages[bestCardmate];
            var worstCardmate = cardmateAverages.OrderBy(c => c.Value).Last().Key;
            var worstCardmateAverageScore = cardmateAverages[worstCardmate];

            var mostPlayedCourse = playerRounds.GroupBy(r => r.CourseName).OrderByDescending(g => g.Count()).First();


            return new UserYearSummary
            {
                HoursPlayed = rounds.Sum(r => (r.DurationMinutes > 10 ? r.DurationMinutes : 0) / 60.0),
                RoundsPlayed = rounds.Count,
                TotalScore = totalScore,
                BestCardmate = bestCardmate,
                BestCardmateAverageScore = bestCardmateAverageScore,
                WorstCardmate = worstCardmate,
                WorstCardmateAverageScore = worstCardmateAverageScore,
                MostPlayedCourse = mostPlayedCourse.Key,
                MostPlayedCourseRoundsCount = mostPlayedCourse.Count(),
            };
        }

        private static Dictionary<string, double> CalculateCardmateAverages(List<Round> playerRounds, string requestUsername)
        {
            var cardmates = playerRounds
                .SelectMany(r => r.PlayerScores.Select(p => p.PlayerName))
                .Where(name => name != requestUsername) 
                .Distinct()
                .ToDictionary(p => p, _ => 0.0);

            foreach (var cardmate in cardmates.Keys.ToList()) 
            {
                var roundsWithCardmate = playerRounds.Where(r => r.PlayerScores.Any(p => p.PlayerName == cardmate));
                if (roundsWithCardmate.Count() < 5)
                {
                    cardmates.Remove(cardmate);
                    continue;
                }

                cardmates[cardmate] = roundsWithCardmate.Sum(r => r.PlayerScore(requestUsername)) / roundsWithCardmate.Count();
            }

            return cardmates;
        }
    }
}