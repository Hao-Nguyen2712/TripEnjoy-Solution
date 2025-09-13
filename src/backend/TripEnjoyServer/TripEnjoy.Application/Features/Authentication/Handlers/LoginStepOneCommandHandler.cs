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
        public async Task<Result> Handle(LoginStepOneCommand request, CancellationToken cancellationToken)
        {
            return await _authenService.LoginStepOneAsync(request.Email, request.Password);
        }
    }
}
