using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Application.Features.Property.Commands;
using TripEnjoy.Application.Features.Property.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/properties")]
[EnableRateLimiting("default")]
public class PropertiesController : ApiControllerBase
{
    private readonly ISender _sender;
    public PropertiesController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = RoleConstant.Partner)]
    [HttpPost]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyCommand command)
    {
        var result = await _sender.Send(command);

        return HandleResult(result);
    }

    [Authorize(Roles = RoleConstant.Partner)]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyProperties()
    {
        var query = new GetMyPropertiesQuery();
        var result = await _sender.Send(query);

        return HandleResult(result);
    }

    [HttpGet("{propertyId:guid}")]
    public async Task<IActionResult> GetPropertyById(Guid propertyId)
    {
        var query = new GetPropertyByIdQuery(propertyId);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProperties([FromQuery] PaginationQuery paginationQuery)
    {
        var query = new GetAllPropertiesQuery(paginationQuery.PageNumber, paginationQuery.PageSize);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }
}

public record PaginationQuery(int PageNumber = 1, int PageSize = 10);
