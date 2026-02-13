using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;

namespace Web.Rounds.Commands
{
    public class CompleteRoundCommand : IRequest<bool>
    {
        public Guid RoundId { get; set; }
        public string Base64Signature { get; set; }
    }

    public class CompleteRoundCommandHandler : IRequestHandler<CompleteRoundCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;
        private readonly IMessageSession _messageSession;

        public CompleteRoundCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IHubContext<RoundsHub> roundsHub, IMessageSession messageSession)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
            _messageSession = messageSession;
        }

        public async Task<bool> Handle(CompleteRoundCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == request.RoundId, cancellationToken);
            if (!round.IsPartOfRound(authenticatedUsername)) throw new UnauthorizedAccessException("You can only complete rounds you are part of");
            if (round.IsCompleted) return true;
            if (round.Signatures.Any(s => s.Username == authenticatedUsername)) return true;

            round.SignRound(authenticatedUsername, request.Base64Signature);

            _dbContext.Rounds.Update(round);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _roundsHub.NotifyPlayersOnUpdatedRound(authenticatedUsername, round);
            if (round.IsCompleted) await _messageSession.Publish(new RoundWasCompleted { RoundId = round.Id });

            return true;
        }
    }
}
