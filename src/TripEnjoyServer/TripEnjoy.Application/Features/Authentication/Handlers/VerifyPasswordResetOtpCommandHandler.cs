using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Extensions;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class VerifyPasswordResetOtpCommandHandler : IRequestHandler<VerifyPasswordResetOtpCommand, Result<string>>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<VerifyPasswordResetOtpCommandHandler> _logger;

        public VerifyPasswordResetOtpCommandHandler(IDistributedCache cache, ILogger<VerifyPasswordResetOtpCommandHandler> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(VerifyPasswordResetOtpCommand request, CancellationToken cancellationToken)
        {
            var cacheKey = $"reset-otp:{request.Email}";
            var storedOtpHash = await _cache.GetStringAsync(cacheKey, cancellationToken);
            var requestOtpHash = HashingOtpExtension.HashWithSHA256(request.Otp);

            if (string.IsNullOrEmpty(storedOtpHash) || storedOtpHash != requestOtpHash)
            {
                _logger.LogWarning("Invalid password reset OTP for email: {Email}", request.Email);
                return Result<string>.Failure(DomainError.Account.InvalidOtp);
            }

            // OTP is valid, remove it from cache
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            // Generate a secure, single-use token for the final reset step
            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var resetTokenCacheKey = $"reset-token:{request.Email}";
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Give user 10 minutes to reset password
            };

            await _cache.SetStringAsync(resetTokenCacheKey, resetToken, cacheOptions, cancellationToken);
            _logger.LogInformation("Password reset OTP verified for {Email}. Single-use reset token generated and cached.", request.Email);

            return Result<string>.Success(resetToken);
        }
    }
}
