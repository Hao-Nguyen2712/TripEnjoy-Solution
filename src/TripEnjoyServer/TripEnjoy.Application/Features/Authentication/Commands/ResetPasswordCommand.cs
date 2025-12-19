using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record ResetPasswordCommand(string Email, string ResetToken, string NewPassword) : IAuditableCommand<Result>;
}
