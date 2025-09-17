using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Handlers;

public class AddPropertyImageCommandHandler : IRequestHandler<AddPropertyImageCommand, Result<PropertyImageId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AddPropertyImageCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public AddPropertyImageCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<AddPropertyImageCommandHandler> logger, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<Result<PropertyImageId>> Handle(AddPropertyImageCommand request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerId))
        {
            return Result<PropertyImageId>.Failure(DomainError.Authentication.Unauthorized);
        }

        var property = await _unitOfWork.Properties.GetByIdAsync(request.PropertyId);

        if (property is null)
        {
            return Result<PropertyImageId>.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId.Id != partnerId)
        {
            _logger.LogWarning("Unauthorized attempt to add image to property {PropertyId} by partner {PartnerId}", request.PropertyId, partnerId);
            return Result<PropertyImageId>.Failure(DomainError.Authentication.Forbidden);
        }

        var result = property.AddImage(request.ImageUrl, request.IsCover);
        if (result.IsFailure)
        {
            return Result<PropertyImageId>.Failure(result.Errors);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveByPrefixAsync("properties:all", cancellationToken);
        await _cacheService.RemoveByPrefixAsync($"properties:{property.Id}:images", cancellationToken);

        var addedImage = property.PropertyImages.OrderByDescending(i => i.UploadAt).First();

        return Result<PropertyImageId>.Success(addedImage.Id);
    }
}
