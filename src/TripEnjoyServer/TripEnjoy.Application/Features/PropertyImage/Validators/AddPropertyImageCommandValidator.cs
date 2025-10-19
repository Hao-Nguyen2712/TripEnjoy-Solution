using FluentValidation;
using TripEnjoy.Application.Features.PropertyImage.Commands;

namespace TripEnjoy.Application.Features.PropertyImage.Validators;

public class AddPropertyImageCommandValidator : AbstractValidator<AddPropertyImageCommand>
{
    public AddPropertyImageCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.");

        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("Public ID is required.")
            .MaximumLength(255)
            .WithMessage("Public ID cannot exceed 255 characters.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .WithMessage("Image URL is required.")
            .Must(BeValidUrl)
            .WithMessage("Image URL must be a valid HTTPS URL.");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("Signature is required.");

        RuleFor(x => x.Timestamp)
            .GreaterThan(0)
            .WithMessage("Timestamp must be a valid positive number.");

        RuleFor(x => x.Caption)
            .MaximumLength(500)
            .WithMessage("Caption cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Caption));
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && uriResult.Scheme == Uri.UriSchemeHttps;
    }
}