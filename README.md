# TripEnjoy - Room Booking Platform

TripEnjoy is an enterprise-grade room booking platform built with .NET 8 that connects travelers with accommodation partners. The platform implements Clean Architecture with Domain-Driven Design (DDD) principles.

## 🛠️ Tech Stack

### Backend
- **.NET 8** - Core platform
- **.NET Aspire 9.1** - Cloud-native orchestration and observability
- **Entity Framework Core 8** - ORM with Npgsql provider
- **PostgreSQL** - Database (localhost:5432)
- **MediatR 11** - CQRS implementation
- **FluentValidation 12** - Input validation
- **JWT Bearer** - Authentication
- **Redis** - Distributed caching
- **Hangfire** - Background jobs with PostgreSQL storage
- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing and metrics
- **Cloudinary** - Image storage
- **RabbitMQ** - Message broker with MassTransit

### Frontend
- **Blazor WebAssembly** - Client-side SPA framework
- **MudBlazor** - Material Design component library
- **Blazored.LocalStorage** - Browser local storage abstraction

### Testing
- **xUnit 2.5** - Test framework
- **Moq 4.20** - Mocking framework
- **FluentAssertions 6.12** - Assertion library
- **AutoFixture** - Test data generation

## 🚀 Getting Started

### Prerequisites

Before running the application, ensure you have the following installed:

1. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Docker Desktop** (Recommended for Aspire) - [Download here](https://www.docker.com/products/docker-desktop)
   - Or manually install: **PostgreSQL 12+**, **Redis**, **RabbitMQ** (see below)

### Running the Application

#### Option 1: Run with .NET Aspire (Recommended) ⭐

The easiest way to run the entire application stack with one command:

```bash
# Navigate to the AppHost project
cd src/TripEnjoyServer/TripEnjoy.AppHost

# Run the orchestration
dotnet run
```

This automatically:
- ✅ Starts PostgreSQL, Redis, and RabbitMQ in Docker containers
- ✅ Applies database migrations
- ✅ Starts the API and Client applications
- ✅ Opens the Aspire Dashboard at `http://localhost:15088` for monitoring

**See [Aspire Setup Guide](docs/ASPIRE-SETUP.md) for detailed information.**

#### Option 2: Run Manually (Traditional Method)

If you prefer to run services individually:

1. **Start Infrastructure Services**

   **PostgreSQL:**
   ```bash
   docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres --name tripenjoy-postgres postgres:15
   ```

   **Redis:**
   ```bash
   docker run -d -p 6379:6379 --name tripenjoy-redis redis:7-alpine
   ```

   **RabbitMQ:**
   ```bash
   docker-compose -f docker-compose.rabbitmq.yml up -d
   # Or:
   docker run -d -p 5672:5672 -p 15672:15672 --name tripenjoy-rabbitmq rabbitmq:3-management-alpine
   ```

2. **Setup Database**
   ```bash
   # Connect to PostgreSQL
   psql -U postgres
   
   # Create database
   CREATE DATABASE TripEnjoy;
   
   # Exit psql
   \q
   ```

   **Configure Connection String**
   
   The connection string is already configured in `src/TripEnjoyServer/TripEnjoy.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=TripEnjoy;Username=postgres;Password=postgres;Include Error Detail=true"
     }
   }
   ```
   
   Update the username and password if you're using different credentials.

   **Apply Database Migrations**
   
   The application automatically applies migrations on startup. Alternatively, you can manually apply migrations:
   
   ```bash
   # From the solution root directory (where TripEnjoyServer.sln is located)
   dotnet ef database update --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence --startup-project src/TripEnjoyServer/TripEnjoy.Api
   ```
   
   **Note:** If you don't have EF Core tools installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Run the API**
   ```bash
   cd src/TripEnjoyServer/TripEnjoy.Api
   dotnet run
   ```
   
   The API will be available at:
   - HTTPS: `https://localhost:7199`
   - HTTP: `http://localhost:5000`
   - Swagger UI: `https://localhost:7199/swagger`
   - Hangfire Dashboard: `https://localhost:7199/hangfire`

4. **Run the Blazor Client** (optional)
   ```bash
   cd src/TripEnjoyServer/TripEnjoy.Client
   dotnet run
   ```

### Database Migrations

The project includes the following PostgreSQL migrations:

1. **InitialPostgreSQLMigration** - Creates all base tables and relationships
2. **AddTransactionAndSettlementEntities** - Adds transaction and settlement support
3. **AddReviewAggregate** - Adds review system with images and replies

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category!=Integration"

# Run with detailed output
dotnet test --verbosity detailed
```

## 📚 Documentation

For more detailed information, see:

- **[.NET Aspire Setup Guide](docs/ASPIRE-SETUP.md)** - Complete guide for Aspire orchestration and observability ⭐
- [Migration Quick Start](docs/MIGRATION-QUICKSTART.md) - Quick reference for database migrations
- [PostgreSQL Migration Guide](docs/POSTGRESQL-MIGRATION.md) - Complete guide for PostgreSQL setup and migration
- [Project Architecture](docs/PROJECT-ANALYSIS.md) - Architecture and design patterns
- [Admin Features](docs/Admin-Features-Documentation.md) - Admin functionality documentation
- [Message Queue Setup](docs/MESSAGE-QUEUE-SETUP-GUIDE.md) - RabbitMQ configuration guide
- [Database ERD](docs/DATABASE-ERD.md) - Database schema and relationships

## 🏗️ Project Structure

```
TripEnjoy-Solution/
├── src/TripEnjoyServer/
│   ├── TripEnjoy.Api/                      # Web API layer
│   ├── TripEnjoy.Application/              # CQRS handlers, validators
│   ├── TripEnjoy.Domain/                   # Domain models, aggregates
│   ├── TripEnjoy.Infrastructure/           # External services
│   ├── TripEnjoy.Infrastructure.Persistence/ # EF Core, repositories, migrations
│   ├── TripEnjoy.ShareKernel/              # Shared DTOs, models
│   ├── TripEnjoy.Client/                   # Blazor WebAssembly
│   └── TripEnjoy.Test/                     # Unit and integration tests
└── docs/                                    # Documentation
```

## 🔧 Configuration

Key configuration files:

- `appsettings.json` - Database, JWT, email, caching, payment settings
- `appsettings.Development.json` - Development-specific overrides
- `.editorconfig` - Code style rules

## 🛡️ Security Features

- JWT-based authentication with two-step login (OTP)
- Role-based authorization (Admin, Partner, User)
- Rate limiting on sensitive endpoints
- Secure password hashing with ASP.NET Core Identity
- HTTPS enforcement

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow TDD principles (write tests first)
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

## 📝 License

This project is proprietary software.


