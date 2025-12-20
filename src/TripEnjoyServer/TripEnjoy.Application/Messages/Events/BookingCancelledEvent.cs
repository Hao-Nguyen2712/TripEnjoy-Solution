using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Events;

public class BookingCancelledEvent : IBookingCancelledEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CancelledAt { get; set; }
}
