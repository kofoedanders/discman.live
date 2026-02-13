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
    public class ChangeEmailCommand : IRequest<string>
    {
        public string NewEmail { get; set; }
    }
    
    public class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, string>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangeEmailCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<string> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);
            
            user.ChangeEmail(request.NewEmail);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user.Email;
        }
    }
}
