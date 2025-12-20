using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Test.UnitTests.Domain.Review;

public class ReviewImageTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();
        var filePath = "https://cloudinary.com/reviews/room-photo-1.jpg";

        // Act
        var image = ReviewImage.Create(reviewId, filePath);

        // Assert
        image.Should().NotBeNull();
        image.ReviewId.Should().Be(reviewId);
        image.FilePath.Should().Be(filePath);
        image.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldHaveUniqueId()
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();
        var filePath = "https://cloudinary.com/reviews/image.jpg";

        // Act
        var image1 = ReviewImage.Create(reviewId, filePath);
        var image2 = ReviewImage.Create(reviewId, filePath);

        // Assert
        image1.Id.Should().NotBe(image2.Id);
    }

    [Fact]
    public void Create_MultipleImagesForSameReview_ShouldShareReviewId()
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();

        // Act
        var image1 = ReviewImage.Create(reviewId, "https://cloudinary.com/reviews/image1.jpg");
        var image2 = ReviewImage.Create(reviewId, "https://cloudinary.com/reviews/image2.jpg");

        // Assert
        image1.ReviewId.Should().Be(reviewId);
        image2.ReviewId.Should().Be(reviewId);
        image1.Id.Should().NotBe(image2.Id);
    }
}
