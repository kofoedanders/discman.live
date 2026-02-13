using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Web.Infrastructure;
using Web.Users.NSBEvents;

namespace Web.Users.Commands
{
    public class AddFriendCommand : IRequest<bool>
    {
        public string Username { get; set; }
    }

    public class AddFriendCommandHandler : IRequestHandler<AddFriendCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageSession _messageSession;

        public AddFriendCommandHandler(DiscmanDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMessageSession messageSession)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _messageSession = messageSession;
        }

        public async Task<bool> Handle(AddFriendCommand request, CancellationToken cancellationToken)
        {
            var authenticatedUsername = _httpContextAccessor.HttpContext?.User.Claims.Single(c => c.Type == ClaimTypes.Name).Value;
            var user = await _dbContext.Users.SingleAsync(u => u.Username == authenticatedUsername, cancellationToken);
            var friend = await _dbContext.Users.SingleAsync(u => u.Username == request.Username.ToLower(), cancellationToken);

            user.AddFriend(friend.Username);
            friend.AddFriend(user.Username);

            _dbContext.Users.Update(user);
            _dbContext.Users.Update(friend);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _messageSession.Publish(new FriendWasAdded
            {
                Username = user.Username,
                FriendName = friend.Username
            });

            return true;
        }
    }
}
