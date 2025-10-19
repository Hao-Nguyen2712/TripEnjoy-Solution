using FluentValidation;
using TripEnjoy.Application.Features.PropertyImage.Commands;

namespace TripEnjoy.Application.Features.PropertyImage.Validators;

public class GeneratePhotoUploadUrlCommandValidator : AbstractValidator<GeneratePhotoUploadUrlCommand>
{
    public GeneratePhotoUploadUrlCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters.")
            .Must(BeValidImageFileName)
            .WithMessage("File name must be a valid image file (jpg, jpeg, png, webp, gif).");

        RuleFor(x => x.Caption)
            .MaximumLength(500)
            .WithMessage("Caption cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Caption));
    }

    private static bool BeValidImageFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}