using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Vouchers.Commands;
using TripEnjoy.Application.Features.Vouchers.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class VouchersController : ApiControllerBase
{
    private readonly ISender _sender;

    public VouchersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a new voucher (Admin or Partner)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Partner}")]
    public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Voucher created successfully");
    }

    /// <summary>
    /// Update an existing voucher
    /// </summary>
    [HttpPut("{voucherId:guid}")]
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Partner}")]
    public async Task<IActionResult> UpdateVoucher(Guid voucherId, [FromBody] UpdateVoucherRequest request)
    {
        var command = new UpdateVoucherCommand(
            voucherId,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.UsageLimit,
            request.UsageLimitPerUser);

        var result = await _sender.Send(command);
        return HandleResult(result, "Voucher updated successfully");
    }

    /// <summary>
    /// Disable a voucher
    /// </summary>
    [HttpPost("{voucherId:guid}/disable")]
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Partner}")]
    public async Task<IActionResult> DisableVoucher(Guid voucherId)
    {
        var command = new DisableVoucherCommand(voucherId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Voucher disabled successfully");
    }

    /// <summary>
    /// Get voucher by code
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVoucherByCode(string code)
    {
        var query = new GetVoucherByCodeQuery(code);
        var result = await _sender.Send(query);
        return HandleResult(result, "Voucher retrieved successfully");
    }

    /// <summary>
    /// Get active vouchers (paginated)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveVouchers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetActiveVouchersQuery(pageNumber, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result, "Active vouchers retrieved successfully");
    }

    /// <summary>
    /// Get voucher usage statistics
    /// </summary>
    [HttpGet("{voucherId:guid}/stats")]
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Partner}")]
    public async Task<IActionResult> GetVoucherUsageStats(Guid voucherId)
    {
        var query = new GetVoucherUsageStatsQuery(voucherId);
        var result = await _sender.Send(query);
        return HandleResult(result, "Voucher usage statistics retrieved successfully");
    }

    /// <summary>
    /// Apply a voucher to calculate discount
    /// </summary>
    [HttpPost("apply")]
    [Authorize(Roles = RoleConstant.User)]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
    {
        var command = new ApplyVoucherCommand(
            request.Code,
            request.OrderAmount,
            request.PropertyId,
            request.RoomTypeId);

        var result = await _sender.Send(command);
        return HandleResult(result, "Voucher applied successfully");
    }
}

public record UpdateVoucherRequest(
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? UsageLimit = null,
    int? UsageLimitPerUser = null
);

public record ApplyVoucherRequest(
    string Code,
    decimal OrderAmount,
    Guid? PropertyId = null,
    Guid? RoomTypeId = null
);
