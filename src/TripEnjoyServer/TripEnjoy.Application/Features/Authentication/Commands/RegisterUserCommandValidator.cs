using FluentValidation;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));
        }
    }
}