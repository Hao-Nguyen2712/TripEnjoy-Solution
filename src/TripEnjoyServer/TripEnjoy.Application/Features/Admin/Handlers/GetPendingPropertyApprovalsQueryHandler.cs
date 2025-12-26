using MediatR;
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
        var properties = await _unitOfWork.Repository<Domain.Property.Property>().GetAllAsync();
        var pendingProperties = properties
            .Where(p => p.Status == PropertyEnum.WaitingForApproval)
            .ToList();

        var propertyDtos = pendingProperties.Select(p => new PropertyApprovalDto
        {
            Id = p.Id.Id,
            Name = p.Name,
            PartnerEmail = string.Empty, // Will need to query separately
            Status = p.Status,
            SubmittedAt = p.CreatedAt,
            Address = $"{p.Address}, {p.City}, {p.Country}",
            PropertyTypeName = string.Empty // Will need to query separately
        });

        return Result<IEnumerable<PropertyApprovalDto>>.Success(propertyDtos);
    }
}
