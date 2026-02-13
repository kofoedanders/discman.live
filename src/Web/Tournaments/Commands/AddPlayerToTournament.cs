using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Courses;
using Web.Infrastructure;
using Web.Tournaments.Domain;
using Web.Tournaments.Notifications;
using Web.Tournaments.Queries;

namespace Web.Tournaments.Commands
{
    public class AddPlayerToTournamentCommand : IRequest<bool>
    {
        public Guid TournamentId { get; set; }
    }

    public class AddPlayerToTournamentCommandHandler : IRequestHandler<AddPlayerToTournamentCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMediator _mediator;

        public AddPlayerToTournamentCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor contextAccessor, IMediator mediator)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _mediator = mediator;
        }

        public async Task<bool> Handle(AddPlayerToTournamentCommand request, CancellationToken cancellationToken)
        {
            var username = _contextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var tournament = await _dbContext.Tournaments.SingleAsync(t => t.Id == request.TournamentId, cancellationToken);
            tournament.AddPlayer(username);

            _dbContext.Tournaments.Update(tournament);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new PlayerJoinedTournament
            {
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                Username = username
            }, cancellationToken);

            return true;
        }
    }
}
