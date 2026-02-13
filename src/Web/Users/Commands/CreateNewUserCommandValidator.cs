using FluentValidation;
using Web.Infrastructure;
using System;
using System.Linq;
using System.Net.Mail;

namespace Web.Users.Commands
{
    public class CreateNewUserCommandValidator : AbstractValidator<CreateNewUserCommand>
    {
        private readonly DiscmanDbContext _dbContext;

        public CreateNewUserCommandValidator(DiscmanDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(v => v.Password)
                .MinimumLength(5)
                .MaximumLength(200)
                .NotEmpty()
                .WithMessage("Password must be between 6 and 200 characters long");

            RuleFor(v => v.Username)
                .MinimumLength(3)
                .MaximumLength(30)
                .NotEmpty()
                .WithMessage("Username must be between 3 and 30 characters long");

            RuleFor(c => c.Username).Must(NotExist).WithMessage("Not a valid request");

            RuleFor(c => c.Email).Must(ValidEmail).WithMessage("Not a valid email");
            RuleFor(c => c.Email).Must(EmailNotExist).WithMessage("Email already used");
        }

        private bool ValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
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

        private bool EmailNotExist(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            return _dbContext.Users.SingleOrDefault(u => u.Email == email) is null;
        }

        private bool NotExist(string username)
        {
            return _dbContext.Users.SingleOrDefault(u => u.Username == username) is null;
        }
    }
}
