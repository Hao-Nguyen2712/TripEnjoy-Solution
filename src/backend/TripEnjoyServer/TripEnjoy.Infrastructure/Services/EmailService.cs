using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Email;
using Microsoft.Extensions.Logging;


namespace TripEnjoy.Infrastructure.Services
{
    public class EmailService : IEmailService
    {

        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <remarks>
        /// Reads SMTP settings from the provided options and stores the logger for use by the service.
        /// </remarks>
        public EmailService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends an HTML confirmation email to the specified recipient using the configured SMTP settings.
        /// </summary>
        /// <param name="sendFor">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="body">Content inserted into the HTML email body (e.g., an OTP or confirmation text).</param>
        /// <param name="cancellationToken">Cancellation token to cancel the send operation.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success, or a failure result containing an <see cref="Error"/>
        /// with code "Email.SendFailed" if sending the message fails.
        /// </returns>
        public async Task<Result> SendEmailConfirmationAsync(string sendFor, string subject, string body, CancellationToken cancellationToken = default)
        {
            var email = _emailConfig.Email;
            var password = _emailConfig.Password;
            var host = _emailConfig.Host;
            var port = _emailConfig.Port;

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(email, password);

            var bodyEmail = MailBodyForOTP(body);
            var message = new MailMessage(
                email!, sendFor, subject, bodyEmail
            )
            {
                IsBodyHtml = true,
            };

            try
            {
                await smtpClient.SendMailAsync(message, cancellationToken);
                return Result.Success();
            }
            catch
            {
                return Result.Failure(new Error("Email.SendFailed", "Failed to send email.", ErrorType.Failure));
            }
        }

        /// <summary>
        /// Simulates sending an OTP email by logging the recipient, subject, and body; does not send network requests.
        /// </summary>
        /// <param name="userEmail">The recipient email address that would receive the OTP (used only for logging).</param>
        /// <param name="otp">The one-time password value included in the logged message.</param>
        /// <returns>A completed <see cref="Task"/>; no asynchronous work is performed.</returns>
        public Task SendOtpAsync(string userEmail, string otp)
        {
            _logger.LogWarning("--- SENDING OTP EMAIL (SIMULATION) ---");
            _logger.LogInformation($"To: {userEmail}");
            _logger.LogInformation($"Subject: Your One-Time Password");
            _logger.LogInformation($"Body: Your OTP is: {otp}. It is valid for 3 minutes.");
            _logger.LogWarning("------------------------------------");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds an HTML-formatted email body for sending a one-time password (OTP).
        /// </summary>
        /// <param name="otp">The OTP value to embed in the message; must be a short-lived code (the template indicates 5-minute validity).</param>
        /// <returns>An HTML string containing styled content with the provided OTP and the current year in the footer.</returns>
        private string MailBodyForOTP(string otp)
        {
            return $@"
 <html>
     <head>
         <style>
             .container {{
                 max-width: 600px;
                 margin: auto;
                 padding: 20px;
                 border: 1px solid #e0e0e0;
                 border-radius: 10px;
                 font-family: Arial, sans-serif;
                 background-color: #f9f9f9;
                 box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
             }}
             .header {{
                 background-color: #4CAF50;
                 color: white;
                 padding: 10px 20px;
                 border-top-left-radius: 10px;
                 border-top-right-radius: 10px;
                 text-align: center;
             }}
             .content {{
                 padding: 20px;
                 text-align: center;
             }}
             .otp {{
                 font-size: 28px;
                 font-weight: bold;
                 color: #4CAF50;
                 margin: 20px 0;
             }}
             .footer {{
                 text-align: center;
                 font-size: 12px;
                 color: #777;
                 margin-top: 16px;
             }}
         </style>
     </head>
     <body>
         <div class='container'>
             <div class='header'>
                 <h2>OTP Verification </h2>
             </div>
             <div class='content'>
                 <p>Thank you for using our service!</p>
                 <p>Please use the following One-Time Password (OTP) to continue:</p>
                 <div class='otp'>{otp}</div>
                 <p>This OTP is valid for 5 minutes. Please do not share it with anyone.</p>
             </div>
             <div class='footer'>
                 &copy; {DateTime.Now.Year} TraVinhGo. All rights reserved.
             </div>
         </div>
     </body>
 </html>";
        }

        /// <summary>
        /// Sends an email confirmation to the specified address.
        /// </summary>
        /// <param name="userId">Identifier of the user the confirmation is for.</param>
        /// <param name="token">Confirmation token to include in the message.</param>
        /// <param name="ct">Cancellation token to cancel the send operation.</param>
        /// <returns>A task that completes with a <see cref="Result"/> indicating success or failure of the send.</returns>
        Task IEmailService.SendEmailConfirmationAsync(string userEmail, string userId, string token, CancellationToken ct)
        {
            return SendEmailConfirmationAsync(userEmail, userId, token, ct);
        }
    }
}
