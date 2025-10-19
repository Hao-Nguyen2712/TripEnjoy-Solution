using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TripEnjoy.Application.Features.Property.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Handlers;

public class GetMyPropertiesQueryHandler : IRequestHandler<GetMyPropertiesQuery, Result<IEnumerable<PropertyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetMyPropertiesQueryHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<IEnumerable<PropertyDto>>> Handle(GetMyPropertiesQuery request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerId))
        {
            return Result<IEnumerable<PropertyDto>>.Failure(new Error("Property.PartnerIdNotFound", "The partner id was not found.", ErrorType.Unauthorized));
        }

        var properties = await _unitOfWork.Properties.GetPropertiesByPartnerIdAsync(partnerId, cancellationToken);

        var propertyDtos = properties.Select(p => new PropertyDto
        {
            Id = p.Id.Id,
            PropertyTypeId = p.PropertyTypeId.Id,
            PropertyTypeName = p.PropertyType.Name,
            Name = p.Name,
            Description = p.Description,
            Address = p.Address,
            City = p.City,
            Country = p.Country,
            Status = p.Status,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            CreatedAt = p.CreatedAt,
            Images = p.PropertyImages.Select(img => new PropertyImageDto
            {
                Id = img.Id.Id,
                ImageUrl = img.ImageUrl,
                IsMain = img.IsMain,
                UploadAt = img.UploadAt
            }).ToList()
        });

        return Result<IEnumerable<PropertyDto>>.Success(propertyDtos);
    }
}
