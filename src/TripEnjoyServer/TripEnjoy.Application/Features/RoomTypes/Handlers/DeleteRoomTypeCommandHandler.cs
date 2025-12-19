using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TripEnjoy.Application.Features.RoomTypes.Commands;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Account.ValueObjects;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.ValueObjects;

namespace TripEnjoy.Application.Features.RoomTypes.Handlers;

public class DeleteRoomTypeCommandHandler : IRequestHandler<DeleteRoomTypeCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteRoomTypeCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteRoomTypeCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteRoomTypeCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(DeleteRoomTypeCommand request, CancellationToken cancellationToken)
    {
        // Get partner ID from claims
        var partnerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("PartnerId");
        if (!Guid.TryParse(partnerIdClaim, out var partnerIdGuid))
        {
            return Result.Failure(
                new Error("RoomType.PartnerIdNotFound", "The partner id was not found in the user's claims.", ErrorType.Unauthorized));
        }

        var partnerId = PartnerId.Create(partnerIdGuid);
        var roomTypeId = RoomTypeId.Create(request.RoomTypeId);

        // Get the room type with its property
        var roomType = await _unitOfWork.Repository<RoomType>()
            .GetByIdAsync(roomTypeId.Value);

        if (roomType == null)
        {
            return Result.Failure(DomainError.RoomType.NotFound);
        }

        // Verify that the property belongs to the partner
        var property = await _unitOfWork.Repository<Domain.Property.Property>()
            .GetByIdAsync(roomType.PropertyId.Id);

        if (property == null)
        {
            return Result.Failure(DomainError.Property.NotFound);
        }

        if (property.PartnerId != partnerId)
        {
            return Result.Failure(DomainError.RoomType.Unauthorized);
        }

        // Delete the room type
        await _unitOfWork.Repository<RoomType>().DeleteAsync(roomType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted room type {RoomTypeId}", request.RoomTypeId);

        return Result.Success();
    }
}
