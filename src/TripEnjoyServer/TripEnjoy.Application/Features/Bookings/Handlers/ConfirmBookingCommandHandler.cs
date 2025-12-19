using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Bookings.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Persistence;
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

    public ConfirmBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ConfirmBookingCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
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

        return Result.Success();
    }
}
