using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingCancelled events
/// Handles async processing after a booking is cancelled
/// </summary>
public class BookingCancelledConsumer : IConsumer<IBookingCancelledEvent>
{
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(ILogger<BookingCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IBookingCancelledEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingCancelled event for BookingId: {BookingId}, Reason: {Reason}",
            message.BookingId, message.CancellationReason ?? "Not specified");

        try
        {
            // TODO: Add async processing logic here:
            // 1. Send cancellation notification email
            // 2. Release room availability
            // 3. Process refund if applicable
            // 4. Update partner dashboard
            
            await Task.CompletedTask;
            
            _logger.LogInformation(
                "Successfully processed BookingCancelled event for BookingId: {BookingId}",
                message.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing BookingCancelled event for BookingId: {BookingId}",
                message.BookingId);
            throw;
        }
    }
}
