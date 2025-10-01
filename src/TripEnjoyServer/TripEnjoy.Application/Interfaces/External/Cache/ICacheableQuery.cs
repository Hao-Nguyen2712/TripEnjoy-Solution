using MediatR;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Application.Interfaces.External.Cache;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}

// This is the key change. 
// The query itself will now define both its Result type and the underlying data type.
public interface ICacheableQuery<TData> : ICacheableQuery, IRequest<Result<TData>>
{
}
