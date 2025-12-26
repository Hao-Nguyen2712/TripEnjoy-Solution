using MediatR;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, Result<IEnumerable<BookingDetailDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllBookingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<BookingDetailDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _unitOfWork.Repository<Domain.Booking.Booking>().GetAllAsync();

        var bookingDtos = bookings.Select(b => new BookingDetailDto
        {
            Id = b.Id.Id,
            UserId = b.UserId.Id,
            PropertyId = b.PropertyId.Id,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            NumberOfGuests = b.NumberOfGuests,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            CreatedAt = b.CreatedAt,
            SpecialRequests = b.SpecialRequests,
            UpdatedAt = b.UpdatedAt
        }).OrderByDescending(b => b.CreatedAt);

        return Result<IEnumerable<BookingDetailDto>>.Success(bookingDtos);
    }
}
