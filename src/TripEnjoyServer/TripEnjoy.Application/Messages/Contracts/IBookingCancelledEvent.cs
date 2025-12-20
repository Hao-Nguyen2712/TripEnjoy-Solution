namespace TripEnjoy.Application.Messages.Contracts;

/// <summary>
/// Event published when a booking is cancelled
/// </summary>
public interface IBookingCancelledEvent
{
    Guid BookingId { get; }
    Guid UserId { get; }
    Guid PropertyId { get; }
    string? CancellationReason { get; }
    DateTime CancelledAt { get; }
}
