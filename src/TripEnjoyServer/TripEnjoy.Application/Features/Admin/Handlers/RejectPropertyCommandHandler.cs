using MediatR;
using TripEnjoy.Application.Features.Admin.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.Admin.Handlers;

public class RejectPropertyCommandHandler : IRequestHandler<RejectPropertyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectPropertyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RejectPropertyCommand request, CancellationToken cancellationToken)
    {
        var propertyId = PropertyId.Create(request.PropertyId);
        var property = await _unitOfWork.Properties.GetByIdAsync(propertyId.Id);

        if (property == null)
        {
            return Result.Failure(DomainError.Property.NotFound);
        }

        var result = property.Reject();
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.Properties.UpdateAsync(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
