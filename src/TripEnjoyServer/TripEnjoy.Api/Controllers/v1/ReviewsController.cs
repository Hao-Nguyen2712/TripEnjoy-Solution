using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripEnjoy.Api.Controllers;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Features.Reviews.Queries;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class ReviewsController : ApiControllerBase
{
    private readonly ISender _sender;

    public ReviewsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a review for a room type after completing a booking
    /// </summary>
    /// <param name="command">Review details including rating, comment, and optional images</param>
    /// <returns>The created review ID</returns>
    [HttpPost]
    [Authorize(Roles = RoleConstant.User)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewCommand command)
    {
        var result = await _sender.Send(command);
        return HandleResult(result, StatusCodes.Status201Created, "Review created successfully");
    }

    /// <summary>
    /// Update an existing review (user can only update their own reviews)
    /// </summary>
    /// <param name="reviewId">The review ID</param>
    /// <param name="command">Updated rating and comment</param>
    /// <returns>Success result</returns>
    [HttpPut("{reviewId:guid}")]
    [Authorize(Roles = RoleConstant.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewCommand command)
    {
        if (reviewId != command.ReviewId)
        {
            return BadRequest("Review ID in URL does not match the request body");
        }

        var result = await _sender.Send(command);
        return HandleResult(result, "Review updated successfully");
    }

    /// <summary>
    /// Delete a review (user can only delete their own reviews)
    /// </summary>
    /// <param name="reviewId">The review ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{reviewId:guid}")]
    [Authorize(Roles = RoleConstant.User)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        var command = new DeleteReviewCommand(reviewId);
        var result = await _sender.Send(command);
        return HandleResult(result, "Review deleted successfully");
    }

    /// <summary>
    /// Hide or unhide a review (Admin only)
    /// </summary>
    /// <param name="reviewId">The review ID</param>
    /// <param name="hide">True to hide, false to unhide</param>
    /// <returns>Success result</returns>
    [HttpPatch("{reviewId:guid}/visibility")]
    [Authorize(Roles = RoleConstant.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleReviewVisibility(Guid reviewId, [FromQuery] bool hide = true)
    {
        var command = new HideReviewCommand(reviewId, hide);
        var result = await _sender.Send(command);
        return HandleResult(result, hide ? "Review hidden successfully" : "Review unhidden successfully");
    }

    /// <summary>
    /// Get a specific review by ID
    /// </summary>
    /// <param name="reviewId">The review ID</param>
    /// <returns>Review details with images and replies</returns>
    [HttpGet("{reviewId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewById(Guid reviewId)
    {
        var query = new GetReviewByIdQuery(reviewId);
        var result = await _sender.Send(query);
        return HandleResult(result, "Review retrieved successfully");
    }

    /// <summary>
    /// Get all reviews for a specific room type
    /// </summary>
    /// <param name="roomTypeId">The room type ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 50)</param>
    /// <returns>Paginated list of reviews</returns>
    [HttpGet("room-type/{roomTypeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByRoomType(
        Guid roomTypeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;

        var query = new GetReviewsByRoomTypeQuery(roomTypeId, pageNumber, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result, "Reviews retrieved successfully");
    }

    /// <summary>
    /// Get all reviews for a property (across all room types)
    /// </summary>
    /// <param name="propertyId">The property ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 50)</param>
    /// <returns>Paginated list of reviews</returns>
    [HttpGet("property/{propertyId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewsByProperty(
        Guid propertyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;

        var query = new GetReviewsByPropertyQuery(propertyId, pageNumber, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result, "Reviews retrieved successfully");
    }

    /// <summary>
    /// Get all reviews by a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 50)</param>
    /// <returns>Paginated list of reviews</returns>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserReviews(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;

        var query = new GetUserReviewsQuery(userId, pageNumber, pageSize);
        var result = await _sender.Send(query);
        return HandleResult(result, "Reviews retrieved successfully");
    }
}
