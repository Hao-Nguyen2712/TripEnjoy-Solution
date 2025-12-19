# Enhanced Logging System Documentation

## Overview
The TripEnjoy platform has been upgraded with an advanced logging system built on top of Serilog. This enhanced logging provides structured logging, request tracking with correlation IDs, performance monitoring, security event tracking, and automatic sensitive data masking.

## Architecture

### Components

#### 1. ILogService Interface
Located: `TripEnjoy.Application/Interfaces/Logging/ILogService.cs`

Provides a clean abstraction for structured logging with the following methods:
- `LogInfo(message, args)` - Information level logging
- `LogWarning(message, args)` - Warning level logging  
- `LogError(exception, message, args)` - Error logging with exception details
- `LogDebug(message, args)` - Debug logging (development only)
- `LogPerformance(operationName, elapsedMilliseconds, additionalData)` - Performance metrics
- `LogSecurity(eventType, message, additionalData)` - Security event tracking
- `LogBusinessEvent(eventName, eventData)` - Business event logging

#### 2. LogService Implementation
Located: `TripEnjoy.Infrastructure/Logging/LogService.cs`

Implements `ILogService` and wraps Serilog's `ILogger` for structured logging with enriched context.

#### 3. Custom Serilog Enrichers

##### CorrelationIdEnricher
Located: `TripEnjoy.Infrastructure/Logging/Enrichers/CorrelationIdEnricher.cs`

**Purpose**: Adds correlation ID to all log events for request tracking across services

**Features**:
- Extracts correlation ID from `X-Correlation-ID` header if present
- Uses ASP.NET Core's TraceIdentifier as fallback
- Generates new GUID if no correlation ID exists
- Stores in HttpContext.Items for middleware access

**Usage in Logs**:
```
2025-12-19 14:30:45.123 +00:00 [INF] [abc-123-def] [user@example.com] Request Starting...
```

##### UserInfoEnricher
Located: `TripEnjoy.Infrastructure/Logging/Enrichers/UserInfoEnricher.cs`

**Purpose**: Adds authenticated user information to all log events

**Features**:
- Extracts UserId from JWT claims (ClaimTypes.NameIdentifier)
- Extracts UserEmail from JWT claims (ClaimTypes.Email)
- Extracts UserRole from JWT claims (ClaimTypes.Role)
- Only enriches when user is authenticated

**Usage in Logs**:
```
2025-12-19 14:30:45.123 +00:00 [INF] [abc-123] [user-789] [partner@tripenjoy.com] [Partner] Business Event...
```

##### SensitiveDataMaskingEnricher
Located: `TripEnjoy.Infrastructure/Logging/Enrichers/SensitiveDataMaskingEnricher.cs`

**Purpose**: Automatically masks sensitive data in log messages

**Patterns Masked**:
- Passwords (password=..., password:...)
- Tokens (token=..., token:...)
- API Keys (apikey=..., apikey:...)
- Secrets (secret=..., secret:...)
- Credit card numbers (13-19 digits)
- Social Security Numbers (xxx-xx-xxxx)

**Example**:
```
Input:  "User login with password=MySecret123"
Output: "User login with password=***MASKED***"
```

#### 4. EnhancedLoggingMiddleware
Located: `TripEnjoy.Api/Middleware/EnhancedLoggingMiddleware.cs`

**Purpose**: Request/response logging with correlation ID propagation and performance tracking

**Features**:
- Generates or extracts correlation ID for each request
- Adds `X-Correlation-ID` to response headers
- Logs request start with method, path, IP address
- Tracks request duration with high precision
- Logs slow requests (threshold: 5000ms by default)
- Optional request body logging (configurable for debugging)
- Performance metrics for every request

**Configuration Options** (appsettings.json):
```json
{
  "Logging": {
    "LogRequestBody": false,           // Enable request body logging (use only in dev)
    "SlowRequestThresholdMs": 5000     // Threshold for slow request warnings
  }
}
```

## Configuration

