using MediatR;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record SetCoverPropertyImageCommand(
    Guid PropertyId,
    Guid ImageId) : IAuditableCommand<Result>;
