using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Web.Common;
using Web.Infrastructure;
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
        private readonly DiscmanDbContext _dbContext;
        private readonly ISendGridClient _sendGridClient;

        public ResetPasswordCommandHandler(DiscmanDbContext dbContext, ISendGridClient sendGridClient)
        {
            _dbContext = dbContext;
            _sendGridClient = sendGridClient;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var resetPasswordRequest = await _dbContext.ResetPasswordRequests
                .SingleOrDefaultAsync(u => u.Id == request.ResetId, cancellationToken);

            if (resetPasswordRequest is null) throw new NotFoundException();

            var user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Username == resetPasswordRequest.Username, cancellationToken);

            Log.Information($"Changing password for user {user.Username} {resetPasswordRequest.Id}");

            var hashedPw = new SaltSeasonedHashedPassword(request.NewPassword);

            user.ChangePassword(hashedPw);

            _dbContext.Users.Update(user);

            _dbContext.ResetPasswordRequests.Remove(resetPasswordRequest);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
