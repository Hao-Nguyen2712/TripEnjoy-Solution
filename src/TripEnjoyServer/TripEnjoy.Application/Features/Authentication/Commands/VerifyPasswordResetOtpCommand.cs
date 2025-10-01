using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record VerifyPasswordResetOtpCommand(string Email, string Otp) : IAuditableCommand<Result<string>>;
}
