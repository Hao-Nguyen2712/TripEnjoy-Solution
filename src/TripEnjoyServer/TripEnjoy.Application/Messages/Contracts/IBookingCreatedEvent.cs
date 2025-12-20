namespace TripEnjoy.Application.Messages.Contracts;

/// <summary>
/// Event published when a booking is successfully created
/// </summary>
public interface IBookingCreatedEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    DateTime CheckInDate { get; }
    DateTime CheckOutDate { get; }
    int NumberOfGuests { get; }
    decimal TotalPrice { get; }
    string? SpecialRequests { get; }
    DateTime CreatedAt { get; }
}
