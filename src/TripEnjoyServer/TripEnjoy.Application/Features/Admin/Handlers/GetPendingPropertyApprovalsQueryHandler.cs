using MediatR;
using Microsoft.EntityFrameworkCore;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.Enums;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetPendingPropertyApprovalsQueryHandler : IRequestHandler<GetPendingPropertyApprovalsQuery, Result<IEnumerable<PropertyApprovalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingPropertyApprovalsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PropertyApprovalDto>>> Handle(GetPendingPropertyApprovalsQuery request, CancellationToken cancellationToken)
    {
        var properties = await _unitOfWork.Repository<Domain.Property.Property>()
            .GetQueryable()
            .Include(p => p.Partner)
                .ThenInclude(p => p.Account)
            .Include(p => p.PropertyType)
            .Where(p => p.Status == PropertyEnum.WaitingForApproval)
            .ToListAsync(cancellationToken);

        var propertyDtos = properties.Select(p => new PropertyApprovalDto
        {
            Id = p.Id.Id,
            Name = p.Name,
            PartnerEmail = p.Partner?.Account?.AccountEmail ?? string.Empty,
            Status = p.Status,
            SubmittedAt = p.CreatedAt,
            Address = $"{p.Address}, {p.City}, {p.Country}",
            PropertyTypeName = p.PropertyType?.Name ?? string.Empty
        });

        return Result<IEnumerable<PropertyApprovalDto>>.Success(propertyDtos);
    }
}
