using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Reviews.Queries;

public record GetReviewsByRoomTypeQuery(Guid RoomTypeId, int PageNumber = 1, int PageSize = 10)
    : ICacheableQuery<PagedList<ReviewDto>>
{
    public string CacheKey => $"reviews:roomtype-{RoomTypeId}:page-{PageNumber}:size-{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
