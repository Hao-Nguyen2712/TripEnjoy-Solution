using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Room.ValueObjects;

public sealed class RoomAvailabilityId : ValueObject
{
    public Guid Value { get; }

    private RoomAvailabilityId(Guid value)
    {
        Value = value;
    }

    public static RoomAvailabilityId CreateUnique()
    {
        return new RoomAvailabilityId(Guid.NewGuid());
    }

    public static RoomAvailabilityId Create(Guid value)
    {
        return new RoomAvailabilityId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
