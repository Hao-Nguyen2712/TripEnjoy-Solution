using MediatR;
using TripEnjoy.Application.Features.RoomTypes.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Property.ValueObjects;
using TripEnjoy.Domain.Room;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.RoomTypes.Handlers;

public class GetRoomTypesByPropertyQueryHandler : IRequestHandler<GetRoomTypesByPropertyQuery, Result<List<RoomTypeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoomTypesByPropertyQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<RoomTypeDto>>> Handle(GetRoomTypesByPropertyQuery request, CancellationToken cancellationToken)
    {
        var propertyId = PropertyId.Create(request.PropertyId);

        // Verify property exists
        var property = await _unitOfWork.Repository<Domain.Property.Property>()
            .GetByIdAsync(propertyId.Id);

        if (property == null)
        {
            return Result<List<RoomTypeDto>>.Failure(DomainError.Property.NotFound);
        }

        // Get all room types for the property
        var roomTypes = await _unitOfWork.Repository<RoomType>()
            .GetAllAsync();

        var propertyRoomTypes = roomTypes
            .Where(rt => rt.PropertyId == propertyId)
            .Select(rt => new RoomTypeDto
            {
                Id = rt.Id.Value,
                PropertyId = rt.PropertyId.Id,
                RoomTypeName = rt.RoomTypeName,
                Description = rt.Description,
                Capacity = rt.Capacity,
                BasePrice = rt.BasePrice,
                TotalQuantity = rt.TotalQuantity,
                Status = rt.Status,
                AverageRating = rt.AverageRating,
                ReviewCount = rt.ReviewCount,
                CreatedAt = rt.CreatedAt,
                UpdatedAt = rt.UpdatedAt,
                Images = rt.RoomTypeImages.Select(img => new RoomTypeImageDto
                {
                    Id = img.Id.Value,
                    RoomTypeId = img.RoomTypeId.Value,
                    FilePath = img.FilePath,
                    IsMain = img.IsMain,
                    UploadedAt = img.UploadedAt
                }).ToList()
            })
            .ToList();

        return Result<List<RoomTypeDto>>.Success(propertyRoomTypes);
    }
}
