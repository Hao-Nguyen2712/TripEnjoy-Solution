using MediatR;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Commands
{
    public record RegisterCommand(string email, string password , string? confirmFor) : IRequest<Result<AccountId>>;
}