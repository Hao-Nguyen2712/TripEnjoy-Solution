using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyType.Commands;

public record CreatePropertyTypeCommand(string Name) : IRequest<Result<PropertyTypeId>>;
