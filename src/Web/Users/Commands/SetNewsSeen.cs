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
    public class SetNewsSeenCommand : IRequest<bool>
    {
        public string NewsId { get; set; }
    }
    
    public class SetNewsSeenCommandHandler : IRequestHandler<SetNewsSeenCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetNewsSeenCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<bool> Handle(SetNewsSeenCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == authenticatedUsername, token: cancellationToken);

            user.SetNewsSeen(request.NewsId);

            _documentSession.Update(user);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}