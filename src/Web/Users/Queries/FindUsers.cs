using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;

namespace Web.Users.Queries
{
    public class FindUsersQuery : IRequest<IReadOnlyList<string>>
    {
        public string UsernameSearchString { get; set; }
    }
    
    public class FindUserQueryHandler: IRequestHandler<FindUsersQuery, IReadOnlyList<string>>
    {
        private readonly DiscmanDbContext _dbContext;

        public FindUserQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IReadOnlyList<string>> Handle(FindUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _dbContext.Users
                .Where(u => u.Username.Contains(request.UsernameSearchString.ToLower()))
                .Select(u => u.Username)
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
