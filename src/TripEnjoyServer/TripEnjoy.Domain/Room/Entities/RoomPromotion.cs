using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room.Enums;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Room.Entities;

public class RoomPromotion : Entity<RoomPromotionId>
{
    public RoomTypeId RoomTypeId { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private RoomPromotion() : base(RoomPromotionId.CreateUnique())
    {
        RoomTypeId = null!;
        Status = null!;
    }

    public RoomPromotion(
        RoomPromotionId id,
        RoomTypeId roomTypeId,
        decimal? discountPercent,
        decimal? discountAmount,
        DateTime startDate,
        DateTime endDate) : base(id)
    {
        RoomTypeId = roomTypeId;
        DiscountPercent = discountPercent;
        DiscountAmount = discountAmount;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        Status = RoomPromotionStatusEnum.Active.ToString();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<RoomPromotion> Create(
        RoomTypeId roomTypeId,
        decimal? discountPercent,
        decimal? discountAmount,
        DateTime startDate,
        DateTime endDate)
    {
        // Business Rule: Either DiscountPercent OR DiscountAmount (not both, not neither)
        if (discountPercent.HasValue && discountAmount.HasValue)
        {
            return Result<RoomPromotion>.Failure(DomainError.RoomPromotion.BothDiscountTypesDefined);
        }

        if (!discountPercent.HasValue && !discountAmount.HasValue)
        {
            return Result<RoomPromotion>.Failure(DomainError.RoomPromotion.NoDiscountDefined);
        }

        // Validate discount percent
        if (discountPercent.HasValue && (discountPercent.Value <= 0 || discountPercent.Value > 100))
        {
            return Result<RoomPromotion>.Failure(DomainError.RoomPromotion.InvalidDiscountPercent);
        }

        // Validate discount amount
        if (discountAmount.HasValue && discountAmount.Value <= 0)
        {
            return Result<RoomPromotion>.Failure(DomainError.RoomPromotion.InvalidDiscountAmount);
        }

        // Validate date range
        if (endDate.Date <= startDate.Date)
        {
            return Result<RoomPromotion>.Failure(DomainError.RoomPromotion.InvalidDateRange);
        }

        var promotion = new RoomPromotion(
            RoomPromotionId.CreateUnique(),
            roomTypeId,
            discountPercent,
            discountAmount,
            startDate,
            endDate);

        return Result<RoomPromotion>.Success(promotion);
    }

    public Result Update(
        decimal? discountPercent,
        decimal? discountAmount,
        DateTime startDate,
        DateTime endDate)
    {
        // Business Rule: Either DiscountPercent OR DiscountAmount (not both, not neither)
        if (discountPercent.HasValue && discountAmount.HasValue)
        {
            return Result.Failure(DomainError.RoomPromotion.BothDiscountTypesDefined);
        }

        if (!discountPercent.HasValue && !discountAmount.HasValue)
        {
            return Result.Failure(DomainError.RoomPromotion.NoDiscountDefined);
        }

        // Validate discount percent
        if (discountPercent.HasValue && (discountPercent.Value <= 0 || discountPercent.Value > 100))
        {
            return Result.Failure(DomainError.RoomPromotion.InvalidDiscountPercent);
        }

        // Validate discount amount
        if (discountAmount.HasValue && discountAmount.Value <= 0)
        {
            return Result.Failure(DomainError.RoomPromotion.InvalidDiscountAmount);
        }

        // Validate date range
        if (endDate.Date <= startDate.Date)
        {
            return Result.Failure(DomainError.RoomPromotion.InvalidDateRange);
        }

        DiscountPercent = discountPercent;
        DiscountAmount = discountAmount;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public void Deactivate()
    {
        Status = RoomPromotionStatusEnum.Inactive.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = RoomPromotionStatusEnum.Active.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsExpired()
    {
        Status = RoomPromotionStatusEnum.Expired.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow.Date;
        return Status == RoomPromotionStatusEnum.Active.ToString() 
            && now >= StartDate 
            && now <= EndDate;
    }

    public decimal CalculateDiscountedPrice(decimal originalPrice)
    {
        if (!IsActive())
        {
            return originalPrice;
        }

        if (DiscountPercent.HasValue)
        {
            return originalPrice * (1 - (DiscountPercent.Value / 100));
        }

        if (DiscountAmount.HasValue)
        {
            var discountedPrice = originalPrice - DiscountAmount.Value;
            return discountedPrice > 0 ? discountedPrice : 0;
        }

        return originalPrice;
    }
}
