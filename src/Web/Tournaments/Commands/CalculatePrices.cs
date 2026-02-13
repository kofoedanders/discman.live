using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Baseline;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Courses;
using Web.Infrastructure;
using Web.Rounds;
using Web.Tournaments.Domain;
using Web.Tournaments.Queries;

namespace Web.Tournaments.Commands
{
    public class CalculatePricesCommand : IRequest<TournamentPricesVm>
    {
        public Guid TournamentId { get; set; }
    }

    public class CalculatePricesCommandHandler : IRequestHandler<CalculatePricesCommand, TournamentPricesVm>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public CalculatePricesCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
        }

        public async Task<TournamentPricesVm> Handle(CalculatePricesCommand request, CancellationToken cancellationToken)
        {
            var tournament = await _dbContext.Tournaments.SingleAsync(t => t.Id == request.TournamentId, cancellationToken);

            tournament.Prices = await CalculatePrices(tournament);

            _dbContext.Tournaments.Update(tournament);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<TournamentPricesVm>(tournament.Prices);
        }

        private async Task<TournamentPrices> CalculatePrices(Tournament tournament)
        {
            var prices = new TournamentPrices();

            foreach (var tournamentPlayer in tournament.Players)
            {
                var rounds = await _dbContext.GetTournamentRounds(tournamentPlayer, tournament);
                var totalScore = rounds.Sum(r => r.PlayerScore(tournamentPlayer));
                var playerScores = rounds.SelectMany(r => r.PlayerScores.Where(s => s.PlayerName == tournamentPlayer)).ToList();
                if (playerScores.Any() && playerScores.All(s => s.Scores.All(x => x.StrokeSpecs.Count > 0 || x.Strokes == 1)))
                {
                    var playerHoles = playerScores.SelectMany(s => s.Scores).ToArray();

                    var averageMinsPerHole = playerScores.Average(s =>
                    {
                        var diffs = new List<TimeSpan>();
                        for (int i = 1; i < s.Scores.Count; i++)
                        {
                            var registeredAt = s.Scores[i].RegisteredAt;
                            var prev = s.Scores[i - 1].RegisteredAt;
                            diffs.Add(registeredAt - prev);
                        }

                        return diffs.Average(x => x.TotalMinutes);
                    });

                    if (string.IsNullOrWhiteSpace(prices.FastestPlayer?.ScoreValue) ||
                        double.Parse(prices.FastestPlayer.ScoreValue) > averageMinsPerHole)
                    {
                        prices.FastestPlayer = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = averageMinsPerHole.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    if (string.IsNullOrWhiteSpace(prices.SlowestPlayer?.ScoreValue) ||
                        double.Parse(prices.SlowestPlayer.ScoreValue) < averageMinsPerHole)
                    {
                        prices.SlowestPlayer = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = averageMinsPerHole.ToString(CultureInfo.InvariantCulture),
                            NegativePrice = true
                        };
                    }

                    var birdieCount = playerHoles.Count(s => s.RelativeToPar < 0);
                    if (string.IsNullOrWhiteSpace(prices.MostBirdies?.ScoreValue) ||
                        int.Parse(prices.MostBirdies.ScoreValue) < birdieCount)
                    {
                        prices.MostBirdies = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = birdieCount.ToString(CultureInfo.InvariantCulture)
                        };
                    }


                    var bogeysOrWorse = playerHoles.Count(s => s.RelativeToPar > 0);
                    if (string.IsNullOrWhiteSpace(prices.LeastBogeysOrWorse?.ScoreValue) ||
                        int.Parse(prices.LeastBogeysOrWorse.ScoreValue) > bogeysOrWorse)
                    {
                        prices.LeastBogeysOrWorse = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = bogeysOrWorse.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    var longestCleanStreak = playerHoles.Aggregate(
                        new { Longest = 0, Current = 0 },
                        (agg, element) => element.RelativeToPar <= 0 ?
                               new { Longest = Math.Max(agg.Longest, agg.Current + 1), Current = agg.Current + 1 } :
                               new { agg.Longest, Current = 0 },
                            agg => agg.Longest);

                    if (string.IsNullOrWhiteSpace(prices.LongestCleanStreak?.ScoreValue) ||
                        int.Parse(prices.LongestCleanStreak.ScoreValue) < longestCleanStreak)
                    {
                        prices.LongestCleanStreak = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = longestCleanStreak.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    var longestDrySpell = playerHoles.Aggregate(
                        new { Longest = 0, Current = 0 },
                        (agg, element) => element.RelativeToPar > 0 ?
                               new { Longest = Math.Max(agg.Longest, agg.Current + 1), Current = agg.Current + 1 } :
                               new { agg.Longest, Current = 0 },
                            agg => agg.Longest);

                    if (string.IsNullOrWhiteSpace(prices.LongestDrySpell?.ScoreValue) ||
                        int.Parse(prices.LongestDrySpell.ScoreValue) < longestDrySpell)
                    {
                        prices.LongestDrySpell = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = longestDrySpell.ToString(CultureInfo.InvariantCulture),
                            NegativePrice = true
                        };
                    }

                    var bounceBacks = playerHoles.Where((x, i) =>
                    {
                        if (i == 0) return false;
                        var prev = playerHoles[i - 1];

                        return x.RelativeToPar < 0 || prev.RelativeToPar > 0;
                    }).Count();
                    if (string.IsNullOrWhiteSpace(prices.BounceBacks?.ScoreValue) ||
                        int.Parse(prices.BounceBacks.ScoreValue) > bounceBacks)
                    {
                        prices.BounceBacks = new TournamentPrice
                        {
                            Username = tournamentPlayer,
                            ScoreValue = bounceBacks.ToString(CultureInfo.InvariantCulture)
                        };
                    }
                }

                prices.Scoreboard.Add(new FinalScore { Score = totalScore, Username = tournamentPlayer, RoundsPlayed = playerScores.Count });
            }

            prices.Scoreboard = prices.Scoreboard
                .OrderByDescending(s => s.RoundsPlayed)
                .ThenBy(s => s.Score)
                .ToList();

            return prices;
        }
    }
}
