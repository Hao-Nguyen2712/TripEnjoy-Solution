using MediatR;
using TripEnjoy.Application.Features.Authentication.Commands;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Authentication.Handlers
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result>
    {
        private readonly IAuthenService _authenService;

        public ConfirmEmailCommandHandler(IAuthenService authenService)
        {
            _authenService = authenService;
        }

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