using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Booking.ValueObjects;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Test.UnitTests.Domain.Account;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var amount = 100.00m;
        var type = TransactionTypeEnum.Payment;
        var description = "Booking payment";

        // Act
        var result = Transaction.Create(walletId, amount, type, null, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.WalletId.Should().Be(walletId);
        result.Value.Amount.Should().Be(amount);
        result.Value.Type.Should().Be(type.ToString());
        result.Value.Status.Should().Be(TransactionStatusEnum.Pending.ToString());
        result.Value.Description.Should().Be(description);
        result.Value.BookingId.Should().BeNull();
    }

    [Fact]
    public void Create_WithBookingId_ShouldReturnSuccessResult()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var bookingId = BookingId.CreateUnique();
        var amount = 250.50m;
        var type = TransactionTypeEnum.Payment;

        // Act
        var result = Transaction.Create(walletId, amount, type, bookingId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BookingId.Should().Be(bookingId);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var amount = 0m;
        var type = TransactionTypeEnum.Payment;

        // Act
        var result = Transaction.Create(walletId, amount, type);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Transaction.InvalidAmount);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnSuccessResult()
    {
        // Arrange (negative amounts are allowed for debits/refunds)
        var walletId = WalletId.CreateUnique();
        var amount = -50.00m;
        var type = TransactionTypeEnum.Refund;

        // Act
        var result = Transaction.Create(walletId, amount, type);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(amount);
    }

    [Fact]
    public void Complete_WhenPending_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;

        // Act
        var result = transaction.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatusEnum.Completed.ToString());
        transaction.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;
        transaction.Complete();

        // Act
        var result = transaction.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Transaction.AlreadyCompleted);
    }

    [Fact]
    public void Fail_WhenPending_ShouldChangeStatusToFailed()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;

        // Act
        var result = transaction.Fail();

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatusEnum.Failed.ToString());
    }

    [Fact]
    public void Fail_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;
        transaction.Complete();

        // Act
        var result = transaction.Fail();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Transaction.AlreadyCompleted);
    }

    [Fact]
    public void Reverse_WhenCompleted_ShouldChangeStatusToReversed()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;
        transaction.Complete();

        // Act
        var result = transaction.Reverse();

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.Status.Should().Be(TransactionStatusEnum.Reversed.ToString());
    }

    [Fact]
    public void Reverse_WhenNotCompleted_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var transactionResult = Transaction.Create(walletId, 100m, TransactionTypeEnum.Payment);
        var transaction = transactionResult.Value;

        // Act
        var result = transaction.Reverse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Transaction.CannotReverse);
    }

    [Fact]
    public void Create_WithDifferentTransactionTypes_ShouldReturnSuccess()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var types = new[] 
        { 
            TransactionTypeEnum.Payment,
            TransactionTypeEnum.Refund,
            TransactionTypeEnum.Settlement,
            TransactionTypeEnum.Commission,
            TransactionTypeEnum.Deposit,
            TransactionTypeEnum.Withdrawal
        };

        // Act & Assert
        foreach (var type in types)
        {
            var result = Transaction.Create(walletId, 100m, type);
            result.IsSuccess.Should().BeTrue();
            result.Value.Type.Should().Be(type.ToString());
        }
    }
}
