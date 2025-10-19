using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.PropertyImage.Handlers;

public class GeneratePhotoUploadUrlCommandHandler : IRequestHandler<GeneratePhotoUploadUrlCommand, Result<PhotoUploadUrlDto>>
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public GeneratePhotoUploadUrlCommandHandler(
        ICloudinaryService cloudinaryService,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _cloudinaryService = cloudinaryService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PhotoUploadUrlDto>> Handle(GeneratePhotoUploadUrlCommand request, CancellationToken cancellationToken)
    {
        // Extract AccountId from JWT claims
        var accountIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("AccountId");
        if (!Guid.TryParse(accountIdClaim, out var accountIdGuid))
        {
            return Result<PhotoUploadUrlDto>.Failure(DomainError.Authentication.Unauthorized);
        }

        // Validate account exists and has partner profile
        var accountId = AccountId.Create(accountIdGuid);
        var account = await _unitOfWork.AccountRepository.GetByAccountIdAsync(accountId);
        if (account is null)
        {
            return Result<PhotoUploadUrlDto>.Failure(DomainError.Account.NotFound);
        }

        if (account.Partner is null)
        {
            return Result<PhotoUploadUrlDto>.Failure(
                new Error("Account.NoPartnerProfile", "This account does not have a partner profile.", ErrorType.NotFound));
        }

        // Validate property exists and belongs to the partner
        var propertyId = PropertyId.Create(request.PropertyId);
        var property = await _unitOfWork.Properties.GetByIdWithImagesAsync(propertyId);
        if (property is null)
        {
            return Result<PhotoUploadUrlDto>.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId != account.Partner.Id)
        {
            return Result<PhotoUploadUrlDto>.Failure(
                new Error("Property.AccessDenied", "You do not have permission to add images to this property.", ErrorType.Forbidden));
        }

        // Validate file extension for images
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return Result<PhotoUploadUrlDto>.Failure(
                new Error("PropertyImage.InvalidFileType",
                    $"Invalid file type. Allowed extensions: {string.Join(", ", allowedExtensions)}",
                    ErrorType.Validation));
        }

        // Generate secure upload parameters for images
        var uploadParams = await _cloudinaryService.GenerateSignedUploadParametersAsync(
            $"property_images/{request.PropertyId}",
            request.FileName,
            cancellationToken);

        // Convert DocumentUploadUrlDto to PhotoUploadUrlDto
        var photoUploadParams = new PhotoUploadUrlDto(
            uploadParams.UploadUrl,
            uploadParams.PublicId,
            uploadParams.UploadParameters,
            uploadParams.Timestamp,
            uploadParams.Signature);

        return Result<PhotoUploadUrlDto>.Success(photoUploadParams);
    }
}