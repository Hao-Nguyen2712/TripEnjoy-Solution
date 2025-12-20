using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using TripEnjoy.Domain.Common.Models;
using Xunit;
using AccountEntity = TripEnjoy.Domain.Account.Account;

namespace TripEnjoy.Test.UnitTests.Messages;

public class BookingConfirmedConsumerTests
{
    [Fact]
    public async Task Consume_ValidBookingConfirmedEvent_ProcessesSuccessfully()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
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
                x.AddConsumer<BookingConfirmedConsumer>();
            })
            .AddLogging()
            .AddSingleton(mockEmailService.Object)
            .AddSingleton(mockUnitOfWork.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var bookingConfirmedEvent = new BookingConfirmedEvent
        {
            BookingId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            ConfirmedAt = DateTime.UtcNow
        };

        // Act
        await harness.Bus.Publish<IBookingConfirmedEvent>(bookingConfirmedEvent);

        // Assert
        Assert.True(await harness.Consumed.Any<IBookingConfirmedEvent>());

        var consumerHarness = harness.GetConsumerHarness<BookingConfirmedConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<IBookingConfirmedEvent>());
        
        // Verify no faults occurred
        Assert.False(await harness.Consumed.Any<Fault<IBookingConfirmedEvent>>());
    }
}
