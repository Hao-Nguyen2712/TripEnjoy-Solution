using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.Property.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.PropertyType.ValueObjects;

namespace TripEnjoy.Application.Features.Property.Handlers;

public class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, Result<PropertyId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePropertyCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreatePropertyCommandHandler(IUnitOfWork unitOfWork, ILogger<CreatePropertyCommandHandler> logger, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<PropertyId>> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerIdGuid))
        {
            return Result<PropertyId>.Failure(new Error("Property.PartnerIdNotFound", "The partner id was not found in the user's claims.", ErrorType.Unauthorized));
        }

        var partnerId = PartnerId.Create(partnerIdGuid);
        var propertyTypeId = PropertyTypeId.Create(request.PropertyTypeId);

        var propertyResult = Domain.Property.Property.Create(
            partnerId,
            propertyTypeId,
            request.Name,
            request.Address,
            request.City,
            request.Country,
            request.Description,
            request.Latitude,
            request.Longitude
        );

        if (propertyResult.IsFailure)
        {
            _logger.LogError("Failed to create property: {Errors}", string.Join(", ", propertyResult.Errors.Select(e => e.Description)));
            return Result<PropertyId>.Failure(propertyResult.Errors);
        }

        var property = propertyResult.Value;

        await _unitOfWork.Repository<Domain.Property.Property>().AddAsync(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created property {PropertyId}", property.Id.Id);

        return Result<PropertyId>.Success(property.Id);
    }
}
