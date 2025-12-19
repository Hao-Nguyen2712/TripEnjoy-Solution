using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Bookings.Queries;

public record GetUserBookingsQuery(
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<List<BookingDto>>>;
