using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Matches;
using Web.Rounds;

namespace Web.Users.Queries
{
    public class GetUserAchievementsQuery : IRequest<List<AchievementAndCount>>
    {
        public string Username { get; set; }
    }
    
    public class GetUserAchievementsQueryHandler : IRequestHandler<GetUserAchievementsQuery, List<AchievementAndCount>>
    {
        private readonly DiscmanDbContext _dbContext;

        public GetUserAchievementsQueryHandler(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<List<AchievementAndCount>> Handle(GetUserAchievementsQuery request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.SingleAsync(u => u.Username == request.Username, cancellationToken);

            if (user.Achievements is null || !user.Achievements.Any())
                return new List<AchievementAndCount>();

            var userAchievements = user
                .Achievements
                .GroupBy(x => x.AchievementName)
                .Select(x => new AchievementAndCount
                {
                    Achievement = x.OrderByDescending(y => y.AchievedAt).First(),
                    Count = x.Count()
                })
                .ToList();

            return userAchievements;
        }
        
    }

    public class AchievementAndCount
    {
        public Achievement Achievement { get; set; }
        public int Count { get; set; }
    }
}
