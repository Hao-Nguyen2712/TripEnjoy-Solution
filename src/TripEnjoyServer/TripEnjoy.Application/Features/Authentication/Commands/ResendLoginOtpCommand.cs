using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record ResendLoginOtpCommand(string Email) : IRequest<Result>;
}
