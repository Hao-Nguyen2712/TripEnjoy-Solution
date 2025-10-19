using FluentValidation;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.expiredAccessToken)
                .NotEmpty()
                .WithMessage("Expired access token is required.");

            RuleFor(x => x.refreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required.")
                .MinimumLength(10)
                .WithMessage("Refresh token must be at least 10 characters long.");
        }
    }
}