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

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly ILogService? _logService;

    public ProcessPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<ProcessPaymentCommandHandler> logger,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
        _logService = logService;
    }

    public async Task<Result<string>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Get booking
        var bookingId = BookingId.Create(request.BookingId);
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId.Id);

        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            return Result<string>.Failure(DomainError.Booking.NotFound);
        }

        // Validate booking status
        if (booking.Status != BookingStatusEnum.Pending.ToString())
        {
            _logger.LogWarning("Booking {BookingId} is not in Pending status", request.BookingId);
            return Result<string>.Failure(new Error(
                "Booking.InvalidStatus",
                "Only pending bookings can be paid.",
                ErrorType.Failure));
        }

        // Parse payment method
        if (!Enum.TryParse<PaymentMethodEnum>(request.PaymentMethod, true, out var paymentMethod))
        {
            _logger.LogWarning("Invalid payment method: {PaymentMethod}", request.PaymentMethod);
            return Result<string>.Failure(new Error(
                "Payment.InvalidPaymentMethod",
                "Invalid payment method specified.",
                ErrorType.Validation));
        }

        // Create payment entity
        var paymentResult = Payment.Create(bookingId, booking.TotalPrice, paymentMethod);
        if (paymentResult.IsFailure)
        {
            _logger.LogError("Failed to create payment: {Errors}",
                string.Join(", ", paymentResult.Errors.Select(e => e.Description)));
            return Result<string>.Failure(paymentResult.Errors);
        }

        var payment = paymentResult.Value;

        // Mark payment as processing
        var processingResult = payment.MarkAsProcessing();
        if (processingResult.IsFailure)
        {
            _logger.LogError("Failed to mark payment as processing: {Errors}",
                string.Join(", ", processingResult.Errors.Select(e => e.Description)));
            return Result<string>.Failure(processingResult.Errors);
        }

        // Save payment to database
        await _unitOfWork.Repository<Payment>().AddAsync(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create payment URL via payment gateway
        var orderInfo = $"Booking {booking.Id.Id} payment";
        var paymentUrlResult = await _paymentService.CreatePaymentUrlAsync(
            payment.Id.Value.ToString(),
            booking.TotalPrice,
            orderInfo,
            request.ReturnUrl);

        if (paymentUrlResult.IsFailure)
        {
            _logger.LogError("Failed to create payment URL: {Errors}",
                string.Join(", ", paymentUrlResult.Errors.Select(e => e.Description)));
            
            // Mark payment as failed
            payment.MarkAsFailed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result<string>.Failure(paymentUrlResult.Errors);
        }

        stopwatch.Stop();

        // Log performance
        _logService?.LogPerformance(
            "ProcessPayment",
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, object>
            {
                ["PaymentId"] = payment.Id.Value.ToString(),
                ["BookingId"] = booking.Id.Id.ToString(),
                ["Amount"] = booking.TotalPrice,
                ["PaymentMethod"] = paymentMethod.ToString()
            });

        _logService?.LogBusinessEvent(
            "PaymentInitiated",
            new Dictionary<string, object>
            {
                ["PaymentId"] = payment.Id.Value.ToString(),
                ["BookingId"] = booking.Id.Id.ToString(),
                ["Amount"] = booking.TotalPrice
            });

        _logger.LogInformation("Successfully created payment URL for booking {BookingId}", request.BookingId);

        return Result<string>.Success(paymentUrlResult.Value);
    }
}
