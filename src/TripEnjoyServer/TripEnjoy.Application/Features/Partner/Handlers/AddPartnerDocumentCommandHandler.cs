using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Partner.Commands;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Partner.Handlers;

public class AddPartnerDocumentCommandHandler : IRequestHandler<AddPartnerDocumentCommand, Result<PartnerDocumentId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICloudinaryService _cloudinaryService;

    public AddPartnerDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<PartnerDocumentId>> Handle(AddPartnerDocumentCommand request, CancellationToken cancellationToken)
    {
        // Extract AccountId from JWT claims
        var accountIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("AccountId");
        if (!Guid.TryParse(accountIdClaim, out var accountIdGuid))
        {
            return Result<PartnerDocumentId>.Failure(DomainError.Authentication.Unauthorized);
        }

        // Validate uploaded file exists in Cloudinary
        var isValidUpload = await _cloudinaryService.ValidateUploadedFileAsync(
            request.PublicId,
            request.Signature,
            request.Timestamp,
            cancellationToken);

        if (!isValidUpload)
        {
            return Result<PartnerDocumentId>.Failure(
                new Error("Partner.InvalidUpload", "File upload could not be validated.", ErrorType.Validation));
        }

        // Get account and validate partner profile exists
        var accountId = AccountId.Create(accountIdGuid);
        var account = await _unitOfWork.AccountRepository.GetByAccountIdAsync(accountId);
        if (account is null)
        {
            return Result<PartnerDocumentId>.Failure(DomainError.Account.NotFound);
        }

        // Add document through domain method
        var addDocumentResult = account.AddPartnerDocument(request.DocumentType, request.DocumentUrl);
        if (addDocumentResult.IsFailure)
        {
            return Result<PartnerDocumentId>.Failure(addDocumentResult.Errors);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the newly created document ID
        var newDocumentId = account.Partner!.PartnerDocuments
            .Where(d => d.DocumentType == request.DocumentType)
            .OrderByDescending(d => d.CreatedAt)
            .First().Id;

        return Result<PartnerDocumentId>.Success(newDocumentId);
    }
}
