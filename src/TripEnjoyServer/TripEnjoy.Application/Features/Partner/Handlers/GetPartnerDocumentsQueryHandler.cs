using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Partner.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Partner.Handlers;

public class GetPartnerDocumentsQueryHandler : IRequestHandler<GetPartnerDocumentsQuery, Result<PagedList<PartnerDocumentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetPartnerDocumentsQueryHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<PagedList<PartnerDocumentDto>>> Handle(GetPartnerDocumentsQuery request, CancellationToken cancellationToken)
    {
        // Extract AccountId from JWT claims
        var accountIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("AccountId");
        if (!Guid.TryParse(accountIdClaim, out var accountIdGuid))
        {
            return Result<PagedList<PartnerDocumentDto>>.Failure(DomainError.Authentication.Unauthorized);
        }

        var accountId = AccountId.Create(accountIdGuid);
        var account = await _unitOfWork.AccountRepository.GetByAccountIdAsync(accountId);
        
        if (account?.Partner is null)
        {
            return Result<PagedList<PartnerDocumentDto>>.Failure(
                new Error("Partner.NotFound", "Partner profile not found.", ErrorType.NotFound));
        }

        // Get paginated documents ordered by latest submission date (CreatedAt descending)
        var partnerId = account.Partner.Id;
        var (documents, totalCount) = await _unitOfWork.PartnerDocuments.GetDocumentsByPartnerIdPaginatedAsync(
            partnerId, request.PageNumber, request.PageSize, cancellationToken);

        var documentDtos = documents.Select(d => new PartnerDocumentDto
        {
            Id = d.Id.Id,
            DocumentType = d.DocumentType,
            DocumentUrl = d.DocumentUrl,
            Status = d.Status,
            CreatedAt = d.CreatedAt,
            ReviewedAt = d.ReviewedAt,
            DocumentTypeName = GetDocumentTypeName(d.DocumentType),
            StatusDisplayName = GetStatusDisplayName(d.Status)
        }).ToList();

        var pagedList = new PagedList<PartnerDocumentDto>(documentDtos, totalCount, request.PageNumber, request.PageSize);
        return Result<PagedList<PartnerDocumentDto>>.Success(pagedList);
    }

    private static string GetDocumentTypeName(string documentType) => documentType switch
    {
        "BusinessLicense" => "Business License",
        "TaxIdentification" => "Tax Identification",
        "ProofOfAddress" => "Proof of Address",
        "CompanyRegistration" => "Company Registration",
        "BankStatement" => "Bank Statement",
        "IdentityDocument" => "Identity Document",
        _ => documentType
    };

    private static string GetStatusDisplayName(string status) => status switch
    {
        "PendingReview" => "Pending Review",
        "Approved" => "Approved",
        "Rejected" => "Rejected",
        _ => status
    };
}