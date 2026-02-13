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
    public class SetEmojiCommand : IRequest<bool>
    {
        public string Emoji { get; set; }
    }

    public class SetEmojiCommandHandler : IRequestHandler<SetEmojiCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetEmojiCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(SetEmojiCommand request, CancellationToken cancellationToken)
        {
            if (request.Emoji.Length > 2) return true;
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);

            user.Emoji = request.Emoji;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
