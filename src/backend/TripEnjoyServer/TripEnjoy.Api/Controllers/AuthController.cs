using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class and configures its dependencies.
        /// </summary>
        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Registers a new user using the provided registration command.
        /// </summary>
        /// <param name="command">The registration command containing the user's signup details.</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the registration operation.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Initiates the first step of the login flow by processing the provided LoginStepOneCommand.
        /// </summary>
        /// <param name="command">Command containing credentials or identifying information required for the first login step (e.g., username or identifier).</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome of the command processing (success or failure payload translated to an HTTP response).</returns>
        [HttpPost("login-step-one")]
        public async Task<IActionResult> LoginStepOne(LoginStepOneCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Completes the second step of the login flow (e.g., MFA verification): forwards the provided <see cref="LoginStepTwoCommand"/> to the application layer and returns the standardized HTTP result.
        /// </summary>
        /// <param name="command">The second-step login command (bound from the request body) containing verification data required to complete authentication (e.g., one-time code).</param>
        /// <returns>An <see cref="IActionResult"/> representing the outcome returned by the command handler.</returns>
        [HttpPost("login-step-two")]
        public async Task<IActionResult> LoginStepTwo(LoginStepTwoCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Confirms a user's email address using the provided user identifier and confirmation token.
        /// </summary>
        /// <param name="userId">The identifier of the user whose email is being confirmed (from query string).</param>
        /// <param name="token">The email confirmation token issued to the user (from query string).</param>
        /// <returns>An <see cref="IActionResult"/> that wraps the command result indicating success or failure of the email confirmation.</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {

            var command = new ConfirmEmailCommand(userId, token);
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Logs out the currently authenticated user by revoking the provided refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke for the current user.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the outcome produced by the logout command:
        /// - 401/Unauthorized result if the current user's identifier cannot be determined.
        /// - The command handler's result (success or failure) otherwise.
        /// </returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var aspNetUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(aspNetUserId))
            {
                return HandleResult(Result.Failure(
                    new Error("Logout.Failure", "Unauthorized", ErrorType.Unauthorized)
                ));
            }

            var secureCommand = new LogoutCommand
            (
                refreshToken,
                aspNetUserId
            );
            var result = await _sender.Send(secureCommand);
            return HandleResult(result);
        }
        
        /// <summary>
        /// Exchanges a valid refresh token for a new access (and refresh) token pair.
        /// </summary>
        /// <param name="command">The RefreshTokenCommand containing the refresh token and any required metadata.</param>
        /// <returns>An IActionResult representing the operation outcome: on success returns tokens, on failure returns an error result.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }
    }
}
