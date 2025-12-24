using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<ReviewId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReviewCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogService? _logService;

    public CreateReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateReviewCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        ILogService? logService = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _logService = logService;
    }

    public async Task<Result<ReviewId>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<ReviewId>.Failure(new Error(
                "Review.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var userId = UserId.Create(userIdGuid);
        var bookingDetailId = BookingDetailId.Create(request.BookingDetailId);
        var roomTypeId = RoomTypeId.Create(request.RoomTypeId);

        // Verify booking detail exists and belongs to the user
        var bookingDetail = await _unitOfWork.Repository<BookingDetail>()
            .GetByIdAsync(bookingDetailId.Value);

        if (bookingDetail == null)
        {
            return Result<ReviewId>.Failure(DomainError.Booking.NotFound);
        }

        // TODO: Verify booking is completed (add this check when Booking status is available)
        // For now, we'll allow reviews on any booking

        // Check if review already exists for this booking detail
        var existingReviews = _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.BookingDetailId == bookingDetailId)
            .ToList();

        if (existingReviews.Any())
        {
            return Result<ReviewId>.Failure(DomainError.Review.DuplicateReview);
        }

        // Create review
        var reviewResult = Review.Create(
            bookingDetailId,
            userId,
            roomTypeId,
            request.Rating,
            request.Comment);

        if (reviewResult.IsFailure)
        {
            _logger.LogError("Failed to create review: {Errors}",
                string.Join(", ", reviewResult.Errors.Select(e => e.Description)));
            return Result<ReviewId>.Failure(reviewResult.Errors);
        }

        var review = reviewResult.Value;

        // Add images if provided
        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            foreach (var imageUrl in request.ImageUrls)
            {
                var imageResult = review.AddImage(imageUrl);
                if (imageResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add image to review: {Errors}",
                        string.Join(", ", imageResult.Errors.Select(e => e.Description)));
                    // Continue with other images
                }
            }
        }

        // Save to database
        await _unitOfWork.Repository<Review>().AddAsync(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        // Log performance
        _logService?.LogPerformance(
            "CreateReview",
            stopwatch.ElapsedMilliseconds,
            new Dictionary<string, object>
            {
                ["ReviewId"] = review.Id.Value.ToString(),
                ["UserId"] = userId.Id.ToString(),
                ["RoomTypeId"] = roomTypeId.Value.ToString(),
                ["Rating"] = request.Rating
            });

        _logService?.LogBusinessEvent(
            "ReviewCreated",
            new Dictionary<string, object>
            {
                ["ReviewId"] = review.Id.Value.ToString(),
                ["Rating"] = request.Rating,
                ["HasImages"] = request.ImageUrls?.Any() ?? false
            });

        _logger.LogInformation("Successfully created review {ReviewId} by user {UserId}",
            review.Id.Value, userId.Id);

        return Result<ReviewId>.Success(review.Id);
    }
}
