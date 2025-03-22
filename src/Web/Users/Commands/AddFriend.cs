using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using NServiceBus;
using Web.Users.NSBEvents;

namespace Web.Users.Commands
{
    public class AddFriendCommand : IRequest<bool>
    {
        public string Username { get; set; }
    }

    public class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageSession _messageSession;

        public AddFriendCommandHandler(IDocumentSession documentSession, IHttpContextAccessor httpContextAccessor, IMessageSession messageSession)
        {
            _documentSession = documentSession;
            _httpContextAccessor = httpContextAccessor;
            _messageSession = messageSession;
        }

        public async Task<bool> Handle(AddFriendCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == authenticatedUsername, token: cancellationToken);
            var friend = await _documentSession.Query<User>().SingleAsync(u => u.Username == request.Username.ToLower(), token: cancellationToken);

            user.AddFriend(friend.Username);
            friend.AddFriend(user.Username);

            _documentSession.Update(user);
            _documentSession.Update(friend);
            await _documentSession.SaveChangesAsync(cancellationToken);

            await _messageSession.Publish(new FriendWasAdded
            {
                Username = user.Username,
                FriendName = friend.Username
            });

            return true;
        }
    }
}