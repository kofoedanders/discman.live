using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Rounds.Queries
{
    public class GetUserRoundsQuery : IRequest<RoundsVm>
    {
        public string Username { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
    
    public class GetUserRoundsQueryHandler : IRequestHandler<GetUserRoundsQuery, RoundsVm>
    {
        private readonly DiscmanDbContext _dbContext;

        public GetUserRoundsQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<RoundsVm> Handle(GetUserRoundsQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _dbContext.Rounds
                .Where(r => !r.Deleted)
                .Where(r => r.PlayerScores.Any(p => p.PlayerName == request.Username));
            var totalItemCount = await baseQuery.CountAsync(cancellationToken);
            var rounds = await baseQuery
                .OrderByDescending(x => x.StartTime)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            var totalPages = request.PageSize > 0
                ? (int)Math.Ceiling(totalItemCount / (double)request.PageSize)
                : 0;
            
            return new RoundsVm
            {
                Rounds = rounds,
                Pages = totalPages,
                PageNumber = request.Page,
                TotalItemCount = totalItemCount
            };
        }
    }
}
