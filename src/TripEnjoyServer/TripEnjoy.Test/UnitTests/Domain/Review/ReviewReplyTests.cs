using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;

namespace TripEnjoy.Test.UnitTests.Domain.Review;

public class ReviewReplyTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();
        var replierId = AccountId.CreateUnique();
        var content = "Thank you for your wonderful feedback!";

        // Act
        var result = ReviewReply.Create(reviewId, ReplierTypeEnum.Partner, replierId, content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ReviewId.Should().Be(reviewId);
        result.Value.ReplierId.Should().Be(replierId);
        result.Value.ReplierType.Should().Be(ReplierTypeEnum.Partner.ToString());
        result.Value.Content.Should().Be(content);
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_AsAdmin_ShouldCreateWithAdminType()
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();
        var adminId = AccountId.CreateUnique();
        var content = "We appreciate your review.";

        // Act
        var result = ReviewReply.Create(reviewId, ReplierTypeEnum.Admin, adminId, content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReplierType.Should().Be(ReplierTypeEnum.Admin.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyContent_ShouldReturnFailure(string invalidContent)
    {
        // Arrange
        var reviewId = ReviewId.CreateUnique();
        var replierId = AccountId.CreateUnique();

        // Act
        var result = ReviewReply.Create(reviewId, ReplierTypeEnum.Partner, replierId, invalidContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.ReviewReply.ContentRequired);
    }

    [Fact]
    public void Update_WithValidContent_ShouldUpdateSuccessfully()
    {
        // Arrange
        var reply = CreateValidReply();
        var newContent = "Updated response after further consideration.";

        // Act
        var result = reply.Update(newContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        reply.Content.Should().Be(newContent);
        reply.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithEmptyContent_ShouldReturnFailure(string invalidContent)
    {
        // Arrange
        var reply = CreateValidReply();

        // Act
        var result = reply.Update(invalidContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.ReviewReply.ContentRequired);
    }

    [Fact]
    public void Update_MultipleTimes_ShouldUpdateTimestamp()
    {
        // Arrange
        var reply = CreateValidReply();
        var firstUpdate = reply.Update("First update");
        var firstTimestamp = reply.UpdatedAt;

        // Small delay to ensure different timestamp
        Thread.Sleep(10);

        // Act
        var secondUpdate = reply.Update("Second update");

        // Assert
        secondUpdate.IsSuccess.Should().BeTrue();
        reply.UpdatedAt.Should().NotBe(firstTimestamp);
        reply.UpdatedAt.Should().BeAfter(firstTimestamp.Value);
    }

    // Helper method
    private static ReviewReply CreateValidReply()
    {
        return ReviewReply.Create(
            ReviewId.CreateUnique(),
            ReplierTypeEnum.Partner,
            AccountId.CreateUnique(),
            "Thank you for your feedback!").Value;
    }
}
