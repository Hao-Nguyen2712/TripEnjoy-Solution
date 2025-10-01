using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Partner.Commands;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Constant;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Partner.Handlers;

public class GenerateDocumentUploadUrlCommandHandler : IRequestHandler<GenerateDocumentUploadUrlCommand, Result<DocumentUploadUrlDto>>
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateDocumentUploadUrlCommandHandler(
        ICloudinaryService cloudinaryService,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DocumentUploadUrlDto>> Handle(GenerateDocumentUploadUrlCommand request, CancellationToken cancellationToken)
    {
        // Extract AccountId from JWT claims
        var accountIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("AccountId");
        if (!Guid.TryParse(accountIdClaim, out var accountIdGuid))
        {
            return Result<DocumentUploadUrlDto>.Failure(DomainError.Authentication.Unauthorized);
        }

        // Validate account exists and has partner profile
        var accountId = AccountId.Create(accountIdGuid);
        var account = await _unitOfWork.AccountRepository.GetByAccountIdAsync(accountId);
        if (account is null)
        {
            return Result<DocumentUploadUrlDto>.Failure(DomainError.Account.NotFound);
        }

        if (account.Partner is null)
        {
            return Result<DocumentUploadUrlDto>.Failure(
                new Error("Account.NoPartnerProfile", "This account does not have a partner profile.", ErrorType.NotFound));
        }

        // Validate document type
        if (!DocumentTypeConstant.IsValidDocumentType(request.DocumentType))
        {
            return Result<DocumentUploadUrlDto>.Failure(DomainError.Partner.InvalidDocumentType);
        }

        // Check for duplicate document type
        if (account.Partner.PartnerDocuments.Any(d =>
            d.DocumentType.Equals(request.DocumentType, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<DocumentUploadUrlDto>.Failure(DomainError.Partner.DuplicateDocumentType);
        }

        // Generate secure upload parameters
        var uploadParams = await _cloudinaryService.GenerateSignedUploadParametersAsync(
            $"partner_documents/{accountIdGuid}/{request.DocumentType}",
            request.FileName,
            cancellationToken);

        return Result<DocumentUploadUrlDto>.Success(uploadParams);
    }
}