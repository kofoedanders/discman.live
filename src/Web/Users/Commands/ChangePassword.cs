using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Web.Infrastructure;

namespace Web.Users.Commands
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public string NewPassword { get; set; }
    }
    
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _tokenSecret;

        public ChangePasswordCommandHandler(DiscmanDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _tokenSecret = configuration.GetValue<string>("TOKEN_SECRET");
        }
        
        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);
            
            var hashedPw = new SaltSeasonedHashedPassword(request.NewPassword);

            user.ChangePassword(hashedPw);

            _dbContext.Users.Update(user);
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
