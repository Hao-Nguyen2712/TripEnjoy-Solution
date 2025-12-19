using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Test.UnitTests.Domain.Room;

public class RoomTypeTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Deluxe Suite";
        var capacity = 2;
        var basePrice = 150.00m;
        var totalQuantity = 10;
        var description = "A luxurious suite with ocean view";

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PropertyId.Should().Be(propertyId);
        result.Value.RoomTypeName.Should().Be(roomTypeName);
        result.Value.Capacity.Should().Be(capacity);
        result.Value.BasePrice.Should().Be(basePrice);
        result.Value.TotalQuantity.Should().Be(totalQuantity);
        result.Value.Description.Should().Be(description);
        result.Value.Status.Should().Be("Active");
        result.Value.AverageRating.Should().Be(0);
        result.Value.ReviewCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "";
        var capacity = 2;
        var basePrice = 150.00m;
        var totalQuantity = 10;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.NameIsRequired);
    }

    [Fact]
    public void Create_WithZeroCapacity_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Standard Room";
        var capacity = 0;
        var basePrice = 100.00m;
        var totalQuantity = 5;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidCapacity);
    }

    [Fact]
    public void Create_WithNegativeCapacity_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Standard Room";
        var capacity = -1;
        var basePrice = 100.00m;
        var totalQuantity = 5;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidCapacity);
    }

    [Fact]
    public void Create_WithZeroBasePrice_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Economy Room";
        var capacity = 1;
        var basePrice = 0m;
        var totalQuantity = 5;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidBasePrice);
    }

    [Fact]
    public void Create_WithNegativeBasePrice_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Economy Room";
        var capacity = 1;
        var basePrice = -50.00m;
        var totalQuantity = 5;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidBasePrice);
    }

    [Fact]
    public void Create_WithZeroTotalQuantity_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Premium Room";
        var capacity = 3;
        var basePrice = 200.00m;
        var totalQuantity = 0;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidTotalQuantity);
    }

    [Fact]
    public void Create_WithNegativeTotalQuantity_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomTypeName = "Premium Room";
        var capacity = 3;
        var basePrice = 200.00m;
        var totalQuantity = -2;

        // Act
        var result = RoomType.Create(propertyId, roomTypeName, capacity, basePrice, totalQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.InvalidTotalQuantity);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Standard Room", 2, 100.00m, 5).Value;
        var newName = "Updated Standard Room";
        var newCapacity = 3;
        var newBasePrice = 120.00m;
        var newTotalQuantity = 8;
        var newDescription = "Updated description";

        // Act
        var result = roomType.Update(newName, newCapacity, newBasePrice, newTotalQuantity, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roomType.RoomTypeName.Should().Be(newName);
        roomType.Capacity.Should().Be(newCapacity);
        roomType.BasePrice.Should().Be(newBasePrice);
        roomType.TotalQuantity.Should().Be(newTotalQuantity);
        roomType.Description.Should().Be(newDescription);
    }

    [Fact]
    public void AddImage_WhenNoExistingImages_ShouldSetAsMain()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Deluxe Room", 2, 150.00m, 5).Value;
        var imageUrl = "https://example.com/image1.jpg";

        // Act
        var result = roomType.AddImage(imageUrl, isCover: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roomType.RoomTypeImages.Should().HaveCount(1);
        roomType.RoomTypeImages.First().IsMain.Should().BeTrue();
    }

    [Fact]
    public void AddImage_WithCoverFlag_ShouldSetAsMain()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Deluxe Room", 2, 150.00m, 5).Value;
        roomType.AddImage("https://example.com/image1.jpg", false);

        // Act
        var result = roomType.AddImage("https://example.com/image2.jpg", isCover: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roomType.RoomTypeImages.Should().HaveCount(2);
        roomType.RoomTypeImages.First().IsMain.Should().BeFalse();
        roomType.RoomTypeImages.Last().IsMain.Should().BeTrue();
    }

    [Fact]
    public void RemoveImage_WhenImageExists_ShouldRemoveSuccessfully()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Premium Room", 2, 200.00m, 3).Value;
        roomType.AddImage("https://example.com/image1.jpg", false);
        var imageId = roomType.RoomTypeImages.First().Id;

        // Act
        var result = roomType.RemoveImage(imageId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        roomType.RoomTypeImages.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_WhenImageDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Premium Room", 2, 200.00m, 3).Value;
        var nonExistentImageId = RoomTypeImageId.CreateUnique();

        // Act
        var result = roomType.RemoveImage(nonExistentImageId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.RoomType.ImageNotFound);
    }

    [Fact]
    public void Activate_ShouldSetStatusToActive()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Standard Room", 2, 100.00m, 5).Value;
        roomType.Deactivate();

        // Act
        roomType.Activate();

        // Assert
        roomType.Status.Should().Be("Active");
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Standard Room", 2, 100.00m, 5).Value;

        // Act
        roomType.Deactivate();

        // Assert
        roomType.Status.Should().Be("Inactive");
    }

    [Fact]
    public void MarkAsOutOfService_ShouldSetStatusToOutOfService()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Standard Room", 2, 100.00m, 5).Value;

        // Act
        roomType.MarkAsOutOfService();

        // Assert
        roomType.Status.Should().Be("OutOfService");
    }

    [Fact]
    public void UpdateRating_ShouldUpdateAverageRatingAndReviewCount()
    {
        // Arrange
        var propertyId = PropertyId.CreateUnique();
        var roomType = RoomType.Create(propertyId, "Luxury Suite", 4, 300.00m, 2).Value;
        var newAverageRating = 4.5m;
        var newReviewCount = 25;

        // Act
        roomType.UpdateRating(newAverageRating, newReviewCount);

        // Assert
        roomType.AverageRating.Should().Be(newAverageRating);
        roomType.ReviewCount.Should().Be(newReviewCount);
    }
}
