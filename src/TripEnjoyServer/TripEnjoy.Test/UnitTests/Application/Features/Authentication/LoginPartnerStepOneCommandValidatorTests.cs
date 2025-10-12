using FluentValidation.TestHelper;
using TripEnjoy.Application.Features.Authentication.Commands;

namespace TripEnjoy.Test.UnitTests.Application.Features.Authentication;

public class LoginPartnerStepOneCommandValidatorTests
{
    private readonly LoginPartnerStepOneCommandValidator _validator;

    public LoginPartnerStepOneCommandValidatorTests()
    {
        _validator = new LoginPartnerStepOneCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: "partner@example.com",
            Password: "ValidPassword123!"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldHaveEmailRequiredError(string invalidEmail)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: invalidEmail,
            Password: "ValidPassword123!"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("partner@")]
    [InlineData("@example.com")]
    [InlineData("partner.example.com")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveEmailFormatError(string invalidEmail)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: invalidEmail,
            Password: "ValidPassword123!"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email must be a valid email address.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidPassword_ShouldHavePasswordRequiredError(string invalidPassword)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: "partner@example.com",
            Password: invalidPassword
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("12345")]      // 5 characters
    [InlineData("123")]        // 3 characters
    [InlineData("a")]          // 1 character
    public void Validate_WithShortPassword_ShouldHavePasswordLengthError(string shortPassword)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: "partner@example.com",
            Password: shortPassword
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 6 characters long.");
    }

    [Theory]
    [InlineData("123456")]         // Exactly 6 characters
    [InlineData("ValidPass123")]   // Valid password
    [InlineData("LongValidPassword123!@#")] // Long valid password
    public void Validate_WithValidPassword_ShouldNotHavePasswordErrors(string validPassword)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: "partner@example.com",
            Password: validPassword
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("partner@example.com")]      // Standard email
    [InlineData("test.partner@domain.co.uk")] // Email with dots and country code
    [InlineData("partner+tag@example.org")]   // Email with plus sign
    public void Validate_WithValidEmail_ShouldNotHaveEmailErrors(string validEmail)
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: validEmail,
            Password: "ValidPassword123!"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new LoginPartnerStepOneCommand(
            Email: "invalid-email",
            Password: "123" // Too short
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}