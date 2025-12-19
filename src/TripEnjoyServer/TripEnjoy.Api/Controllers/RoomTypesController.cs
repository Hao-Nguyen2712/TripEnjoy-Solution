using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Application.Features.RoomTypes.Commands;
using TripEnjoy.Application.Features.RoomTypes.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/room-types")]
[EnableRateLimiting("default")]
public class RoomTypesController : ApiControllerBase
{
    private readonly ISender _sender;

    public RoomTypesController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize(Roles = RoleConstant.Partner)]
    [HttpPost]
    public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, "Room type created successfully");
    }

    [Authorize(Roles = RoleConstant.Partner)]
    [HttpPut("{roomTypeId:guid}")]
    public async Task<IActionResult> UpdateRoomType(Guid roomTypeId, [FromBody] UpdateRoomTypeCommand command)
    {
        // Ensure the route parameter matches the command
        if (roomTypeId != command.RoomTypeId)
        {
            return BadRequest("Room type ID in route does not match command");
        }

        var result = await _sender.Send(command);
        return HandleResult(result, "Room type updated successfully");
    }

    [Authorize(Roles = RoleConstant.Partner)]
    [HttpDelete("{roomTypeId:guid}")]
    public async Task<IActionResult> DeleteRoomType(Guid roomTypeId)
    {
        var command = new DeleteRoomTypeCommand(roomTypeId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Room type deleted successfully");
    }

    [HttpGet("property/{propertyId:guid}")]
    public async Task<IActionResult> GetRoomTypesByProperty(Guid propertyId)
    {
        var query = new GetRoomTypesByPropertyQuery(propertyId);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{roomTypeId:guid}")]
    public async Task<IActionResult> GetRoomTypeById(Guid roomTypeId)
    {
        var query = new GetRoomTypeByIdQuery(roomTypeId);
        var result = await _sender.Send(query);
        return HandleResult(result);
    }
}
