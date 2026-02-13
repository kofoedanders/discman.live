using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NServiceBus;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Rounds;
using Web.Rounds.NSBEvents;
using Web.Users;
using Action = Web.Feeds.Domain.Action;

namespace Web.Feeds.Handlers
{
    public class UpdateFeedsOnRoundStarted : IHandleMessages<RoundWasStarted>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public UpdateFeedsOnRoundStarted(DiscmanDbContext dbContext, IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _roundsHub = roundsHub;
        }

        public async Task Handle(RoundWasStarted notification, IMessageHandlerContext context)
        {
            var round = await _dbContext.Rounds.SingleAsync(x => x.Id == notification.RoundId, context.CancellationToken);

            var players = round.PlayerScores.Select(s => s.PlayerName).ToList();
            var friends = players.ToList();
            foreach (var player in players)
            {
                var user = await _dbContext.Users.SingleAsync(x => x.Username == player, context.CancellationToken);
                friends.AddRange(user.Friends ?? new List<string>());
            }

            friends = friends.Distinct().ToList();


            var feedItem = new GlobalFeedItem
            {
                Subjects = players,
                ItemType = ItemType.Round,
                Action = Action.Started,
                RegisteredAt = DateTime.UtcNow,
                CourseName = round.CourseName,
                RoundId = round.Id
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
