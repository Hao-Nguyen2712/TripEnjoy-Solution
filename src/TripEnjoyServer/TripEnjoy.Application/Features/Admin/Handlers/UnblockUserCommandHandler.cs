using MediatR;
using TripEnjoy.Application.Features.Admin.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnblockUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        var accountId = AccountId.Create(request.UserId);
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId.Id);

        if (account == null)
        {
            return Result.Failure(DomainError.Account.NotFound);
        }

        var result = account.MarkAsActive();
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.AccountRepository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
