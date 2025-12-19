using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Bookings.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Bookings.Handlers;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, Result<List<BookingDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUserBookingsQueryHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<List<BookingDto>>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<List<BookingDto>>.Failure(new Error(
                "Booking.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        // Get all bookings for user
        var allBookings = await _unitOfWork.Repository<Booking>().GetAllAsync();
        var userBookings = allBookings
            .Where(b => b.UserId.Id == userIdGuid)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BookingDto
            {
                Id = b.Id.Id,
                PropertyId = b.PropertyId.Id,
                PropertyName = b.Property?.Name ?? "Unknown Property",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfGuests = b.NumberOfGuests,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .ToList();

        return Result<List<BookingDto>>.Success(userBookings);
    }
}
