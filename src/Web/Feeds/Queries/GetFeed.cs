using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Feeds.Domain;
using Web.Infrastructure;

namespace Web.Feeds.Queries
{
    public class GetFeedCommand : IRequest<FeedVm>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public ItemType? ItemType { get; set; }
    }

    public class GetFeedCommandHandler : IRequestHandler<GetFeedCommand, FeedVm>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public GetFeedCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<FeedVm> Handle(GetFeedCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

            var userFeedQuery = _dbContext
                .UserFeedItems
                .Where(x => x.Username == authenticatedUsername)
                .OrderByDescending(x => x.RegisteredAt);

            if (request.ItemType != null) userFeedQuery = (IOrderedQueryable<UserFeedItem>) userFeedQuery.Where(x => x.ItemType == request.ItemType);

            var totalCount = await userFeedQuery.CountAsync(cancellationToken);
            var userFeed = await userFeedQuery
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var feedItemIds = userFeed.Select(x => x.FeedItemId).ToList();
            var feedItems = await _dbContext.GlobalFeedItems
                .Where(x => feedItemIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var isLastPage = request.PageNumber * request.PageSize >= totalCount;

            return new FeedVm
            {
                FeedItems = feedItems.OrderByDescending(x => x.RegisteredAt).Select(x => _mapper.Map<FeedItemVm>(x)).ToList(),
                IsLastPage = isLastPage
            };
        }
    }
}
