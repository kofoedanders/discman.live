using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using Web.Courses;
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
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMediator _mediator;

        public AddPlayerToTournamentCommandHandler(IDocumentSession documentSession, IHttpContextAccessor contextAccessor, IMediator mediator)
        {
            _documentSession = documentSession;
            _contextAccessor = contextAccessor;
            _mediator = mediator;
        }

        public async Task<bool> Handle(AddPlayerToTournamentCommand request, CancellationToken cancellationToken)
        {
            var username = _contextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var tournament = await _documentSession.Query<Tournament>().SingleAsync(t => t.Id == request.TournamentId, token: cancellationToken);
            tournament.AddPlayer(username);

            _documentSession.Update(tournament);
            await _documentSession.SaveChangesAsync(cancellationToken);

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