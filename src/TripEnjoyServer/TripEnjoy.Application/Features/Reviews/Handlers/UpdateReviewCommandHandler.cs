using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReviewCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateReviewCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result.Failure(new Error(
                "Review.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var userId = UserId.Create(userIdGuid);
        var reviewId = ReviewId.Create(request.ReviewId);

        // Get review
        var review = await _unitOfWork.Repository<Review>()
            .GetByIdAsync(reviewId.Value);

        if (review == null)
        {
            return Result.Failure(DomainError.Review.NotFound);
        }

        // Verify ownership
        if (review.UserId != userId)
        {
            return Result.Failure(new Error(
                "Review.Unauthorized",
                "You are not authorized to update this review.",
                ErrorType.Forbidden));
        }

        // Update review
        var updateResult = review.Update(request.Rating, request.Comment);

        if (updateResult.IsFailure)
        {
            _logger.LogError("Failed to update review {ReviewId}: {Errors}",
                reviewId.Value, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated review {ReviewId} by user {UserId}",
            reviewId.Value, userId.Id);

        return Result.Success();
    }
}
