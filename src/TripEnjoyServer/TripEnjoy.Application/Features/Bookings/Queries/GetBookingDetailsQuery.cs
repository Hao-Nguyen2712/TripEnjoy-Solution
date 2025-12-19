using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Bookings.Queries;

public record GetBookingDetailsQuery(
    Guid BookingId
) : IRequest<Result<BookingDetailDto>>;
