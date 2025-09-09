using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class LoginStepTwoCommandHandler : IRequestHandler<LoginStepTwoCommand, Result<AuthResultDTO>>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<LoginStepTwoCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="LoginStepTwoCommandHandler"/> with required dependencies.
        /// </summary>
        public LoginStepTwoCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<LoginStepTwoCommandHandler> logger)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Handles the second-step OTP login: validates the OTP, associates and persists a refresh token with the account, clears the related cache, and returns authentication data.
        /// </summary>
        /// <param name="request">The login command containing the user's email and one-time password (OTP).</param>
        /// <returns>
        /// A <see cref="Result{AuthResultDTO}"/> that is successful when authentication succeeds and the refresh token is persisted; the value contains the authentication result (including the refresh token and AspNetUserId).
        /// On failure, the result contains one of:
        /// - errors from the OTP validation step,
        /// - <c>DomainError.Account.NotFound</c> if the account cannot be located by AspNetUserId,
        /// - errors from adding the refresh token to the account,
        /// - or an <c>Error</c> with code "LoginStepTwo.Failure" when persisting changes or removing the cache fails.
        /// </returns>
        public async Task<Result<AuthResultDTO>> Handle(LoginStepTwoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling LoginStepTwoCommand for user {Email}", request.Email);

            var loginResult = await _authenService.LoginStepTwoAsync(request.Email, request.Otp);
            if (loginResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(loginResult.Errors);
            }

            var (authResult, cacheKey) = loginResult.Value;

            // Note: LoginStepTwoAsync in AuthenService needs to be modified to return AspNetUserId to save the refresh token.
            // For now, assuming it does, we proceed. We will modify AuthenService later.

            var account = await _unitOfWork.AccountRepository.FindByAspNetUserIdAsync(authResult.AspNetUserId); // This line will need authResult to contain AspNetUserId
            if (account is null)
            {
                return Result<AuthResultDTO>.Failure(DomainError.Account.NotFound);
            }

            var addTokenResult = account.AddRefreshToken(authResult.RefreshToken);
            if (addTokenResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(addTokenResult.Errors);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save refresh token for user {Email}", request.Email);
                return Result<AuthResultDTO>.Failure(new Error("LoginStepTwo.Failure", ex.Message, ErrorType.Failure));
            }

            _logger.LogInformation("User {Email} successfully logged in and refresh token was saved.", request.Email);
            return Result<AuthResultDTO>.Success(authResult);
        }
    }
}
