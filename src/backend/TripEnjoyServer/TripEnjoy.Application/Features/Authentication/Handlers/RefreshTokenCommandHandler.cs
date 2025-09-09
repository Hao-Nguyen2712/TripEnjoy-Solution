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

        public RefreshTokenCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, ILogger<RefreshTokenCommandHandler> logger)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<AuthResultDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = await _authenService.GetPrincipalFromExpiredToken(request.expiredAccessToken);
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
            var account = await _unitOfWork.AccountRepository.FindByAspNetUserIdAsync(aspNetUserId);
            if (account is null)
            {
                return Result<AuthResultDTO>.Failure(DomainError.Account.NotFound);
            }

            var newRefreshTokenString = _authenService.GenerateRefreshToken();

            // 4. Gọi aggregate để xoay vòng token
            var rotateResult = account.RotateRefreshToken(request.refreshToken, newRefreshTokenString, DateTime.UtcNow.AddDays(7));
            if (rotateResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(rotateResult.Errors);
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