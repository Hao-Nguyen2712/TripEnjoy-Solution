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
    /// <summary>
    /// Legacy registration handler. Use RegisterUserCommandHandler or RegisterPartnerCommandHandler instead.
    /// </summary>
    [Obsolete("Use RegisterUserCommandHandler or RegisterPartnerCommandHandler instead. This will be removed in a future version.", false)]
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AccountId>>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes a new instance of <see cref="RegisterCommandHandler"/> with required services.
        /// </summary>
        /// <remarks>
        /// Dependencies (injected): authentication service, unit of work, and email service used to create users, persist accounts, and send confirmation emails.
        /// </remarks>
        public RegisterCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        /// <summary>
        /// Handles a registration request by creating an authentication user, building the domain Account aggregate,
        /// persisting the account, and sending an email confirmation link to the provided address.
        /// </summary>
        /// <param name="request">The registration command containing the user's email and password.</param>
        /// <param name="cancellationToken">Token to cancel async persistence and email operations.</param>
        /// <returns>
        /// A <see cref="Result{AccountId}"/> that is successful with the created account's Id on success,
        /// or a failure containing the originating errors if user creation or account creation fails.
        /// </returns>
        public async Task<Result<AccountId>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var role = request.confirmFor?.ToUpper() == RoleConstant.User.ToString().ToUpper() ? RoleConstant.User : RoleConstant.Partner;
            var createUserResult = await _authenService.CreateUserAsync(request.email, request.password, role.ToString());
            if (createUserResult.IsFailure)
            {
                return Result<AccountId>.Failure(createUserResult.Errors);
            }

            var (userId, confirmToken) = createUserResult.Value;

            var accountExisting = await _unitOfWork.Repository<Account>().GetAsync(x => x.AccountEmail == request.email);
            if (accountExisting != null)
            {
                return Result<AccountId>.Failure(DomainError.Account.DuplicateEmail);
            }

            var accountResult = Account.Create(userId, request.email);
            if (accountResult.IsFailure)
            {
                return Result<AccountId>.Failure(accountResult.Errors);
            }

            var account = accountResult.Value;

            await _unitOfWork.Repository<Account>().AddAsync(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AccountId>.Success(account.Id);
        }
    }
}