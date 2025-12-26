using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record BlockUserCommand(Guid UserId, string Reason) : IRequest<Result>;
