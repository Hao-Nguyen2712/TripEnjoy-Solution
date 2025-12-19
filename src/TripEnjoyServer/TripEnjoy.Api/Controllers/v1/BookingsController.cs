using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Bookings.Commands;
using TripEnjoy.Application.Features.Bookings.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class BookingsController : ApiControllerBase
{
    private readonly ISender _sender;

    public BookingsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleConstant.User)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Booking created successfully");
    }

    /// <summary>
    /// Get user's bookings
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize(Roles = RoleConstant.User)]
    public async Task<IActionResult> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetUserBookingsQuery(page, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result, "Bookings retrieved successfully");
    }

    /// <summary>
    /// Get booking details by ID
    /// </summary>
    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetBookingDetails(Guid bookingId)
    {
        var query = new GetBookingDetailsQuery(bookingId);
        var result = await _sender.Send(query);
        return HandleResult(result, "Booking details retrieved successfully");
    }

    /// <summary>
    /// Confirm a booking (after payment)
    /// </summary>
    [HttpPost("{bookingId:guid}/confirm")]
    [Authorize(Roles = $"{RoleConstant.Partner},{RoleConstant.Admin}")]
    public async Task<IActionResult> ConfirmBooking(Guid bookingId)
    {
        var command = new ConfirmBookingCommand(bookingId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Booking confirmed successfully");
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid bookingId, [FromBody] CancelBookingRequest request)
    {
        var command = new CancelBookingCommand(bookingId, request.CancellationReason);
        var result = await _sender.Send(command);
        return HandleResult(result, "Booking cancelled successfully");
    }
}

public record CancelBookingRequest(string? CancellationReason);
