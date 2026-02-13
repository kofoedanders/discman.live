using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Web.Infrastructure;
using Web.Users.Domain;

namespace Web.Users.Commands
{
    public class InitiatePasswordResetCommand : IRequest<bool>
    {
        private string _email;

        public string Email
        {
            get => _email.Trim().ToLowerInvariant();
            set => _email = value;
        }
    }

    public class InitiatePasswordResetCommandHandler : IRequestHandler<InitiatePasswordResetCommand, bool>
    {
        private readonly DiscmanDbContext _dbContext;
        private readonly ISendGridClient _sendGridClient;

        public InitiatePasswordResetCommandHandler(DiscmanDbContext dbContext, ISendGridClient sendGridClient)
        {
            _dbContext = dbContext;
            _sendGridClient = sendGridClient;
        }

        public async Task<bool> Handle(InitiatePasswordResetCommand request, CancellationToken cancellationToken)
        {
            var resetId = Guid.NewGuid();
            Log.Information("Password reset requested for {Email} {ResetId}", request.Email, resetId);
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (user is null)
            {
                Log.Information("Email {Email} does not exist {ResetId}", request.Email, resetId);
                return true;
            }

            var ongoing = await _dbContext.ResetPasswordRequests.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (ongoing != null)
            {
                Log.Information("Already ongoing reset processes for {Email} exists {ResetId}", request.Email, ongoing.Id);
                return true;
            }

            var resetRequest = new ResetPasswordRequest
            {
                Id = resetId,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow
            };

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("pw@discman.live", "Discman Live"),
                Subject = "Password reset - Discman.live"
            };
            msg.AddContent(MimeType.Text,
                $"Go to this one-time url to reset your password. It will only work for 60 minutes. https://discman.live/resetpassword?resetId={resetId}");
            msg.AddTo(new EmailAddress(user.Email, user.Username));
            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
            Log.Information("Sent reset url to {Email}, for user {Username} {ResetId}", user.Email, user.Username, resetId);

            _dbContext.ResetPasswordRequests.Add(resetRequest);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
