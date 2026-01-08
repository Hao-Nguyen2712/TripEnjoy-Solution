# Database Migration Quick Reference

## Quick Start (Automatic)

**The easiest way**: Just run the application! Migrations are applied automatically on startup.

```bash
cd src/TripEnjoyServer/TripEnjoy.Api
dotnet run
```

The application will:
1. Apply any pending migrations automatically
2. Seed initial data (PropertyTypes, Roles, test accounts)
3. Start the API server

## Manual Migration Commands

If you prefer manual control or need to troubleshoot, use these commands:

### Prerequisites

Install EF Core tools (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

### Common Commands

All commands should be run from the **solution root directory**.

#### 1. Apply Migrations
```bash
# Update database to latest migration
dotnet ef database update \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api

# Alternative: Navigate to API folder first
cd src/TripEnjoyServer/TripEnjoy.Api
dotnet ef database update --project ../TripEnjoy.Infrastructure.Persistence
```

#### 2. View Migration Status
```bash
# List all migrations and their status
dotnet ef migrations list \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api
```

#### 3. Create New Migration
```bash
# Add a new migration
dotnet ef migrations add YourMigrationName \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api
```

#### 4. Rollback Migration
```bash
# Remove last migration (if not yet applied)
dotnet ef migrations remove \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api

# Rollback to specific migration
dotnet ef database update MigrationName \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api
```

#### 5. Generate SQL Script
```bash
# Generate SQL for all migrations
dotnet ef migrations script \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api \
  --output migration.sql
```

## Existing Migrations

The project has **3 PostgreSQL migrations**:

| Migration Name | Date | Description |
|----------------|------|-------------|
| `InitialPostgreSQLMigration` | 2025-12-19 | Creates all base tables, Identity, accounts, properties, bookings |
| `AddTransactionAndSettlementEntities` | 2025-12-19 | Adds transaction and settlement support for wallet system |
| `AddReviewAggregate` | 2025-12-20 | Adds review system with images and replies |

## Database Configuration

### Connection String
Located in: `src/TripEnjoyServer/TripEnjoy.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TripEnjoy;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

### DbContext
- **Class**: `TripEnjoyDbContext`
- **Location**: `TripEnjoy.Infrastructure.Persistence`
- **Provider**: Npgsql (PostgreSQL)
- **Inheritance**: `IdentityDbContext<ApplicationUser>`

## Troubleshooting

### "Could not execute because dotnet-ef was not found"
**Solution**: Install EF Core tools globally
```bash
dotnet tool install --global dotnet-ef
```

### "No migrations configuration found"
**Solution**: Ensure you're specifying both projects correctly:
- `--project`: Points to `TripEnjoy.Infrastructure.Persistence` (where DbContext is)
- `--startup-project`: Points to `TripEnjoy.Api` (where appsettings.json is)

### "Connection refused" or "Authentication failed"
**Solution**: 
1. Ensure PostgreSQL is running:
   ```bash
   # Check PostgreSQL status (Linux/Mac)
   sudo systemctl status postgresql
   
   # Or use Docker
   docker ps | grep postgres
   ```
2. Verify connection string credentials match your PostgreSQL setup
3. Create database if it doesn't exist:
   ```bash
   psql -U postgres -c "CREATE DATABASE TripEnjoy;"
   ```

### "Pending model changes detected"
**Solution**: Create a new migration to capture the changes
```bash
dotnet ef migrations add DescriptiveNameForChanges \
  --project src/TripEnjoyServer/TripEnjoy.Infrastructure.Persistence \
  --startup-project src/TripEnjoyServer/TripEnjoy.Api
```

### Migration fails with "relation already exists"
**Solution**: Database is out of sync. Options:
1. **Clean database** (development only):
   ```bash
   # Drop and recreate database
   psql -U postgres -c "DROP DATABASE IF EXISTS TripEnjoy;"
   psql -U postgres -c "CREATE DATABASE TripEnjoy;"
   # Then apply migrations
   dotnet ef database update --project ... --startup-project ...
   ```

2. **Manual sync** (production-like):
   - Check current database state
   - Apply migrations selectively
   - See [POSTGRESQL-MIGRATION.md](POSTGRESQL-MIGRATION.md) for detailed recovery steps

## PostgreSQL-Specific Notes

### Case Sensitivity
PostgreSQL table and column names are **case-sensitive** when quoted. EF Core generates quoted identifiers by default.

```sql
-- Correct
SELECT * FROM "Users"

-- Wrong
SELECT * FROM users
```

### Data Types
PostgreSQL uses different data type names:
- `uuid` instead of `uniqueidentifier`
- `timestamp with time zone` instead of `datetime2`
- `text` instead of `nvarchar(max)`
- `boolean` instead of `bit`
- `numeric(18,2)` instead of `decimal(18,2)`

### Connection Pooling
Npgsql enables connection pooling by default. No additional configuration needed.

## Verification

After running migrations, verify the database:

```bash
# Connect to database
psql -U postgres -d TripEnjoy

# List all tables
\dt

# Check specific table structure
\d "Bookings"

# View data
SELECT * FROM "PropertyTypes";

# Exit
\q
```

## Additional Resources

- **Detailed Guide**: [POSTGRESQL-MIGRATION.md](POSTGRESQL-MIGRATION.md)
- **Project Context**: [TripEnjoy-Project-Context.md](TripEnjoy-Project-Context.md)
- **Database ERD**: [DATABASE-ERD.md](DATABASE-ERD.md)
- **EF Core Documentation**: https://learn.microsoft.com/en-us/ef/core/
- **Npgsql Documentation**: https://www.npgsql.org/efcore/

## Quick Setup (Docker)

For a complete local development environment:

```bash
# Start PostgreSQL
docker run -d -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  --name tripenjoy-postgres \
  postgres:15

# Start Redis (for caching)
docker run -d -p 6379:6379 \
  --name tripenjoy-redis \
  redis:7-alpine

# Start RabbitMQ (optional, for message queue)
docker-compose -f docker-compose.rabbitmq.yml up -d

# Create database
docker exec -it tripenjoy-postgres psql -U postgres -c "CREATE DATABASE TripEnjoy;"

# Run application (applies migrations automatically)
cd src/TripEnjoyServer/TripEnjoy.Api
dotnet run
```

---

**Pro Tip**: For the smoothest experience, just run `dotnet run` from the API project. The application handles migrations automatically! 🚀
