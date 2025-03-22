using System;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
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
        private readonly IDocumentSession _documentSession;
        private readonly ISendGridClient _sendGridClient;

        public InitiatePasswordResetCommandHandler(IDocumentSession documentSession, ISendGridClient sendGridClient)
        {
            _documentSession = documentSession;
            _sendGridClient = sendGridClient;
        }

        public async Task<bool> Handle(InitiatePasswordResetCommand request, CancellationToken cancellationToken)
        {
            var resetId = Guid.NewGuid();
            Log.Information("Password reset requested for {Email} {ResetId}", request.Email, resetId);
            var user = await _documentSession.Query<User>().SingleOrDefaultAsync(u => u.Email == request.Email, token: cancellationToken);
            if (user is null)
            {
                Log.Information("Email {Email} does not exist {ResetId}", request.Email, resetId);
                return true;
            }

            var ongoing = await _documentSession.Query<ResetPasswordRequest>().SingleOrDefaultAsync(u => u.Email == request.Email, token: cancellationToken);
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
                CreatedAt = DateTime.Now
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

            _documentSession.Store(resetRequest);
            await _documentSession.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}