using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AccountId>>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of <see cref="RegisterUserCommandHandler"/> with required services.
        /// </summary>
        public RegisterUserCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        /// <summary>
        /// Handles user registration by creating an ASP.NET Identity user with User role,
        /// creating the domain Account aggregate, and optionally adding User information.
        /// </summary>
        /// <param name="request">The user registration command containing email, password, and optional full name.</param>
        /// <param name="cancellationToken">Token to cancel async operations.</param>
        /// <returns>A Result containing the created AccountId on success, or errors on failure.</returns>
        public async Task<Result<AccountId>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Check for existing account first to prevent race conditions
            var accountExisting = await _unitOfWork.Repository<Account>()
                .GetAsync(x => x.AccountEmail == request.Email);
            if (accountExisting != null)
            {
                return Result<AccountId>.Failure(DomainError.Account.DuplicateEmail);
            }

            // Create ASP.NET Identity user with User role
            var createUserResult = await _authenService.CreateUserAsync(
                request.Email,
                request.Password,
                RoleConstant.User);

            if (createUserResult.IsFailure)
            {
                return Result<AccountId>.Failure(createUserResult.Errors);
            }

            var (userId, confirmToken) = createUserResult.Value;

            // Create domain Account
            var accountResult = Account.Create(userId, request.Email);
            if (accountResult.IsFailure)
            {
                return Result<AccountId>.Failure(accountResult.Errors);
            }

            var account = accountResult.Value;

            // Add User information if FullName is provided
            if (!string.IsNullOrEmpty(request.FullName))
            {
                var userInfoResult = account.AddUserInformation(request.FullName);
                if (userInfoResult.IsFailure)
                {
                    return Result<AccountId>.Failure(userInfoResult.Errors);
                }
            }

            await _unitOfWork.Repository<Account>().AddAsync(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AccountId>.Success(account.Id);
        }
    }
}