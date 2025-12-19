using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room.ValueObjects;
using TripEnjoy.Application.Common.Interfaces;

namespace TripEnjoy.Application.Features.RoomTypes.Commands;

public record CreateRoomTypeCommand(
    Guid PropertyId,
    string RoomTypeName,
    int Capacity,
    decimal BasePrice,
    int TotalQuantity,
    string? Description) : IAuditableCommand<Result<RoomTypeId>>;
