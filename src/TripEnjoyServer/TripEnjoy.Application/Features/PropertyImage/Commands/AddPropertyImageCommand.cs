using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Commands;

public record AddPropertyImageCommand(
    Guid PropertyId,
    string ImageUrl,
    bool IsCover) : IRequest<Result<PropertyImageId>>;
