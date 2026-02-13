using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;


namespace Web.Rounds.Commands
{
    public class DeleteRoundCommand : IRequest<bool>
    {
        public Guid RoundId { get; set; }
    }

    public class DeleteRoundCommandHandler : IRequestHandler<DeleteRoundCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageSession _messageSession;

        public DeleteRoundCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMessageSession messageSession)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _messageSession = messageSession;
        }

        public async Task<bool> Handle(DeleteRoundCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == request.RoundId, cancellationToken);

            if (round.CreatedBy != username) throw new UnauthorizedAccessException("Only rounds created by yourself can be deleted");

            _dbContext.Rounds.Remove(round);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _messageSession.Publish(new RoundWasDeleted { RoundId = round.Id, Players = round.PlayerScores.Select(s => s.PlayerName).ToList() });

            return true;
        }
    }
}
