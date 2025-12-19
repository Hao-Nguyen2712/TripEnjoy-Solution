using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace TripEnjoy.Infrastructure.Logging.Enrichers;

/// <summary>
/// Enricher to add user information to log events
/// </summary>
public class UserInfoEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserInfoEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserEmail", userEmail));
            }

            if (!string.IsNullOrEmpty(userRole))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRole", userRole));
            }
        }
    }
}
