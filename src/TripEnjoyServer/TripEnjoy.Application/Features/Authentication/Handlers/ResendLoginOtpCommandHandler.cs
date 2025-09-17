using Hangfire;
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
    public class ResendLoginOtpCommandHandler : IRequestHandler<ResendLoginOtpCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<ResendLoginOtpCommandHandler> _logger;
        private readonly IDistributedCache _cache;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public ResendLoginOtpCommandHandler(
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<ResendLoginOtpCommandHandler> logger,
            IDistributedCache cache,
            IBackgroundJobClient backgroundJobClient)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
            _cache = cache;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Result> Handle(ResendLoginOtpCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling Resend OTP request for email: {Email}", request.Email);
            var account = await _accountRepository.GetAccountByEmail(request.Email);

            if (account != null)
            {
                _logger.LogInformation("Account found for email {Email}. Generating new OTP.", request.Email);
                var newOtp = account.GenerateNewOtp();

                var cacheKey = $"otp:{request.Email}";

                // Remove old OTP before setting a new one
                await _cache.RemoveAsync(cacheKey, cancellationToken);

                var hashOtp = HashingOtpExtension.HashWithSHA256(newOtp);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, hashOtp, cacheOptions, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("New OTP generated and saved for account {AccountId}", account.Id);

                // Enqueue the email sending job to Hangfire
                _backgroundJobClient.Enqueue(() => _emailService.SendOtpAsync(account.AccountEmail, newOtp, CancellationToken.None));
                _logger.LogInformation("Enqueued job to send new OTP to email {Email}", account.AccountEmail);
            }
            else
            {
                _logger.LogWarning("No account found for email {Email} during Resend OTP request. Proceeding without action for security.", request.Email);
            }

            return Result.Success();
        }
    }
}
