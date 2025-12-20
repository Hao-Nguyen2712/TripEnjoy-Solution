using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Domain.Review.Entities;

public class ReviewImage : Entity<ReviewImageId>
{
    public ReviewId ReviewId { get; private set; }
    public string FilePath { get; private set; }
    public DateTime UploadedAt { get; private set; }

    // Navigation property
    public Review Review { get; private set; }

    private ReviewImage() : base(ReviewImageId.CreateUnique())
    {
        ReviewId = null!;
        FilePath = null!;
        Review = null!;
    }

    public ReviewImage(
        ReviewImageId id,
        ReviewId reviewId,
        string filePath) : base(id)
    {
        ReviewId = reviewId;
        FilePath = filePath;
        UploadedAt = DateTime.UtcNow;
    }

    public static ReviewImage Create(ReviewId reviewId, string filePath)
    {
        return new ReviewImage(ReviewImageId.CreateUnique(), reviewId, filePath);
    }
}
