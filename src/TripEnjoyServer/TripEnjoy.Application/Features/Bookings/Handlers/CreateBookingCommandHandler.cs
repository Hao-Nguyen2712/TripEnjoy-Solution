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
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateBookingCommandHandler> logger,
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

        // Publish BookingCreated event to message queue for async processing
        try
        {
            await _publishEndpoint.Publish<IBookingCreatedEvent>(new BookingCreatedEvent
            {
                BookingId = booking.Id.Id,
                UserId = userId.Id,
                PropertyId = propertyId.Id,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NumberOfGuests = request.NumberOfGuests,
                TotalPrice = booking.TotalPrice,
                SpecialRequests = request.SpecialRequests,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Published BookingCreated event for BookingId: {BookingId}", booking.Id.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish BookingCreated event for BookingId: {BookingId}. Booking was saved but event not published.", booking.Id.Id);
            // Note: We don't fail the booking creation if event publishing fails
            // The booking is already saved, and we can retry publishing later if needed
        }

        return Result<BookingId>.Success(booking.Id);
    }
}
