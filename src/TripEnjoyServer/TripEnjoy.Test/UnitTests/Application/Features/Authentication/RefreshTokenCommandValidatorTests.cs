using FluentAssertions;
using TripEnjoy.Application.Features.Authentication.Commands;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication
{
    public class RefreshTokenCommandValidatorTests
    {
        private readonly RefreshTokenCommandValidator _validator;

        public RefreshTokenCommandValidatorTests()
        {
            _validator = new RefreshTokenCommandValidator();
        }

        [Fact]
        public void Validate_WithValidCommand_ShouldReturnValid()
        {
            // Arrange
            var command = new RefreshTokenCommand(
                expiredAccessToken: "valid_access_token_that_is_expired",
                refreshToken: "valid_refresh_token"
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_WithInvalidExpiredAccessToken_ShouldReturnInvalid(string expiredAccessToken)
        {
            // Arrange
            var command = new RefreshTokenCommand(
                expiredAccessToken: expiredAccessToken,
                refreshToken: "valid_refresh_token"
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(command.expiredAccessToken));
            result.Errors.Should().Contain(e => e.ErrorMessage == "Expired access token is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_WithEmptyRefreshToken_ShouldReturnInvalid(string refreshToken)
        {
            // Arrange
            var command = new RefreshTokenCommand(
                expiredAccessToken: "valid_access_token",
                refreshToken: refreshToken
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(command.refreshToken));
            result.Errors.Should().Contain(e => e.ErrorMessage == "Refresh token is required.");
        }

        [Theory]
        [InlineData("short")]
        [InlineData("123456789")] // 9 characters - less than minimum of 10
        public void Validate_WithShortRefreshToken_ShouldReturnInvalid(string refreshToken)
        {
            // Arrange
            var command = new RefreshTokenCommand(
                expiredAccessToken: "valid_access_token",
                refreshToken: refreshToken
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(command.refreshToken));
            result.Errors.Should().Contain(e => e.ErrorMessage == "Refresh token must be at least 10 characters long.");
        }

        [Fact]
        public void Validate_WithMinimumLengthRefreshToken_ShouldReturnValid()
        {
            // Arrange
            var command = new RefreshTokenCommand(
                expiredAccessToken: "valid_access_token",
                refreshToken: "1234567890" // exactly 10 characters
            );

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }
    }
}