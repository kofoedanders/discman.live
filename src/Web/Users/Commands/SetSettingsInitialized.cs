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
    public class SetSettingsInitializedCommand : IRequest<bool>
    {

    }

    public class SetSettingsInitializedCommandHandler : IRequestHandler<SetSettingsInitializedCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetSettingsInitializedCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(SetSettingsInitializedCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == authenticatedUsername, token: cancellationToken);

            user.SettingsInitialized = true;

            _documentSession.Update(user);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}