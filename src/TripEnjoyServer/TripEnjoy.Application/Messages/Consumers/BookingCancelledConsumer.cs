using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingCancelled events
/// Handles async processing after a booking is cancelled
/// </summary>
public class BookingCancelledConsumer : IConsumer<IBookingCancelledEvent>
{
    private readonly ILogger<BookingCancelledConsumer> _logger;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public BookingCancelledConsumer(
        ILogger<BookingCancelledConsumer> logger,
        IEmailService emailService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _emailService = emailService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IBookingCancelledEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingCancelled event for BookingId: {BookingId}, Reason: {Reason}",
            message.BookingId, message.CancellationReason ?? "Not specified");

        try
        {
            // 1. Get user and property details for email
            var accountRepository = _unitOfWork.Repository<Domain.Account.Account>();
            var propertyRepository = _unitOfWork.Properties;
            
            var account = await accountRepository.GetAsync(a => a.User != null && a.User.Id.Id == message.UserId);
            var property = await propertyRepository.GetByIdAsync(message.PropertyId);
            
            if (account != null && property != null)
            {
                // 2. Send cancellation notification email
                var emailSubject = "Booking Cancelled - " + property.Name;
                var emailBody = BuildCancellationEmailBody(message, property.Name);
                
                var emailResult = await _emailService.SendEmailConfirmationAsync(
                    account.AccountEmail,
                    emailSubject,
                    emailBody,
                    context.CancellationToken);
                
                if (emailResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Cancellation notification email sent to {Email} for BookingId: {BookingId}",
                        account.AccountEmail, message.BookingId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send cancellation notification email for BookingId: {BookingId}",
                        message.BookingId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Could not find user or property for BookingId: {BookingId}",
                    message.BookingId);
            }
            
            // 3. Invalidate room availability cache to release rooms
            var cacheKey = $"room-availability:{message.PropertyId}";
            await _cacheService.RemoveByPrefixAsync(cacheKey, context.CancellationToken);
            
            _logger.LogInformation(
                "Released room availability cache for PropertyId: {PropertyId}",
                message.PropertyId);
            
            // 4. Log refund requirement (future: trigger refund processing)
            _logger.LogInformation(
                "Refund processing may be required for BookingId: {BookingId}",
                message.BookingId);
            
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
    
    private string BuildCancellationEmailBody(IBookingCancelledEvent booking, string propertyName)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #f44336;'>Booking Cancelled</h2>
                <p>Your booking has been cancelled as requested.</p>
                
                <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #f44336;'>
                    <h3 style='margin-top: 0; color: #f44336;'>Cancellation Details</h3>
                    <p><strong>Property:</strong> {propertyName}</p>
                    <p><strong>Booking ID:</strong> {booking.BookingId}</p>
                    <p><strong>Cancelled At:</strong> {booking.CancelledAt:MMMM dd, yyyy HH:mm} UTC</p>
                    {(!string.IsNullOrEmpty(booking.CancellationReason) ? $"<p><strong>Reason:</strong> {booking.CancellationReason}</p>" : "")}
                </div>
                
                <p><strong>What Happens Next?</strong></p>
                <ul>
                    <li>The rooms associated with this booking are now available for other guests</li>
                    <li>If you made a payment, refund processing will be handled according to the cancellation policy</li>
                    <li>You can make a new booking anytime on TripEnjoy</li>
                </ul>
                
                <p>We hope to serve you again soon!</p>
                
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    &copy; {DateTime.UtcNow.Year} TripEnjoy. All rights reserved.
                </p>
            </div>";
    }
}
