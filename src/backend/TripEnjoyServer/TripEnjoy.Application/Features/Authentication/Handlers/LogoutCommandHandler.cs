using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<LogoutCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="LogoutCommandHandler"/> with required dependencies.
        /// </summary>
        /// <remarks>Requires an <see cref="IUnitOfWork"/> for repository and persistence operations and an <see cref="ILogger{LogoutCommandHandler}"/> for logging.</remarks>
        public LogoutCommandHandler(IUnitOfWork unitOfWork, ILogger<LogoutCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Processes a logout request: revokes the provided refresh token on the account, adds that token to a blacklist, and persists the change.
        /// </summary>
        /// <param name="request">Command containing the AspNet user id and the refresh token to revoke and blacklist.</param>
        /// <param name="cancellationToken">Cancellation token for async operations.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure:
        /// - Failure when the account is not found, when revoking the token fails, when adding the token to the blacklist fails,
        ///   or when saving changes throws an exception (converted into an error with code "Logout.Failure").
        /// - Success returns a <see cref="Result{T}"/> with the string "Logout Succe".
        /// </returns>
        /// <remarks>
        /// The blacklisted token is stored with an expiry of 7 days from the current UTC time.
        /// Side effects: updates the account state (revocation and blacklist) and persists changes via the unit of work.
        /// </remarks>
        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling LogoutCommand for user {RefreshToken}", request.refreshToken);

            var account = await _unitOfWork.AccountRepository.FindByAspNetUserIdAsync(request.aspNetUserId);
            if (account == null)
            {
                return Result<Error>.Failure(DomainError.Account.NotFound);
            }

            var result = account.RevokeRefreshToken(request.refreshToken);
            if (result.IsFailure)
            {
                return Result.Failure(result.Errors);
            }
            var resultAddBlackListToken = account.AddBlackListToken(request.refreshToken, DateTime.UtcNow.AddDays(7));
            if (resultAddBlackListToken.IsFailure)
            {
                return Result.Failure(resultAddBlackListToken.Errors);
            }
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save black list token for user {AspNetUserId}", request.aspNetUserId);
                return Result.Failure(new Error("Logout.Failure", ex.Message, ErrorType.Failure));
            }

            return Result<string>.Success("Logout Succe");
        }
    }
}