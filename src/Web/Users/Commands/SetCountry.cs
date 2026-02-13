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
    public class SetCountryCommand : IRequest<bool>
    {
        public string Country { get; set; }
    }

    public class SetCountryCommandHandler : IRequestHandler<SetCountryCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetCountryCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(SetCountryCommand request, CancellationToken cancellationToken)
        {
            if (request.Country.Length > 20) return true;
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);

            user.Country = request.Country;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
