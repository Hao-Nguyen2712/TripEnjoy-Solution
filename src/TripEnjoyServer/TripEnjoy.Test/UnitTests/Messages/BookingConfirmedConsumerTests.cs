using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using Xunit;

namespace TripEnjoy.Test.UnitTests.Messages;

public class BookingConfirmedConsumerTests
{
    [Fact]
    public async Task Consume_ValidBookingConfirmedEvent_ProcessesSuccessfully()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingConfirmedConsumer>();
            })
            .AddLogging()
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
