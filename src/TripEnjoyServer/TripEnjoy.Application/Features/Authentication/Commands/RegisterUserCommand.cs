using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record RegisterUserCommand(
        string Email,
        string Password,
        string? FullName = null
    ) : IAuditableCommand<Result<AccountId>>;
}