using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Rounds;
using Web.Infrastructure;
using Web.Tournaments.Domain;

namespace Web.Tournaments.Queries
{
    public class GetTournamentsCommand : IRequest<IEnumerable<TournamentListing>>
    {
        public Guid TournamentId { get; set; }
        public bool OnlyActive { get; set; }
        public string Username { get; set; }
    }

    public class GetTournamentsCommandHandler : IRequestHandler<GetTournamentsCommand, IEnumerable<TournamentListing>>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly TournamentCache _tournamentCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetTournamentsCommandHandler(DiscmanDbContext dbContext, IMapper mapper, TournamentCache tournamentCache,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _tournamentCache = tournamentCache;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TournamentListing>> Handle(GetTournamentsCommand request, CancellationToken cancellationToken)
        {
            var username = request.Username ?? _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var tournaments = await _dbContext.Tournaments
                .Where(t => t.Players.Any(p => p == username))
                .Where(t => !request.OnlyActive || t.End >= DateTime.UtcNow.Date)
                .ToListAsync(cancellationToken);

            return tournaments.Select(t => _mapper.Map<TournamentListing>(t)).OrderByDescending(s => s.Start);
        }
    }
}
