using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Application.Features.Partner.Commands;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/partner")]
[Authorize(Roles = RoleConstant.Partner)]
[EnableRateLimiting("default")]
public class PartnerController : ApiControllerBase
{
    private readonly ISender _sender;

    public PartnerController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Generates a secure upload URL for document upload to Cloudinary
    /// </summary>
    /// <param name="command">The command containing document type and file name</param>
    /// <returns>Upload parameters including URL, signature, and other required fields</returns>
    [HttpPost("documents/upload-url")]
    public async Task<IActionResult> GenerateDocumentUploadUrl([FromBody] GenerateDocumentUploadUrlCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, "Upload URL generated successfully");
    }

    /// <summary>
    /// Adds a document after successful upload to Cloudinary
    /// </summary>
    /// <param name="command">The command containing document details from Cloudinary</param>
    /// <returns>The ID of the created document</returns>
    [HttpPost("documents")]
    public async Task<IActionResult> AddDocument([FromBody] AddPartnerDocumentCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, "Document added successfully");
    }
}
