using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.PropertyImage.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;

namespace TripEnjoy.Application.Features.PropertyImage.Handlers;

public class DeletePropertyImageCommandHandler : IRequestHandler<DeletePropertyImageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeletePropertyImageCommandHandler> _logger;

    public DeletePropertyImageCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<DeletePropertyImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePropertyImageCommand request, CancellationToken cancellationToken)
    {
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerId))
        {
            return Result.Failure(DomainError.Authentication.Unauthorized);
        }

        var property = await _unitOfWork.Properties.GetByIdAsync(request.PropertyId);
        if (property is null)
        {
            return Result.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId.Id != partnerId)
        {
            _logger.LogWarning("Unauthorized attempt to delete image from property {PropertyId} by partner {PartnerId}", request.PropertyId, partnerId);
            return Result.Failure(DomainError.Authentication.Forbidden);
        }

        var result = property.RemoveImage(PropertyImageId.Create(request.ImageId));
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
