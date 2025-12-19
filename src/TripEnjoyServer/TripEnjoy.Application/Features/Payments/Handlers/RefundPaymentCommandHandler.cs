using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Payments.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Payment;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Payments.Handlers;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;
    private readonly ILogService? _logService;

    public RefundPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<RefundPaymentCommandHandler> logger,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
        _logService = logService;
    }

    public async Task<Result<string>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Get payment
        var paymentId = PaymentId.Create(request.PaymentId);
        var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId.Value);

        if (payment == null)
        {
            _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
            return Result<string>.Failure(DomainError.Payment.NotFound);
        }

        // Validate payment can be refunded
        if (payment.TransactionId == null)
        {
            _logger.LogWarning("Payment {PaymentId} has no transaction ID", request.PaymentId);
            return Result<string>.Failure(DomainError.Payment.InvalidTransactionId);
        }

        // Process refund via payment gateway
        var refundResult = await _paymentService.ProcessRefundAsync(
            payment.TransactionId,
            payment.Amount,
            request.Reason);

        if (refundResult.IsFailure)
        {
            _logger.LogError("Failed to process refund for payment {PaymentId}: {Errors}",
                request.PaymentId,
                string.Join(", ", refundResult.Errors.Select(e => e.Description)));
            return Result<string>.Failure(refundResult.Errors);
        }

        // Mark payment as refunded
        var markRefundedResult = payment.MarkAsRefunded();
        if (markRefundedResult.IsFailure)
        {
            _logger.LogError("Failed to mark payment as refunded: {Errors}",
                string.Join(", ", markRefundedResult.Errors.Select(e => e.Description)));
            return Result<string>.Failure(markRefundedResult.Errors);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        // Log performance
        _logService?.LogPerformance(
            "RefundPayment",
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, object>
            {
                ["PaymentId"] = request.PaymentId.ToString(),
                ["Amount"] = payment.Amount,
                ["RefundTransactionId"] = refundResult.Value
            });

        _logService?.LogBusinessEvent(
            "PaymentRefunded",
            new Dictionary<string, object>
            {
                ["PaymentId"] = request.PaymentId.ToString(),
                ["Amount"] = payment.Amount,
                ["Reason"] = request.Reason,
                ["RefundTransactionId"] = refundResult.Value
            });

        _logger.LogInformation("Successfully refunded payment {PaymentId}", request.PaymentId);

        return Result<string>.Success(refundResult.Value);
    }
}
