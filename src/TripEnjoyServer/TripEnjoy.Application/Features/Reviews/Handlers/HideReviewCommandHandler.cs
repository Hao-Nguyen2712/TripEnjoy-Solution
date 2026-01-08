using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class HideReviewCommandHandler : IRequestHandler<HideReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HideReviewCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HideReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<HideReviewCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(HideReviewCommand request, CancellationToken cancellationToken)
    {
        // Get user role from claims
        var userRole = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        
        // Only admins can hide/unhide reviews
        if (userRole != RoleConstant.Admin)
        {
            return Result.Failure(new Error(
                "Review.Unauthorized",
                "Only administrators can hide or unhide reviews.",
                ErrorType.Forbidden));
        }

        var reviewId = ReviewId.Create(request.ReviewId);

        // Get review
        var review = await _unitOfWork.Repository<Review>()
            .GetByIdAsync(reviewId.Value);

        if (review == null)
        {
            return Result.Failure(DomainError.Review.NotFound);
        }

        // Hide or unhide based on command parameter
        var result = request.Hide ? review.Hide() : review.Unhide();

        if (result.IsFailure)
        {
            _logger.LogError("Failed to {Action} review {ReviewId}: {Errors}",
                request.Hide ? "hide" : "unhide",
                reviewId.Value,
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return result;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully {Action} review {ReviewId}",
            request.Hide ? "hidden" : "unhidden", reviewId.Value);

        return Result.Success();
    }
}
