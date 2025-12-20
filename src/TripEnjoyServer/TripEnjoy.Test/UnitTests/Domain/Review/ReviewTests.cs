using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.Domain.Room.ValueObjects;
using ReviewEntity = TripEnjoy.Domain.Review.Review;

namespace TripEnjoy.Test.UnitTests.Domain.Review;

public class ReviewTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookingDetailId = BookingDetailId.CreateUnique();
        var userId = UserId.CreateUnique();
        var roomTypeId = RoomTypeId.CreateUnique();
        var rating = 5;
        var comment = "Excellent room with great amenities!";

        // Act
        var result = ReviewEntity.Create(bookingDetailId, userId, roomTypeId, rating, comment);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.BookingDetailId.Should().Be(bookingDetailId);
        result.Value.UserId.Should().Be(userId);
        result.Value.RoomTypeId.Should().Be(roomTypeId);
        result.Value.Rating.Should().Be(rating);
        result.Value.Comment.Should().Be(comment);
        result.Value.Status.Should().Be(ReviewStatusEnum.Active.ToString());
        result.Value.ReviewImages.Should().BeEmpty();
        result.Value.ReviewReplies.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Create_WithInvalidRating_ShouldReturnFailure(int invalidRating)
    {
        // Arrange
        var bookingDetailId = BookingDetailId.CreateUnique();
        var userId = UserId.CreateUnique();
        var roomTypeId = RoomTypeId.CreateUnique();
        var comment = "Test comment";

        // Act
        var result = ReviewEntity.Create(bookingDetailId, userId, roomTypeId, invalidRating, comment);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.InvalidRating);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyComment_ShouldReturnFailure(string invalidComment)
    {
        // Arrange
        var bookingDetailId = BookingDetailId.CreateUnique();
        var userId = UserId.CreateUnique();
        var roomTypeId = RoomTypeId.CreateUnique();
        var rating = 4;

        // Act
        var result = ReviewEntity.Create(bookingDetailId, userId, roomTypeId, rating, invalidComment);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.CommentRequired);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var review = CreateValidReview();
        var newRating = 4;
        var newComment = "Updated comment after reconsideration";

        // Act
        var result = review.Update(newRating, newComment);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Rating.Should().Be(newRating);
        review.Comment.Should().Be(newComment);
        review.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Update_WithInvalidRating_ShouldReturnFailure(int invalidRating)
    {
        // Arrange
        var review = CreateValidReview();
        var newComment = "Updated comment";

        // Act
        var result = review.Update(invalidRating, newComment);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.InvalidRating);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithEmptyComment_ShouldReturnFailure(string invalidComment)
    {
        // Arrange
        var review = CreateValidReview();
        var newRating = 3;

        // Act
        var result = review.Update(newRating, invalidComment);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.CommentRequired);
    }

    [Fact]
    public void Update_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        review.Delete();

        // Act
        var result = review.Update(4, "New comment");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.CannotUpdateDeleted);
    }

    [Fact]
    public void Hide_ShouldSetStatusToHidden()
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        var result = review.Hide();

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Status.Should().Be(ReviewStatusEnum.Hidden.ToString());
        review.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Hide_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        review.Delete();

        // Act
        var result = review.Hide();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.CannotUpdateDeleted);
    }

    [Fact]
    public void Unhide_ShouldSetStatusToActive()
    {
        // Arrange
        var review = CreateValidReview();
        review.Hide();

        // Act
        var result = review.Unhide();

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Status.Should().Be(ReviewStatusEnum.Active.ToString());
        review.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Unhide_WhenDeleted_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        review.Delete();

        // Act
        var result = review.Unhide();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.CannotUpdateDeleted);
    }

    [Fact]
    public void Delete_ShouldSetStatusToDeleted()
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        var result = review.Delete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.Status.Should().Be(ReviewStatusEnum.Deleted.ToString());
        review.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddImage_WithValidPath_ShouldAddImageSuccessfully()
    {
        // Arrange
        var review = CreateValidReview();
        var filePath = "https://cloudinary.com/reviews/image1.jpg";

        // Act
        var result = review.AddImage(filePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.ReviewImages.Should().HaveCount(1);
        review.ReviewImages.First().FilePath.Should().Be(filePath);
        review.ReviewImages.First().ReviewId.Should().Be(review.Id);
    }

    [Fact]
    public void AddImage_WhenMaxImagesReached_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        for (int i = 0; i < 10; i++)
        {
            review.AddImage($"https://cloudinary.com/reviews/image{i}.jpg");
        }

        // Act
        var result = review.AddImage("https://cloudinary.com/reviews/image11.jpg");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.MaxImagesExceeded);
        review.ReviewImages.Should().HaveCount(10);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void AddImage_WithInvalidPath_ShouldReturnFailure(string invalidPath)
    {
        // Arrange
        var review = CreateValidReview();

        // Act
        var result = review.AddImage(invalidPath);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.InvalidImageUrl);
    }

    [Fact]
    public void RemoveImage_WhenImageExists_ShouldRemoveSuccessfully()
    {
        // Arrange
        var review = CreateValidReview();
        var imageResult = review.AddImage("https://cloudinary.com/reviews/image1.jpg");
        var imageId = imageResult.Value.Id;

        // Act
        var result = review.RemoveImage(imageId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.ReviewImages.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_WhenImageDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        var nonExistentImageId = ReviewImageId.CreateUnique();

        // Act
        var result = review.RemoveImage(nonExistentImageId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Review.ImageNotFound);
    }

    [Fact]
    public void AddReply_WithValidData_ShouldAddReplySuccessfully()
    {
        // Arrange
        var review = CreateValidReview();
        var replierId = AccountId.CreateUnique();
        var content = "Thank you for your feedback!";

        // Act
        var result = review.AddReply(ReplierTypeEnum.Partner, replierId, content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.ReviewReplies.Should().HaveCount(1);
        review.ReviewReplies.First().Content.Should().Be(content);
        review.ReviewReplies.First().ReplierId.Should().Be(replierId);
        review.ReviewReplies.First().ReplierType.Should().Be(ReplierTypeEnum.Partner.ToString());
    }

    [Fact]
    public void AddReply_WhenReplierAlreadyReplied_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        var replierId = AccountId.CreateUnique();
        review.AddReply(ReplierTypeEnum.Partner, replierId, "First reply");

        // Act
        var result = review.AddReply(ReplierTypeEnum.Partner, replierId, "Second reply");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.ReviewReply.DuplicateReply);
        review.ReviewReplies.Should().HaveCount(1);
    }

    [Fact]
    public void AddReply_MultipleDifferentRepliers_ShouldAddAllReplies()
    {
        // Arrange
        var review = CreateValidReview();
        var partnerId = AccountId.CreateUnique();
        var adminId = AccountId.CreateUnique();

        // Act
        var partnerReply = review.AddReply(ReplierTypeEnum.Partner, partnerId, "Partner response");
        var adminReply = review.AddReply(ReplierTypeEnum.Admin, adminId, "Admin response");

        // Assert
        partnerReply.IsSuccess.Should().BeTrue();
        adminReply.IsSuccess.Should().BeTrue();
        review.ReviewReplies.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveReply_WhenReplyExists_ShouldRemoveSuccessfully()
    {
        // Arrange
        var review = CreateValidReview();
        var replierId = AccountId.CreateUnique();
        var replyResult = review.AddReply(ReplierTypeEnum.Partner, replierId, "Test reply");
        var replyId = replyResult.Value.Id;

        // Act
        var result = review.RemoveReply(replyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        review.ReviewReplies.Should().BeEmpty();
    }

    [Fact]
    public void RemoveReply_WhenReplyDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var review = CreateValidReview();
        var nonExistentReplyId = ReviewReplyId.CreateUnique();

        // Act
        var result = review.RemoveReply(nonExistentReplyId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.ReviewReply.NotFound);
    }

    // Helper method
    private static ReviewEntity CreateValidReview()
    {
        return ReviewEntity.Create(
            BookingDetailId.CreateUnique(),
            UserId.CreateUnique(),
            RoomTypeId.CreateUnique(),
            5,
            "Great experience!").Value;
    }
}
