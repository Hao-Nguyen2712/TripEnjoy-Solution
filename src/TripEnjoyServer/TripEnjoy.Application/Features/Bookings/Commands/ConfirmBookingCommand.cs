using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Bookings.Commands;

public record ConfirmBookingCommand(
    Guid BookingId
) : IAuditableCommand<Result>;
