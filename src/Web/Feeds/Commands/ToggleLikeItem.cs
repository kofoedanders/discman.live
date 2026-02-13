using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web.Feeds.Domain;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Feeds.Commands
{
    public class ToggleLikeItemCommand : IRequest<bool>
    {
        public Guid FeedItemId { get; set; }
    }

    public class ToggleLikeItemCommandHandler : IRequestHandler<ToggleLikeItemCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public ToggleLikeItemCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            IHubContext<RoundsHub> roundsHub)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<bool> Handle(ToggleLikeItemCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var feedItem = await _dbContext.GlobalFeedItems.SingleAsync(x => x.Id == request.FeedItemId, cancellationToken);


            if (feedItem.Likes.Any(x => x == username))
            {
                feedItem.Likes.Remove(username);
            }
            else
            {
                feedItem.Likes.Add(username);
            }

            _dbContext.GlobalFeedItems.Update(feedItem);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}