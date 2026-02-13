using FluentValidation;

namespace Web.Users.Commands
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        
        public ResetPasswordCommandValidator()
        {
            RuleFor(v => v.NewPassword)
                .MinimumLength(6)
                .MaximumLength(200)
                .NotEmpty()
                .WithMessage("Password must be between 6 and 200 characters long");
        }
    }
}
