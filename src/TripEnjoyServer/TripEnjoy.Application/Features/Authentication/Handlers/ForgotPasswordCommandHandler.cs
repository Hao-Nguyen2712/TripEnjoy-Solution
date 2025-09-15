using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Extensions;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            IAccountRepository accountRepository,
            IEmailService emailService,
            IDistributedCache cache,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _accountRepository = accountRepository;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling forgot password request for email: {Email}", request.Email);
            var account = await _accountRepository.GetAccountByEmail(request.Email);

            if (account != null)
            {
                _logger.LogInformation("Account found for email {Email}. Generating OTP for password reset.", request.Email);
                var otp = new Random().Next(100000, 999999).ToString();
                
                var cacheKey = $"reset-otp:{request.Email}";
                var hashOtp = HashingOtpExtension.HashWithSHA256(otp);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                
                await _cache.SetStringAsync(cacheKey, hashOtp, cacheOptions, cancellationToken);
                _logger.LogInformation("Password reset OTP saved to cache for email: {Email}", request.Email);

                await _emailService.SendOtpAsync(request.Email, otp, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No account found for email {Email} during forgot password request. Proceeding without action for security.", request.Email);
            }
            
            return Result.Success();
        }
    }
}
