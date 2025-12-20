using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using Xunit;

namespace TripEnjoy.Test.UnitTests.Messages;

public class BookingCancelledConsumerTests
{
    [Fact]
    public async Task Consume_ValidBookingCancelledEvent_ProcessesSuccessfully()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCancelledConsumer>();
            })
            .AddLogging()
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
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCancelledConsumer>();
            })
            .AddLogging()
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
