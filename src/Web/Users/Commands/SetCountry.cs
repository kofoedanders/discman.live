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
    public class SetCountryCommand : IRequest<bool>
    {
        public string Country { get; set; }
    }

    public class SetCountryCommandHandler : IRequestHandler<SetCountryCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetCountryCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Handle(SetCountryCommand request, CancellationToken cancellationToken)
        {
            if (request.Country.Length > 20) return true;
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == authenticatedUsername, token: cancellationToken);

            user.Country = request.Country;

            _documentSession.Update(user);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}