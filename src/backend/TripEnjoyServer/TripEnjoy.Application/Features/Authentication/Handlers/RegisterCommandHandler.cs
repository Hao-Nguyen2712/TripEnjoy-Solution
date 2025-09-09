using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AccountId>>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IEmailService _emailService;

        public RegisterCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<Result<AccountId>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var createUserResult = await _authenService.CreateUserAsync(request.email, request.password, RoleConstant.User);
            if (createUserResult.IsFailure)
            {
                return Result<AccountId>.Failure(createUserResult.Errors);
            }

            var (userId, confirmToken) = createUserResult.Value;

            // 2. Tạo Account Aggregate
            var accountResult = Account.Create(userId, request.email);
            if (accountResult.IsFailure)
            {
                return Result<AccountId>.Failure(accountResult.Errors);
            }

            var account = accountResult.Value;

            // 3. Dùng Unit of Work để lưu Account vào DB
            await _unitOfWork.Repository<Account>().AddAsync(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var confirmationLink = $"https://localhost:7199/api/v1/auth/confirm-email?userId={userId}&token={System.Web.HttpUtility.UrlEncode(confirmToken)}";

            await _emailService.SendEmailConfirmationAsync(request.email, "Confirm your email",
             "Please confirm your email by clicking the link below: " + confirmationLink, cancellationToken);

            return Result<AccountId>.Success(account.Id);
        }
    }
}