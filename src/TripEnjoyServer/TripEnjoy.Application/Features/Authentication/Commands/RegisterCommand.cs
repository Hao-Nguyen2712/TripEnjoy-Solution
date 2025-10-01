using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    /// <summary>
    /// Legacy unified registration command. 
    /// Use RegisterUserCommand or RegisterPartnerCommand instead for better type safety and validation.
    /// </summary>
    [Obsolete("Use RegisterUserCommand or RegisterPartnerCommand instead. This will be removed in a future version.", false)]
    public record RegisterCommand(
        string email,
        string password,
        string? confirmFor) : IAuditableCommand<Result<AccountId>>;
}