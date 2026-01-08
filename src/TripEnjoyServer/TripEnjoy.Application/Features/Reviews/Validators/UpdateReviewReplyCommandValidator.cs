using FluentValidation;
using TripEnjoy.Application.Features.Reviews.Commands;

namespace TripEnjoy.Application.Features.Reviews.Validators;

public class UpdateReviewReplyCommandValidator : AbstractValidator<UpdateReviewReplyCommand>
{
    public UpdateReviewReplyCommandValidator()
    {
        RuleFor(x => x.ReviewReplyId)
            .NotEmpty()
            .WithMessage("Review reply ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Reply content is required.")
            .MaximumLength(500)
            .WithMessage("Reply content cannot exceed 500 characters.");
    }
}
