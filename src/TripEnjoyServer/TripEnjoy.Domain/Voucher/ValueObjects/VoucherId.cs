using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Voucher.ValueObjects;

public sealed class VoucherId : ValueObject
{
    public Guid Value { get; private set; }

    private VoucherId(Guid value)
    {
        Value = value;
    }

    public static VoucherId CreateUnique()
    {
        return new VoucherId(Guid.NewGuid());
    }

    public static VoucherId Create(Guid value)
    {
        return new VoucherId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
