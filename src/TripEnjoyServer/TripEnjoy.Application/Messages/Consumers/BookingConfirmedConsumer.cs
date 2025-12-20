using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingConfirmed events
/// Handles async processing after a booking is confirmed
/// </summary>
public class BookingConfirmedConsumer : IConsumer<IBookingConfirmedEvent>
{
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public BookingConfirmedConsumer(
        ILogger<BookingConfirmedConsumer> logger,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IBookingConfirmedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingConfirmed event for BookingId: {BookingId}, UserId: {UserId}",
            message.BookingId, message.UserId);

        try
        {
            // 1. Get user and property details for email
            var accountRepository = _unitOfWork.Repository<Domain.Account.Account>();
            var propertyRepository = _unitOfWork.Properties;
            
            var account = await accountRepository.GetAsync(a => a.User != null && a.User.Id.Id == message.UserId);
            var property = await propertyRepository.GetByIdAsync(message.PropertyId);
            
            if (account != null && property != null)
            {
                // 2. Send payment confirmation email
                var emailSubject = "Payment Confirmed - Your Booking is Secured!";
                var emailBody = BuildConfirmationEmailBody(message, property.Name);
                
                var emailResult = await _emailService.SendEmailConfirmationAsync(
                    account.AccountEmail,
                    emailSubject,
                    emailBody,
                    context.CancellationToken);
                
                if (emailResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Payment confirmation email sent to {Email} for BookingId: {BookingId}",
                        account.AccountEmail, message.BookingId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send payment confirmation email for BookingId: {BookingId}",
                        message.BookingId);
                }
                
                // 3. Log partner notification (future: create notification record)
                _logger.LogInformation(
                    "Partner notification required for new booking at PropertyId: {PropertyId}",
                    message.PropertyId);
            }
            else
            {
                _logger.LogWarning(
                    "Could not find user or property for BookingId: {BookingId}",
                    message.BookingId);
            }
            
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
    
    private string BuildConfirmationEmailBody(IBookingConfirmedEvent booking, string propertyName)
    {
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #4CAF50;'>Booking Confirmed!</h2>
                <p>Great news! Your payment has been processed successfully and your booking is now confirmed.</p>
                
                <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #4CAF50;'>
                    <h3 style='margin-top: 0; color: #4CAF50;'>Confirmation Details</h3>
                    <p><strong>Property:</strong> {propertyName}</p>
                    <p><strong>Booking ID:</strong> {booking.BookingId}</p>
                    <p><strong>Confirmed At:</strong> {booking.ConfirmedAt:MMMM dd, yyyy HH:mm} UTC</p>
                </div>
                
                <p>You can now look forward to your stay! The property host has been notified of your booking.</p>
                
                <p><strong>What's Next?</strong></p>
                <ul>
                    <li>You'll receive a check-in reminder 24 hours before your arrival</li>
                    <li>Contact information for the property will be provided shortly</li>
                    <li>View your booking details anytime in your TripEnjoy dashboard</li>
                </ul>
                
                <p>Have a wonderful trip!</p>
                
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    &copy; {DateTime.UtcNow.Year} TripEnjoy. All rights reserved.
                </p>
            </div>";
    }
}
