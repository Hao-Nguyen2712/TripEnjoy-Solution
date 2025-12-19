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

## Testing Strategy

### Test Commands
```bash
# Run all tests
dotnet test

# Run unit tests only (faster, no external dependencies)
dotnet test --filter "Category!=Integration"

# Run integration tests (requires Redis and SQL Server)
dotnet test --filter "Category=Integration"

# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test project
dotnet test src/TripEnjoyServer/TripEnjoy.Test/TripEnjoy.Test.csproj

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### Test Organization
- **Unit Tests**: Located in `TripEnjoy.Test/UnitTests/`
  - Test handlers, validators, domain logic in isolation
  - Use Moq for mocking dependencies
  - No database or external service dependencies
  
- **Integration Tests**: Located in `TripEnjoy.Test/IntegrationTests/`
  - Test API endpoints end-to-end
  - Require running Redis (localhost:6379)
  - Use in-memory database or test database
  - Tagged with `[Trait("Category", "Integration")]`

### Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccess()

[Fact]
public async Task Handle_InvalidPropertyId_ReturnsNotFoundError()
```

### Test Data Generation
- Use AutoFixture for test data generation
- Use FluentAssertions for readable assertions
- Mock external services (Cloudinary, Email) in unit tests

## Code Quality & Linting

### Code Style
- Follow C# coding conventions and .NET naming guidelines
- Use nullable reference types (`#nullable enable`)
- Prefer explicit over implicit typing for clarity
- Keep methods small and focused (Single Responsibility)

### EditorConfig
The project uses `.editorconfig` for consistent formatting:
- 4 spaces for indentation (no tabs)
- UTF-8 encoding
- LF line endings
- Trim trailing whitespace

### Common Code Patterns to Follow
```csharp
// ✅ Good: Using Result pattern
public async Task<Result<PropertyId>> Handle(CreatePropertyCommand request)
{
    var property = Property.Create(...);
    await _unitOfWork.PropertyRepository.AddAsync(property);
    await _unitOfWork.SaveChangesAsync();
    return Result<PropertyId>.Success(property.Id);
}

// ❌ Bad: Throwing exceptions for business logic
public async Task<PropertyId> Handle(CreatePropertyCommand request)
{
    if (!await _propertyTypeRepository.ExistsAsync(request.PropertyTypeId))
        throw new NotFoundException("Property type not found");
    // ...
}

// ✅ Good: Strongly-typed IDs
var propertyId = PropertyId.CreateUnique();
var property = await _repository.GetByIdAsync(propertyId);

// ❌ Bad: Using raw Guids
Guid propertyId = Guid.NewGuid();
var property = await _repository.GetByIdAsync(propertyId);
```

## Environment Setup

### Prerequisites
1. **.NET 8 SDK** - Download from https://dotnet.microsoft.com/download
2. **SQL Server** - Local instance or connection to remote server
3. **Redis** - Required for caching and integration tests
   ```bash
   # Run Redis via Docker
   docker run -d -p 6379:6379 redis:7-alpine
   
   # Or install locally on Windows/Mac/Linux
   # Windows: https://github.com/microsoftarchive/redis/releases
   # Mac: brew install redis
   # Linux: apt-get install redis-server
   ```
4. **Cloudinary Account** (for image storage) - Sign up at https://cloudinary.com

