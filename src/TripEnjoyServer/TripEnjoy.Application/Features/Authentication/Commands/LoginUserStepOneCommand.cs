using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    /// <summary>
    /// Command for initiating user login step one with role validation.
    /// Only accounts with the "User" role can use this command.
    /// </summary>
    public record LoginUserStepOneCommand(
        string Email,
        string Password
    ) : IRequest<Result>;
}