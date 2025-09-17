using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;
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

    [HttpPost]
    public async Task<IActionResult> AddPropertyImage(Guid propertyId, [FromBody] AddPropertyImageRequest request)
    {
        var command = new AddPropertyImageCommand(propertyId, request.ImageUrl, request.IsCover);
        var result = await _sender.Send(command);
        return HandleResult(result);
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

public record AddPropertyImageRequest(string ImageUrl, bool IsCover);
