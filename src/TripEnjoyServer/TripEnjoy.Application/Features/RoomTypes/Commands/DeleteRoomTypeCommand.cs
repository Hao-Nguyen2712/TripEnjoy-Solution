using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Application.Common.Interfaces;

namespace TripEnjoy.Application.Features.RoomTypes.Commands;

public record DeleteRoomTypeCommand(Guid RoomTypeId) : IAuditableCommand<Result>;
