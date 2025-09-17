using MediatR;
using System.Collections.Generic;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Queries;

public record GetMyPropertiesQuery() : IRequest<Result<IEnumerable<PropertyDto>>>;
