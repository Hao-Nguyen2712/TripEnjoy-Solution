using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.ShareKernel.Dtos;

namespace TripEnjoy.Application.Features.Property.Queries;

// The query now implements the new generic ICacheableQuery interface
public record GetAllPropertiesQuery(int PageNumber, int PageSize)
    : ICacheableQuery<PagedList<PropertySummaryDto>>
{
    public string CacheKey => $"properties:all:page-{PageNumber}:size-{PageSize}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
