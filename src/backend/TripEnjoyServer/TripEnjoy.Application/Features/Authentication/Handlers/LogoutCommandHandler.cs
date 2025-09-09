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

        public LogoutCommandHandler(IUnitOfWork unitOfWork, ILogger<LogoutCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

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