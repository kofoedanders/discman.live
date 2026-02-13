using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web.Infrastructure;

namespace Web.Users.Commands
{
    public class AuthenticateUserCommand : IRequest<AuthenticatedUser>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticatedUser>
    {
        private static readonly List<DateTime> FailedLoginRequests = new List<DateTime>();
        private readonly DiscmanDbContext _dbContext;
        private readonly string _tokenSecret;

        public AuthenticateUserCommandHandler(DiscmanDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _tokenSecret = configuration.GetValue<string>("TOKEN_SECRET");
        }
        
        public async Task<AuthenticatedUser> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var requestsLast10Sec = FailedLoginRequests.Count(r => r > DateTime.UtcNow.AddSeconds(-10));
            FailedLoginRequests.RemoveAll(r => r < DateTime.UtcNow.AddSeconds(-20));
            if (requestsLast10Sec > 10)
            {
                return null;
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user is null)
            {
                FailedLoginRequests.Add(DateTime.UtcNow);
                return null;
            }

            var hashedPw = new SaltSeasonedHashedPassword(request.Password, user.Salt);
            if (!hashedPw.Hash.SequenceEqual(user.Password))
            {
                FailedLoginRequests.Add(DateTime.UtcNow);
                return null;
            }

            Console.WriteLine(_tokenSecret);
            return user.Authenticated(_tokenSecret);
        }
    }
}
