using FluentAssertions;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Voucher.Enums;
using VoucherEntity = TripEnjoy.Domain.Voucher.Voucher;

namespace TripEnjoy.Test.UnitTests.Domain.Voucher;

public class VoucherTests
{
    [Fact]
    public void Create_WithValidPercentDiscount_ShouldReturnSuccessResult()
    {
        // Arrange
        var code = "SUMMER2024";
        var discountType = VoucherDiscountTypeEnum.Percent;
        var discountValue = 20m;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(30);
        var creatorType = VoucherCreatorTypeEnum.Admin;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            code,
            discountType,
            discountValue,
            startDate,
            endDate,
            creatorType,
            creatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Code.Should().Be(code.ToUpperInvariant());
        result.Value.DiscountType.Should().Be(discountType.ToString());
        result.Value.DiscountValue.Should().Be(discountValue);
        result.Value.StartDate.Should().Be(startDate);
        result.Value.EndDate.Should().Be(endDate);
        result.Value.Status.Should().Be(VoucherStatusEnum.Active.ToString());
        result.Value.UsedCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithValidAmountDiscount_ShouldReturnSuccessResult()
    {
        // Arrange
        var code = "SAVE50";
        var discountType = VoucherDiscountTypeEnum.Amount;
        var discountValue = 50m;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(30);
        var creatorType = VoucherCreatorTypeEnum.Partner;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            code,
            discountType,
            discountValue,
            startDate,
            endDate,
            creatorType,
            creatorId,
            description: "Save $50 on your next booking");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("Save $50 on your next booking");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidCode_ShouldReturnFailure(string invalidCode)
    {
        // Arrange
        var discountType = VoucherDiscountTypeEnum.Percent;
        var discountValue = 20m;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(30);
        var creatorType = VoucherCreatorTypeEnum.Admin;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            invalidCode,
            discountType,
            discountValue,
            startDate,
            endDate,
            creatorType,
            creatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.InvalidCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(150)]
    public void Create_WithInvalidPercentValue_ShouldReturnFailure(decimal invalidPercent)
    {
        // Arrange
        var code = "INVALID";
        var discountType = VoucherDiscountTypeEnum.Percent;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(30);
        var creatorType = VoucherCreatorTypeEnum.Admin;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            code,
            discountType,
            invalidPercent,
            startDate,
            endDate,
            creatorType,
            creatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.InvalidDiscount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidAmountValue_ShouldReturnFailure(decimal invalidAmount)
    {
        // Arrange
        var code = "INVALID";
        var discountType = VoucherDiscountTypeEnum.Amount;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(30);
        var creatorType = VoucherCreatorTypeEnum.Admin;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            code,
            discountType,
            invalidAmount,
            startDate,
            endDate,
            creatorType,
            creatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.InvalidDiscount);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldReturnFailure()
    {
        // Arrange
        var code = "TEST";
        var discountType = VoucherDiscountTypeEnum.Percent;
        var discountValue = 20m;
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = DateTime.UtcNow; // Before start date
        var creatorType = VoucherCreatorTypeEnum.Admin;
        var creatorId = AccountId.CreateUnique();

        // Act
        var result = VoucherEntity.Create(
            code,
            discountType,
            discountValue,
            startDate,
            endDate,
            creatorType,
            creatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code.Contains("InvalidDateRange"));
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var voucher = CreateValidVoucher();
        var newDescription = "Updated description";
        var newUsageLimit = 100;

        // Act
        var result = voucher.Update(
            description: newDescription,
            usageLimit: newUsageLimit);

        // Assert
        result.IsSuccess.Should().BeTrue();
        voucher.Description.Should().Be(newDescription);
        voucher.UsageLimit.Should().Be(newUsageLimit);
        voucher.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_DisabledVoucher_ShouldReturnFailure()
    {
        // Arrange
        var voucher = CreateValidVoucher();
        voucher.Disable();

        // Act
        var result = voucher.Update(description: "New description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.Disabled);
    }

    [Fact]
    public void Disable_ActiveVoucher_ShouldDisableSuccessfully()
    {
        // Arrange
        var voucher = CreateValidVoucher();

        // Act
        var result = voucher.Disable();

        // Assert
        result.IsSuccess.Should().BeTrue();
        voucher.Status.Should().Be(VoucherStatusEnum.Disabled.ToString());
    }

    [Fact]
    public void Enable_ExpiredVoucher_ShouldReturnFailure()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1); // Expired
        var voucher = CreateValidVoucher(startDate, endDate);
        voucher.Disable();

        // Act
        var result = voucher.Enable();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.Expired);
    }

    [Fact]
    public void IncrementUsageCount_BelowLimit_ShouldIncrement()
    {
        // Arrange
        var voucher = CreateValidVoucher(usageLimit: 10);
        var initialCount = voucher.UsedCount;

        // Act
        var result = voucher.IncrementUsageCount();

        // Assert
        result.IsSuccess.Should().BeTrue();
        voucher.UsedCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void IncrementUsageCount_AtLimit_ShouldReturnFailure()
    {
        // Arrange
        var usageLimit = 5;
        var voucher = CreateValidVoucher(usageLimit: usageLimit);

        // Increment to limit
        for (int i = 0; i < usageLimit; i++)
        {
            voucher.IncrementUsageCount();
        }

        // Act
        var result = voucher.IncrementUsageCount();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.UsageLimitReached);
    }

    [Fact]
    public void IsActive_WithinDateRangeAndActive_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);
        var voucher = CreateValidVoucher(startDate, endDate);

