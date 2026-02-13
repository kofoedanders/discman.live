using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Rounds.NSBEvents;
using Web.Users;

namespace Web.Feeds.Handlers
{
    public class UpdateFeedsOnScoreUpdated : IHandleMessages<ScoreWasUpdated>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public UpdateFeedsOnScoreUpdated(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(ScoreWasUpdated notification, IMessageHandlerContext context)
        {
            await CleanupFeedsIfScoreWasChanged(notification);
            if (notification.RelativeScore > -1) return;

            var user = await _dbContext.Users.SingleAsync(x => x.Username == notification.Username, context.CancellationToken);
            var friends = user.Friends ?? new List<string>();
            friends.Add(notification.Username);

            var feedItem = new GlobalFeedItem
            {
                Subjects = new List<string> { user.Username },
                ItemType = ItemType.Hole,
                CourseName = notification.CourseName,
                HoleScore = notification.RelativeScore,
                HoleNumber = notification.HoleNumber,
                RegisteredAt = DateTime.UtcNow,
                RoundId = notification.RoundId
            };

            _dbContext.GlobalFeedItems.Add(feedItem);

            _dbContext.UpdateFriendsFeeds(friends, feedItem);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }

        private async Task CleanupFeedsIfScoreWasChanged(ScoreWasUpdated notification)
        {
            if (!notification.ScoreWasChanged) return;

            var globalItem = await _dbContext.GlobalFeedItems
                .Where(i =>
                    i.RoundId == notification.RoundId &&
                    i.HoleNumber == notification.HoleNumber &&
                    i.Subjects.Any(s => s == notification.Username))
                .SingleOrDefaultAsync();

            if (globalItem is null) return;

            var userItems = await _dbContext.UserFeedItems
                .Where(i => i.FeedItemId == globalItem.Id)
                .ToListAsync();

            _dbContext.UserFeedItems.RemoveRange(userItems);
            _dbContext.GlobalFeedItems.Remove(globalItem);
            await _dbContext.SaveChangesAsync();
        }
    }
}
