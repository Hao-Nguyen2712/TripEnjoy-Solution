using MediatR;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.Enums;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class GetPendingPartnerApprovalsQueryHandler : IRequestHandler<GetPendingPartnerApprovalsQuery, Result<IEnumerable<PartnerApprovalDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingPartnerApprovalsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PartnerApprovalDto>>> Handle(GetPendingPartnerApprovalsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Repository<Domain.Account.Account>().GetAllAsync();
        var partners = accounts
            .Where(a => a.Partner != null && a.Partner.Status == PartnerStatusEnum.Pending.ToString())
            .ToList();

        var partnerDtos = partners.Select(a => new PartnerApprovalDto
        {
            Id = a.Partner!.Id.Id,
            Email = a.AccountEmail,
            BusinessName = a.Partner.CompanyName ?? string.Empty,
            Status = a.Partner.Status,
            RequestedAt = a.CreatedAt,
            Documents = a.Partner.PartnerDocuments.Select(d => new PartnerDocumentDto
            {
                Id = d.Id.Id,
                DocumentType = d.DocumentType,
                DocumentUrl = d.DocumentUrl,
                Status = d.Status,
                UploadedAt = d.CreatedAt
            }).ToList()
        });

        return Result<IEnumerable<PartnerApprovalDto>>.Success(partnerDtos);
    }
}
