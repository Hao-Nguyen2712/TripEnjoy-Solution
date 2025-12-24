using MediatR;
using TripEnjoy.Application.Features.Reviews.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, Result<PagedList<ReviewDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserReviewsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedList<ReviewDto>>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.UserId);

        // Query all reviews by the user (include all statuses for user's own reviews)
        var allReviews = _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        // Get total count for pagination
        var totalCount = allReviews.Count;

        // Apply pagination
        var reviews = allReviews
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            Id = r.Id.Value,
            BookingDetailId = r.BookingDetailId.Value,
            UserId = r.UserId.Id,
            UserName = r.User?.FullName ?? "Unknown",
            RoomTypeId = r.RoomTypeId.Value,
            RoomTypeName = r.RoomType?.RoomTypeName ?? "Unknown",
            Rating = r.Rating,
            Comment = r.Comment,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            Images = r.ReviewImages.Select(img => new ReviewImageDto
            {
                Id = img.Id.Value,
                FilePath = img.FilePath,
                UploadedAt = img.UploadedAt
            }).ToList(),
            Replies = r.ReviewReplies.Select(reply => new ReviewReplyDto
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
        }).ToList();

        var pagedList = new PagedList<ReviewDto>(reviewDtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedList<ReviewDto>>.Success(pagedList);
    }
}
