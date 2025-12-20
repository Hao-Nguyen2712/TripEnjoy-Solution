using MassTransit;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Contracts;

namespace TripEnjoy.Application.Messages.Consumers;

/// <summary>
/// Consumer for BookingCreated events
/// Handles async processing after a booking is created
/// </summary>
public class BookingCreatedConsumer : IConsumer<IBookingCreatedEvent>
{
    private readonly ILogger<BookingCreatedConsumer> _logger;
    private readonly IEmailService _emailService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;

    public BookingCreatedConsumer(
        ILogger<BookingCreatedConsumer> logger,
        IEmailService emailService,
        ICacheService cacheService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _emailService = emailService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<IBookingCreatedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Processing BookingCreated event for BookingId: {BookingId}, UserId: {UserId}, Property: {PropertyId}",
            message.BookingId, message.UserId, message.PropertyId);

        try
        {
            // 1. Get user and property details for email
            var accountRepository = _unitOfWork.Repository<Domain.Account.Account>();
            var propertyRepository = _unitOfWork.Properties;
            
            var account = await accountRepository.GetAsync(a => a.User != null && a.User.Id.Id == message.UserId);
            var property = await propertyRepository.GetByIdAsync(message.PropertyId);
            
            if (account != null && property != null)
            {
                // 2. Send booking confirmation email
                var emailSubject = "Booking Confirmation - Your Stay at " + property.Name;
                var emailBody = BuildBookingConfirmationEmailBody(message, property.Name);
                
                var emailResult = await _emailService.SendEmailConfirmationAsync(
                    account.AccountEmail,
                    emailSubject,
                    emailBody,
                    context.CancellationToken);
                
                if (emailResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Booking confirmation email sent to {Email} for BookingId: {BookingId}",
                        account.AccountEmail, message.BookingId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send booking confirmation email for BookingId: {BookingId}",
                        message.BookingId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Could not find user or property for BookingId: {BookingId}",
                    message.BookingId);
            }
            
            // 3. Invalidate room availability cache to reflect updated availability
            var cacheKey = $"room-availability:{message.PropertyId}";
            await _cacheService.RemoveByPrefixAsync(cacheKey, context.CancellationToken);
            
            _logger.LogInformation(
                "Invalidated room availability cache for PropertyId: {PropertyId}",
                message.PropertyId);
            
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
    
    private string BuildBookingConfirmationEmailBody(IBookingCreatedEvent booking, string propertyName)
    {
        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
        
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: #2196F3;'>Booking Confirmation</h2>
                <p>Thank you for your booking! Your reservation has been created and is pending confirmation.</p>
                
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0;'>Booking Details</h3>
                    <p><strong>Property:</strong> {propertyName}</p>
                    <p><strong>Check-in:</strong> {booking.CheckInDate:MMMM dd, yyyy}</p>
                    <p><strong>Check-out:</strong> {booking.CheckOutDate:MMMM dd, yyyy}</p>
                    <p><strong>Nights:</strong> {nights}</p>
                    <p><strong>Guests:</strong> {booking.NumberOfGuests}</p>
                    <p><strong>Total Price:</strong> ${booking.TotalPrice:N2}</p>
                    {(!string.IsNullOrEmpty(booking.SpecialRequests) ? $"<p><strong>Special Requests:</strong> {booking.SpecialRequests}</p>" : "")}
                </div>
                
                <p>Please complete your payment to confirm this reservation. We look forward to hosting you!</p>
                
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    &copy; {DateTime.UtcNow.Year} TripEnjoy. All rights reserved.
                </p>
            </div>";
    }
}
