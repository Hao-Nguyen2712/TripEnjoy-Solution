using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record LoginStepOneCommand(
        string Email,
        string Password
    ) : IRequest<Result>;
}
