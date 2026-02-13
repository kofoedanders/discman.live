using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;
using Web.Rounds;

namespace Web.Users.Queries
{
    public class GetUserDetailsQuery : IRequest<UserDetails>
    {
        public string Username { get; set; }
    }

    public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, UserDetails>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public GetUserDetailsQueryHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<UserDetails> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims
                .Single(c => c.Type == ClaimTypes.Name).Value;
            var username = !string.IsNullOrWhiteSpace(request.Username) ? request.Username : authenticatedUsername;
            var user = await _dbContext.Users
                .SingleAsync(u => u.Username == username, cancellationToken);
            var details = _mapper.Map<UserDetails>(user);
            details.RatingHistory = details.RatingHistory.Where(r => r.DateTime > DateTime.UtcNow.AddYears(-1)).ToList();


            var activeRound = await _dbContext.Rounds
                .Where(r => !r.Deleted)
                .Where(r => !r.IsCompleted)
                .Where(r => r.PlayerScores.Any(s => s.PlayerName == username))
                .FirstOrDefaultAsync(cancellationToken);

            if (activeRound != null) details.ActiveRound = activeRound.Id;

            return details;
        }
    }
}
