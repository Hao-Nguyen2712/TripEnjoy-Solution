using MediatR;
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

        public LoginStepTwoCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResultDTO>> Handle(LoginStepTwoCommand request, CancellationToken cancellationToken)
        {
            var loginResult = await _authenService.LoginStepTwoAsync(request.Email, request.Otp);
            if (loginResult.IsFailure)
            {
                return loginResult;
            }
            
            var authResult = loginResult.Value;

            // Note: LoginStepTwoAsync in AuthenService needs to be modified to return AspNetUserId to save the refresh token.
            // For now, assuming it does, we proceed. We will modify AuthenService later.

            var account = await _unitOfWork.AccountRepository.FindByAspNetUserIdAsync(authResult.AspNetUserId); // This line will need authResult to contain AspNetUserId
            if (account is null)
            {
                return Result<AuthResultDTO>.Failure(DomainError.Account.NotFound);
            }

            var addTokenResult = account.AddRefreshToken(authResult.RefreshToken, DateTime.UtcNow.AddDays(7));
            if (addTokenResult.IsFailure)
            {
                return Result<AuthResultDTO>.Failure(addTokenResult.Errors);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthResultDTO>.Success(authResult);
        }
    }
}
