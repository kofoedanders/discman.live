using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Tournaments.Notifications;
using Web.Users;
using Action = Web.Feeds.Domain.Action;

namespace Web.Feeds.Handlers
{
    public class UpdateFeedsOnUserJoinedTournament : IHandleMessages<PlayerJoinedTournament>
    {
        private readonly DiscmanDbContext _dbContext;

        public UpdateFeedsOnUserJoinedTournament(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(PlayerJoinedTournament notification, IMessageHandlerContext context)
        {
            var user = await _dbContext.Users.SingleAsync(x => x.Username == notification.Username, context.CancellationToken);
            var friends = user.Friends ?? new List<string>();

            var feedItem = new GlobalFeedItem
            {
                Subjects = new List<string> { user.Username },
                ItemType = ItemType.Tournament,
                Action = Action.Joined,
                RegisteredAt = DateTime.UtcNow,
                TournamentId = notification.TournamentId,
                TournamentName = notification.TournamentName
            };

            _dbContext.GlobalFeedItems.Add(feedItem);

            var userFeedItems = friends.Select(friend => new UserFeedItem
            {
                FeedItemId = feedItem.Id,
                ItemType = feedItem.ItemType,
                RegisteredAt = feedItem.RegisteredAt,
                Username = friend
            }).ToList();
            _dbContext.UserFeedItems.AddRange(userFeedItems);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
