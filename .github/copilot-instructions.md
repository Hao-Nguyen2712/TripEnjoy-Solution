# TripEnjoy Copilot Instructions

## Development Methodology
TripEnjoy follows **Test-Driven Development (TDD)** principles. When implementing new features:

1. **Red**: Write failing tests first (unit tests, then integration tests)
2. **Green**: Write minimal code to make tests pass
3. **Refactor**: Improve code while keeping tests green
4. **Document**: Update context documentation in `docs/` folder after completion

## Architecture Overview
TripEnjoy is a .NET 8 room booking platform implementing Clean Architecture with DDD (Domain-Driven Design). The solution contains **two nested solution files**:
- **Root solution**: `TripEnjoyServer.sln` (contains all 7 projects)  
- **API-only solution**: `src/TripEnjoyServer/TripEnjoyServer.sln` (subset for API development)

The architecture is organized into **7 main projects**:

- **TripEnjoy.Api** - Web API controllers, middleware, and JWT authentication
- **TripEnjoy.Application** - CQRS handlers, MediatR pipeline behaviors, and validation
- **TripEnjoy.Domain** - Domain models, value objects, aggregates with business rules
- **TripEnjoy.Infrastructure** - External service implementations (email, caching, auth services)
- **TripEnjoy.Infrastructure.Persistence** - EF Core, repositories, configurations, and data access
- **TripEnjoy.ShareKernel** - Cross-cutting concerns, shared DTOs, and common models
- **TripEnjoy.Client** - ASP.NET Core MVC frontend with cookie authentication and API integration

## TripEnjoy.Client (Frontend)
The `TripEnjoy.Client` project is an ASP.NET Core MVC application that serves as the frontend.

- **Authentication**: Uses cookie-based authentication (`CookieAuthenticationDefaults.AuthenticationScheme`).
- **API Communication**: Interacts with the `TripEnjoy.Api` using a typed `HttpClient` named `ApiClient`.
- **Token Management**: An `AuthenticationDelegatingHandler` is used to automatically manage and refresh JWT tokens for API requests.
- **Key Routes**:
    - Login: `/authen/sign-in`
    - Logout: `/authen/sign-out`

## Core Patterns

### Result Pattern
All operations return `Result<T>` or `Result` from `TripEnjoy.Domain.Common.Models`:
```csharp
// Success
return Result<PropertyId>.Success(property.Id);

// Failure with domain errors
return Result<PropertyId>.Failure(DomainError.Property.NotFound);
```

### CQRS with MediatR
Commands and queries are handled through MediatR pipeline with automatic validation:
- Commands: `IRequest<Result<T>>` for mutations
- Queries: `IRequest<Result<T>>` for data retrieval
- Handlers: `IRequestHandler<TRequest, TResponse>`

### Domain Error System
Centralized error definitions in `TripEnjoy.Domain.Common.Errors.DomainError`:
```csharp
public static readonly Error NotFound = new(
    "Property.NotFound",
    "The property was not found.",
    ErrorType.NotFound);
```

### API Response Standardization
Controllers inherit from `ApiControllerBase` and use `HandleResult()` to convert domain results to HTTP responses:
```csharp
var result = await _sender.Send(command);
return HandleResult(result, "Operation successful");
```

## Key Development Patterns

### Domain Models
- Use strongly-typed IDs as value objects (e.g., `PropertyId`, `AccountId`) - all inherit from `ValueObject`
- Domain entities have static factory methods: `Property.Create(...)`, `Account.Create(...)`
- Rich domain models with business validation in Create methods and entity methods
- Value objects provide `CreateUnique()` and `Create(Guid id)` factory methods

### Authentication & Authorization
- **JWT-based authentication with two-step login**: 
  - Step 1: `/api/v1/auth/login-step-one` validates email/password, sends OTP
  - Step 2: `/api/v1/auth/login-step-two` validates OTP, returns JWT tokens
- **Client-side authentication**: Uses cookie-based auth with `AuthenticationDelegatingHandler` for automatic JWT token refresh
- **Role-based authorization** using `RoleConstant.Admin`, `RoleConstant.User`, `RoleConstant.Partner`
- **Rate limiting** configured per controller (`[EnableRateLimiting("auth")]` - 5 requests per minute for auth endpoints)

### Validation Pipeline
FluentValidation integrated via `ValidationBehavior<TRequest, TResponse>` - validators are automatically discovered and executed before handlers.

### Dependency Injection Registration
Each layer has its own DI extension:
- `services.AddApplication(configuration)` - MediatR + behaviors
- `services.AddInfrastructure(configuration)` - External services + repositories

## Development Workflow

### Database
- EF Core with automatic migrations on startup
- DbContext: `TripEnjoyDbContext`
- Seeding via `DataSeeder.SeedAsync(services)` - includes PropertyTypes, Roles, and test accounts
- Connection string: `"DefaultConnection"`

### Caching & Background Jobs
- Redis caching via `ICacheService`
- Hangfire for background processing
- Dashboard available at `/hangfire`

### Logging
- Serilog for structured logging
- Request/response logging via `LoggingMiddleware`
- Global exception handling via `ExceptionHandlingMiddleware`

## Development Workflow

### Build Commands
```powershell
# Build root solution (all 7 projects)
dotnet build TripEnjoyServer.sln

# Build API-only solution
dotnet build src/TripEnjoyServer/TripEnjoyServer.sln

# Run API project (auto-applies migrations and seeds data)
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api

# Run MVC Client
dotnet run --project src/TripEnjoyServer/TripEnjoy.Client

# Run with specific launch profile
dotnet run --project src/TripEnjoyServer/TripEnjoy.Api --launch-profile https
```

### Key Development URLs
- API Swagger: `https://localhost:7199/swagger`
- Hangfire Dashboard: `https://localhost:7199/hangfire`
- MVC Client: `https://localhost:7100` (or check launchSettings.json)

## Common Conventions

### File Organization
- Features organized by domain: `Features/{Domain}/{Commands|Queries|Handlers}`
- One handler per file, named: `{Operation}CommandHandler` or `{Operation}QueryHandler`
- DTOs in `TripEnjoy.ShareKernel.Dtos`

### API Versioning
- Controllers use `[ApiVersion("1.0")]` and route `"api/v{version:apiVersion}/..."`
- Swagger UI available in development mode

### Error Handling
- Controllers never throw exceptions - always return Results
- Domain errors are mapped to appropriate HTTP status codes
- Consistent ApiResponse format with error details

## TDD Workflow for New Features

When adding new features, follow TDD principles with the established CQRS pattern:

### 1. Test-First Approach
- **Write unit tests** for command/query handlers first
- **Write integration tests** for API endpoints
- **Write validator tests** for input validation
- Ensure tests fail initially (Red phase)

### 2. Implementation Order
- Create command/query objects
- Implement validators with FluentValidation
- Create handlers implementing business logic
- Add controller endpoints
- Ensure all tests pass (Green phase)

### 3. Documentation Requirements
- **Always update context files** in `docs/TripEnjoy-Project-Context.md` folder after completing tasks
- Document architectural decisions and patterns used
- Include test coverage information
- Update this instruction file when new patterns emerge

### 4. Code Quality Standards
- All database operations should go through the UnitOfWork pattern
- Business logic belongs in domain entities, not handlers
- Follow Result pattern for error handling
- Maintain comprehensive test coverage