using FluentValidation;
using TripEnjoy.Application.Features.Payments.Commands;

namespace TripEnjoy.Application.Features.Payments.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("Booking ID is required.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required.")
            .Must(BeValidPaymentMethod)
            .WithMessage("Invalid payment method. Must be one of: VNPay, Momo, ZaloPay, BankTransfer.");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .WithMessage("Return URL is required.")
            .Must(BeValidUrl)
            .WithMessage("Return URL must be a valid URL.");
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "VNPay", "Momo", "ZaloPay", "BankTransfer" };
        return validMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
