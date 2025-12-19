using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IAuthenService _authenService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ResetPasswordCommandHandler> _logger;

        public ResetPasswordCommandHandler(
            IAuthenService authenService,
            IDistributedCache cache,
            ILogger<ResetPasswordCommandHandler> logger)
        {
            _authenService = authenService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // Verify reset token from cache
            var resetTokenCacheKey = $"reset-token:{request.Email}";
            var storedToken = await _cache.GetStringAsync(resetTokenCacheKey, cancellationToken);

            if (string.IsNullOrEmpty(storedToken) || storedToken != request.ResetToken)
            {
                _logger.LogWarning("Invalid or expired reset token for email: {Email}", request.Email);
                return Result.Failure(DomainError.Account.InvalidOrExpiredResetToken);
            }

            // Reset password using the auth service
            var resetResult = await _authenService.ResetPasswordAsync(request.Email, request.NewPassword);
            if (resetResult.IsFailure)
            {
                _logger.LogError("Failed to reset password for user: {Email}", request.Email);
                return Result.Failure(resetResult.Errors);
            }

            // Remove reset token from cache to prevent reuse
            await _cache.RemoveAsync(resetTokenCacheKey, cancellationToken);

            _logger.LogInformation("Password successfully reset for user: {Email}", request.Email);
            return Result.Success();
        }
    }
}
