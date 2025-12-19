using TripEnjoy.Domain.Account.Entities;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;

namespace TripEnjoy.Test.UnitTests.Domain.Account;

public class SettlementTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow.AddDays(-30);
        var periodEnd = DateTime.UtcNow;
        var totalAmount = 1000.00m;
        var commissionAmount = 100.00m;

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.WalletId.Should().Be(walletId);
        result.Value.PeriodStart.Should().Be(periodStart);
        result.Value.PeriodEnd.Should().Be(periodEnd);
        result.Value.TotalAmount.Should().Be(totalAmount);
        result.Value.CommissionAmount.Should().Be(commissionAmount);
        result.Value.NetAmount.Should().Be(900.00m);
        result.Value.Status.Should().Be(SettlementStatusEnum.Pending.ToString());
        result.Value.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidPeriod_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow;
        var periodEnd = DateTime.UtcNow.AddDays(-30); // End before start
        var totalAmount = 1000.00m;
        var commissionAmount = 100.00m;

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.InvalidPeriod);
    }

    [Fact]
    public void Create_WithZeroTotalAmount_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow.AddDays(-30);
        var periodEnd = DateTime.UtcNow;
        var totalAmount = 0m;
        var commissionAmount = 0m;

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.InvalidAmount);
    }

    [Fact]
    public void Create_WithNegativeTotalAmount_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow.AddDays(-30);
        var periodEnd = DateTime.UtcNow;
        var totalAmount = -100.00m;
        var commissionAmount = 10.00m;

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.InvalidAmount);
    }

    [Fact]
    public void Create_WithNegativeCommission_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow.AddDays(-30);
        var periodEnd = DateTime.UtcNow;
        var totalAmount = 1000.00m;
        var commissionAmount = -50.00m;

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.InvalidCommission);
    }

    [Fact]
    public void Create_WithCommissionExceedingTotal_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var periodStart = DateTime.UtcNow.AddDays(-30);
        var periodEnd = DateTime.UtcNow;
        var totalAmount = 1000.00m;
        var commissionAmount = 1500.00m; // More than total

        // Act
        var result = Settlement.Create(walletId, periodStart, periodEnd, totalAmount, commissionAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.InvalidCommission);
    }

    [Fact]
    public void Process_WhenPending_ShouldChangeStatusToProcessing()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;

        // Act
        var result = settlement.Process();

        // Assert
        result.IsSuccess.Should().BeTrue();
        settlement.Status.Should().Be(SettlementStatusEnum.Processing.ToString());
    }

    [Fact]
    public void Complete_WhenProcessing_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;
        settlement.Process();

        // Act
        var result = settlement.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        settlement.Status.Should().Be(SettlementStatusEnum.Completed.ToString());
        settlement.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;
        settlement.Complete();

        // Act
        var result = settlement.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.AlreadyProcessed);
    }

    [Fact]
    public void Fail_WhenProcessing_ShouldChangeStatusToFailed()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;
        settlement.Process();

        // Act
        var result = settlement.Fail();

        // Assert
        result.IsSuccess.Should().BeTrue();
        settlement.Status.Should().Be(SettlementStatusEnum.Failed.ToString());
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;

        // Act
        var result = settlement.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        settlement.Status.Should().Be(SettlementStatusEnum.Cancelled.ToString());
    }

    [Fact]
    public void Cancel_WhenProcessing_ShouldReturnFailure()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var settlementResult = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            1000.00m,
            100.00m);
        var settlement = settlementResult.Value;
        settlement.Process();

        // Act
        var result = settlement.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Settlement.CannotCancel);
    }

    [Fact]
    public void NetAmount_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var walletId = WalletId.CreateUnique();
        var totalAmount = 5000.00m;
        var commissionAmount = 500.00m;
        var expectedNetAmount = 4500.00m;

        // Act
        var result = Settlement.Create(
            walletId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            totalAmount,
            commissionAmount);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NetAmount.Should().Be(expectedNetAmount);
    }
}
