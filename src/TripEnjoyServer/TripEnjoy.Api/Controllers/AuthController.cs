using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class and configures its dependencies.
        /// </summary>
        public AuthController(ISender sender, ILogger<AuthController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account with email, password, and optional full name.
        /// </summary>
        /// <param name="command">The user registration command containing email, password, and optional full name.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the user registration operation.</returns>
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser(RegisterUserCommand command)
        {
            _logger.LogInformation("Attempting to register user with email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "User registration successful");
        }

        /// <summary>
        /// Registers a new partner account with email, password, and business information.
        /// </summary>
        /// <param name="command">The partner registration command containing email, password, and business details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the partner registration operation.</returns>
        [HttpPost("register-partner")]
        public async Task<IActionResult> RegisterPartner(RegisterPartnerCommand command)
        {
            _logger.LogInformation("Attempting to register partner {CompanyName} with email {Email}",
                command.CompanyName, command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "Partner registration successful");
        }

        /// <summary>
        /// Initiates the first step of the login flow specifically for user accounts by validating role and processing credentials.
        /// Only accounts with the "User" role can use this endpoint.
        /// </summary>
        /// <param name="command">Command containing user credentials for the first login step.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the user login step one operation.</returns>
        [HttpPost("login-user-step-one")]
        public async Task<IActionResult> LoginUserStepOne(LoginUserStepOneCommand command)
        {
            _logger.LogInformation("Attempting user login step one for user with email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "User login step one successful");
        }

        /// <summary>
        /// Initiates the first step of the login flow specifically for partner accounts by validating role and processing credentials.
        /// Only accounts with the "Partner" role can use this endpoint.
        /// </summary>
        /// <param name="command">Command containing partner credentials for the first login step.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the partner login step one operation.</returns>
        [HttpPost("login-partner-step-one")]
        public async Task<IActionResult> LoginPartnerStepOne(LoginPartnerStepOneCommand command)
        {
            _logger.LogInformation("Attempting partner login step one for partner with email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "Partner login step one successful");
        }

        /// <summary>
        /// Completes the second step of the login flow (e.g., MFA verification): forwards the provided <see cref="LoginStepTwoCommand"/> to the application layer and returns the standardized HTTP result.
        /// </summary>
        /// <param name="command">The second-step login command (bound from the request body) containing verification data required to complete authentication (e.g., one-time code).</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome returned by the command handler.</returns>
        [HttpPost("login-step-two")]
        public async Task<IActionResult> LoginStepTwo(LoginStepTwoCommand command)
        {
            _logger.LogInformation("Attempting login step two for user with email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "Login step two successful");
        }

        /// <summary>
        /// Confirms a user's email address using the provided user identifier and confirmation token.
        /// </summary>
        /// <param name="userId">The identifier of the user whose email is being confirmed (from query string).</param>
        /// <param name="token">The email confirmation token issued to the user (from query string).</param>
        /// <returns>An <see cref="IActionResult"/> that wraps the command result indicating success or failure of the email confirmation.</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token, [FromQuery] string confirmFor)
        {
            _logger.LogInformation("Attempting to confirm email for user ID {UserId}", userId);
            var command = new ConfirmEmailCommand(userId, token, confirmFor);
            var result = await _sender.Send(command);
            return HandleResult(result, "Confirm email successful");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} is attempting to logout", aspNetUserId);
            if (string.IsNullOrEmpty(aspNetUserId))
            {
                _logger.LogWarning("Logout failed for an unauthenticated user.");
                return HandleResult(Result.Failure(
                    new Error("Logout.Failure", "Unauthorized", ErrorType.Unauthorized)
                ), "Logout failed");
            }

            var secureCommand = new LogoutCommand
            (
                refreshToken,
                aspNetUserId
            );
            var result = await _sender.Send(secureCommand);
            return HandleResult(result, "Logout successful");
        }

        /// <summary>
        /// Exchanges a valid refresh token for a new access (and refresh) token pair.
        /// </summary>
        /// <param name="command">The RefreshTokenCommand containing the refresh token and any required metadata.</param>
        /// <returns>An IActionResult representing the operation outcome: on success returns tokens, on failure returns an error result.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            _logger.LogInformation("Attempting to refresh token");
            var result = await _sender.Send(command);
            return HandleResult(result, "Refresh token successful");
        }

        /// <summary>
        /// Resends the One-Time Password (OTP) to the user's registered email address.
        /// </summary>
        /// <param name="command">The command containing the user's email.</param>
        /// <returns>An IActionResult indicating the outcome of the operation.</returns>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp(ResendLoginOtpCommand command)
        {
            _logger.LogInformation("Resending OTP for user with email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "OTP resent successfully");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
        {
            _logger.LogInformation("Forgot password request for email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "If an account with this email exists, a password reset link has been sent.");
        }

        [HttpPost("verify-password-reset-otp")]
        public async Task<IActionResult> VerifyPasswordResetOtp(VerifyPasswordResetOtpCommand command)
        {
            _logger.LogInformation("Verifying password reset OTP for email {Email}", command.Email);
            var result = await _sender.Send(command);
            return HandleResult(result, "OTP verified successfully. Please reset your password.");
        }
    }
}
