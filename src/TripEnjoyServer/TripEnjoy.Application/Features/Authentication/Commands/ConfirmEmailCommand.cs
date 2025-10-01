using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record ConfirmEmailCommand(
         string UserId,
         string Token,
         string ConfirmFor
     ) : IAuditableCommand<Result>;
}