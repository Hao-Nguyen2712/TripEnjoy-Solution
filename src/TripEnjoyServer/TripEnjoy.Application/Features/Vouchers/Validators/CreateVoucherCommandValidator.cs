using FluentValidation;
using TripEnjoy.Application.Features.Vouchers.Commands;
using TripEnjoy.Domain.Voucher.Enums;

namespace TripEnjoy.Application.Features.Vouchers.Validators;

public class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
{
    public CreateVoucherCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Voucher code is required.")
            .MinimumLength(3)
            .WithMessage("Voucher code must be at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Voucher code cannot exceed 50 characters.")
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Voucher code must contain only uppercase letters and numbers.");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .WithMessage("Discount type is required.")
            .Must(BeValidDiscountType)
            .WithMessage("Discount type must be either 'Percent' or 'Amount'.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Discount value must be greater than zero.")
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == VoucherDiscountTypeEnum.Percent.ToString())
            .WithMessage("Percentage discount cannot exceed 100%.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum order amount cannot be negative.")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaximumDiscountAmount)
            .GreaterThan(0)
            .WithMessage("Maximum discount amount must be greater than zero.")
            .When(x => x.MaximumDiscountAmount.HasValue);

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .WithMessage("Usage limit must be greater than zero.")
            .When(x => x.UsageLimit.HasValue);

        RuleFor(x => x.UsageLimitPerUser)
            .GreaterThan(0)
            .WithMessage("Usage limit per user must be greater than zero.")
            .When(x => x.UsageLimitPerUser.HasValue);

        // Validate targets if provided
        RuleForEach(x => x.Targets)
            .ChildRules(target =>
            {
                target.RuleFor(t => t.TargetType)
                    .NotEmpty()
                    .WithMessage("Target type is required.")
                    .Must(BeValidTargetType)
                    .WithMessage("Target type must be one of: Global, Partner, Property, RoomType.");

                target.RuleFor(t => t.PartnerId)
                    .NotEmpty()
                    .When(t => t.TargetType == VoucherTargetTypeEnum.Partner.ToString())
                    .WithMessage("Partner ID is required for Partner target type.");

                target.RuleFor(t => t.PropertyId)
                    .NotEmpty()
                    .When(t => t.TargetType == VoucherTargetTypeEnum.Property.ToString())
                    .WithMessage("Property ID is required for Property target type.");

                target.RuleFor(t => t.RoomTypeId)
                    .NotEmpty()
                    .When(t => t.TargetType == VoucherTargetTypeEnum.RoomType.ToString())
                    .WithMessage("Room Type ID is required for RoomType target type.");
            })
            .When(x => x.Targets != null && x.Targets.Any());
    }

    private bool BeValidDiscountType(string discountType)
    {
        return Enum.TryParse<VoucherDiscountTypeEnum>(discountType, out _);
    }

    private bool BeValidTargetType(string targetType)
    {
        return Enum.TryParse<VoucherTargetTypeEnum>(targetType, out _);
    }
}
