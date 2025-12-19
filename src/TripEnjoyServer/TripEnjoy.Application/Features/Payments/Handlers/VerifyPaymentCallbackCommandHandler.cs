using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Payments.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Payment;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Payments.Handlers;

public class VerifyPaymentCallbackCommandHandler : IRequestHandler<VerifyPaymentCallbackCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<VerifyPaymentCallbackCommandHandler> _logger;
    private readonly ILogService? _logService;

    public VerifyPaymentCallbackCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<VerifyPaymentCallbackCommandHandler> logger,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
        _logService = logService;
    }

    public async Task<Result<bool>> Handle(VerifyPaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Verify payment callback
        var verificationResult = await _paymentService.VerifyPaymentCallbackAsync(request.CallbackData);
        if (verificationResult.IsFailure)
        {
            _logger.LogError("Payment verification failed: {Errors}",
                string.Join(", ", verificationResult.Errors.Select(e => e.Description)));
            return Result<bool>.Failure(verificationResult.Errors);
        }

        var callbackResult = verificationResult.Value;

        // Parse payment ID from order ID
        if (!Guid.TryParse(callbackResult.OrderId, out var paymentIdGuid))
        {
            _logger.LogError("Invalid payment ID in callback: {OrderId}", callbackResult.OrderId);
            return Result<bool>.Failure(new Error(
                "Payment.InvalidPaymentId",
                "Invalid payment ID in callback data.",
                ErrorType.Validation));
        }

        var paymentId = PaymentId.Create(paymentIdGuid);
        var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId.Value);

        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found", paymentIdGuid);
            return Result<bool>.Failure(DomainError.Payment.NotFound);
        }

        // Get associated booking
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(payment.BookingId.Id);
        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found for payment {PaymentId}", 
                payment.BookingId.Id, paymentIdGuid);
            return Result<bool>.Failure(DomainError.Booking.NotFound);
        }

        // Update payment status based on callback result
        if (callbackResult.IsSuccess)
        {
            var markSuccessResult = payment.MarkAsSuccess(callbackResult.TransactionId);
            if (markSuccessResult.IsFailure)
            {
                _logger.LogError("Failed to mark payment as success: {Errors}",
                    string.Join(", ", markSuccessResult.Errors.Select(e => e.Description)));
                return Result<bool>.Failure(markSuccessResult.Errors);
            }

            // Confirm booking
            var confirmResult = booking.Confirm();
            if (confirmResult.IsFailure)
            {
                _logger.LogError("Failed to confirm booking: {Errors}",
                    string.Join(", ", confirmResult.Errors.Select(e => e.Description)));
                return Result<bool>.Failure(confirmResult.Errors);
            }

            _logger.LogInformation("Payment {PaymentId} successful, booking {BookingId} confirmed",
                paymentIdGuid, booking.Id.Id);
        }
        else
        {
            payment.MarkAsFailed();
            _logger.LogWarning("Payment {PaymentId} failed: {Message}",
                paymentIdGuid, callbackResult.Message);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        // Log performance
        _logService?.LogPerformance(
            "VerifyPaymentCallback",
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, object>
            {
                ["PaymentId"] = paymentIdGuid.ToString(),
                ["BookingId"] = booking.Id.Id.ToString(),
                ["IsSuccess"] = callbackResult.IsSuccess,
                ["TransactionId"] = callbackResult.TransactionId
            });

        _logService?.LogBusinessEvent(
            callbackResult.IsSuccess ? "PaymentCompleted" : "PaymentFailed",
            new Dictionary<string, object>
            {
                ["PaymentId"] = paymentIdGuid.ToString(),
                ["BookingId"] = booking.Id.Id.ToString(),
                ["Amount"] = payment.Amount,
                ["TransactionId"] = callbackResult.TransactionId
            });

        return Result<bool>.Success(callbackResult.IsSuccess);
    }
}
