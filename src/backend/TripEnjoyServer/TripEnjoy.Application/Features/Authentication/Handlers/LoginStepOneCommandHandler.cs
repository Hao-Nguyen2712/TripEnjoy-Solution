using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class LoginStepOneCommandHandler : IRequestHandler<LoginStepOneCommand, Result>
    {
        private readonly IAuthenService _authenService;

        /// <summary>
        /// Initializes a new instance of <see cref="LoginStepOneCommandHandler"/> with the required authentication service.
        /// </summary>
        public LoginStepOneCommandHandler(IAuthenService authenService)
        {
            _authenService = authenService;
        }

        /// <summary>
        /// Handles the first step of login by delegating to the authentication service.
        /// </summary>
        /// <remarks>
        /// Forwards the command's Email and Password to <c>IAuthenService.LoginStepOneAsync</c>.
        /// The provided <paramref name="cancellationToken"/> is accepted but not observed by this implementation.
        /// </remarks>
        /// <param name="request">Command containing the user's Email and Password.</param>
        /// <param name="cancellationToken">Cancellation token passed by MediatR; not used by this method.</param>
        /// <returns>A <see cref="Result"/> representing the outcome of the authentication step.</returns>
        public async Task<Result> Handle(LoginStepOneCommand request, CancellationToken cancellationToken)
        {
            return await _authenService.LoginStepOneAsync(request.Email, request.Password);
        }
    }
}