### Configuration
Update `appsettings.Development.json` in the API project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TripEnjoyDb;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "CacheSettings": {
    "ConnectionString": "localhost:6379"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

### Database Setup
```bash
# Navigate to API project
cd src/TripEnjoyServer/TripEnjoy.Api

# Apply migrations (creates database and schema)
dotnet ef database update

# Or run the API (auto-applies migrations on startup)
dotnet run
```

## Troubleshooting Common Issues

### Database Issues
**Problem**: "Cannot open database" or connection errors
**Solution**: 
- Verify SQL Server is running
- Check connection string in `appsettings.Development.json`
- Run `dotnet ef database update` manually
- Check if database exists with proper permissions

### Redis Issues
**Problem**: Integration tests failing with "Redis connection failed"
**Solution**:
- Ensure Redis is running on localhost:6379
- Test connection: `redis-cli ping` should return `PONG`
- Check firewall settings
- For Docker: `docker ps` to verify container is running

### Migration Issues
**Problem**: "Pending model changes" error
**Solution**:
```bash
# Add new migration
cd src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence
dotnet ef migrations add YourMigrationName --startup-project ../TripEnjoy.Api

# Remove last migration if needed
dotnet ef migrations remove --startup-project ../TripEnjoy.Api
```

### Authentication Issues
**Problem**: JWT token invalid or expired
**Solution**:
- Tokens expire after configured time (check JWT settings)
- Use refresh token endpoint to get new access token
- Clear browser cookies/local storage for client app
- Check token is included in Authorization header: `Bearer {token}`

### Build Issues
**Problem**: "The type or namespace could not be found"
**Solution**:
```bash
# Clean and restore
dotnet clean
dotnet restore

# Build in correct order
dotnet build TripEnjoyServer.sln
```

## Security Best Practices

### Input Validation
- **Always validate** input at multiple layers:
  1. Client-side validation (DataAnnotations in ViewModels)
  2. FluentValidation in Application layer
  3. Domain entity validation in Create/Update methods

### Authentication & Authorization
- Use `[Authorize(Roles = RoleConstant.Partner)]` for role-based endpoints
- Verify resource ownership in handlers (e.g., partner can only edit own properties)
- Never trust client-provided IDs without verification
- Use rate limiting on sensitive endpoints

### Sensitive Data
- **Never** commit secrets to source control
- Use User Secrets for local development: `dotnet user-secrets set "Key" "Value"`
- Use environment variables or Azure Key Vault in production
- Cloudinary API keys should be in configuration, not code

### SQL Injection Prevention
- EF Core parameterizes queries automatically
- Avoid raw SQL queries when possible
- If using raw SQL, always use parameterized queries

### XSS Prevention
- Razor automatically HTML-encodes output
- Be careful with `@Html.Raw()` - only use with trusted content
- Validate and sanitize all user input

## Common Development Scenarios

### Adding a New Feature (TDD Approach)

1. **Write failing unit test** for command/query handler
   ```csharp
   [Fact]
   public async Task Handle_ValidRequest_ReturnsSuccess()
   {
       // Arrange
       var command = new CreateFeatureCommand(...);
       // Act & Assert - expect NotImplementedException initially
   }
   ```

2. **Create command/query** in `TripEnjoy.Application/Features/{Domain}/Commands/`
   ```csharp
   public record CreateFeatureCommand(...) : IRequest<Result<FeatureId>>;
   ```

3. **Create validator** with FluentValidation
   ```csharp
   public class CreateFeatureCommandValidator : AbstractValidator<CreateFeatureCommand>
   {
       public CreateFeatureCommandValidator()
       {
           RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
       }
   }
   ```

4. **Create handler** implementing business logic
   ```csharp
   public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, Result<FeatureId>>
   {
       public async Task<Result<FeatureId>> Handle(CreateFeatureCommand request, CancellationToken ct)
       {
           // Implementation
       }
   }
   ```

5. **Add controller endpoint** in `TripEnjoy.Api/Controllers/`
   ```csharp
   [HttpPost]
   public async Task<IActionResult> CreateFeature([FromBody] CreateFeatureCommand command)
   {
       var result = await _sender.Send(command);
       return HandleResult(result);
   }
   ```

6. **Run tests** and ensure they pass
   ```bash
   dotnet test --filter "Category!=Integration"
   ```

7. **Add integration test** for API endpoint
   ```csharp
   [Fact]
   [Trait("Category", "Integration")]
   public async Task CreateFeature_WithValidData_ReturnsOk()
   {
       // Test API endpoint end-to-end
   }
   ```

### Adding a Database Entity

1. **Create domain entity** in `TripEnjoy.Domain/Entities/`
2. **Add strongly-typed ID** value object
3. **Add entity configuration** in `TripEnjoy.Infrastructure.Persistence/Configurations/`
4. **Add DbSet** to `TripEnjoyDbContext`
5. **Create migration**:
   ```bash
   cd src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence
   dotnet ef migrations add AddNewEntity --startup-project ../TripEnjoy.Api
   ```
6. **Review migration** file for correctness
7. **Apply migration**: `dotnet ef database update --startup-project ../TripEnjoy.Api`

## Performance Considerations

### Database Queries
- Use `AsNoTracking()` for read-only queries
- Include related entities explicitly: `.Include(x => x.RelatedEntity)`
- Avoid N+1 queries - use eager loading or projection
- Add pagination for list queries to prevent loading too much data

### Caching
- Cache frequently accessed, rarely changing data
- Use `ICacheService` for distributed caching
- Set appropriate expiration times
- Cache invalidation on entity updates

### Background Jobs
- Use Hangfire for long-running operations
- Don't block HTTP requests with heavy processing
- Examples: Email sending, image processing, report generation

## Documentation Requirements

After implementing a feature, update:
1. **This file** (copilot-instructions.md) if new patterns emerge
2. **docs/TripEnjoy-Project-Context.md** for business context changes
3. **API documentation** via XML comments for Swagger
4. **README.md** if setup instructions change

## Quick Reference

### Useful Commands
```bash
# Check solution builds
dotnet build

# Run API with hot reload
dotnet watch run --project src/TripEnjoyServer/TripEnjoy.Api

# Run client MVC app
dotnet run --project src/TripEnjoyServer/TripEnjoy.Client

# Format code (if dotnet-format is installed)
dotnet format

# List migrations
dotnet ef migrations list --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence --startup-project src/TripEnjoyServer/TripEnjoy.Api
```

### Key Directories
- `TripEnjoy.Api/Controllers/` - API endpoints
- `TripEnjoy.Application/Features/` - CQRS handlers and commands
- `TripEnjoy.Domain/Entities/` - Domain models and business logic
- `TripEnjoy.Infrastructure.Persistence/` - Database configurations and repositories
- `TripEnjoy.Client/Views/` - MVC Razor views
- `TripEnjoy.Test/UnitTests/` - Unit tests
- `TripEnjoy.Test/IntegrationTests/` - API integration tests