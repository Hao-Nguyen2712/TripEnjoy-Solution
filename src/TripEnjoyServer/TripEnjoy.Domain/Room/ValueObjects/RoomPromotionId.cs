using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Room.ValueObjects;

public sealed class RoomPromotionId : ValueObject
{
    public Guid Value { get; }

    private RoomPromotionId(Guid value)
    {
        Value = value;
    }

    public static RoomPromotionId CreateUnique()
    {
        return new RoomPromotionId(Guid.NewGuid());
    }

    public static RoomPromotionId Create(Guid value)
    {
        return new RoomPromotionId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
