using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingConfirmed events
/// Handles async processing after a booking is confirmed
/// </summary>
public class BookingConfirmedConsumer : IConsumer<IBookingConfirmedEvent>
{
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(ILogger<BookingConfirmedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBookingConfirmedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingConfirmed event for BookingId: {BookingId}, UserId: {UserId}",
            message.BookingId, message.UserId);

        try
        {
            // TODO: Add async processing logic here:
            // 1. Send confirmation email with booking details
            // 2. Update property availability
            // 3. Create partner notification
            // 4. Process payment settlement
            
            await Task.CompletedTask;
            
            _logger.LogInformation(
                "Successfully processed BookingConfirmed event for BookingId: {BookingId}",
                message.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing BookingConfirmed event for BookingId: {BookingId}",
                message.BookingId);
            throw;
        }
    }
}
