using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Reviews.Commands;

public record UpdateReviewCommand(
    Guid ReviewId,
    int Rating,
    string Comment
) : IAuditableCommand<Result>;
