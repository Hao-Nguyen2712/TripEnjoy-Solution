using MediatR;
using TripEnjoy.Application.Features.Bookings.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Booking;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Bookings.Handlers;

public class GetBookingDetailsQueryHandler : IRequestHandler<GetBookingDetailsQuery, Result<BookingDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookingDetailsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BookingDetailDto>> Handle(GetBookingDetailsQuery request, CancellationToken cancellationToken)
    {
        var bookingId = BookingId.Create(request.BookingId);

        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId.Id);

        if (booking == null)
        {
            return Result<BookingDetailDto>.Failure(DomainError.Booking.NotFound);
        }

        var bookingDetailDto = new BookingDetailDto
        {
            Id = booking.Id.Id,
            UserId = booking.UserId.Id,
            PropertyId = booking.PropertyId.Id,
            PropertyName = booking.Property?.Name ?? "Unknown Property",
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            NumberOfGuests = booking.NumberOfGuests,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            SpecialRequests = booking.SpecialRequests,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
            Items = booking.BookingDetails.Select(bd => new BookingItemDto
            {
                RoomTypeId = bd.RoomTypeId.Value,
                RoomTypeName = "Room", // Would need to join with RoomType table
                Quantity = bd.Quantity,
                Nights = bd.Nights,
                PricePerNight = bd.PricePerNight,
                DiscountAmount = bd.DiscountAmount,
                TotalPrice = bd.TotalPrice
            }).ToList(),
            History = booking.BookingHistory.Select(bh => new BookingHistoryItemDto
            {
                Description = bh.Description,
                Status = bh.Status,
                ChangedAt = bh.ChangedAt,
                ChangedBy = bh.ChangedBy?.Id.ToString()
            }).ToList()
        };

        return Result<BookingDetailDto>.Success(bookingDetailDto);
    }
}
