using MediatR;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Queries;

public record GetAllUsersQuery() : IRequest<Result<IEnumerable<UserManagementDto>>>;
