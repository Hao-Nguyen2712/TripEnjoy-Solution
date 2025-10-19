using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record DeletePropertyImageCommand(
    Guid PropertyId,
    Guid ImageId) : IAuditableCommand<Result>;
