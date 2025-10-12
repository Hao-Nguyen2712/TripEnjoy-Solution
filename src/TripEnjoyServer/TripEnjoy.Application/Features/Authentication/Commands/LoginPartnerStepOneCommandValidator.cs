using FluentValidation;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    /// <summary>
    /// Validator for LoginPartnerStepOneCommand to ensure email and password are provided and valid.
    /// </summary>
    public class LoginPartnerStepOneCommandValidator : AbstractValidator<LoginPartnerStepOneCommand>
    {
        public LoginPartnerStepOneCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");
        }
    }
}