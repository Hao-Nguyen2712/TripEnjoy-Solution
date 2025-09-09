using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private readonly IAuthenService _authenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmEmailCommandHandler"/> class with the specified authentication service.
        /// </summary>
        public ConfirmEmailCommandHandler(IAuthenService authenService)
        {
            _authenService = authenService;
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

            return Result.Success();
        }
    }
}