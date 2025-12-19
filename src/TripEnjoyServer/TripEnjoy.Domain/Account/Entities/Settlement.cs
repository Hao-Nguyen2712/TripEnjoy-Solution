using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.Entities;

public class Settlement : Entity<SettlementId>
{
    public WalletId WalletId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // Navigation properties
    public Wallet Wallet { get; private set; }

    private Settlement() : base(SettlementId.CreateUnique())
    {
        WalletId = null!;
        Wallet = null!;
        Status = null!;
    }

    public Settlement(
        SettlementId id,
        WalletId walletId,
        DateTime periodStart,
        DateTime periodEnd,
        decimal totalAmount,
        decimal commissionAmount) : base(id)
    {
        WalletId = walletId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        TotalAmount = totalAmount;
        CommissionAmount = commissionAmount;
        NetAmount = totalAmount - commissionAmount;
        Status = SettlementStatusEnum.Pending.ToString();
        CreatedAt = DateTime.UtcNow;
        PaidAt = null;
    }

    public static Result<Settlement> Create(
        WalletId walletId,
        DateTime periodStart,
        DateTime periodEnd,
        decimal totalAmount,
        decimal commissionAmount)
    {
        if (periodEnd <= periodStart)
        {
            return Result<Settlement>.Failure(DomainError.Settlement.InvalidPeriod);
        }

        if (totalAmount <= 0)
        {
            return Result<Settlement>.Failure(DomainError.Settlement.InvalidAmount);
        }

        if (commissionAmount < 0 || commissionAmount > totalAmount)
        {
            return Result<Settlement>.Failure(DomainError.Settlement.InvalidCommission);
        }

        var settlement = new Settlement(
            SettlementId.CreateUnique(),
            walletId,
            periodStart,
            periodEnd,
            totalAmount,
            commissionAmount);

        return Result<Settlement>.Success(settlement);
    }

    public Result Process()
    {
        if (Status == SettlementStatusEnum.Completed.ToString())
        {
            return Result.Failure(DomainError.Settlement.AlreadyProcessed);
        }

        Status = SettlementStatusEnum.Processing.ToString();
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status == SettlementStatusEnum.Completed.ToString())
        {
            return Result.Failure(DomainError.Settlement.AlreadyProcessed);
        }

        Status = SettlementStatusEnum.Completed.ToString();
        PaidAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Fail()
    {
        Status = SettlementStatusEnum.Failed.ToString();
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != SettlementStatusEnum.Pending.ToString())
        {
            return Result.Failure(DomainError.Settlement.CannotCancel);
        }

        Status = SettlementStatusEnum.Cancelled.ToString();
        return Result.Success();
    }
}
