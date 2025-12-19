using MediatR;
using TripEnjoy.Application.Features.RoomTypes.Queries;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Domain.Common.Errors;
using TripEnjoy.Domain.Common.Models;
using TripEnjoy.Domain.Room;
using TripEnjoy.Domain.Room.ValueObjects;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.RoomTypes.Handlers;

public class GetRoomTypeByIdQueryHandler : IRequestHandler<GetRoomTypeByIdQuery, Result<RoomTypeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoomTypeByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoomTypeDto>> Handle(GetRoomTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var roomTypeId = RoomTypeId.Create(request.RoomTypeId);
        var roomType = await _unitOfWork.Repository<RoomType>()
            .GetByIdAsync(roomTypeId.Value);

        if (roomType == null)
        {
            return Result<RoomTypeDto>.Failure(DomainError.RoomType.NotFound);
        }

        var roomTypeDto = new RoomTypeDto
        {
            Id = roomType.Id.Value,
            PropertyId = roomType.PropertyId.Id,
            RoomTypeName = roomType.RoomTypeName,
            Description = roomType.Description,
            Capacity = roomType.Capacity,
            BasePrice = roomType.BasePrice,
            TotalQuantity = roomType.TotalQuantity,
            Status = roomType.Status,
            AverageRating = roomType.AverageRating,
            ReviewCount = roomType.ReviewCount,
            CreatedAt = roomType.CreatedAt,
            UpdatedAt = roomType.UpdatedAt,
            Images = roomType.RoomTypeImages.Select(img => new RoomTypeImageDto
            {
                Id = img.Id.Value,
                RoomTypeId = img.RoomTypeId.Value,
                FilePath = img.FilePath,
                IsMain = img.IsMain,
                UploadedAt = img.UploadedAt
            }).ToList()
        };

        return Result<RoomTypeDto>.Success(roomTypeDto);
    }
}
