using MediatR;
using System.Collections.Generic;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.PropertyType.Queries;

public record GetAllPropertyTypesQuery() : IRequest<Result<IEnumerable<PropertyTypeDto>>>;
