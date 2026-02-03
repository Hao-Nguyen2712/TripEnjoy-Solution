# .NET Aspire Integration Guide

This guide explains how to use .NET Aspire with the TripEnjoy project.

## What is .NET Aspire?

.NET Aspire is a cloud-native application development framework that provides:

- **Orchestration**: Simplified local development and deployment of distributed applications
- **Service Discovery**: Automatic service discovery between microservices
- **Observability**: Built-in OpenTelemetry integration for distributed tracing, metrics, and logging
- **Resilience**: Standard resilience patterns (retry, circuit breaker, timeout) for HTTP clients
- **Health Checks**: Comprehensive health check infrastructure

## Project Structure

The TripEnjoy solution now includes two new Aspire projects:

```
src/TripEnjoyServer/
├── TripEnjoy.AppHost/           # Aspire orchestration project
│   ├── Program.cs               # Defines all services and their dependencies
│   ├── appsettings.json         # Configuration for AppHost
│   └── launchSettings.json      # Launch profiles
│
├── TripEnjoy.ServiceDefaults/   # Shared Aspire configuration
│   └── Extensions.cs            # OpenTelemetry, health checks, service discovery
│
└── TripEnjoy.Api/               # API now uses ServiceDefaults
    └── Program.cs               # Calls builder.AddServiceDefaults()
```

## Running with Aspire

### Prerequisites

1. **.NET 8 SDK** - Already required
2. **Docker Desktop** - Required for running infrastructure services (PostgreSQL, Redis, RabbitMQ)
   - Download from: https://www.docker.com/products/docker-desktop

### Option 1: Run with Aspire Dashboard (Recommended for Development)

The Aspire AppHost provides an orchestration layer that automatically starts all required services:

```bash
# Navigate to the AppHost project
cd src/TripEnjoyServer/TripEnjoy.AppHost

# Run the AppHost
dotnet run
```

This will:
1. Start the Aspire Dashboard at `http://localhost:15000` or `https://localhost:17000`
2. Launch Docker containers for:
   - PostgreSQL database
   - Redis cache
   - RabbitMQ message broker
3. Start the TripEnjoy.Api application
4. Start the TripEnjoy.Client Blazor application

### Option 2: Run Services Individually (Traditional Method)

You can still run services individually as before:

```bash
# Start infrastructure with Docker
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres --name tripenjoy-postgres postgres:15
docker run -d -p 6379:6379 --name tripenjoy-redis redis:7-alpine
docker-compose -f docker-compose.rabbitmq.yml up -d

# Run the API
cd src/TripEnjoyServer/TripEnjoy.Api
dotnet run

# Run the Client
cd src/TripEnjoyServer/TripEnjoy.Client
dotnet run
```

## Aspire Dashboard Features

When running with Aspire, you get access to the Aspire Dashboard at `http://localhost:15000` which provides:

### 1. **Resources View**
- View all running services and their status
- See connection strings and endpoints
- Monitor resource health

### 2. **Console Logs**
- View logs from all services in one place
- Filter by service, log level, or search terms
- Real-time log streaming

### 3. **Traces**
- Distributed tracing across services
- View request flows through the system
- Identify performance bottlenecks

### 4. **Metrics**
- CPU, memory, and request metrics
- Custom application metrics
- Real-time charts and graphs

### 5. **Structured Logs**
- Enhanced logging with structured data
- Correlation IDs for request tracking
- User context information

## Service Discovery

Aspire automatically configures service discovery. The API can discover services by name:

```csharp
// In TripEnjoy.AppHost/Program.cs
var postgres = builder.AddPostgres("postgres")
    .AddDatabase("TripEnjoy");

var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

// The API automatically receives connection strings via environment variables
builder.AddProject<Projects.TripEnjoy_Api>("tripenjoy-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);
```

Connection strings are automatically injected as configuration:
- `ConnectionStrings__TripEnjoy` - PostgreSQL connection
- `ConnectionStrings__redis` - Redis connection
- `ConnectionStrings__rabbitmq` - RabbitMQ connection

## OpenTelemetry Integration

The `ServiceDefaults` project adds OpenTelemetry instrumentation automatically:

### Traces
- Automatic HTTP request tracing
- Database query tracing (Entity Framework Core)
- Custom trace creation:

