using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Application.Messages.Contracts;
using TripEnjoy.Application.Messages.Events;
using Xunit;

namespace TripEnjoy.Test.UnitTests.Messages;

public class BookingCreatedConsumerTests
{
    [Fact]
    public async Task Consume_ValidBookingCreatedEvent_LogsSuccessfully()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCreatedConsumer>();
            })
            .AddLogging()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var bookingCreatedEvent = new BookingCreatedEvent
        {
            BookingId = bookingId,
            UserId = userId,
            PropertyId = propertyId,
            CheckInDate = DateTime.UtcNow.AddDays(7),
            CheckOutDate = DateTime.UtcNow.AddDays(10),
            NumberOfGuests = 2,
            TotalPrice = 500.00m,
            SpecialRequests = "Late check-in",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await harness.Bus.Publish<IBookingCreatedEvent>(bookingCreatedEvent);

        // Assert
        Assert.True(await harness.Consumed.Any<IBookingCreatedEvent>());

        var consumerHarness = harness.GetConsumerHarness<BookingCreatedConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<IBookingCreatedEvent>());
    }

    [Fact]
    public async Task Consume_BookingCreatedEvent_ProcessesMessage()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<BookingCreatedConsumer>();
            })
            .AddLogging()
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var bookingCreatedEvent = new BookingCreatedEvent
        {
            BookingId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            CheckInDate = DateTime.UtcNow.AddDays(1),
            CheckOutDate = DateTime.UtcNow.AddDays(3),
            NumberOfGuests = 4,
            TotalPrice = 1200.00m,
            SpecialRequests = null,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await harness.Bus.Publish<IBookingCreatedEvent>(bookingCreatedEvent);

        // Assert - Wait for the message to be consumed
        Assert.True(await harness.Consumed.Any<IBookingCreatedEvent>());

        var consumerHarness = harness.GetConsumerHarness<BookingCreatedConsumer>();
        Assert.True(await consumerHarness.Consumed.Any<IBookingCreatedEvent>());
        
        // Verify no faults occurred
        Assert.False(await harness.Consumed.Any<Fault<IBookingCreatedEvent>>());
    }
}
