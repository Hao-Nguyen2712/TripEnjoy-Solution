using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record RejectPropertyCommand(Guid PropertyId, string Reason) : IRequest<Result>;
