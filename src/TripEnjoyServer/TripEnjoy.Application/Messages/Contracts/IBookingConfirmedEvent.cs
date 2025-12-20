namespace TripEnjoy.Application.Messages.Contracts;

/// <summary>
/// Event published when a booking is confirmed (after payment)
/// </summary>
public interface IBookingConfirmedEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    DateTime ConfirmedAt { get; }
}
