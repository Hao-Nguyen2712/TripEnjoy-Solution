using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Admin.Commands;
using TripEnjoy.Application.Features.Admin.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = RoleConstant.Admin)]
[EnableRateLimiting("default")]
public class AdminController : ApiControllerBase
{
    private readonly ISender _sender;

    public AdminController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get dashboard statistics for admin
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var query = new GetAdminDashboardStatsQuery();
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Block a user
    /// </summary>
    [HttpPost("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(Guid userId, [FromBody] BlockUserRequest request)
    {
        var command = new BlockUserCommand(userId, request.Reason);
        var result = await _sender.Send(command);
        return HandleResult(result, "User blocked successfully");
    }

    /// <summary>
    /// Unblock a user
    /// </summary>
    [HttpPost("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        var command = new UnblockUserCommand(userId);
        var result = await _sender.Send(command);
        return HandleResult(result, "User unblocked successfully");
    }

    /// <summary>
    /// Get pending partner approvals
    /// </summary>
    [HttpGet("partners/pending")]
    public async Task<IActionResult> GetPendingPartnerApprovals()
    {
        var query = new GetPendingPartnerApprovalsQuery();
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve a partner
    /// </summary>
    [HttpPost("partners/{partnerId}/approve")]
    public async Task<IActionResult> ApprovePartner(Guid partnerId)
    {
        var command = new ApprovePartnerCommand(partnerId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Partner approved successfully");
    }

    /// <summary>
    /// Reject a partner
    /// </summary>
    [HttpPost("partners/{partnerId}/reject")]
    public async Task<IActionResult> RejectPartner(Guid partnerId, [FromBody] RejectPartnerRequest request)
    {
        var command = new RejectPartnerCommand(partnerId, request.Reason);
        var result = await _sender.Send(command);
        return HandleResult(result, "Partner rejected successfully");
    }

    /// <summary>
    /// Get pending property approvals
    /// </summary>
    [HttpGet("properties/pending")]
    public async Task<IActionResult> GetPendingPropertyApprovals()
    {
        var query = new GetPendingPropertyApprovalsQuery();
        var result = await _sender.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Approve a property
    /// </summary>
    [HttpPost("properties/{propertyId}/approve")]
    public async Task<IActionResult> ApproveProperty(Guid propertyId)
    {
        var command = new ApprovePropertyCommand(propertyId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Property approved successfully");
    }

    /// <summary>
    /// Reject a property
    /// </summary>
    [HttpPost("properties/{propertyId}/reject")]
    public async Task<IActionResult> RejectProperty(Guid propertyId, [FromBody] RejectPropertyRequest request)
    {
        var command = new RejectPropertyCommand(propertyId, request.Reason);
        var result = await _sender.Send(command);
        return HandleResult(result, "Property rejected successfully");
    }
}

public record BlockUserRequest(string Reason);
public record RejectPartnerRequest(string Reason);
public record RejectPropertyRequest(string Reason);
