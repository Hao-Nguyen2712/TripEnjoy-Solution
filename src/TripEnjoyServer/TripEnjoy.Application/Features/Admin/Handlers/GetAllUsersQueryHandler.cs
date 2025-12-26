using MediatR;
using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserManagementDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<UserManagementDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Repository<Domain.Account.Account>()
            .GetQueryable()
            .Include(a => a.User)
            .Include(a => a.Partner)
            .Where(a => a.User != null)
            .ToListAsync(cancellationToken);

        var userDtos = accounts.Select(a => new UserManagementDto
        {
            Id = a.Id.Id,
            Email = a.AccountEmail,
            FirstName = a.User?.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
            LastName = a.User?.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
            Role = "User",
            IsActive = a.Status == Domain.Account.Enums.AccountStatusEnum.Active.ToString(),
            CreatedAt = a.CreatedAt,
            LastLoginAt = null
        });

        return Result<IEnumerable<UserManagementDto>>.Success(userDtos);
    }
}
