using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.RoomTypes.Queries;

public record GetRoomTypeByIdQuery(Guid RoomTypeId) : IRequest<Result<RoomTypeDto>>;
