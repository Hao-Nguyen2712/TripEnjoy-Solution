using TripEnjoy.Domain.Booking.Entities;
using TripEnjoy.Domain.Booking.Enums;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Test.UnitTests.Domain.Booking;

public class PaymentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var amount = 1000m;
        var paymentMethod = PaymentMethodEnum.VNPay;

        // Act
        var result = Payment.Create(bookingId, amount, paymentMethod);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.BookingId.Should().Be(bookingId);
        result.Value.Amount.Should().Be(amount);
        result.Value.PaymentMethod.Should().Be(paymentMethod);
        result.Value.Status.Should().Be(PaymentStatusEnum.Pending);
        result.Value.TransactionId.Should().BeNull();
        result.Value.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var amount = 0m;
        var paymentMethod = PaymentMethodEnum.VNPay;

        // Act
        var result = Payment.Create(bookingId, amount, paymentMethod);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.InvalidAmount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var amount = -100m;
        var paymentMethod = PaymentMethodEnum.VNPay;

        // Act
        var result = Payment.Create(bookingId, amount, paymentMethod);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.InvalidAmount);
    }

    [Fact]
    public void MarkAsProcessing_WhenPending_ShouldSucceed()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;

        // Act
        var result = payment.MarkAsProcessing();

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatusEnum.Processing);
    }

    [Fact]
    public void MarkAsProcessing_WhenNotPending_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();
        payment.MarkAsSuccess("TXN123");

        // Act
        var result = payment.MarkAsProcessing();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.InvalidStatusTransition);
    }

    [Fact]
    public void MarkAsSuccess_WithValidTransactionId_ShouldSucceed()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();
        var transactionId = "TXN123456";

        // Act
        var result = payment.MarkAsSuccess(transactionId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatusEnum.Success);
        payment.TransactionId.Should().Be(transactionId);
        payment.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsSuccess_WhenNotProcessing_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;

        // Act
        var result = payment.MarkAsSuccess("TXN123");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.InvalidStatusTransition);
    }

    [Fact]
    public void MarkAsSuccess_WithEmptyTransactionId_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();

        // Act
        var result = payment.MarkAsSuccess("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.InvalidTransactionId);
    }

    [Fact]
    public void MarkAsFailed_WhenNotCompleted_ShouldSucceed()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();

        // Act
        var result = payment.MarkAsFailed();

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatusEnum.Failed);
    }

    [Fact]
    public void MarkAsFailed_WhenSuccess_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();
        payment.MarkAsSuccess("TXN123");

        // Act
        var result = payment.MarkAsFailed();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.CannotFailCompletedPayment);
    }

    [Fact]
    public void MarkAsRefunded_WhenSuccess_ShouldSucceed()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();
        payment.MarkAsSuccess("TXN123");

        // Act
        var result = payment.MarkAsRefunded();

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatusEnum.Refunded);
    }

    [Fact]
    public void MarkAsRefunded_WhenNotSuccess_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;

        // Act
        var result = payment.MarkAsRefunded();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.CanOnlyRefundSuccessfulPayment);
    }

    [Fact]
    public void Cancel_WhenPending_ShouldSucceed()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;

        // Act
        var result = payment.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatusEnum.Cancelled);
    }

    [Fact]
    public void Cancel_WhenSuccess_ShouldReturnFailure()
    {
        // Arrange
        var bookingId = BookingId.CreateUnique();
        var payment = Payment.Create(bookingId, 1000m, PaymentMethodEnum.VNPay).Value;
        payment.MarkAsProcessing();
        payment.MarkAsSuccess("TXN123");

        // Act
        var result = payment.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Payment.CannotCancelCompletedPayment);
    }
}
