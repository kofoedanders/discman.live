using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;

namespace Web.Users.Commands
{
    public class SetNewsSeenCommand : IRequest<bool>
    {
        public string NewsId { get; set; }
    }
    
    public class SetNewsSeenCommandHandler : IRequestHandler<SetNewsSeenCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetNewsSeenCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<bool> Handle(SetNewsSeenCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);

            user.SetNewsSeen(request.NewsId);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
