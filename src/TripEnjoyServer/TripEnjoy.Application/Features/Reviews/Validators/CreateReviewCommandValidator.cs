using FluentValidation;
using TripEnjoy.Application.Features.Reviews.Commands;

namespace TripEnjoy.Application.Features.Reviews.Validators;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.BookingDetailId)
            .NotEmpty()
            .WithMessage("Booking detail ID is required.");

        RuleFor(x => x.RoomTypeId)
            .NotEmpty()
            .WithMessage("Room type ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required.")
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters.");

        RuleFor(x => x.ImageUrls)
            .Must(urls => urls == null || urls.Count <= 10)
            .WithMessage("A maximum of 10 images can be uploaded per review.");
    }
}
