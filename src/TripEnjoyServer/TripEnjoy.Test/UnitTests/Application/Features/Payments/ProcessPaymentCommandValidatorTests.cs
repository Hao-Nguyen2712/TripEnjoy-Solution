using TripEnjoy.Application.Features.Payments.Commands;
using TripEnjoy.Application.Features.Payments.Validators;

namespace TripEnjoy.Test.UnitTests.Application.Features.Payments;

public class ProcessPaymentCommandValidatorTests
{
    private readonly ProcessPaymentCommandValidator _validator;

    public ProcessPaymentCommandValidatorTests()
    {
        _validator = new ProcessPaymentCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            "VNPay",
            "https://localhost/payment/return");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyBookingId_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.Empty,
            "VNPay",
            "https://localhost/payment/return");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BookingId");
    }

    [Fact]
    public void Validate_WithEmptyPaymentMethod_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            "",
            "https://localhost/payment/return");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PaymentMethod");
    }

    [Fact]
    public void Validate_WithInvalidPaymentMethod_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            "InvalidMethod",
            "https://localhost/payment/return");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "PaymentMethod" && 
            e.ErrorMessage.Contains("Invalid payment method"));
    }

    [Fact]
    public void Validate_WithEmptyReturnUrl_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            "VNPay",
            "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ReturnUrl");
    }

    [Fact]
    public void Validate_WithInvalidReturnUrl_ShouldFail()
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            "VNPay",
            "not-a-valid-url");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "ReturnUrl" && 
            e.ErrorMessage.Contains("valid URL"));
    }

    [Theory]
    [InlineData("VNPay")]
    [InlineData("Momo")]
    [InlineData("ZaloPay")]
    [InlineData("BankTransfer")]
    public void Validate_WithValidPaymentMethods_ShouldPass(string paymentMethod)
    {
        // Arrange
        var command = new ProcessPaymentCommand(
            Guid.NewGuid(),
            paymentMethod,
            "https://localhost/payment/return");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
