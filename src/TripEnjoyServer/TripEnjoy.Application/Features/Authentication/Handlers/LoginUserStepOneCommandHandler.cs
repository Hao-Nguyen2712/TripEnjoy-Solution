using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Constant;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    /// <summary>
    /// Handler for LoginUserStepOneCommand that validates the account has the "User" role
    /// before initiating the OTP login process.
    /// </summary>
    public class LoginUserStepOneCommandHandler : IRequestHandler<LoginUserStepOneCommand, Result>
    {
        private readonly IAuthenService _authenService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoginUserStepOneCommandHandler"/> with the required authentication service.
        /// </summary>
        public LoginUserStepOneCommandHandler(IAuthenService authenService)
        {
            _authenService = authenService;
        }

        /// <summary>
        /// Handles the user login step one command by validating the user role and initiating OTP login.
        /// </summary>
        /// <param name="request">The login command containing email and password for a user account.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A Result indicating success or failure of the login initiation.</returns>
        public async Task<Result> Handle(LoginUserStepOneCommand request, CancellationToken cancellationToken)
        {
            return await _authenService.LoginStepOneAsync(request.Email, request.Password, RoleConstant.User);
        }
    }
}