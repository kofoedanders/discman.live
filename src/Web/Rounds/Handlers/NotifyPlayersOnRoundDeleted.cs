using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;
using NServiceBus;

namespace Web.Rounds.Notifications
{
    public class NotifyPlayersOnRoundDeleted : IHandleMessages<RoundWasDeleted>
    {
        private readonly IHubContext<RoundsHub> _roundsHub;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotifyPlayersOnRoundDeleted(IHubContext<RoundsHub> roundsHub, IHttpContextAccessor httpContextAccessor)
        {
            _roundsHub = roundsHub;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task Handle(RoundWasDeleted notification, IMessageHandlerContext context)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var notificationPlayers = notification.Players.Where(p => p != username).ToList();
            return _roundsHub.NotifyPlayersOnDeletedRound(notification.RoundId, notificationPlayers);
        }
    }
}
