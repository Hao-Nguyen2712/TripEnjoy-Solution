using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Room.ValueObjects;

public sealed class RoomTypeImageId : ValueObject
{
    public Guid Value { get; }

    private RoomTypeImageId(Guid value)
    {
        Value = value;
    }

    public static RoomTypeImageId CreateUnique()
    {
        return new RoomTypeImageId(Guid.NewGuid());
    }

    public static RoomTypeImageId Create(Guid value)
    {
        return new RoomTypeImageId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
