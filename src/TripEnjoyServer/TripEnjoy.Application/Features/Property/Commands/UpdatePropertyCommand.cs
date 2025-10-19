using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Application.Common.Interfaces;

namespace TripEnjoy.Application.Features.Property.Commands;

public record UpdatePropertyCommand(
    Guid PropertyId,
    Guid PropertyTypeId,
    string Name,
    string Address,
    string City,
    string Country,
    string? Description,
    double? Latitude,
    double? Longitude) : IAuditableCommand<Result>;