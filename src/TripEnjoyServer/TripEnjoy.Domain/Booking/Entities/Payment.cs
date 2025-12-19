using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.Entities;

/// <summary>
/// Represents a payment transaction for a booking
/// </summary>
public class Payment : Entity<PaymentId>
{
    public BookingId BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethodEnum PaymentMethod { get; private set; }
    public string? TransactionId { get; private set; }
    public PaymentStatusEnum Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Booking Booking { get; private set; } = null!;

    private Payment() : base(PaymentId.CreateUnique())
    {
        BookingId = null!;
    }

    private Payment(
        PaymentId id,
        BookingId bookingId,
        decimal amount,
        PaymentMethodEnum paymentMethod) : base(id)
    {
        BookingId = bookingId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PaymentStatusEnum.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Payment> Create(
        BookingId bookingId,
        decimal amount,
        PaymentMethodEnum paymentMethod)
    {
        if (amount <= 0)
        {
            return Result<Payment>.Failure(DomainError.Payment.InvalidAmount);
        }

        var payment = new Payment(
            PaymentId.CreateUnique(),
            bookingId,
            amount,
            paymentMethod);

        return Result<Payment>.Success(payment);
    }

    public Result MarkAsProcessing()
    {
        if (Status != PaymentStatusEnum.Pending)
        {
            return Result.Failure(DomainError.Payment.InvalidStatusTransition);
        }

        Status = PaymentStatusEnum.Processing;
        return Result.Success();
    }

    public Result MarkAsSuccess(string transactionId)
    {
        if (Status != PaymentStatusEnum.Processing)
        {
            return Result.Failure(DomainError.Payment.InvalidStatusTransition);
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return Result.Failure(DomainError.Payment.InvalidTransactionId);
        }

        Status = PaymentStatusEnum.Success;
        TransactionId = transactionId;
        PaidAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result MarkAsFailed()
    {
        if (Status == PaymentStatusEnum.Success || Status == PaymentStatusEnum.Refunded)
        {
            return Result.Failure(DomainError.Payment.CannotFailCompletedPayment);
        }

        Status = PaymentStatusEnum.Failed;
        return Result.Success();
    }

    public Result MarkAsRefunded()
    {
        if (Status != PaymentStatusEnum.Success)
        {
            return Result.Failure(DomainError.Payment.CanOnlyRefundSuccessfulPayment);
        }

        Status = PaymentStatusEnum.Refunded;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == PaymentStatusEnum.Success || Status == PaymentStatusEnum.Refunded)
        {
            return Result.Failure(DomainError.Payment.CannotCancelCompletedPayment);
        }

        Status = PaymentStatusEnum.Cancelled;
        return Result.Success();
    }
}
