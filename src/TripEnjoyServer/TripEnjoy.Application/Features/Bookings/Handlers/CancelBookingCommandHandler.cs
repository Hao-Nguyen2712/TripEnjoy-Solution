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

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelBookingCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogService? _logService;

    public CancelBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CancelBookingCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _logService = logService;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
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

        // Cancel booking
        var cancelResult = booking.Cancel(userId);
        if (cancelResult.IsFailure)
        {
            _logger.LogError("Failed to cancel booking {BookingId}: {Errors}",
                booking.Id.Id, string.Join(", ", cancelResult.Errors.Select(e => e.Description)));
            return cancelResult;
        }

        // Update in database
        await _unitOfWork.Repository<Booking>().UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log event
        _logService?.LogBusinessEvent(
            "BookingCancelled",
            new Dictionary<string, object>
            {
                ["BookingId"] = booking.Id.Id.ToString(),
                ["UserId"] = userId?.Id.ToString() ?? "System",
                ["Reason"] = request.CancellationReason ?? "No reason provided"
            });

        _logger.LogInformation("Successfully cancelled booking {BookingId}", booking.Id.Id);

        return Result.Success();
    }
}
