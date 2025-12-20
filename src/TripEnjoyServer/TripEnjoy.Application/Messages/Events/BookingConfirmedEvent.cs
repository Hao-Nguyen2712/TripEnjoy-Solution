using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Events;

public class BookingConfirmedEvent : IBookingConfirmedEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
