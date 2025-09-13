using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record LoginStepTwoCommand(
        string Email,
        string Otp
    ) : IRequest<Result<AuthResultDTO>>;
}
