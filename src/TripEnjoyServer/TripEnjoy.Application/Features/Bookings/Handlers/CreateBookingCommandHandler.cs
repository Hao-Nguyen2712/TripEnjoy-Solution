using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Bookings.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Application.Features.Bookings.Handlers;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBookingCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogService? _logService;

    public CreateBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateBookingCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _logService = logService;
    }

    public async Task<Result<BookingId>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<BookingId>.Failure(new Error(
                "Booking.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var userId = UserId.Create(userIdGuid);
        var propertyId = PropertyId.Create(request.PropertyId);

        // Calculate nights
        var nights = (request.CheckOutDate - request.CheckInDate).Days;

        // Create booking details
        var bookingDetails = new List<BookingDetail>();
        decimal totalPrice = 0;

        foreach (var detailDto in request.BookingDetails)
        {
            var roomTypeId = RoomTypeId.Create(detailDto.RoomTypeId);
            
            var bookingDetailResult = BookingDetail.Create(
                BookingId.CreateUnique(), // Temporary ID, will be set when added to booking
                roomTypeId,
                detailDto.Quantity,
                nights,
                detailDto.PricePerNight,
                detailDto.DiscountAmount);

            if (bookingDetailResult.IsFailure)
            {
                _logger.LogError("Failed to create booking detail: {Errors}",
                    string.Join(", ", bookingDetailResult.Errors.Select(e => e.Description)));
                return Result<BookingId>.Failure(bookingDetailResult.Errors);
            }

            bookingDetails.Add(bookingDetailResult.Value);
            totalPrice += bookingDetailResult.Value.TotalPrice;
        }

        // Create booking
        var bookingResult = Booking.Create(
            userId,
            propertyId,
            request.CheckInDate,
            request.CheckOutDate,
            request.NumberOfGuests,
            totalPrice,
            request.SpecialRequests);

        if (bookingResult.IsFailure)
        {
            _logger.LogError("Failed to create booking: {Errors}",
                string.Join(", ", bookingResult.Errors.Select(e => e.Description)));
            return Result<BookingId>.Failure(bookingResult.Errors);
        }

        var booking = bookingResult.Value;

        // Add booking details to booking
        foreach (var detail in bookingDetails)
        {
            booking.AddBookingDetail(detail);
        }

        // Recalculate total price from details
        booking.RecalculateTotalPrice();

        // Save to database
        await _unitOfWork.Repository<Booking>().AddAsync(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        // Log performance
        _logService?.LogPerformance(
            "CreateBooking",
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, object>
            {
                ["BookingId"] = booking.Id.Id.ToString(),
                ["UserId"] = userId.Id.ToString(),
                ["PropertyId"] = propertyId.Id.ToString(),
                ["TotalPrice"] = booking.TotalPrice,
                ["NumberOfRoomTypes"] = bookingDetails.Count
            });

        _logService?.LogBusinessEvent(
            "BookingCreated",
            new Dictionary<string, object>
            {
                ["BookingId"] = booking.Id.Id.ToString(),
                ["CheckInDate"] = request.CheckInDate.ToString("yyyy-MM-dd"),
                ["CheckOutDate"] = request.CheckOutDate.ToString("yyyy-MM-dd"),
                ["Nights"] = nights
            });

        _logger.LogInformation("Successfully created booking {BookingId} for user {UserId}",
            booking.Id.Id, userId.Id);

        return Result<BookingId>.Success(booking.Id);
    }
}
