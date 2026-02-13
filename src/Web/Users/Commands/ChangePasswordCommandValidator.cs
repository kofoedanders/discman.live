using FluentValidation;

namespace Web.Users.Commands
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(v => v.NewPassword)
                .MinimumLength(6)
                .MaximumLength(200)
                .NotEmpty()
                .WithMessage("Password must be between 6 and 200 characters long");
        }
    }
}
