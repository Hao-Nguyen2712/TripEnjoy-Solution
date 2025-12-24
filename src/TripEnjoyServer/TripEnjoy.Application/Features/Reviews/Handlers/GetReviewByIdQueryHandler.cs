using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Reviews.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetReviewByIdQueryHandler> _logger;

    public GetReviewByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetReviewByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var reviewId = ReviewId.Create(request.ReviewId);

        // Query for the specific review
        var review = _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.Id == reviewId)
            .FirstOrDefault();

        if (review == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} not found", request.ReviewId);
            return Result<ReviewDto>.Failure(DomainError.Review.NotFound);
        }

        // Map to DTO
        var reviewDto = new ReviewDto
        {
            Id = review.Id.Value,
            BookingDetailId = review.BookingDetailId.Value,
            UserId = review.UserId.Id,
            UserName = review.User?.FullName ?? "Unknown",
            RoomTypeId = review.RoomTypeId.Value,
            RoomTypeName = review.RoomType?.RoomTypeName ?? "Unknown",
            Rating = review.Rating,
            Comment = review.Comment,
            Status = review.Status,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            Images = review.ReviewImages.Select(img => new ReviewImageDto
            {
                Id = img.Id.Value,
                FilePath = img.FilePath,
                UploadedAt = img.UploadedAt
            }).ToList(),
            Replies = review.ReviewReplies.Select(reply => new ReviewReplyDto
            {
                Id = reply.Id.Value,
                ReviewId = reply.ReviewId.Value,
                ReplierType = reply.ReplierType,
                ReplierId = reply.ReplierId.Id,
                ReplierName = "Unknown", // TODO: Load replier name from Account
                Content = reply.Content,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            }).ToList()
        };

        return Result<ReviewDto>.Success(reviewDto);
    }
}
