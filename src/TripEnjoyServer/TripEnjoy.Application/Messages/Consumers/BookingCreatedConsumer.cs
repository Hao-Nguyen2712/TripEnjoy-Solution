using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingCreated events
/// Handles async processing after a booking is created
/// </summary>
public class BookingCreatedConsumer : IConsumer<IBookingCreatedEvent>
{
    private readonly ILogger<BookingCreatedConsumer> _logger;

    public BookingCreatedConsumer(ILogger<BookingCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBookingCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingCreated event for BookingId: {BookingId}, UserId: {UserId}, Property: {PropertyId}",
            message.BookingId, message.UserId, message.PropertyId);

        try
        {
            // TODO: Add async processing logic here:
            // 1. Send booking confirmation email
            // 2. Update room availability
            // 3. Create notifications
            // 4. Trigger any business workflows
            
            await Task.CompletedTask;
            
            _logger.LogInformation(
                "Successfully processed BookingCreated event for BookingId: {BookingId}",
                message.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing BookingCreated event for BookingId: {BookingId}",
                message.BookingId);
            throw;
        }
    }
}
