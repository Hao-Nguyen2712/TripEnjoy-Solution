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

        public EmailService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
        }

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

        public Task SendOtpAsync(string userEmail, string otp)
        {
            _logger.LogWarning("--- SENDING OTP EMAIL (SIMULATION) ---");
            _logger.LogInformation($"To: {userEmail}");
            _logger.LogInformation($"Subject: Your One-Time Password");
            _logger.LogInformation($"Body: Your OTP is: {otp}. It is valid for 3 minutes.");
            _logger.LogWarning("------------------------------------");

            return Task.CompletedTask;
        }

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

        Task IEmailService.SendEmailConfirmationAsync(string userEmail, string userId, string token, CancellationToken ct)
        {
            return SendEmailConfirmationAsync(userEmail, userId, token, ct);
        }
    }
}
