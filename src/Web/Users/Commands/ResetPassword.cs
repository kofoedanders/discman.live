using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using Marten.Linq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Web.Common;
using Web.Users.Domain;

namespace Web.Users.Commands
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public Guid ResetId { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IDocumentSession _documentSession;
        private readonly ISendGridClient _sendGridClient;

        public ResetPasswordCommandHandler(IDocumentSession documentSession, ISendGridClient sendGridClient)
        {
            _documentSession = documentSession;
            _sendGridClient = sendGridClient;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var resetPasswordRequest = await _documentSession
                .Query<ResetPasswordRequest>()
                .SingleOrDefaultAsync(u => u.Id == request.ResetId, token: cancellationToken);

            if (resetPasswordRequest is null) throw new NotFoundException();

            var user = await _documentSession.Query<User>()
                .SingleOrDefaultAsync(u => u.Username == resetPasswordRequest.Username, token: cancellationToken);

            Log.Information($"Changing password for user {user.Username} {resetPasswordRequest.Id}");

            var hashedPw = new SaltSeasonedHashedPassword(request.NewPassword);

            user.ChangePassword(hashedPw);

            _documentSession.Update(user);

            _documentSession.Delete(resetPasswordRequest);

            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}