using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Payments.Commands;
using TripEnjoy.Application.Features.Payments.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class PaymentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Process payment for a booking
    /// </summary>
    [HttpPost("process")]
    [Authorize(Roles = RoleConstant.User)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var command = new ProcessPaymentCommand(
            request.BookingId,
            request.PaymentMethod,
            request.ReturnUrl);

        var result = await _sender.Send(command);
        return HandleResult(result, "Payment URL created successfully");
    }

    /// <summary>
    /// Verify payment callback from payment gateway
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyPaymentCallback()
    {
        // Extract all query parameters
        var callbackData = Request.Query.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToString());

        var command = new VerifyPaymentCallbackCommand(callbackData);
        var result = await _sender.Send(command);

        if (result.IsSuccess && result.Value)
        {
            // Redirect to success page
            return Redirect("/payment/success");
        }

        // Redirect to failure page
        return Redirect("/payment/failed");
    }

    /// <summary>
    /// Get payment status by payment ID
    /// </summary>
    [HttpGet("{paymentId:guid}")]
    public async Task<IActionResult> GetPaymentStatus(Guid paymentId)
    {
        var query = new GetPaymentStatusQuery(paymentId);
        var result = await _sender.Send(query);
        return HandleResult(result, "Payment status retrieved successfully");
    }

    /// <summary>
    /// Refund a payment (Admin/Partner only)
    /// </summary>
    [HttpPost("{paymentId:guid}/refund")]
    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Partner}")]
    public async Task<IActionResult> RefundPayment(Guid paymentId, [FromBody] RefundPaymentRequest request)
    {
        var command = new RefundPaymentCommand(paymentId, request.Reason);
        var result = await _sender.Send(command);
        return HandleResult(result, "Payment refunded successfully");
    }
}

public record ProcessPaymentRequest(
    Guid BookingId,
    string PaymentMethod,
    string ReturnUrl);

public record RefundPaymentRequest(string Reason);
