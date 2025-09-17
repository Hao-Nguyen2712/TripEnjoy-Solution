using MediatR;
using TripEnjoy.Application.Features.PropertyType.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.PropertyType.Handlers;

public class GetAllPropertyTypesQueryHandler : IRequestHandler<GetAllPropertyTypesQuery, Result<IEnumerable<PropertyTypeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPropertyTypesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<PropertyTypeDto>>> Handle(GetAllPropertyTypesQuery request, CancellationToken cancellationToken)
    {
        var propertyTypes = await _unitOfWork.Repository<Domain.PropertyType.PropertyType>()
            .GetAllAsync();

        var propertyTypeDtos = propertyTypes.Select(pt => new PropertyTypeDto
        {
            Id = pt.Id.Id,
            Name = pt.Name,
            Status = pt.Status
        });

        return Result<IEnumerable<PropertyTypeDto>>.Success(propertyTypeDtos);
    }
}
