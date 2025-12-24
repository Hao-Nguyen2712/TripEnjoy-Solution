using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Voucher.Entities;
using TripEnjoy.Domain.Voucher.Enums;
using TripEnjoy.Domain.Voucher.ValueObjects;

namespace TripEnjoy.Domain.Voucher;

public class Voucher : AggregateRoot<VoucherId>
{
    private readonly List<VoucherTarget> _voucherTargets = new();

    public string Code { get; private set; }
    public string? Description { get; private set; }
    public string DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal? MinimumOrderAmount { get; private set; }
    public decimal? MaximumDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UsageLimitPerUser { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; }
    public string CreatorType { get; private set; }
    public AccountId CreatorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public IReadOnlyList<VoucherTarget> VoucherTargets => _voucherTargets.AsReadOnly();

    private Voucher() : base(VoucherId.CreateUnique())
    {
        Code = null!;
        DiscountType = null!;
        Status = null!;
        CreatorType = null!;
        CreatorId = null!;
    }

    public Voucher(
        VoucherId id,
        string code,
        VoucherDiscountTypeEnum discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        VoucherCreatorTypeEnum creatorType,
        AccountId creatorId,
        string? description = null,
        decimal? minimumOrderAmount = null,
        decimal? maximumDiscountAmount = null,
        int? usageLimit = null,
        int? usageLimitPerUser = null) : base(id)
    {
        Code = code;
        Description = description;
        DiscountType = discountType.ToString();
        DiscountValue = discountValue;
        MinimumOrderAmount = minimumOrderAmount;
        MaximumDiscountAmount = maximumDiscountAmount;
        UsageLimit = usageLimit;
        UsageLimitPerUser = usageLimitPerUser;
        UsedCount = 0;
        StartDate = startDate;
        EndDate = endDate;
        Status = VoucherStatusEnum.Active.ToString();
        CreatorType = creatorType.ToString();
        CreatorId = creatorId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<Voucher> Create(
        string code,
        VoucherDiscountTypeEnum discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        VoucherCreatorTypeEnum creatorType,
        AccountId creatorId,
        string? description = null,
        decimal? minimumOrderAmount = null,
        decimal? maximumDiscountAmount = null,
        int? usageLimit = null,
        int? usageLimitPerUser = null)
    {
        // Validate code
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Voucher>.Failure(DomainError.Voucher.InvalidCode);
        }

        // Validate discount value
        if (discountType == VoucherDiscountTypeEnum.Percent)
        {
            if (discountValue < 0 || discountValue > 100)
            {
                return Result<Voucher>.Failure(DomainError.Voucher.InvalidDiscount);
            }
        }
        else if (discountType == VoucherDiscountTypeEnum.Amount)
        {
            if (discountValue <= 0)
            {
                return Result<Voucher>.Failure(DomainError.Voucher.InvalidDiscount);
            }
        }

        // Validate date range
        if (endDate <= startDate)
        {
            return Result<Voucher>.Failure(new Error(
                "Voucher.InvalidDateRange",
                "End date must be after start date.",
                ErrorType.Validation));
        }

        // Validate usage limits
        if (usageLimit.HasValue && usageLimit.Value < 0)
        {
            return Result<Voucher>.Failure(new Error(
                "Voucher.InvalidUsageLimit",
                "Usage limit must be non-negative.",
                ErrorType.Validation));
        }

        if (usageLimitPerUser.HasValue && usageLimitPerUser.Value < 0)
        {
            return Result<Voucher>.Failure(new Error(
                "Voucher.InvalidUsageLimitPerUser",
                "Usage limit per user must be non-negative.",
                ErrorType.Validation));
        }

        // Validate minimum order amount
        if (minimumOrderAmount.HasValue && minimumOrderAmount.Value < 0)
        {
            return Result<Voucher>.Failure(new Error(
                "Voucher.InvalidMinimumOrderAmount",
                "Minimum order amount must be non-negative.",
                ErrorType.Validation));
        }

        var voucher = new Voucher(
            VoucherId.CreateUnique(),
            code.ToUpperInvariant(),
            discountType,
            discountValue,
            startDate,
            endDate,
            creatorType,
            creatorId,
            description,
            minimumOrderAmount,
            maximumDiscountAmount,
            usageLimit,
            usageLimitPerUser);

        return Result<Voucher>.Success(voucher);
    }

    public Result Update(
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? usageLimit = null,
        int? usageLimitPerUser = null)
    {
        if (Status == VoucherStatusEnum.Disabled.ToString())
        {
            return Result.Failure(DomainError.Voucher.Disabled);
        }

        if (description != null)
        {
            Description = description;
        }

        if (startDate.HasValue || endDate.HasValue)
        {
            var newStartDate = startDate ?? StartDate;
            var newEndDate = endDate ?? EndDate;

            if (newEndDate <= newStartDate)
            {
                return Result.Failure(new Error(
                    "Voucher.InvalidDateRange",
                    "End date must be after start date.",
                    ErrorType.Validation));
            }

            StartDate = newStartDate;
            EndDate = newEndDate;
        }

        if (usageLimit.HasValue)
        {
            UsageLimit = usageLimit.Value;
        }

        if (usageLimitPerUser.HasValue)
        {
            UsageLimitPerUser = usageLimitPerUser.Value;
        }

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Disable()
    {
        Status = VoucherStatusEnum.Disabled.ToString();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Enable()
    {
        if (DateTime.UtcNow > EndDate)
        {
            return Result.Failure(DomainError.Voucher.Expired);
        }

        Status = VoucherStatusEnum.Active.ToString();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result<VoucherTarget> AddTarget(VoucherTarget target)
    {
        _voucherTargets.Add(target);
        UpdatedAt = DateTime.UtcNow;

        return Result<VoucherTarget>.Success(target);
    }

    public Result RemoveTarget(VoucherTargetId targetId)
    {
        var target = _voucherTargets.FirstOrDefault(t => t.Id == targetId);
        if (target == null)
        {
            return Result.Failure(new Error(
                "Voucher.TargetNotFound",
                "The specified target was not found.",
                ErrorType.NotFound));
        }

        _voucherTargets.Remove(target);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result IncrementUsageCount()
    {
        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
        {
            return Result.Failure(DomainError.Voucher.UsageLimitReached);
        }

        UsedCount++;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return Status == VoucherStatusEnum.Active.ToString()
               && now >= StartDate
               && now <= EndDate;
    }

    public bool CanBeUsed()
    {
        if (!IsActive())
        {
            return false;
        }

        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
        {
            return false;
        }

        return true;
    }

    public Result ValidateForUse()
    {
        if (Status == VoucherStatusEnum.Disabled.ToString())
        {
            return Result.Failure(DomainError.Voucher.Disabled);
        }

        var now = DateTime.UtcNow;

        if (now < StartDate)
        {
            return Result.Failure(DomainError.Voucher.NotStarted);
        }

        if (now > EndDate)
        {
            Status = VoucherStatusEnum.Expired.ToString();
            return Result.Failure(DomainError.Voucher.Expired);
        }

        if (UsageLimit.HasValue && UsedCount >= UsageLimit.Value)
        {
            return Result.Failure(DomainError.Voucher.UsageLimitReached);
        }

        return Result.Success();
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (DiscountType == VoucherDiscountTypeEnum.Percent.ToString())
        {
            var discount = orderAmount * (DiscountValue / 100);

            if (MaximumDiscountAmount.HasValue && discount > MaximumDiscountAmount.Value)
            {
                return MaximumDiscountAmount.Value;
            }

            return discount;
        }
        else // Amount
        {
            return Math.Min(DiscountValue, orderAmount);
        }
    }
}
