using MediatR;
using TripEnjoy.Application.Features.Admin.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class ApprovePartnerCommandHandler : IRequestHandler<ApprovePartnerCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePartnerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApprovePartnerCommand request, CancellationToken cancellationToken)
    {
        var partnerId = PartnerId.Create(request.PartnerId);
        
        var accounts = await _unitOfWork.Repository<Domain.Account.Account>().GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.Partner != null && a.Partner.Id == partnerId);

        if (account?.Partner == null)
        {
            return Result.Failure(new Error("Partner.NotFound", "Partner not found.", ErrorType.NotFound));
        }

        var result = account.Partner.Approve();
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.Repository<Domain.Account.Account>().UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
