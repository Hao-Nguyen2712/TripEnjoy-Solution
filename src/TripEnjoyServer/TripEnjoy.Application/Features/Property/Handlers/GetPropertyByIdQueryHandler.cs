using MediatR;
using TripEnjoy.Application.Features.Property.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Handlers;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, Result<PropertyDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPropertyByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PropertyDto>> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var propertyId = PropertyId.Create(request.PropertyId);
        var property = await _unitOfWork.Properties.GetByIdWithImagesAsync(propertyId);

        if (property is null)
        {
            return Result<PropertyDto>.Failure(DomainError.Property.NotFound);
        }

        var propertyDto = new PropertyDto
        {
            Id = property.Id.Id,
            PropertyTypeId = property.PropertyTypeId.Id,
            PropertyTypeName = property.PropertyType.Name,
            Name = property.Name,
            Description = property.Description,
            Address = property.Address,
            City = property.City,
            Country = property.Country,
            Status = property.Status,
            AverageRating = property.AverageRating,
            ReviewCount = property.ReviewCount,
            CreatedAt = property.CreatedAt,
            Images = property.PropertyImages.Select(img => new PropertyImageDto
            {
                Id = img.Id.Id,
                ImageUrl = img.ImageUrl,
                IsMain = img.IsMain,
                UploadAt = img.UploadAt
            }).ToList()
        };

        return Result<PropertyDto>.Success(propertyDto);
    }
}
