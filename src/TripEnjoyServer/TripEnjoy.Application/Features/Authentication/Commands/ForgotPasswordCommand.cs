using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result>;
}
