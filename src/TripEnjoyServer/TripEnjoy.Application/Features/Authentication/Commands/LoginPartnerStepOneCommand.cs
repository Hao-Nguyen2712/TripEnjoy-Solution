using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    /// <summary>
    /// Command for initiating partner login step one with role validation.
    /// Only accounts with the "Partner" role can use this command.
    /// </summary>
    public record LoginPartnerStepOneCommand(
        string Email,
        string Password
    ) : IRequest<Result>;
}