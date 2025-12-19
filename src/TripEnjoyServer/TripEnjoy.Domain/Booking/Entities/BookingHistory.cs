using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Booking.Entities;

/// <summary>
/// Immutable audit trail for booking status changes
/// </summary>
public class BookingHistory : Entity<BookingHistoryId>
{
    public BookingId BookingId { get; private set; }
    public string Description { get; private set; }
    public string Status { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public UserId? ChangedBy { get; private set; }

    // Navigation property
    public Booking Booking { get; private set; } = null!;

    private BookingHistory() : base(BookingHistoryId.CreateUnique())
    {
        BookingId = null!;
        Description = null!;
        Status = null!;
    }

    private BookingHistory(
        BookingHistoryId id,
        BookingId bookingId,
        string description,
        string status,
        UserId? changedBy) : base(id)
    {
        BookingId = bookingId;
        Description = description;
        Status = status;
        ChangedAt = DateTime.UtcNow;
        ChangedBy = changedBy;
    }

    public static BookingHistory CreateEntry(
        BookingId bookingId,
        string description,
        string status,
        UserId? changedBy = null)
    {
        return new BookingHistory(
            BookingHistoryId.CreateUnique(),
            bookingId,
            description,
            status,
            changedBy);
    }
}
