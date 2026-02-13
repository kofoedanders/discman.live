using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Web.Rounds;
using System.Linq;
using Web.Common;
using Web.Infrastructure;

namespace Web.Rounds.Queries
{
    public class GetRoundQuery : IRequest<Round>
    {
        public Guid RoundId { get; set; }
    }

    public class GetRoundsQueryHandler : IRequestHandler<GetRoundQuery, Round>
    {
        private readonly DiscmanDbContext _dbContext;

        public GetRoundsQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<Round> Handle(GetRoundQuery request, CancellationToken cancellationToken)
        {
            var round =  _dbContext.Rounds
                .Where(r => !r.Deleted)
                .SingleOrDefault(x => x.Id == request.RoundId);
            
            if (round is null)
            {
                throw new NotFoundException(nameof(Round), request.RoundId);
            }

            return await Task.FromResult(round);
        }
    }
}
