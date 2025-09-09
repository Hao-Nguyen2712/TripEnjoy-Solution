using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Interfaces.External.Email
{
        public interface IEmailService
        {
                /// <summary>
                /// Sends an account email confirmation message to the specified recipient containing the provided confirmation token.
                /// </summary>
                /// <param name="userEmail">Recipient's email address.</param>
                /// <param name="userId">Identifier of the user the confirmation is for.</param>
                /// <param name="token">Confirmation token to include in the email (e.g., used to verify the account).</param>
                /// <param name="ct">Cancellation token to cancel the asynchronous operation.</param>
                /// <returns>A task that represents the asynchronous send operation.</returns>
                Task SendEmailConfirmationAsync(string userEmail, string userId, string token, CancellationToken ct);
                /// <summary>
                /// Asynchronously sends a one-time password (OTP) to the specified email address.
                /// </summary>
                /// <param name="userEmail">The recipient's email address.</param>
                /// <param name="otp">The one-time password to deliver in the email.</param>
                /// <returns>A task that represents the asynchronous send operation.</returns>
                Task SendOtpAsync(string userEmail, string otp);
        }
}