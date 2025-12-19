using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.ValueObjects;

public class BookingHistoryId : ValueObject
{
    public Guid Value { get; private set; }

    private BookingHistoryId(Guid value)
    {
        Value = value;
    }

    public static BookingHistoryId CreateUnique()
    {
        return new BookingHistoryId(Guid.NewGuid());
    }

    public static BookingHistoryId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BookingHistoryId cannot be empty", nameof(value));
        }
        return new BookingHistoryId(value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
