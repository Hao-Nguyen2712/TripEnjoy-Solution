using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Domain.Review.Entities;

public class ReviewReply : Entity<ReviewReplyId>
{
    public ReviewId ReviewId { get; private set; }
    public string ReplierType { get; private set; }
    public AccountId ReplierId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property
    public Review Review { get; private set; }

    private ReviewReply() : base(ReviewReplyId.CreateUnique())
    {
        ReviewId = null!;
        ReplierType = null!;
        ReplierId = null!;
        Content = null!;
        Review = null!;
    }

    public ReviewReply(
        ReviewReplyId id,
        ReviewId reviewId,
        ReplierTypeEnum replierType,
        AccountId replierId,
        string content) : base(id)
    {
        ReviewId = reviewId;
        ReplierType = replierType.ToString();
        ReplierId = replierId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public static Result<ReviewReply> Create(
        ReviewId reviewId,
        ReplierTypeEnum replierType,
        AccountId replierId,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<ReviewReply>.Failure(DomainError.ReviewReply.ContentRequired);
        }

        var reply = new ReviewReply(
            ReviewReplyId.CreateUnique(),
            reviewId,
            replierType,
            replierId,
            content);

        return Result<ReviewReply>.Success(reply);
    }

    public Result Update(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure(DomainError.ReviewReply.ContentRequired);
        }

        Content = content;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