        // Act
        var isActive = voucher.IsActive();

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_BeforeStartDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var voucher = CreateValidVoucher(startDate, endDate);

        // Act
        var isActive = voucher.IsActive();

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_AfterEndDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);
        var voucher = CreateValidVoucher(startDate, endDate);

        // Act
        var isActive = voucher.IsActive();

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void ValidateForUse_DisabledVoucher_ShouldReturnFailure()
    {
        // Arrange
        var voucher = CreateValidVoucher();
        voucher.Disable();

        // Act
        var result = voucher.ValidateForUse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.Disabled);
    }

    [Fact]
    public void ValidateForUse_BeforeStartDate_ShouldReturnFailure()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var voucher = CreateValidVoucher(startDate, endDate);

        // Act
        var result = voucher.ValidateForUse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.NotStarted);
    }

    [Fact]
    public void ValidateForUse_AfterEndDate_ShouldReturnFailure()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);
        var voucher = CreateValidVoucher(startDate, endDate);

        // Act
        var result = voucher.ValidateForUse();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainError.Voucher.Expired);
    }

    [Fact]
    public void CalculateDiscount_PercentType_ShouldCalculateCorrectly()
    {
        // Arrange
        var voucher = CreateValidVoucher(
            discountType: VoucherDiscountTypeEnum.Percent,
            discountValue: 20m);
        var orderAmount = 100m;

        // Act
        var discount = voucher.CalculateDiscount(orderAmount);

        // Assert
        discount.Should().Be(20m); // 20% of 100
    }

    [Fact]
    public void CalculateDiscount_PercentTypeWithMaxCap_ShouldCapAtMaximum()
    {
        // Arrange
        var voucher = CreateValidVoucher(
            discountType: VoucherDiscountTypeEnum.Percent,
            discountValue: 50m,
            maximumDiscountAmount: 30m);
        var orderAmount = 100m;

        // Act
        var discount = voucher.CalculateDiscount(orderAmount);

        // Assert
        discount.Should().Be(30m); // Capped at maximum
    }

    [Fact]
    public void CalculateDiscount_AmountType_ShouldReturnFixedAmount()
    {
        // Arrange
        var voucher = CreateValidVoucher(
            discountType: VoucherDiscountTypeEnum.Amount,
            discountValue: 25m);
        var orderAmount = 100m;

        // Act
        var discount = voucher.CalculateDiscount(orderAmount);

        // Assert
        discount.Should().Be(25m);
    }

    [Fact]
    public void CalculateDiscount_AmountTypeExceedsOrder_ShouldCapAtOrderAmount()
    {
        // Arrange
        var voucher = CreateValidVoucher(
            discountType: VoucherDiscountTypeEnum.Amount,
            discountValue: 150m);
        var orderAmount = 100m;

        // Act
        var discount = voucher.CalculateDiscount(orderAmount);

        // Assert
        discount.Should().Be(100m); // Cannot exceed order amount
    }

    // Helper method
    private VoucherEntity CreateValidVoucher(
        DateTime? startDate = null,
        DateTime? endDate = null,
        VoucherDiscountTypeEnum? discountType = null,
        decimal? discountValue = null,
        int? usageLimit = null,
        decimal? maximumDiscountAmount = null)
    {
        var result = VoucherEntity.Create(
            "TESTCODE",
            discountType ?? VoucherDiscountTypeEnum.Percent,
            discountValue ?? 20m,
            startDate ?? DateTime.UtcNow,
            endDate ?? DateTime.UtcNow.AddDays(30),
            VoucherCreatorTypeEnum.Admin,
            AccountId.CreateUnique(),
            usageLimit: usageLimit,
            maximumDiscountAmount: maximumDiscountAmount);

        return result.Value;
    }
}
