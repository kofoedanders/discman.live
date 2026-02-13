using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;

namespace Web.Feeds.Handlers
{
    public class UpdateFeedsOnRoundDeleted : IHandleMessages<RoundWasDeleted>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public UpdateFeedsOnRoundDeleted(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(RoundWasDeleted notification, IMessageHandlerContext context)
        {
            var globalFeedItems = await _dbContext.GlobalFeedItems
                .Where(x => x.RoundId == notification.RoundId)
                .ToListAsync(context.CancellationToken);
            var globalItemIds = globalFeedItems.Select(item => item.Id).ToArray();
            var userFeedItems = await _dbContext.UserFeedItems
                .Where(x => globalItemIds.Contains(x.FeedItemId))
                .ToListAsync(context.CancellationToken);

            _dbContext.GlobalFeedItems.RemoveRange(globalFeedItems);
            _dbContext.UserFeedItems.RemoveRange(userFeedItems);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
