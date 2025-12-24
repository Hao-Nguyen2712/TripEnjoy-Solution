using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Application.Features.Reviews.Commands;

public record CreateReviewReplyCommand(
    Guid ReviewId,
    string Content
) : IAuditableCommand<Result<ReviewReplyId>>;
