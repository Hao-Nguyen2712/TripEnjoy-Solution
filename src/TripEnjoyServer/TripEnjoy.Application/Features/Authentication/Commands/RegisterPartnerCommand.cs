using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record RegisterPartnerCommand(
        string Email,
        string Password,
        string CompanyName,
        string? ContactNumber = null,
        string? Address = null
    ) : IAuditableCommand<Result<AccountId>>;
}