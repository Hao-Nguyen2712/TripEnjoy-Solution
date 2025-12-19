using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Room.Entities;

public class RoomAvailability : Entity<RoomAvailabilityId>
{
    public RoomTypeId RoomTypeId { get; private set; }
    public DateTime Date { get; private set; }
    public int AvailableQuantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private RoomAvailability() : base(RoomAvailabilityId.CreateUnique())
    {
        RoomTypeId = null!;
    }

    public RoomAvailability(
        RoomAvailabilityId id, 
        RoomTypeId roomTypeId, 
        DateTime date, 
        int availableQuantity, 
        decimal price) : base(id)
    {
        RoomTypeId = roomTypeId;
        Date = date.Date; // Store only the date part
        AvailableQuantity = availableQuantity;
        Price = price;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<RoomAvailability> Create(
        RoomTypeId roomTypeId,
        DateTime date,
        int availableQuantity,
        decimal price)
    {
        if (availableQuantity < 0)
        {
            return Result<RoomAvailability>.Failure(DomainError.RoomAvailability.InvalidAvailableQuantity);
        }

        if (price <= 0)
        {
            return Result<RoomAvailability>.Failure(DomainError.RoomAvailability.InvalidPrice);
        }

        if (date.Date < DateTime.UtcNow.Date)
        {
            return Result<RoomAvailability>.Failure(DomainError.RoomAvailability.InvalidDate);
        }

        var availability = new RoomAvailability(
            RoomAvailabilityId.CreateUnique(),
            roomTypeId,
            date,
            availableQuantity,
            price);

        return Result<RoomAvailability>.Success(availability);
    }

    public Result UpdateAvailability(int availableQuantity, decimal price)
    {
        if (availableQuantity < 0)
        {
            return Result.Failure(DomainError.RoomAvailability.InvalidAvailableQuantity);
        }

        if (price <= 0)
        {
            return Result.Failure(DomainError.RoomAvailability.InvalidPrice);
        }

        AvailableQuantity = availableQuantity;
        Price = price;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result DecrementQuantity(int quantity)
    {
        if (AvailableQuantity < quantity)
        {
            return Result.Failure(DomainError.RoomAvailability.InsufficientQuantity);
        }

        AvailableQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result IncrementQuantity(int quantity)
    {
        AvailableQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
