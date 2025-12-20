using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities;

public class Transaction : Entity<TransactionId>
{
    public WalletId WalletId { get; private set; }
    public BookingId? BookingId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; }
    public string Status { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Navigation properties
    public Wallet Wallet { get; private set; }

    private Transaction() : base(TransactionId.CreateUnique())
    {
        WalletId = null!;
        Wallet = null!;
        Type = null!;
        Status = null!;
    }

    public Transaction(
        TransactionId id,
        WalletId walletId,
        decimal amount,
        TransactionTypeEnum type,
        BookingId? bookingId = null,
        string? description = null) : base(id)
    {
        WalletId = walletId;
        BookingId = bookingId;
        Amount = amount;
        Type = type.ToString();
        Status = TransactionStatusEnum.Pending.ToString();
        Description = description;
        CreatedAt = DateTime.UtcNow;
        CompletedAt = null;
    }

    public static Result<Transaction> Create(
        WalletId walletId,
        decimal amount,
        TransactionTypeEnum type,
        BookingId? bookingId = null,
        string? description = null)
    {
        if (amount == 0)
        {
            return Result<Transaction>.Failure(DomainError.Transaction.InvalidAmount);
        }

        var transaction = new Transaction(
            TransactionId.CreateUnique(),
            walletId,
            amount,
            type,
            bookingId,
            description);

        return Result<Transaction>.Success(transaction);
    }

    public Result Complete()
    {
        if (Status == TransactionStatusEnum.Completed.ToString())
        {
            return Result.Failure(DomainError.Transaction.AlreadyCompleted);
        }

        Status = TransactionStatusEnum.Completed.ToString();
        CompletedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Fail()
    {
        if (Status == TransactionStatusEnum.Completed.ToString())
        {
            return Result.Failure(DomainError.Transaction.AlreadyCompleted);
        }

        Status = TransactionStatusEnum.Failed.ToString();
        return Result.Success();
    }

    public Result Reverse()
    {
        if (Status != TransactionStatusEnum.Completed.ToString())
        {
            return Result.Failure(DomainError.Transaction.CannotReverse);
        }

        Status = TransactionStatusEnum.Reversed.ToString();
        return Result.Success();
    }
}
