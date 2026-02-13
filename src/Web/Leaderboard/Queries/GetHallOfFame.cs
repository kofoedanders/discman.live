using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Web.Infrastructure;

namespace Web.Leaderboard.Queries
{
    public class GetHallOfFameQuery : IRequest<HallOfFame>
    {
    }

    public class GetHallOfFameQueryHandler : IRequestHandler<GetHallOfFameQuery, HallOfFame>
    {
        private readonly DiscmanDbContext _dbContext;

        public GetHallOfFameQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HallOfFame> Handle(GetHallOfFameQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.HallOfFames.SingleAsync(cancellationToken);
        }
    }
}