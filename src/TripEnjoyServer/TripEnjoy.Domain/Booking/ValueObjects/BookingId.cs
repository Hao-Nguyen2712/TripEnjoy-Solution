using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.ValueObjects
{
    public class BookingId : ValueObject
    {
        public Guid Id { get; private set; }

        public BookingId(Guid id)
        {
            Id = id;
        }

        public static BookingId CreateUnique()
        {
            return new BookingId(Guid.NewGuid());
        }

        public static BookingId Create(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("BookingId cannot be empty", nameof(id));
            }
            return new BookingId(id);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
