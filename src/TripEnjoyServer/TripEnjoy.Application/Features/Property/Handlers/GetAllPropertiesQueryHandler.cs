using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TripEnjoy.Application.Features.Property.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Handlers;

public class GetAllPropertiesQueryHandler : IRequestHandler<GetAllPropertiesQuery, Result<PagedList<PropertySummaryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPropertiesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedList<PropertySummaryDto>>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
    {
        var (properties, totalCount) = await _unitOfWork.Properties.GetAllPaginatedAsync(request.PageNumber, request.PageSize, cancellationToken);

        var propertyDtos = properties.Select(p => new PropertySummaryDto
        {
            Id = p.Id.Id,
            Name = p.Name,
            City = p.City,
            Country = p.Country,
            PropertyTypeName = p.PropertyType.Name,
            AverageRating = p.AverageRating,
            CoverImageUrl = p.PropertyImages.FirstOrDefault(img => img.IsMain)?.ImageUrl
        }).ToList();

        var pagedList = new PagedList<PropertySummaryDto>(propertyDtos, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedList<PropertySummaryDto>>.Success(pagedList);
    }
}
