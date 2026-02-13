using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Users;
using Web.Users.NSBEvents;
using Action = Web.Feeds.Domain.Action;

namespace Web.Feeds.Handlers
{
    public class UpdateFeedsFriendsWasAdded : IHandleMessages<FriendWasAdded>
    {
        private readonly DiscmanDbContext _dbContext;

        public UpdateFeedsFriendsWasAdded(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(FriendWasAdded notification, IMessageHandlerContext context)
        {
            var user = await _dbContext.Users.SingleAsync(x => x.Username == notification.Username, context.CancellationToken);
            var friend = await _dbContext.Users.SingleAsync(x => x.Username == notification.FriendName, context.CancellationToken);

            var feedItem = new GlobalFeedItem
            {
                Subjects = new List<string> { user.Username },
                ItemType = ItemType.Friend,
                Action = Action.Added,
                RegisteredAt = DateTime.UtcNow,
                FriendName = friend.Username
            };

            _dbContext.GlobalFeedItems.Add(feedItem);

            var userFeedItems = new List<string> { user.Username, friend.Username }.Select(username => new UserFeedItem
            {
                FeedItemId = feedItem.Id,
                ItemType = feedItem.ItemType,
                RegisteredAt = feedItem.RegisteredAt,
                Username = username
            }).ToList();

            _dbContext.UserFeedItems.AddRange(userFeedItems);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}