using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.RoomTypes.Queries;

public record GetRoomTypesByPropertyQuery(Guid PropertyId) : IRequest<Result<List<RoomTypeDto>>>;
