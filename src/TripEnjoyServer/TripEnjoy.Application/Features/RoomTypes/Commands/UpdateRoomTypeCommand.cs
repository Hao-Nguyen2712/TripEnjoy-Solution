using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Application.Common.Interfaces;

namespace TripEnjoy.Application.Features.RoomTypes.Commands;

public record UpdateRoomTypeCommand(
    Guid RoomTypeId,
    string RoomTypeName,
    int Capacity,
    decimal BasePrice,
    int TotalQuantity,
    string? Description) : IAuditableCommand<Result>;
