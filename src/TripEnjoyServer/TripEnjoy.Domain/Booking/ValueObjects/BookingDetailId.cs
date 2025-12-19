using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.ValueObjects;

public class BookingDetailId : ValueObject
{
    public Guid Value { get; private set; }

    private BookingDetailId(Guid value)
    {
        Value = value;
    }

    public static BookingDetailId CreateUnique()
    {
        return new BookingDetailId(Guid.NewGuid());
    }

    public static BookingDetailId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingDetailId cannot be empty", nameof(value));
        }
        return new BookingDetailId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
