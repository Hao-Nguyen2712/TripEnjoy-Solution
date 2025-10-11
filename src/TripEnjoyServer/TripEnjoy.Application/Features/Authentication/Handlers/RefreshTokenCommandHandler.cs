using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDTO>>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenCommandHandler"/> class.
        /// </summary>
        /// <remarks>
        /// Assigns the injected authentication service, unit-of-work, and logger to the handler instance.
        /// Dependencies are expected to be provided by dependency injection and non-null.
        /// </remarks>
        public RefreshTokenCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, ILogger<RefreshTokenCommandHandler> logger)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Refreshes an expired access token using a provided refresh token: validates the expired token, locates the account,
        /// rotates the refresh token, issues a new access token, persists the updated account, and returns the new credentials.
        /// </summary>
        /// <param name="request">Command containing the expired access token and the refresh token to rotate.</param>
        /// <param name="cancellationToken">Cancellation token forwarded to the persistence call (<see cref="IUnitOfWork.SaveChangesAsync(CancellationToken)"/>).</param>
        /// <returns>
        /// A <see cref="Result{AuthResultDTO}"/> that is:
        /// - Success containing <see cref="AuthResultDTO"/> with the new access token, new refresh token, and user id; or
        /// - Failure for invalid/expired token, missing account, refresh-token rotation errors, or persistence failures (error code "RefreshToken.Failure").
        /// </returns>
        public async Task<Result<AuthResultDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = await _authenService.GetPrincipalFromExpiredToken(request.refreshToken);
            if (principal is null)
            {
                _logger.LogError("Invalid token for user {Email}", request.expiredAccessToken);
                return Result<AuthResultDTO>.Failure(DomainError.Account.InvalidToken);
            }

            var aspNetUserId = principal.Value?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (aspNetUserId is null)
            {
                _logger.LogError("User is null");
                return Result<AuthResultDTO>.Failure(DomainError.Account.InvalidToken);
            }

            // 2. Tìm Account
            var account = await _unitOfWork.AccountRepository.FindByAspNetUserIdWithBlackListTokensAsync(aspNetUserId);
            if (account is null)
            {
                return Result<AuthResultDTO>.Failure(DomainError.Account.NotFound);
            }

            var isBlackListToken = account.BlackListTokens.Any(bt => bt.Token == request.refreshToken);
            if (isBlackListToken)
            {
                return Result<AuthResultDTO>.Failure(DomainError.Account.InvalidToken);
            }
             
            var newRefreshTokenString = _authenService.GenerateRefreshToken();
            
            // Debug: Log the tokens available in the account
            _logger.LogInformation("Account {AccountId} has {TokenCount} refresh tokens: [{Tokens}]", 
                account.Id, 
                account.RefreshTokens.Count, 
                string.Join(", ", account.RefreshTokens.Select(rt => $"'{rt.Token}'")));
            _logger.LogInformation("Looking for refresh token: '{RefreshToken}'", request.refreshToken);
            
            var rotateResult = account.RotateRefreshToken(request.refreshToken, newRefreshTokenString, DateTime.UtcNow.AddDays(7));
            if (rotateResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(rotateResult.Errors);
            }
            
            // Add the new refresh token to the account
            var addTokenResult = account.AddRefreshToken(newRefreshTokenString);
            if (addTokenResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(addTokenResult.Errors);
            }
            
            var newAccessToken = await _authenService.GenerateAccessTokenAsync(aspNetUserId);

            try
            {
               await _unitOfWork.Repository<Domain.Account.Account>().UpdateAsync(account);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save refresh token for user {AspNetUserId}", aspNetUserId);
                return Result<AuthResultDTO>.Failure(new Error("RefreshToken.Failure", ex.Message, ErrorType.Failure));
            }
            _logger.LogInformation("Refresh token for user {AspNetUserId} successfully", aspNetUserId);
            var authResult = new AuthResultDTO(newAccessToken, newRefreshTokenString, aspNetUserId);
            return Result<AuthResultDTO>.Success(authResult);
        }
    }
}