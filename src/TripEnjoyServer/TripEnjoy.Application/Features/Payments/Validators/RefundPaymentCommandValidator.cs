using FluentValidation;
using TripEnjoy.Application.Features.Payments.Commands;

namespace TripEnjoy.Application.Features.Payments.Validators;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required.")
            .MaximumLength(500)
            .WithMessage("Refund reason must not exceed 500 characters.");
    }
}
