using System.Collections.Generic;
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
    public class UpdateFeedsOnAchievementEarned : IHandleMessages<UserEarnedAchievement>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public UpdateFeedsOnAchievementEarned(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(UserEarnedAchievement notification, IMessageHandlerContext context)
        {
            var user = await _dbContext.Users.SingleAsync(x => x.Username == notification.Username, context.CancellationToken);
            var friends = user.Friends ?? new List<string>();
            friends.Add(notification.Username);

            var feedItem = new GlobalFeedItem
            {
                Subjects = new List<string> { user.Username },
                ItemType = ItemType.Achievement,
                RegisteredAt = notification.AchievedAt,
                RoundId = notification.RoundId,
                AchievementName = notification.AchievementName
            };

            _dbContext.GlobalFeedItems.Add(feedItem);

            _dbContext.UpdateFriendsFeeds(friends, feedItem);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
