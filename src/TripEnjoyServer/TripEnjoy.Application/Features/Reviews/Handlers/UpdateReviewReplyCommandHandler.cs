using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Reviews.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Review.Entities;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class UpdateReviewReplyCommandHandler : IRequestHandler<UpdateReviewReplyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateReviewReplyCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateReviewReplyCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateReviewReplyCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(UpdateReviewReplyCommand request, CancellationToken cancellationToken)
    {
        // Get user ID from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result.Failure(new Error(
                "ReviewReply.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var accountId = AccountId.Create(userIdGuid);
        var replyId = ReviewReplyId.Create(request.ReviewReplyId);

        // Get review reply
        var reply = await _unitOfWork.Repository<ReviewReply>()
            .GetByIdAsync(replyId.Value);

        if (reply == null)
        {
            return Result.Failure(DomainError.ReviewReply.NotFound);
        }

        // Verify ownership
        if (reply.ReplierId != accountId)
        {
            return Result.Failure(new Error(
                "ReviewReply.Unauthorized",
                "You are not authorized to update this reply.",
                ErrorType.Forbidden));
        }

        // Update reply
        var updateResult = reply.Update(request.Content);

        if (updateResult.IsFailure)
        {
            _logger.LogError("Failed to update review reply {ReplyId}: {Errors}",
                replyId.Value, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult;
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated review reply {ReplyId} by account {AccountId}",
            replyId.Value, accountId.Id);

        return Result.Success();
    }
}
