using MediatR;
using Microsoft.AspNetCore.Mvc;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
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
    }
}
