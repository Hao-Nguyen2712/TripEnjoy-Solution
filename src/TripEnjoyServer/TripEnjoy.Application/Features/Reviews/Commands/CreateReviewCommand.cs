using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Application.Features.Reviews.Commands;

public record CreateReviewCommand(
    Guid BookingDetailId,
    Guid RoomTypeId,
    int Rating,
    string Comment,
    List<string>? ImageUrls = null
) : IAuditableCommand<Result<ReviewId>>;
