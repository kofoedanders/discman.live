using System.Collections.Generic;
using System.Linq;
using Web.Feeds.Domain;
using Web.Infrastructure;

namespace Web.Feeds
{
    public static class StorageExtensions
    {
        public static void UpdateFriendsFeeds(this DiscmanDbContext dbContext, List<string> friends, GlobalFeedItem feedItem)
        {
            var userFeedItems = friends.Select(friend => new UserFeedItem
            {
                FeedItemId = feedItem.Id,
                ItemType = feedItem.ItemType,
                RegisteredAt = feedItem.RegisteredAt,
                Username = friend
            }).ToList();

            dbContext.UserFeedItems.AddRange(userFeedItems);
        }
    }
}
