using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Web.Courses;
using Microsoft.EntityFrameworkCore;
using Web.Matches;
using Web.Infrastructure;
using Web.Tournaments.Domain;

namespace Web.Tournaments.Queries
{
    public class GetTournamentCommand : IRequest<TournamentVm>
    {
        public Guid TournamentId { get; set; }
    }

    public class GetTournamentCommandHandler : IRequestHandler<GetTournamentCommand, TournamentVm>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly TournamentCache _tournamentCache;

        public GetTournamentCommandHandler(DiscmanDbContext dbContext, IMapper mapper, TournamentCache tournamentCache)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _tournamentCache = tournamentCache;
        }

        public async Task<TournamentVm> Handle(GetTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournamentVm = new TournamentVm();

            var tournament = await _dbContext.Tournaments.SingleAsync(x => x.Id == request.TournamentId, cancellationToken);
            tournamentVm.Info = _mapper.Map<TournamentInfo>(tournament);
            tournamentVm.Info.Courses = tournament.Courses.Select(cid =>
            {
                var course = _dbContext.Courses.Single(c => c.Id == cid);
                return new CourseNameAndId
                {
                    Id = course.Id,
                    Name = course.Name,
                    Layout = course.Layout
                };
            }).ToList();

            if (tournament.Start < DateTime.UtcNow)
            {
                tournamentVm.Leaderboard = await _tournamentCache.GetOrCreate(tournament.Id, () => CalculateLeaderboard(tournament));
            }

            if (tournament.Prices != null)
            {
                tournamentVm.Prices = _mapper.Map<TournamentPricesVm>(tournament.Prices);
            }

            return tournamentVm;
        }

        private async Task<TournamentLeaderboard> CalculateLeaderboard(Tournament tournament)
        {
            var leaderboard = new TournamentLeaderboard();
            foreach (var tournamentPlayer in tournament.Players)
            {
                var tournamentRounds = await _dbContext.GetTournamentRounds(tournamentPlayer, tournament);

                var totalScore = tournamentRounds.Sum(r => r.PlayerScore(tournamentPlayer));
                var coursesPlayed = tournamentRounds.Select(r => r.CourseId).Distinct().ToList();
                var hcpScore = tournamentRounds.Sum(r => r.PlayerHcpScore(tournamentPlayer));

                leaderboard.Scores.Add(new TournamentScore
                {
                    Name = tournamentPlayer,
                    TotalScore = totalScore,
                    CoursesPlayed = coursesPlayed,
                    TotalHcpScore = hcpScore
                });
            }

            leaderboard.Scores = leaderboard.Scores
                .OrderByDescending(s => s.CoursesPlayed.Count)
                .ThenBy(s => s.TotalHcpScore)
                .ToList();

            return leaderboard;
        }
    }
}
