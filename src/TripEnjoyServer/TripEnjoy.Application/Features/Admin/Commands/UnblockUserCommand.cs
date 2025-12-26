using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record UnblockUserCommand(Guid UserId) : IRequest<Result>;
