using System;
using FluentValidation;
using System.Net.Mail;

namespace Web.Users.Commands
{
    public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
    {
        public ChangeEmailCommandValidator()
        {
            RuleFor(v => v.NewEmail)
                .MinimumLength(5)
                .MaximumLength(200)
                .NotEmpty()
                .WithMessage("Must be a valid email");
            
            RuleFor(c => c.NewEmail).Must(ValidEmail).WithMessage("Not a valid email");
        }

        private bool ValidEmail(string email)
        {
            try
            {
                var mail = new MailAddress(email);
                return mail.Address == email;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
