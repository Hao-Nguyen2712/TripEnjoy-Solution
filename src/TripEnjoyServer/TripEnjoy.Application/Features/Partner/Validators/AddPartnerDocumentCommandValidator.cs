using FluentValidation;
using TripEnjoy.Application.Features.Partner.Commands;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Partner.Validators;

public class AddPartnerDocumentCommandValidator : AbstractValidator<AddPartnerDocumentCommand>
{
    public AddPartnerDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .WithMessage("Document type is required.")
            .Must(DocumentTypeConstant.IsValidDocumentType)
            .WithMessage("Invalid document type. Valid types are: " + string.Join(", ", DocumentTypeConstant.ValidDocumentTypes));

        RuleFor(x => x.PublicId)
            .NotEmpty()
            .WithMessage("Public ID is required.")
            .MaximumLength(500)
            .WithMessage("Public ID cannot exceed 500 characters.");

        RuleFor(x => x.DocumentUrl)
            .NotEmpty()
            .WithMessage("Document URL is required.")
            .MaximumLength(500)
            .WithMessage("Document URL cannot exceed 500 characters.")
            .Must(BeAValidUrl)
            .WithMessage("Document URL must be a valid HTTPS URL.");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("Signature is required.")
            .MaximumLength(100)
            .WithMessage("Signature cannot exceed 100 characters.");

        RuleFor(x => x.Timestamp)
            .GreaterThan(0)
            .WithMessage("Timestamp must be a valid Unix timestamp.")
            .Must(BeARecentTimestamp)
            .WithMessage("Timestamp is too old. Upload must be completed within 1 hour.");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttps);
    }

    private static bool BeARecentTimestamp(long timestamp)
    {
        var uploadTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var now = DateTimeOffset.UtcNow;
        var timeDifference = now - uploadTime;
        
        // Allow uploads within the last hour
        return timeDifference <= TimeSpan.FromHours(1) && timeDifference >= TimeSpan.Zero;
    }
}