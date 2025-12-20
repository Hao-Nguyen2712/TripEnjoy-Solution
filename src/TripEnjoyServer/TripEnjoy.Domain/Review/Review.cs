using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Domain.Review;

public class Review : AggregateRoot<ReviewId>
{
    private const int MaxImagesPerReview = 10;
    private readonly List<ReviewImage> _reviewImages = new();
    private readonly List<ReviewReply> _reviewReplies = new();

    public BookingDetailId BookingDetailId { get; private set; }
    public UserId UserId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public Domain.Account.Entities.User User { get; private set; }
    public Domain.Room.RoomType RoomType { get; private set; }
    public Domain.Booking.Entities.BookingDetail BookingDetail { get; private set; }
    public IReadOnlyList<ReviewImage> ReviewImages => _reviewImages.AsReadOnly();
    public IReadOnlyList<ReviewReply> ReviewReplies => _reviewReplies.AsReadOnly();

    private Review() : base(ReviewId.CreateUnique())
    {
        BookingDetailId = null!;
        UserId = null!;
        RoomTypeId = null!;
        Comment = null!;
        Status = null!;
        User = null!;
        RoomType = null!;
        BookingDetail = null!;
    }

    public Review(
        ReviewId id,
        BookingDetailId bookingDetailId,
        UserId userId,
        RoomTypeId roomTypeId,
        int rating,
        string comment) : base(id)
    {
        BookingDetailId = bookingDetailId;
        UserId = userId;
        RoomTypeId = roomTypeId;
        Rating = rating;
        Comment = comment;
        Status = ReviewStatusEnum.Active.ToString();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<Review> Create(
        BookingDetailId bookingDetailId,
        UserId userId,
        RoomTypeId roomTypeId,
        int rating,
        string comment)
    {
        if (rating < 1 || rating > 5)
        {
            return Result<Review>.Failure(DomainError.Review.InvalidRating);
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return Result<Review>.Failure(DomainError.Review.CommentRequired);
        }

        var review = new Review(
            ReviewId.CreateUnique(),
            bookingDetailId,
            userId,
            roomTypeId,
            rating,
            comment);

        return Result<Review>.Success(review);
    }

    public Result Update(int rating, string comment)
    {
        if (Status == ReviewStatusEnum.Deleted.ToString())
        {
            return Result.Failure(DomainError.Review.CannotUpdateDeleted);
        }

        if (rating < 1 || rating > 5)
        {
            return Result.Failure(DomainError.Review.InvalidRating);
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return Result.Failure(DomainError.Review.CommentRequired);
        }

        Rating = rating;
        Comment = comment;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Hide()
    {
        if (Status == ReviewStatusEnum.Deleted.ToString())
        {
            return Result.Failure(DomainError.Review.CannotUpdateDeleted);
        }

        Status = ReviewStatusEnum.Hidden.ToString();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Unhide()
    {
        if (Status == ReviewStatusEnum.Deleted.ToString())
        {
            return Result.Failure(DomainError.Review.CannotUpdateDeleted);
        }

        Status = ReviewStatusEnum.Active.ToString();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Delete()
    {
        Status = ReviewStatusEnum.Deleted.ToString();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result<ReviewImage> AddImage(string filePath)
    {
        if (_reviewImages.Count >= MaxImagesPerReview)
        {
            return Result<ReviewImage>.Failure(DomainError.Review.MaxImagesExceeded);
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result<ReviewImage>.Failure(DomainError.Review.InvalidImageUrl);
        }

        var image = ReviewImage.Create(Id, filePath);
        _reviewImages.Add(image);

        return Result<ReviewImage>.Success(image);
    }

    public Result RemoveImage(ReviewImageId imageId)
    {
        var image = _reviewImages.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return Result.Failure(DomainError.Review.ImageNotFound);
        }

        _reviewImages.Remove(image);

        return Result.Success();
    }

    public Result<ReviewReply> AddReply(
        ReplierTypeEnum replierType,
        AccountId replierId,
        string content)
    {
        // Check if replier has already replied
        var existingReply = _reviewReplies.FirstOrDefault(r => r.ReplierId == replierId);
        if (existingReply != null)
        {
            return Result<ReviewReply>.Failure(DomainError.ReviewReply.DuplicateReply);
        }

        var replyResult = ReviewReply.Create(Id, replierType, replierId, content);
        if (replyResult.IsFailure)
        {
            return Result<ReviewReply>.Failure(replyResult.Errors);
        }

        _reviewReplies.Add(replyResult.Value);

        return Result<ReviewReply>.Success(replyResult.Value);
    }

    public Result RemoveReply(ReviewReplyId replyId)
    {
        var reply = _reviewReplies.FirstOrDefault(r => r.Id == replyId);
        if (reply == null)
        {
            return Result.Failure(DomainError.ReviewReply.NotFound);
        }

        _reviewReplies.Remove(reply);

        return Result.Success();
    }
}
