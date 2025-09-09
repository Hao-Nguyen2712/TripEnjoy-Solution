using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record ConfirmEmailCommand(
         string UserId,
         string Token
     ) : IRequest<Result>;
}