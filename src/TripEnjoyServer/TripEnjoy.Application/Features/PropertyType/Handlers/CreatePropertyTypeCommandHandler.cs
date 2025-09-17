using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TripEnjoy.Application.Features.PropertyType.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.PropertyType.ValueObjects;
using TripEnjoy.ShareKernel.Extensions;

namespace TripEnjoy.Application.Features.PropertyType.Handlers;

public class CreatePropertyTypeCommandHandler : IRequestHandler<CreatePropertyTypeCommand, Result<PropertyTypeId>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePropertyTypeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PropertyTypeId>> Handle(CreatePropertyTypeCommand request, CancellationToken cancellationToken)
    {
        var formattedName = request.Name.ToTitleCase();

        var propertyTypeResult = Domain.PropertyType.PropertyType.Create(formattedName);
        if (propertyTypeResult.IsFailure)
        {
            return Result<PropertyTypeId>.Failure(propertyTypeResult.Errors);
        }

        var propertyType = propertyTypeResult.Value;

        await _unitOfWork.Repository<Domain.PropertyType.PropertyType>().AddAsync(propertyType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PropertyTypeId>.Success(propertyType.Id);
    }
}
