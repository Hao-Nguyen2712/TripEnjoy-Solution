using FluentValidation;
using TripEnjoy.Application.Features.Reviews.Commands;

namespace TripEnjoy.Application.Features.Reviews.Validators;

public class CreateReviewReplyCommandValidator : AbstractValidator<CreateReviewReplyCommand>
{
    public CreateReviewReplyCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Review ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Reply content is required.")
            .MaximumLength(500)
            .WithMessage("Reply content cannot exceed 500 characters.");
    }
}