```csharp
using var activity = Activity.StartActivity("MyOperation");
activity?.SetTag("userId", userId);
// Your code here
```

### Metrics
- ASP.NET Core metrics (requests, duration)
- HTTP client metrics
- Runtime metrics (GC, memory, threads)
- Custom metrics:

```csharp
private static readonly Counter<int> _myCounter = 
    Meter.CreateCounter<int>("my_counter");

_myCounter.Add(1);
```

### Logs
- All Serilog logs are forwarded to OpenTelemetry
- Automatic correlation with traces
- Structured logging support

## Health Checks

ServiceDefaults adds comprehensive health checks:

### Default Endpoints
- `/health` - Overall health status (dev only)
- `/alive` - Liveness check (dev only)

### Monitored Resources
- Database connectivity (Entity Framework Core)
- Redis connectivity (via Aspire)
- RabbitMQ connectivity (via Aspire)

## Resilience Patterns

HTTP clients configured via `ServiceDefaults` automatically include:

### Standard Resilience Handler
- **Retry**: 3 attempts with exponential backoff
- **Circuit Breaker**: Opens after 5 consecutive failures
- **Timeout**: 30 seconds per request
- **Rate Limiter**: Concurrent request limiting

```csharp
// Automatically applied to all HttpClients
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});
```

## Configuration

### AppHost Configuration

Edit `src/TripEnjoyServer/TripEnjoy.AppHost/appsettings.json` to customize:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "",  // Optional: OpenTelemetry endpoint
  "Aspire": {
    "Dashboard": {
      "Port": 15000  // Dashboard port
    }
  }
}
```

### Service Configuration

Services receive configuration automatically via Aspire:
- Connection strings from referenced resources
- Service endpoints via service discovery
- Shared configuration from AppHost

## Deployment

### Development
Use the Aspire AppHost as shown above.

### Production
For production deployments:

1. **Container Orchestration** (Kubernetes, Docker Swarm)
   - Deploy services as containers
   - Use environment variables for configuration
   - Keep existing deployment strategy

2. **Azure Container Apps** (Recommended with Aspire)
   - Aspire can generate Azure Container Apps manifests
   - Automatic scaling and service discovery
   - Built-in observability

```bash
# Generate Azure Container Apps manifest
cd src/TripEnjoyServer/TripEnjoy.AppHost
azd init
azd up
```

3. **Traditional Deployment**
   - Continue using existing deployment methods
   - ServiceDefaults provides observability benefits
   - Infrastructure services run separately

## Troubleshooting

### Docker Connection Issues
If services fail to start:
```bash
# Ensure Docker is running
docker ps

# Check Docker logs
docker logs tripenjoy-postgres
```

### Port Conflicts
If Aspire Dashboard won't start:
- Default ports: 15000 (HTTP), 17000 (HTTPS)
- Change in `launchSettings.json` if needed

### Service Discovery Not Working
- Ensure services reference each other in AppHost
- Check that ServiceDefaults is added to projects
- Verify environment variables are injected

## Benefits Summary

### For Development
- ✅ One command to start entire stack
- ✅ Unified logging and monitoring
- ✅ Built-in distributed tracing
- ✅ Automatic service wiring

### For Production
- ✅ OpenTelemetry-ready observability
- ✅ Standard resilience patterns
- ✅ Comprehensive health checks
- ✅ Easy deployment to cloud platforms

### For Operations
- ✅ Real-time service monitoring
- ✅ Request flow visualization
- ✅ Performance metrics
- ✅ Centralized logging

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Samples](https://github.com/dotnet/aspire-samples)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Aspire Community](https://github.com/dotnet/aspire)

## Migration Notes

### Backward Compatibility
- ✅ Existing manual startup still works
- ✅ No breaking changes to API endpoints
- ✅ Connection strings work as before (when not using Aspire)
- ✅ All existing features preserved

### What Changed
- ✨ Added Aspire AppHost project for orchestration
- ✨ Added ServiceDefaults project for shared configuration
- ✨ API now includes OpenTelemetry instrumentation
- ✨ Enhanced health checks
- ✨ HTTP client resilience patterns

### What Didn't Change
- ✅ API endpoints and contracts
- ✅ Database schema and migrations
- ✅ Authentication and authorization
- ✅ Business logic and domain models
- ✅ Client application functionality
