using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using TripEnjoy.Domain.Common.Models;
using Xunit;
using AccountEntity = TripEnjoy.Domain.Account.Account;

namespace TripEnjoy.Test.UnitTests.Messages;

public class BookingCancelledConsumerTests
{
    [Fact]
    public async Task Consume_ValidBookingCancelledEvent_ProcessesSuccessfully()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockCacheService = new Mock<ICacheService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAccountRepository = new Mock<IGenericRepository<AccountEntity>>();
        var mockPropertyRepository = new Mock<IPropertyRepository>();

        mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        mockUnitOfWork.Setup(x => x.Repository<AccountEntity>())
            .Returns(mockAccountRepository.Object);
        mockUnitOfWork.Setup(x => x.Properties)
            .Returns(mockPropertyRepository.Object);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCancelledConsumer>();
            })
            .AddLogging()
            .AddSingleton(mockEmailService.Object)
            .AddSingleton(mockCacheService.Object)
            .AddSingleton(mockUnitOfWork.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var bookingCancelledEvent = new BookingCancelledEvent
        {
            BookingId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            CancellationReason = "Change of plans",
            CancelledAt = DateTime.UtcNow
        };

        // Act
        await harness.Bus.Publish<IBookingCancelledEvent>(bookingCancelledEvent);

        // Assert
        Assert.True(await harness.Consumed.Any<IBookingCancelledEvent>());

        var consumerHarness = harness.GetConsumerHarness<BookingCancelledConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<IBookingCancelledEvent>());
        
        // Verify no faults occurred
        Assert.False(await harness.Consumed.Any<Fault<IBookingCancelledEvent>>());
    }

    [Fact]
    public async Task Consume_BookingCancelledEventWithoutReason_ProcessesSuccessfully()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var mockCacheService = new Mock<ICacheService>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockAccountRepository = new Mock<IGenericRepository<AccountEntity>>();
        var mockPropertyRepository = new Mock<IPropertyRepository>();

        mockEmailService.Setup(x => x.SendEmailConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        mockUnitOfWork.Setup(x => x.Repository<AccountEntity>())
            .Returns(mockAccountRepository.Object);
        mockUnitOfWork.Setup(x => x.Properties)
            .Returns(mockPropertyRepository.Object);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCancelledConsumer>();
            })
            .AddLogging()
            .AddSingleton(mockEmailService.Object)
            .AddSingleton(mockCacheService.Object)
            .AddSingleton(mockUnitOfWork.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var bookingCancelledEvent = new BookingCancelledEvent
        {
            BookingId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            CancellationReason = null,
            CancelledAt = DateTime.UtcNow
        };

        // Act
        await harness.Bus.Publish<IBookingCancelledEvent>(bookingCancelledEvent);

        // Assert
        Assert.True(await harness.Consumed.Any<IBookingCancelledEvent>());

        var consumerHarness = harness.GetConsumerHarness<BookingCancelledConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<IBookingCancelledEvent>());
    }
}
