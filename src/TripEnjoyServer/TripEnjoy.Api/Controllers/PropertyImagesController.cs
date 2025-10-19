using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/properties/{propertyId:guid}/images")]
[Authorize(Roles = RoleConstant.Partner)]
[EnableRateLimiting("default")]
public class PropertyImagesController : ApiControllerBase
{
    private readonly ISender _sender;

    public PropertyImagesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Generates a secure upload URL for property image upload to Cloudinary
    /// </summary>
    /// <param name="propertyId">The ID of the property</param>
    /// <param name="command">The command containing file name and optional caption</param>
    /// <returns>Upload parameters including URL, signature, and other required fields</returns>
    [HttpPost("upload-url")]
    public async Task<IActionResult> GeneratePhotoUploadUrl(Guid propertyId, [FromBody] GeneratePhotoUploadUrlRequest request)
    {
        var command = new GeneratePhotoUploadUrlCommand(propertyId, request.FileName, request.Caption);
        var result = await _sender.Send(command);
        return HandleResult(result, "Upload URL generated successfully");
    }

    /// <summary>
    /// Adds a property image after successful upload to Cloudinary
    /// </summary>
    /// <param name="propertyId">The ID of the property</param>
    /// <param name="request">The request containing image details from Cloudinary</param>
    /// <returns>The ID of the created property image</returns>
    [HttpPost]
    public async Task<IActionResult> AddPropertyImage(Guid propertyId, [FromBody] AddPropertyImageRequest request)
    {
        var command = new AddPropertyImageCommand(
            propertyId,
            request.PublicId,
            request.ImageUrl,
            request.Signature,
            request.Timestamp,
            request.IsCover,
            request.Caption);
        var result = await _sender.Send(command);
        return HandleResult(result, "Image added successfully");
    }

    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> DeletePropertyImage(Guid propertyId, Guid imageId)
    {
        var command = new DeletePropertyImageCommand(propertyId, imageId);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{imageId:guid}/set-cover")]
    public async Task<IActionResult> SetCoverPropertyImage(Guid propertyId, Guid imageId)
    {
        var command = new SetCoverPropertyImageCommand(propertyId, imageId);
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}

public record GeneratePhotoUploadUrlRequest(string FileName, string? Caption = null);

public record AddPropertyImageRequest(
    string PublicId,
    string ImageUrl,
    string Signature,
    long Timestamp,
    bool IsCover,
    string? Caption = null);
