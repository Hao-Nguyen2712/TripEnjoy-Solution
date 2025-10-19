using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Handlers;

public class AddPropertyImageCommandHandler : IRequestHandler<AddPropertyImageCommand, Result<PropertyImageId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AddPropertyImageCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly ICloudinaryService _cloudinaryService;

    public AddPropertyImageCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AddPropertyImageCommandHandler> logger,
        ICacheService cacheService,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cacheService = cacheService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<PropertyImageId>> Handle(AddPropertyImageCommand request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerId))
        {
            return Result<PropertyImageId>.Failure(DomainError.Authentication.Unauthorized);
        }

        // Validate Cloudinary upload signature
        var isValidUpload = await _cloudinaryService.ValidateUploadedFileAsync(
            request.PublicId,
            request.Signature,
            request.Timestamp,
            cancellationToken);

        if (!isValidUpload)
        {
            _logger.LogWarning("Invalid Cloudinary signature for property image upload. PublicId: {PublicId}, PropertyId: {PropertyId}",
                request.PublicId, request.PropertyId);
            return Result<PropertyImageId>.Failure(
                new Error("PropertyImage.InvalidUpload",
                    "The uploaded image could not be validated. Please try uploading again.",
                    ErrorType.Validation));
        }

        var propertyId = PropertyId.Create(request.PropertyId);
        var property = await _unitOfWork.Properties.GetByIdWithImagesAsync(propertyId);

        if (property is null)
        {
            return Result<PropertyImageId>.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId.Id != partnerId)
        {
            _logger.LogWarning("Unauthorized attempt to add image to property {PropertyId} by partner {PartnerId}", request.PropertyId, partnerId);
            return Result<PropertyImageId>.Failure(DomainError.Authentication.Forbidden);
        }

        // Get the secure URL from Cloudinary
        var secureUrl = await _cloudinaryService.GetSecureUrlAsync(request.PublicId, cancellationToken);

        var result = property.AddImage(secureUrl, request.IsCover);
        if (result.IsFailure)
        {
            return Result<PropertyImageId>.Failure(result.Errors);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveByPrefixAsync("properties:all", cancellationToken);
        await _cacheService.RemoveByPrefixAsync($"properties:{property.Id}:images", cancellationToken);

        var addedImage = property.PropertyImages.OrderByDescending(i => i.UploadAt).First();

        return Result<PropertyImageId>.Success(addedImage.Id);
    }
}
