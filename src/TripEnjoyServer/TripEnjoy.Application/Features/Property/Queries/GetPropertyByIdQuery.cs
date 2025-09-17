using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Queries;

public record GetPropertyByIdQuery(Guid PropertyId) : IRequest<Result<PropertyDto>>;
