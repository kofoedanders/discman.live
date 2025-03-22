using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

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
        private readonly IDocumentSession _documentSession;
        private readonly string _tokenSecret;

        public AuthenticateUserCommandHandler(IDocumentSession documentSession, IConfiguration configuration)
        {
            _documentSession = documentSession;
            _tokenSecret = configuration.GetValue<string>("TOKEN_SECRET");
        }
        
        public async Task<AuthenticatedUser> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var requestsLast10Sec = FailedLoginRequests.Count(r => r > DateTime.Now.AddSeconds(-10));
            FailedLoginRequests.RemoveAll(r => r < DateTime.Now.AddSeconds(-20));
            if (requestsLast10Sec > 10)
            {
                return null;
            }

            var user = await _documentSession.Query<User>().SingleAsync(u => u.Username == request.Username, token: cancellationToken);
            if (user is null)
            {
                FailedLoginRequests.Add(DateTime.Now);
                return null;
            }

            var hashedPw = new SaltSeasonedHashedPassword(request.Password, user.Salt);
            if (!hashedPw.Hash.SequenceEqual(user.Password))
            {
                FailedLoginRequests.Add(DateTime.Now);
                return null;
            }

            Console.WriteLine(_tokenSecret);
            return user.Authenticated(_tokenSecret);
        }
    }
}