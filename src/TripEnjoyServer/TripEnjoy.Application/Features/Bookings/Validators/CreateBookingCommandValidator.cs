using FluentValidation;
using TripEnjoy.Application.Features.Bookings.Commands;

namespace TripEnjoy.Application.Features.Bookings.Validators;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.");

        RuleFor(x => x.CheckInDate)
            .NotEmpty()
            .WithMessage("Check-in date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Check-in date cannot be in the past.");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty()
            .WithMessage("Check-out date is required.")
            .GreaterThan(x => x.CheckInDate)
            .WithMessage("Check-out date must be after check-in date.");

        RuleFor(x => x.NumberOfGuests)
            .GreaterThan(0)
            .WithMessage("Number of guests must be greater than zero.");

        RuleFor(x => x.BookingDetails)
            .NotEmpty()
            .WithMessage("At least one room type must be selected.");

        RuleForEach(x => x.BookingDetails)
            .ChildRules(detail =>
            {
                detail.RuleFor(d => d.RoomTypeId)
                    .NotEmpty()
                    .WithMessage("Room type ID is required.");

                detail.RuleFor(d => d.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than zero.");

                detail.RuleFor(d => d.PricePerNight)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Price per night cannot be negative.");

                detail.RuleFor(d => d.DiscountAmount)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Discount amount cannot be negative.");
            });

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000)
            .WithMessage("Special requests cannot exceed 1000 characters.");
    }
}
