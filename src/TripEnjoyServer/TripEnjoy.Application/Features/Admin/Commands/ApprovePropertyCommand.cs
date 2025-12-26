using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Commands;

public record ApprovePropertyCommand(Guid PropertyId) : IRequest<Result>;
