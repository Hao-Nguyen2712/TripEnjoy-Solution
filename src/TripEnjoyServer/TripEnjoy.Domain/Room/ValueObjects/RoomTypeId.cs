using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Room.ValueObjects;

public sealed class RoomTypeId : ValueObject
{
    public Guid Value { get; }

    private RoomTypeId(Guid value)
    {
        Value = value;
    }

    public static RoomTypeId CreateUnique()
    {
        return new RoomTypeId(Guid.NewGuid());
    }

    public static RoomTypeId Create(Guid value)
    {
        return new RoomTypeId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
