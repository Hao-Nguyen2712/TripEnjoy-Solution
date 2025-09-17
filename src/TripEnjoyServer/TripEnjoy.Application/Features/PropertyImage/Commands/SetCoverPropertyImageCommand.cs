using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record SetCoverPropertyImageCommand(
    Guid PropertyId,
    Guid ImageId) : IRequest<Result>;
