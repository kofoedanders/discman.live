using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Commands
{
    public class DeleteHoleCommand : IRequest<bool>
    {
        public Guid RoundId { get; set; }
        public int HoleNumber { get; set; }
    }

    public class DeleteHoleCommandHandler : IRequestHandler<DeleteHoleCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public DeleteHoleCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<bool> Handle(DeleteHoleCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var round = await _documentSession.Query<Round>().SingleAsync(x => x.Id == request.RoundId, token: cancellationToken);

            if (round.CreatedBy != username) throw new UnauthorizedAccessException("Only rounds created by yourself can be modified");

            foreach (var playerScore in round.PlayerScores)
            {
                playerScore.Scores = playerScore.Scores.Where(s => s.Hole.Number != request.HoleNumber).ToList();
            }

            _documentSession.Update(round);
            await _documentSession.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(username, round);

            return true;
        }
    }
}