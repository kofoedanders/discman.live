using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<RoundsHub> _roundsHub;

        public ToggleLikeItemCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor,
            IHubContext<RoundsHub> roundsHub)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
            _roundsHub = roundsHub;
        }

        public async Task<bool> Handle(ToggleLikeItemCommand request, CancellationToken cancellationToken)
        {
            var username = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var feedItem = await _documentSession.Query<GlobalFeedItem>().SingleAsync(x => x.Id == request.FeedItemId, token: cancellationToken);


            if (feedItem.Likes.Any(x => x == username))
            {
                feedItem.Likes.Remove(username);
            }
            else
            {
                feedItem.Likes.Add(username);
            }

            _documentSession.Update(feedItem);
            await _documentSession.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}