### Serilog Configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithProcessId",
      "WithThreadId"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] [{UserId}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{CorrelationId}] [{UserId}] [{UserRole}] {Message:lj}{NewLine}{Exception}",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/errors/error-.txt",
          "rollingInterval": "Day",
          "restrictedToMinimumLevel": "Error",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{CorrelationId}] [{UserId}] [{UserRole}] {Message:lj}{NewLine}{Exception}",
          "retainedFileCountLimit": 60
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

### Log Sinks

1. **Console**: Real-time logs with correlation ID and user info
2. **File (General)**: Daily rolling logs, 30-day retention
3. **File (Errors)**: Separate error-only logs, 60-day retention
4. **Seq**: Advanced log analysis server (optional)

## Usage Examples

### In Controllers (via Dependency Injection)

```csharp
public class BookingsController : ApiControllerBase
{
    private readonly ILogService _logService;

    public BookingsController(ILogService logService, ISender sender)
    {
        _logService = logService;
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(CreateBookingCommand command)
    {
        _logService.LogBusinessEvent("BookingCreationAttempt", new Dictionary<string, object>
        {
            ["UserId"] = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            ["PropertyId"] = command.PropertyId.ToString()
        });

        var result = await _sender.Send(command);

        if (result.IsSuccess)
        {
            _logService.LogBusinessEvent("BookingCreated", new Dictionary<string, object>
            {
                ["BookingId"] = result.Value.ToString()
            });
        }

        return HandleResult(result, "Booking created successfully");
    }
}
```

### In Command Handlers

```csharp
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingId>>
{
    private readonly ILogService _logService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(ILogService logService, IUnitOfWork unitOfWork)
    {
        _logService = logService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BookingId>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Business logic here
            var booking = Booking.Create(...);
            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            stopwatch.Stop();
            _logService.LogPerformance("CreateBooking", stopwatch.ElapsedMilliseconds, new Dictionary<string, object>
            {
                ["BookingId"] = booking.Id.Value.ToString(),
                ["TotalAmount"] = booking.TotalAmount
            });

            return Result<BookingId>.Success(booking.Id);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logService.LogError(ex, "Failed to create booking after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return Result<BookingId>.Failure(DomainError.Booking.CreationFailed);
        }
    }
}
```

### Security Event Logging

```csharp
// In Authentication Handler
if (loginAttempt.Failed)
{
    _logService.LogSecurity("FailedLogin", "Invalid credentials provided", new Dictionary<string, object>
    {
        ["Email"] = request.Email,
        ["IpAddress"] = httpContext.Connection.RemoteIpAddress?.ToString(),
        ["AttemptCount"] = failedAttempts
    });
}

// In Authorization Middleware
if (!user.HasPermission(resource))
{
    _logService.LogSecurity("UnauthorizedAccess", "User attempted to access restricted resource", new Dictionary<string, object>
    {
        ["UserId"] = user.Id,
        ["ResourceId"] = resource.Id,
        ["RequiredPermission"] = permission.Name
    });
}
```

## Log Levels and When to Use Them

| Level       | Use Case                                                      | Examples                                        |
|-------------|---------------------------------------------------------------|-------------------------------------------------|
| Debug       | Detailed diagnostic info for development                      | Variable values, method entry/exit              |
| Information | General informational messages                                | Request started, operation completed            |
| Warning     | Unexpected but handled situations                             | Slow queries, deprecated API usage              |
| Error       | Error events that might allow app to continue                 | Validation failures, external service errors    |
| Critical    | Serious errors requiring immediate attention                  | Database connection lost, unhandled exceptions  |

## Performance Considerations

1. **Async Logging**: Serilog writes logs asynchronously to minimize performance impact
2. **Buffering**: File sinks use buffering for efficient disk I/O
3. **Sampling**: Consider log sampling for very high-traffic endpoints
4. **Enrichers**: Enrichers add minimal overhead (<1ms per request)
5. **Conditional Logging**: Request body logging disabled by default

