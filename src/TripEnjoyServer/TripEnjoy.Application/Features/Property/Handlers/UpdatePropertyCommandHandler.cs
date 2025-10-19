using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Property.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Application.Features.Property.Handlers;

public class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePropertyCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdatePropertyCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdatePropertyCommandHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerId))
        {
            return Result.Failure(new Error("Property.PartnerIdNotFound", "The partner id was not found.", ErrorType.Unauthorized));
        }

        var property = await _unitOfWork.Repository<Domain.Property.Property>().GetByIdAsync(request.PropertyId);

        if (property is null)
        {
            return Result.Failure(DomainError.Property.NotFound);
        }

        // Verify that the property belongs to the current partner
        if (property.PartnerId.Id != partnerId)
        {
            return Result.Failure(new Error("Property.Unauthorized", "You don't have permission to update this property.", ErrorType.Unauthorized));
        }

        var propertyTypeId = PropertyTypeId.Create(request.PropertyTypeId);
        
        // Verify that the property type exists
        var propertyType = await _unitOfWork.Repository<Domain.PropertyType.PropertyType>().GetByIdAsync(request.PropertyTypeId);
        if (propertyType is null)
        {
            return Result.Failure(new Error("PropertyType.NotFound", "The specified property type was not found.", ErrorType.NotFound));
        }

        var updateResult = property.Update(
            propertyTypeId,
            request.Name,
            request.Address,
            request.City,
            request.Country,
            request.Description,
            request.Latitude,
            request.Longitude);

        if (updateResult.IsFailure)
        {
            _logger.LogError("Failed to update property: {Errors}", string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            return updateResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated property {PropertyId}", property.Id.Id);

        return Result.Success();
    }
}