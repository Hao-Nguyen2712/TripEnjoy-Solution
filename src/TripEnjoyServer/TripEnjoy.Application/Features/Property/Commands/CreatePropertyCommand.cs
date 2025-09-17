using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.Property.Commands;

public record CreatePropertyCommand(
    Guid PropertyTypeId,
    string Name,
    string Address,
    string City,
    string Country,
    string? Description,
    double? Latitude,
    double? Longitude) : IRequest<Result<PropertyId>>;