## Monitoring and Alerting

### Using Seq (Optional)

Seq provides advanced log analysis capabilities:
- Full-text search across all logs
- Correlation ID-based request tracing
- Performance metric dashboards
- Real-time alerting on errors
- Query language for complex analysis

Access Seq at: `http://localhost:5341`

### Log Queries

```sql
-- Find all slow requests (>5 seconds)
PerformanceData.ElapsedMilliseconds > 5000

-- Find all failed login attempts for a user
SecurityEventType = 'FailedLogin' AND SecurityData.Email = 'user@example.com'

-- Trace a complete request flow by correlation ID
CorrelationId = 'abc-123-def-456'

-- Find all booking creation errors
@Exception IS NOT NULL AND EventName = 'BookingCreated'
```

## Troubleshooting

### Common Issues

**Issue**: Logs not showing correlation IDs
**Solution**: Ensure `EnhancedLoggingMiddleware` is registered in Program.cs after authentication

**Issue**: User info not in logs
**Solution**: Verify JWT token is valid and contains required claims (NameIdentifier, Email, Role)

**Issue**: Sensitive data visible in logs
**Solution**: Check `SensitiveDataMaskingEnricher` patterns match your data format

**Issue**: Logs too verbose
**Solution**: Adjust MinimumLevel in appsettings.json or add specific overrides

## Best Practices

1. **Always use correlation IDs**: Include correlation ID in client requests for end-to-end tracing
2. **Structure your logs**: Use LogBusinessEvent/LogPerformance instead of generic LogInfo when possible
3. **Don't log sensitive data**: Password, tokens, API keys are automatically masked but avoid logging them
4. **Log at appropriate levels**: Don't use Error for validation failures, use Warning
5. **Include context**: Always add relevant data (IDs, counts, etc.) to log messages
6. **Performance logging**: Track slow operations to identify bottlenecks
7. **Security logging**: Log all authentication/authorization events
8. **Test your logs**: Write unit tests to verify logging behavior

## Migration from Old Logging

### Before (Old LoggingMiddleware)
```csharp
_logger.LogInformation("Request Starting: HTTP {RequestMethod} {RequestPath}", requestMethod, requestPath);
```

### After (EnhancedLoggingMiddleware)
```csharp
// Automatic logging with correlation ID, user info, and performance metrics
// No manual logging needed in controllers - middleware handles it
```

### Before (Manual Performance Tracking)
```csharp
var stopwatch = Stopwatch.StartNew();
// ... operation ...
stopwatch.Stop();
_logger.LogInformation("Operation took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

### After (Using ILogService)
```csharp
var stopwatch = Stopwatch.StartNew();
// ... operation ...
stopwatch.Stop();
_logService.LogPerformance("OperationName", stopwatch.ElapsedMilliseconds, additionalData);
```

## Testing

Unit tests for logging components are located in:
`TripEnjoy.Test/UnitTests/Infrastructure/Logging/LogServiceTests.cs`

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~LogServiceTests"
```

All 10 tests should pass:
- LogInfo_ShouldCallLogInformation
- LogWarning_ShouldCallLogWarning
- LogError_ShouldCallLogErrorWithException
- LogDebug_ShouldCallLogDebug
- LogPerformance_WithoutAdditionalData_ShouldLogInformationWithMetrics
- LogPerformance_WithAdditionalData_ShouldLogInformationWithAllData
- LogSecurity_WithoutAdditionalData_ShouldLogWarning
- LogSecurity_WithAdditionalData_ShouldLogWarningWithAllData
- LogBusinessEvent_WithoutEventData_ShouldLogInformation
- LogBusinessEvent_WithEventData_ShouldLogInformationWithAllData

## Conclusion

The enhanced logging system provides comprehensive observability for the TripEnjoy platform. With structured logging, correlation IDs, automatic enrichment, and sensitive data masking, the system enables effective debugging, monitoring, and security auditing while maintaining high performance and user privacy.
