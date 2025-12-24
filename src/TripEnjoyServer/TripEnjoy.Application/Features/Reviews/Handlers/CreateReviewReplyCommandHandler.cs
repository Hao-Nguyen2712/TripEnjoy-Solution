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
using TripEnjoy.Domain.Review.Enums;
using TripEnjoy.Domain.Review.ValueObjects;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Reviews.Handlers;

public class CreateReviewReplyCommandHandler : IRequestHandler<CreateReviewReplyCommand, Result<ReviewReplyId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReviewReplyCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateReviewReplyCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateReviewReplyCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<ReviewReplyId>> Handle(CreateReviewReplyCommand request, CancellationToken cancellationToken)
    {
        // Get user ID and role from claims
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userIdGuid))
        {
            return Result<ReviewReplyId>.Failure(new Error(
                "ReviewReply.UserIdNotFound",
                "The user ID was not found in the user's claims.",
                ErrorType.Unauthorized));
        }

        var userRole = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
        var accountId = AccountId.Create(userIdGuid);
        var reviewId = ReviewId.Create(request.ReviewId);

        // Determine replier type based on role
        ReplierTypeEnum replierType;
        if (userRole == RoleConstant.Admin)
        {
            replierType = ReplierTypeEnum.Admin;
        }
        else if (userRole == RoleConstant.Partner)
        {
            replierType = ReplierTypeEnum.Partner;
        }
        else
        {
            return Result<ReviewReplyId>.Failure(new Error(
                "ReviewReply.Unauthorized",
                "Only partners and administrators can reply to reviews.",
                ErrorType.Forbidden));
        }

        // Get review
        var review = await _unitOfWork.Repository<Review>()
            .GetByIdAsync(reviewId.Value);

        if (review == null)
        {
            return Result<ReviewReplyId>.Failure(DomainError.Review.NotFound);
        }

        // TODO: For partners, verify they own the property (requires navigation to Property through RoomType)
        // This verification should be added when the full navigation is available

        // Add reply to review
        var replyResult = review.AddReply(replierType, accountId, request.Content);

        if (replyResult.IsFailure)
        {
            _logger.LogError("Failed to create review reply: {Errors}",
                string.Join(", ", replyResult.Errors.Select(e => e.Description)));
            return Result<ReviewReplyId>.Failure(replyResult.Errors);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var reply = replyResult.Value;

        _logger.LogInformation("Successfully created review reply {ReplyId} by {ReplierType} {AccountId}",
            reply.Id.Value, replierType, accountId.Id);

        return Result<ReviewReplyId>.Success(reply.Id);
    }
}
