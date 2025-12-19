using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.RoomTypes.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Application.Features.RoomTypes.Handlers;

public class CreateRoomTypeCommandHandler : IRequestHandler<CreateRoomTypeCommand, Result<RoomTypeId>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateRoomTypeCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateRoomTypeCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateRoomTypeCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<RoomTypeId>> Handle(CreateRoomTypeCommand request, CancellationToken cancellationToken)
    {
        // Get partner ID from claims
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerIdGuid))
        {
            return Result<RoomTypeId>.Failure(
                new Error("RoomType.PartnerIdNotFound", "The partner id was not found in the user's claims.", ErrorType.Unauthorized));
        }

        var partnerId = PartnerId.Create(partnerIdGuid);
        var propertyId = PropertyId.Create(request.PropertyId);

        // Verify that the property exists and belongs to the partner
        var property = await _unitOfWork.Repository<Domain.Property.Property>()
            .GetByIdAsync(propertyId.Id);

        if (property == null)
        {
            return Result<RoomTypeId>.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId != partnerId)
        {
            return Result<RoomTypeId>.Failure(
                new Error("RoomType.Unauthorized", "You are not authorized to create room types for this property.", ErrorType.Unauthorized));
        }

        // Create the room type
        var roomTypeResult = RoomType.Create(
            propertyId,
            request.RoomTypeName,
            request.Capacity,
            request.BasePrice,
            request.TotalQuantity,
            request.Description
        );

        if (roomTypeResult.IsFailure)
        {
            _logger.LogError("Failed to create room type: {Errors}", 
                string.Join(", ", roomTypeResult.Errors.Select(e => e.Description)));
            return Result<RoomTypeId>.Failure(roomTypeResult.Errors);
        }

        var roomType = roomTypeResult.Value;

        await _unitOfWork.Repository<RoomType>().AddAsync(roomType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created room type {RoomTypeId} for property {PropertyId}", 
            roomType.Id.Value, request.PropertyId);

        return Result<RoomTypeId>.Success(roomType.Id);
    }
}
