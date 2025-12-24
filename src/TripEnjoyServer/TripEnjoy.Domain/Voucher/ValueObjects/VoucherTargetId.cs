using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Voucher.ValueObjects;

public sealed class VoucherTargetId : ValueObject
{
    public Guid Value { get; private set; }

    private VoucherTargetId(Guid value)
    {
        Value = value;
    }

    public static VoucherTargetId CreateUnique()
    {
        return new VoucherTargetId(Guid.NewGuid());
    }

    public static VoucherTargetId Create(Guid value)
    {
        return new VoucherTargetId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
