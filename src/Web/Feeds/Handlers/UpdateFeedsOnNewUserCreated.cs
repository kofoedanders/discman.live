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
    public class UpdateFeedsOnNewUserCreated : IHandleMessages<NewUserWasCreated>
    {
        private readonly DiscmanDbContext _dbContext;

        public UpdateFeedsOnNewUserCreated(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(NewUserWasCreated notification, IMessageHandlerContext context)
        {
            var user = await _dbContext.Users.SingleAsync(x => x.Username == notification.Username, context.CancellationToken);

            var feedItem = new GlobalFeedItem
            {
                Subjects = new List<string> { user.Username },
                ItemType = ItemType.User,
                Action = Action.Created,
                RegisteredAt = DateTime.UtcNow,
            };

            _dbContext.GlobalFeedItems.Add(feedItem);

            var userFeedItems = new List<UserFeedItem> { new UserFeedItem
            {
                FeedItemId = feedItem.Id,
                ItemType = feedItem.ItemType,
                RegisteredAt = feedItem.RegisteredAt,
                Username = user.Username
            } };
            _dbContext.UserFeedItems.AddRange(userFeedItems);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}