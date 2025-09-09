using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Interfaces.External.Email
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string userEmail, string userId, string token, CancellationToken ct);
        Task SendOtpAsync(string userEmail, string otp);
    }
}