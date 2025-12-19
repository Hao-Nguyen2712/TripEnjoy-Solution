using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.ValueObjects;

public class PaymentId : ValueObject
{
    public Guid Value { get; private set; }

    private PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId CreateUnique()
    {
        return new PaymentId(Guid.NewGuid());
    }

    public static PaymentId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PaymentId cannot be empty", nameof(value));
        }
        return new PaymentId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
