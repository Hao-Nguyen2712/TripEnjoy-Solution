using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Application.Common.Models;

namespace TripEnjoy.Application.Behaviors;

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableCommand<TResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public AuditLoggingBehavior(IHttpContextAccessor httpContextAccessor, IBackgroundJobClient backgroundJobClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Process the request first
        var response = await next();

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue("AccountId") ?? "System";

        // Serialize the entire request as the 'NewValue' for the audit log.
        var requestJson = JsonSerializer.Serialize(request);

        // Truncate JSON if it's too long to prevent database errors
        const int maxLength = 3800; // Leave some buffer from the 4000 character limit
        var truncatedJson = requestJson.Length > maxLength
            ? requestJson.Substring(0, maxLength) + "... [TRUNCATED]"
            : requestJson;

        var logEntry = new AuditLogEntry
        {
            UserId = userId,
            Action = typeof(TRequest).Name,
            EntityName = typeof(TRequest).Name.Replace("Command", ""), // Simple convention-based entity name
            NewValue = truncatedJson
        };

        // Enqueue the audit logging to Hangfire to be processed in the background
        _backgroundJobClient.Enqueue<IAuditLogService>(service => service.CreateAuditLogAsync(logEntry, CancellationToken.None));

        return response;
    }
}
