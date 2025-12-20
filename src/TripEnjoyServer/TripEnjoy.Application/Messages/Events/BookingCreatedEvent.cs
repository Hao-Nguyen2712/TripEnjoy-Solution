using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Events;

public class BookingCreatedEvent : IBookingCreatedEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
}
