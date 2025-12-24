using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reviews/{reviewId:guid}/replies")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ReviewRepliesController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReviewRepliesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a reply to a review (Partners can reply to reviews on their properties, Admins can reply to any review)
    /// </summary>
    /// <param name="reviewId">The review ID</param>
    /// <param name="command">Reply content</param>
    /// <returns>The created reply ID</returns>
    [HttpPost]
    [Authorize(Roles = $"{RoleConstant.Partner},{RoleConstant.Admin}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReply(Guid reviewId, [FromBody] CreateReviewReplyCommand command)
    {
        if (reviewId != command.ReviewId)
        {
            return BadRequest("Review ID in URL does not match the request body");
        }

        var result = await _sender.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Reply created successfully");
    }

    /// <summary>
    /// Update a reply (replier can only update their own replies)
    /// </summary>
    /// <param name="reviewId">The review ID (for route consistency)</param>
    /// <param name="replyId">The reply ID</param>
    /// <param name="command">Updated reply content</param>
    /// <returns>Success result</returns>
    [HttpPut("{replyId:guid}")]
    [Authorize(Roles = $"{RoleConstant.Partner},{RoleConstant.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReply(Guid reviewId, Guid replyId, [FromBody] UpdateReviewReplyCommand command)
    {
        if (replyId != command.ReviewReplyId)
        {
            return BadRequest("Reply ID in URL does not match the request body");
        }

        var result = await _sender.Send(command);
        return HandleResult(result, "Reply updated successfully");
    }

    /// <summary>
    /// Delete a reply (replier can delete their own replies, admins can delete any reply)
    /// </summary>
    /// <param name="reviewId">The review ID (for route consistency)</param>
    /// <param name="replyId">The reply ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{replyId:guid}")]
    [Authorize(Roles = $"{RoleConstant.Partner},{RoleConstant.Admin}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReply(Guid reviewId, Guid replyId)
    {
        var command = new DeleteReviewReplyCommand(replyId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Reply deleted successfully");
    }
}
