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

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        [HttpPost("login-step-one")]
        public async Task<IActionResult> LoginStepOne(LoginStepOneCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        [HttpPost("login-step-two")]
        public async Task<IActionResult> LoginStepTwo(LoginStepTwoCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {

            var command = new ConfirmEmailCommand(userId, token);
            var result = await _sender.Send(command);
            return HandleResult(result);
        }

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
        
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await _sender.Send(command);
            return HandleResult(result);
        }
    }
}
