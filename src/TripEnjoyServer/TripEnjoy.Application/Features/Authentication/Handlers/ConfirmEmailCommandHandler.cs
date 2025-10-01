using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private readonly IAuthenService _authenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ConfirmEmailCommandHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmEmailCommandHandler"/> class with the specified authentication service.
        /// </summary>
        public ConfirmEmailCommandHandler(IAuthenService authenService, IUnitOfWork unitOfWork, ILogger<ConfirmEmailCommandHandler> logger)
        {
            _authenService = authenService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Handles a confirm-email command by delegating to the authentication service to confirm the user's email.
        /// </summary>
        /// <param name="request">The <see cref="ConfirmEmailCommand"/> containing the user ID and confirmation token.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="Result"/> representing the outcome: success when the email was confirmed; failure containing the authentication service errors when confirmation failed.
        /// </returns>
        public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var result = await _authenService.ConfirmEmailAsync(request.UserId, request.Token);

            if (result.IsFailure)
            {
                return Result.Failure(result.Errors);
            }

            var account = await _unitOfWork.Repository<Account>().GetAsync(a => a.AspNetUserId == request.UserId);
            if (account == null)
            {
                _logger.LogError("Identity user {UserId} confirmed email, but no corresponding domain account was found.", request.UserId);
                return Result.Failure(DomainError.Account.NotFound);
            }

            var activationResult = account.MarkAsActive();
            if (activationResult.IsFailure)
            {
                return Result.Failure(activationResult.Errors);
            }


            if (request.ConfirmFor == RoleConstant.User.ToString())
            {
                var addUserResult = account.AddNewUser(null, null, null, null);
                if (addUserResult.IsFailure)
                {
                    return Result.Failure(result.Errors);
                }
            }
            else if (request.ConfirmFor == RoleConstant.Partner.ToString())
            {
                var addPartnerResult = account.AddNewPartner(null, null, null, null);
                if (addPartnerResult.IsFailure)
                {
                    return Result.Failure(result.Errors);
                }
            }
            // add New Wallet for account 
            account.AddWallet(account.Id);     
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save account {AccountId} for Identity User {UserId}.", account.Id, request.UserId);
                return Result.Failure(new Error("ConfirmEmail.Failure", ex.Message, ErrorType.Failure));
            }
            _logger.LogInformation("Account {AccountId} for Identity User {UserId} has been activated.", account.Id, request.UserId);
            return Result.Success();
        }
    }
}