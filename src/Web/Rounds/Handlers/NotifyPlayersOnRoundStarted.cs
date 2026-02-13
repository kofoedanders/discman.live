using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Web.Infrastructure;
using NServiceBus;
using Web.Rounds.NSBEvents;
using Microsoft.EntityFrameworkCore;

namespace Web.Rounds.Notifications
{
    public class NotifyPlayersOnRoundStarted : IHandleMessages<RoundWasStarted>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public NotifyPlayersOnRoundStarted(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(RoundWasStarted notification, IMessageHandlerContext context)
        {
            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == notification.RoundId, context.CancellationToken);
            await _roundsHub.NotifyPlayersOnNewRound(round);
        }
    }
}
