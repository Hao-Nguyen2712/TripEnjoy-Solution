using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Bookings.Commands;

public record CreateBookingCommand(
    Guid PropertyId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfGuests,
    string? SpecialRequests,
    List<BookingDetailDto> BookingDetails
) : IAuditableCommand<Result<BookingId>>;

public record BookingDetailDto(
    Guid RoomTypeId,
    int Quantity,
    decimal PricePerNight,
    decimal DiscountAmount = 0
);
