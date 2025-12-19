using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Bookings.Commands;

public record CancelBookingCommand(
    Guid BookingId,
    string? CancellationReason
) : IAuditableCommand<Result>;
