using FluentValidation;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public class RegisterPartnerCommandValidator : AbstractValidator<RegisterPartnerCommand>
    {
        public RegisterPartnerCommandValidator()
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

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(200).WithMessage("Company name must not exceed 200 characters");

            RuleFor(x => x.ContactNumber)
                .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Contact number format is invalid")
                .MaximumLength(20).WithMessage("Contact number must not exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.ContactNumber));

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}