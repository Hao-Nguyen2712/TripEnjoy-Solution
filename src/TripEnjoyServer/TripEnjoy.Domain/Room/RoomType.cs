using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room.Entities;
using TripEnjoy.Domain.Room.Enums;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Room;

public class RoomType : AggregateRoot<RoomTypeId>
{
    public PropertyId PropertyId { get; private set; }
    public string RoomTypeName { get; private set; }
    public string? Description { get; private set; }
    public int Capacity { get; private set; }
    public decimal BasePrice { get; private set; }
    public int TotalQuantity { get; private set; }
    public string Status { get; private set; }
    public decimal? AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Domain.Property.Property Property { get; private set; }

    private readonly List<RoomTypeImage> _roomTypeImages = new();
    public IReadOnlyList<RoomTypeImage> RoomTypeImages => _roomTypeImages.AsReadOnly();

    private readonly List<RoomAvailability> _roomAvailabilities = new();
    public IReadOnlyList<RoomAvailability> RoomAvailabilities => _roomAvailabilities.AsReadOnly();

    private readonly List<RoomPromotion> _roomPromotions = new();
    public IReadOnlyList<RoomPromotion> RoomPromotions => _roomPromotions.AsReadOnly();

    private RoomType() : base(RoomTypeId.CreateUnique())
    {
        PropertyId = null!;
        Property = null!;
        RoomTypeName = null!;
        Status = null!;
    }

    public RoomType(
        RoomTypeId id,
        PropertyId propertyId,
        string roomTypeName,
        int capacity,
        decimal basePrice,
        int totalQuantity,
        string? description = null) : base(id)
    {
        PropertyId = propertyId;
        RoomTypeName = roomTypeName;
        Description = description;
        Capacity = capacity;
        BasePrice = basePrice;
        TotalQuantity = totalQuantity;
        Status = RoomTypeStatusEnum.Active.ToString();
        AverageRating = 0;
        ReviewCount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<RoomType> Create(
        PropertyId propertyId,
        string roomTypeName,
        int capacity,
        decimal basePrice,
        int totalQuantity,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(roomTypeName))
        {
            return Result<RoomType>.Failure(DomainError.RoomType.NameIsRequired);
        }

        if (capacity <= 0)
        {
            return Result<RoomType>.Failure(DomainError.RoomType.InvalidCapacity);
        }

        if (basePrice <= 0)
        {
            return Result<RoomType>.Failure(DomainError.RoomType.InvalidBasePrice);
        }

        if (totalQuantity <= 0)
        {
            return Result<RoomType>.Failure(DomainError.RoomType.InvalidTotalQuantity);
        }

        var roomType = new RoomType(
            RoomTypeId.CreateUnique(),
            propertyId,
            roomTypeName,
            capacity,
            basePrice,
            totalQuantity,
            description);

        return Result<RoomType>.Success(roomType);
    }

    public Result Update(
        string roomTypeName,
        int capacity,
        decimal basePrice,
        int totalQuantity,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(roomTypeName))
        {
            return Result.Failure(DomainError.RoomType.NameIsRequired);
        }

        if (capacity <= 0)
        {
            return Result.Failure(DomainError.RoomType.InvalidCapacity);
        }

        if (basePrice <= 0)
        {
            return Result.Failure(DomainError.RoomType.InvalidBasePrice);
        }

        if (totalQuantity <= 0)
        {
            return Result.Failure(DomainError.RoomType.InvalidTotalQuantity);
        }

        RoomTypeName = roomTypeName;
        Capacity = capacity;
        BasePrice = basePrice;
        TotalQuantity = totalQuantity;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result AddImage(string imageUrl, bool isCover)
    {
        var roomTypeImage = new RoomTypeImage(RoomTypeImageId.CreateUnique(), Id, imageUrl);

        if (isCover)
        {
            // Set all other images to not be the main one
            _roomTypeImages.ForEach(img => img.SetAsNotMain());
            roomTypeImage.SetAsMain();
        }
        else if (!_roomTypeImages.Any(img => img.IsMain))
        {
            // If no other image is the main one, make this the main one
            roomTypeImage.SetAsMain();
        }

        _roomTypeImages.Add(roomTypeImage);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveImage(RoomTypeImageId imageId)
    {
        var imageToRemove = _roomTypeImages.FirstOrDefault(img => img.Id == imageId);
        if (imageToRemove == null)
        {
            return Result.Failure(DomainError.RoomType.ImageNotFound);
        }

        _roomTypeImages.Remove(imageToRemove);

        // If the removed image was the main one, and there are other images, set the first one as the new main
        if (imageToRemove.IsMain && _roomTypeImages.Any())
        {
            _roomTypeImages.First().SetAsMain();
        }

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result SetCoverImage(RoomTypeImageId imageId)
    {
        var imageToSetAsMain = _roomTypeImages.FirstOrDefault(img => img.Id == imageId);
        if (imageToSetAsMain == null)
        {
            return Result.Failure(DomainError.RoomType.ImageNotFound);
        }

        // Reset all other images
        _roomTypeImages.ForEach(img => img.SetAsNotMain());

        // Set the new main image
        imageToSetAsMain.SetAsMain();

        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void Activate()
    {
        Status = RoomTypeStatusEnum.Active.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = RoomTypeStatusEnum.Inactive.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOutOfService()
    {
        Status = RoomTypeStatusEnum.OutOfService.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(decimal averageRating, int reviewCount)
    {
        AverageRating = averageRating;
        ReviewCount = reviewCount;
        UpdatedAt = DateTime.UtcNow;
    }
}
