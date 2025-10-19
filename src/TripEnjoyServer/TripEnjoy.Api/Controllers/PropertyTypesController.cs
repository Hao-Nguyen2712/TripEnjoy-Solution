using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Application.Features.PropertyType.Commands;
using TripEnjoy.Application.Features.PropertyType.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/property-types")]
[EnableRateLimiting("default")]
public class PropertyTypesController : ApiControllerBase
{
    private readonly ISender _sender;

    public PropertyTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPropertyTypes()
    {
        var query = new GetAllPropertyTypesQuery();
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreatePropertyType([FromBody] CreatePropertyTypeCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result);
    }
}
