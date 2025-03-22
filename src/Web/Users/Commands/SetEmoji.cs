using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Linq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Web.Users.Commands
{
    public class SetEmojiCommand : IRequest<bool>
    {
        public string Emoji { get; set; }
    }

    public class SetEmojiCommandHandler : IRequestHandler<SetEmojiCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetEmojiCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(SetEmojiCommand request, CancellationToken cancellationToken)
        {
            if (request.Emoji.Length > 2) return true;
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == authenticatedUsername, token: cancellationToken);

            user.Emoji = request.Emoji;

            _documentSession.Update(user);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}