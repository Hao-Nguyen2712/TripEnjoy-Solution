using FluentValidation;

namespace TripEnjoy.Application.Features.RoomTypes.Commands;

public class CreateRoomTypeCommandValidator : AbstractValidator<CreateRoomTypeCommand>
{
    public CreateRoomTypeCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .WithMessage("Property ID is required.");

        RuleFor(x => x.RoomTypeName)
            .NotEmpty()
            .WithMessage("Room type name is required.")
            .MaximumLength(100)
            .WithMessage("Room type name must not exceed 100 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0.")
            .LessThanOrEqualTo(20)
            .WithMessage("Capacity must not exceed 20.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0)
            .WithMessage("Base price must be greater than 0.");

        RuleFor(x => x.TotalQuantity)
            .GreaterThan(0)
            .WithMessage("Total quantity must be greater than 0.")
            .LessThanOrEqualTo(1000)
            .WithMessage("Total quantity must not exceed 1000.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters.");
    }
}
