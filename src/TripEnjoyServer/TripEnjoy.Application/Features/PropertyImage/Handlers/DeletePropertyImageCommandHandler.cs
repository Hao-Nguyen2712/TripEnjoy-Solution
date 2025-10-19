using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Handlers;

public class DeletePropertyImageCommandHandler : IRequestHandler<DeletePropertyImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeletePropertyImageCommandHandler> _logger;
    private readonly ICloudinaryService _cloudinaryService;

    public DeletePropertyImageCommandHandler(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DeletePropertyImageCommandHandler> logger,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result> Handle(DeletePropertyImageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting deletion of image {ImageId} from property {PropertyId}", request.ImageId, request.PropertyId);

        try
        {
            var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
            if (!Guid.TryParse(partnerIdClaim, out var partnerId))
            {
                _logger.LogWarning("Unauthorized deletion attempt - invalid partner ID claim");
                return Result.Failure(DomainError.Authentication.Unauthorized);
            }

            _logger.LogInformation("Authenticated partner {PartnerId} attempting to delete image", partnerId);

            var propertyId = PropertyId.Create(request.PropertyId);
            var property = await _unitOfWork.Properties.GetByIdWithImagesAsync(propertyId);
            if (property is null)
            {
                _logger.LogWarning("Property {PropertyId} not found", request.PropertyId);
                return Result.Failure(DomainError.Property.NotFound);
            }

            if (property.PartnerId.Id != partnerId)
            {
                _logger.LogWarning("Unauthorized attempt to delete image from property {PropertyId} by partner {PartnerId}", request.PropertyId, partnerId);
                return Result.Failure(DomainError.Authentication.Forbidden);
            }

            _logger.LogInformation("Found property {PropertyId} with {ImageCount} images", request.PropertyId, property.PropertyImages.Count);

            // Find the image to delete first
            var propertyImageId = PropertyImageId.Create(request.ImageId);
            var imageToDelete = property.PropertyImages.FirstOrDefault(img => img.Id == propertyImageId);
            if (imageToDelete == null)
            {
                _logger.LogWarning("Image {ImageId} not found in property {PropertyId}", request.ImageId, request.PropertyId);
                return Result.Failure(DomainError.Property.ImageNotFound);
            }

            _logger.LogInformation("Found image to delete: {ImageUrl} (Length: {UrlLength})", imageToDelete.ImageUrl, imageToDelete.ImageUrl?.Length);

            // Extract public ID from Cloudinary URL
            var publicId = ExtractPublicIdFromCloudinaryUrl(imageToDelete.ImageUrl ?? string.Empty);
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Could not extract public ID from image URL: {ImageUrl}", imageToDelete.ImageUrl);
                return Result.Failure(DomainError.Property.InvalidImageUrl);
            }

            _logger.LogInformation("Extracted public ID: '{PublicId}' (Length: {PublicIdLength}) from URL: {ImageUrl}",
                publicId, publicId.Length, imageToDelete.ImageUrl);

            // Delete from Cloudinary first
            try
            {
                var cloudinaryDeleteResult = await _cloudinaryService.DeleteFileAsync(publicId, cancellationToken);
                if (!cloudinaryDeleteResult)
                {
                    _logger.LogError("Failed to delete image from Cloudinary. PublicId: {PublicId}", publicId);
                    return Result.Failure(DomainError.Property.CloudinaryDeletionFailed);
                }

                _logger.LogInformation("Successfully deleted image from Cloudinary. PublicId: {PublicId}", publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting image from Cloudinary. PublicId: {PublicId}", publicId);
                return Result.Failure(DomainError.Property.CloudinaryDeletionFailed);
            }

            // Delete from database after successful Cloudinary deletion
            var result = property.RemoveImage(propertyImageId);
            if (result.IsFailure)
            {
                // If database deletion fails after Cloudinary deletion, log a warning
                // The image is already deleted from Cloudinary, so this is an inconsistent state
                _logger.LogWarning("Database deletion failed after successful Cloudinary deletion. PublicId: {PublicId}, ImageId: {ImageId}",
                    publicId, request.ImageId);
                return result;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted property image. PropertyId: {PropertyId}, ImageId: {ImageId}",
                request.PropertyId, request.ImageId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting image {ImageId} from property {PropertyId}",
                request.ImageId, request.PropertyId);
            return Result.Failure(DomainError.Property.CloudinaryDeletionFailed);
        }
    }

    /// <summary>
    /// Extracts the public ID from a Cloudinary URL
    /// Example: https://res.cloudinary.com/demo/image/upload/v1234567890/sample.jpg -> sample
    /// Example: https://res.cloudinary.com/demo/auto/upload/property_images/image123.jpg -> property_images/image123
    /// </summary>
    private static string ExtractPublicIdFromCloudinaryUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return string.Empty;

        try
        {
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath;

            // Cloudinary URL pattern: /{resource_type}/upload/[version/]public_id[.extension]
            // Look for /upload/ after resource type (image, auto, video, etc.)
            var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
            if (uploadIndex == -1)
                return string.Empty;

            var pathAfterUpload = path.Substring(uploadIndex + "/upload/".Length);

            // Remove version if present (starts with 'v' followed by digits and a slash)
            if (pathAfterUpload.StartsWith("v") && pathAfterUpload.Length > 1)
            {
                var nextSlash = pathAfterUpload.IndexOf('/', 1);
                if (nextSlash > 0 && pathAfterUpload.Substring(1, nextSlash - 1).All(char.IsDigit))
                {
                    pathAfterUpload = pathAfterUpload.Substring(nextSlash + 1);
                }
            }

            // For Cloudinary URLs, the public ID might not have an extension
            // Only remove extension if there's a dot and it looks like a file extension (3-4 chars)
            var lastDotIndex = pathAfterUpload.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                var extension = pathAfterUpload.Substring(lastDotIndex + 1);
                // Only treat as extension if it's 2-4 characters and contains only letters/numbers
                if (extension.Length >= 2 && extension.Length <= 4 && extension.All(c => char.IsLetterOrDigit(c)))
                {
                    pathAfterUpload = pathAfterUpload.Substring(0, lastDotIndex);
                }
            }

            return pathAfterUpload;
        }
        catch (Exception)
        {
            // Log the exception for debugging if needed
            return string.Empty;
        }
    }
}
