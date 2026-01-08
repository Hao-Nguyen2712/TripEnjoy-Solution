using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class DeleteReviewReplyCommandHandler : IRequestHandler<DeleteReviewReplyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReviewReplyCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteReviewReplyCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteReviewReplyCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(DeleteReviewReplyCommand request, CancellationToken cancellationToken)
    {
        // Get user ID and role from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result.Failure(new Error(
                "ReviewReply.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var accountId = AccountId.Create(userIdGuid);
        var userRole = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        var replyId = ReviewReplyId.Create(request.ReviewReplyId);

        // Get the review that contains this reply
        var reviews = _unitOfWork.Repository<Review>()
            .GetQueryable()
            .Where(r => r.ReviewReplies.Any(reply => reply.Id == replyId))
            .ToList();

        var review = reviews.FirstOrDefault();
        if (review == null)
        {
            return Result.Failure(DomainError.ReviewReply.NotFound);
        }

        var reply = review.ReviewReplies.First(r => r.Id == replyId);

        // Check authorization: either owner or admin can delete
        if (reply.ReplierId != accountId && userRole != RoleConstant.Admin)
        {
            return Result.Failure(new Error(
                "ReviewReply.Unauthorized",
                "You are not authorized to delete this reply.",
                ErrorType.Forbidden));
        }

        // Remove reply from review
        var removeResult = review.RemoveReply(replyId);

        if (removeResult.IsFailure)
        {
            _logger.LogError("Failed to delete review reply {ReplyId}: {Errors}",
                replyId.Value, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
            return removeResult;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted review reply {ReplyId}",
            replyId.Value);

        return Result.Success();
    }
}
