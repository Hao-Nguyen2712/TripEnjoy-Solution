using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Bookings.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Bookings.Handlers;

public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogService? _logService;
    private readonly IPublishEndpoint _publishEndpoint;

    public ConfirmBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ConfirmBookingCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _publishEndpoint = publishEndpoint;
        _logService = logService;
    }

    public async Task<Result> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var bookingId = BookingId.Create(request.BookingId);

        // Get booking
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId.Id);
        if (booking == null)
        {
            return Result.Failure(DomainError.Booking.NotFound);
        }

        // Get user ID for audit
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId? userId = null;
        if (Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            userId = UserId.Create(userIdGuid);
        }

        // Confirm booking
        var confirmResult = booking.Confirm(userId);
        if (confirmResult.IsFailure)
        {
            _logger.LogError("Failed to confirm booking {BookingId}: {Errors}",
                booking.Id.Id, string.Join(", ", confirmResult.Errors.Select(e => e.Description)));
            return confirmResult;
        }

        // Update in database
        await _unitOfWork.Repository<Booking>().UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log event
        _logService?.LogBusinessEvent(
            "BookingConfirmed",
            new Dictionary<string, object>
            {
                ["BookingId"] = booking.Id.Id.ToString(),
                ["UserId"] = userId?.Id.ToString() ?? "System"
            });

        _logger.LogInformation("Successfully confirmed booking {BookingId}", booking.Id.Id);

        // Publish BookingConfirmed event to message queue for async processing
        try
        {
            await _publishEndpoint.Publish<IBookingConfirmedEvent>(new BookingConfirmedEvent
            {
                BookingId = booking.Id.Id,
                UserId = booking.UserId.Id,
                PropertyId = booking.PropertyId.Id,
                ConfirmedAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Published BookingConfirmed event for BookingId: {BookingId}", booking.Id.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingConfirmed event for BookingId: {BookingId}", booking.Id.Id);
            // Note: We don't fail the confirmation if event publishing fails
        }

        return Result.Success();
    }
}
