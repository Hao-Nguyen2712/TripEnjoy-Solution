using FluentValidation;
using TripEnjoy.Application.Features.Partner.Commands;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Partner.Validators;

public class GenerateDocumentUploadUrlCommandValidator : AbstractValidator<GenerateDocumentUploadUrlCommand>
{
    public GenerateDocumentUploadUrlCommandValidator()
    {
        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .WithMessage("Document type is required.")
            .Must(DocumentTypeConstant.IsValidDocumentType)
            .WithMessage("Invalid document type. Valid types are: " + string.Join(", ", DocumentTypeConstant.ValidDocumentTypes));

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.")
            .MaximumLength(255)
            .WithMessage("File name cannot exceed 255 characters.")
            .Must(HaveValidExtension)
            .WithMessage("File must have a valid extension (.jpg, .jpeg, .png, .pdf, .doc, .docx).");
    }

    private static bool HaveValidExtension(string fileName)
    {
        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && validExtensions.Contains(extension);
    }
}