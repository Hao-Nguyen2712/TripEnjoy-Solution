using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record VerifyPasswordResetOtpCommand(string Email, string Otp) : IRequest<Result<string>>;
}
