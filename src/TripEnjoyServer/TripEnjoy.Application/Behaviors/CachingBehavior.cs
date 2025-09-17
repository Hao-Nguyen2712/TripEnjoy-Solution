using MediatR;
using Microsoft.Extensions.Logging;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Behaviors;

// The behavior is now generic on TRequest and TData (the data to be cached)
public class CachingBehavior<TRequest, TData> : IPipelineBehavior<TRequest, Result<TData>>
    where TRequest : ICacheableQuery<TData>
    where TData : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TData>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TData>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<TData>> Handle(TRequest request, RequestHandlerDelegate<Result<TData>> next, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Checking cache for request {RequestName} with cache key {CacheKey}", typeof(TRequest).Name, request.CacheKey);

        var cachedData = await _cacheService.GetAsync<TData>(request.CacheKey, cancellationToken);
        if (cachedData is not null)
        {
            _logger.LogDebug("Cache hit for request {RequestName} with cache key {CacheKey}", typeof(TRequest).Name, request.CacheKey);
            return Result<TData>.Success(cachedData);
        }

        _logger.LogDebug("Cache miss for request {RequestName} with cache key {CacheKey}", typeof(TRequest).Name, request.CacheKey);

        var response = await next();

        if (response.IsSuccess)
        {
            await _cacheService.SetAsync(request.CacheKey, response.Value, request.Expiration, cancellationToken);
        }

        return response;
    }
}
