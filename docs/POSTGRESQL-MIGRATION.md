# PostgreSQL Migration Guide

## Overview
The TripEnjoy application has been migrated from Microsoft SQL Server to PostgreSQL for .NET 8.

## Changes Made

### 1. Package Updates

**Infrastructure Project (`TripEnjoy.Infrastructure.csproj`)**
- Removed: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.4)
- Removed: `Hangfire.SqlServer` (1.8.21)
- Added: `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.4)
- Added: `Hangfire.PostgreSql` (1.20.9)
- Added: `System.IdentityModel.Tokens.Jwt` (8.0.0)

**Persistence Project (`TripEnjoy.Infrastructure.Persistence.csproj`)**
- Removed: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.4)
- Added: `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.4)

### 2. Code Changes

**DependencyInjection.cs**
```csharp
// Before
options.UseSqlServer(
    configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(TripEnjoyDbContext).Assembly.FullName))

// After
options.UseNpgsql(
    configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(TripEnjoyDbContext).Assembly.FullName))
```

**Program.cs**
```csharp
// Before
using Hangfire.Redis.StackExchange;
.UseRedisStorage(configuration.GetValue<string>("CacheSettings:ConnectionString"))

// After
using Hangfire.PostgreSql;
.UsePostgreSqlStorage(options => 
    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
```

### 3. Connection String Format

**Before (SQL Server)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=DESKTOP-JI4CSAU;Initial Catalog=TripEnjoy;User ID=sa;Password=***;Trusted_Connection=False;Encrypt=True;Trust Server Certificate=True"
  }
}
```

**After (PostgreSQL)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TripEnjoy;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

### 4. Database Migrations

All previous SQL Server migrations have been removed and replaced with a new initial migration for PostgreSQL:
- Migration: `InitialPostgreSQLMigration`
- Creates all tables, relationships, and constraints for PostgreSQL

## Setup Instructions

### Prerequisites
1. **PostgreSQL Server** (version 12 or higher recommended)
   - Download from: https://www.postgresql.org/download/
   - Or use Docker: `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres --name tripenjoy-postgres postgres:15`

2. **pgAdmin** (optional, for database management)
   - Download from: https://www.pgadmin.org/download/

### Installation Steps

1. **Install PostgreSQL**
   ```bash
   # On Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install postgresql postgresql-contrib
   
   # On macOS (with Homebrew)
   brew install postgresql@15
   brew services start postgresql@15
   
   # On Windows
   # Download and run the installer from postgresql.org
   ```

2. **Create Database**
   ```bash
   # Connect to PostgreSQL
   psql -U postgres
   
   # Create database
   CREATE DATABASE TripEnjoy;
   
   # Create user (if needed)
   CREATE USER tripenjoyuser WITH PASSWORD 'your_password';
   GRANT ALL PRIVILEGES ON DATABASE TripEnjoy TO tripenjoyuser;
   ```

3. **Update Connection String**
   
   Update `appsettings.json` or `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=TripEnjoy;Username=postgres;Password=your_password;Include Error Detail=true"
     }
   }
   ```

4. **Apply Migrations**
   ```bash
   cd src/TripEnjoyServer/TripEnjoy.Api
   dotnet ef database update --project ../TripEnjoy.Infrastructure.Persistence
   ```
   
   Or simply run the application - migrations are applied automatically on startup.

5. **Verify Setup**
   ```bash
   # Connect to database
   psql -U postgres -d TripEnjoy
   
   # List tables
   \dt
   
   # Check a specific table
   \d "Bookings"
   ```

## PostgreSQL vs SQL Server Differences

### Case Sensitivity
- PostgreSQL table and column names are case-sensitive when quoted
- EF Core generates quoted identifiers by default
- Use exact casing when querying: `SELECT * FROM "Users"` (not `users`)

### Data Types
- `nvarchar(max)` → `text`
- `datetime2` → `timestamp without time zone`
- `decimal(18,2)` → `numeric(18,2)`
- `bit` → `boolean`
- `uniqueidentifier` → `uuid`

### String Functions
- `CHARINDEX()` → `POSITION()`
- `LEN()` → `LENGTH()`
- `STUFF()` → `OVERLAY()`

### Sequence Management
PostgreSQL uses sequences differently:
```sql
-- SQL Server
SELECT SCOPE_IDENTITY()

-- PostgreSQL
SELECT lastval()
```

## Hangfire Configuration

Hangfire now uses PostgreSQL instead of Redis for job storage:
- Jobs are persisted in the same database
- Dashboard remains at `/hangfire`
- No Redis dependency required for Hangfire

## Performance Considerations

1. **Indexes**: All existing indexes are maintained in the migration
2. **Constraints**: Foreign keys and unique constraints preserved
3. **Connection Pooling**: PostgreSQL uses Npgsql connection pooling by default
4. **Vacuum**: Consider setting up automatic vacuuming:
   ```sql
   ALTER TABLE "Bookings" SET (autovacuum_vacuum_scale_factor = 0.1);
   ```

## Troubleshooting

### Connection Refused
```
Error: connection refused
```
**Solution**: Ensure PostgreSQL is running
```bash
# Check status
sudo systemctl status postgresql

# Start service
sudo systemctl start postgresql
```

### Authentication Failed
```
Error: password authentication failed for user "postgres"
```
**Solution**: Update `pg_hba.conf` to allow password authentication
```bash
# Edit pg_hba.conf
sudo nano /etc/postgresql/15/main/pg_hba.conf

# Change method from 'peer' to 'md5'
local   all   all   md5
host    all   all   127.0.0.1/32   md5
```

### Migration Issues
```
Error: relation "..." does not exist
```
**Solution**: Drop and recreate the database
```sql
DROP DATABASE IF EXISTS TripEnjoy;
CREATE DATABASE TripEnjoy;
```
Then apply migrations again.

### Port Already in Use
```
Error: port 5432 is already in use
```
**Solution**: Either stop the conflicting service or change PostgreSQL port
```bash
# Change port in postgresql.conf
sudo nano /etc/postgresql/15/main/postgresql.conf
# Set: port = 5433

# Update connection string accordingly
"Host=localhost;Port=5433;Database=TripEnjoy;..."
```

## Backup and Restore

### Backup
```bash
pg_dump -U postgres -d TripEnjoy -F c -f tripenjoy_backup.dump
```

### Restore
```bash
pg_restore -U postgres -d TripEnjoy tripenjoy_backup.dump
```

## Development vs Production

### Development
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TripEnjoy;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

### Production (Recommended)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-prod-server;Port=5432;Database=TripEnjoy;Username=tripenjoyuser;Password=strong_password;SSL Mode=Require;Trust Server Certificate=false;Include Error Detail=false"
  }
}
```

## Docker Compose Setup

For local development with Docker:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15-alpine
    container_name: tripenjoy-postgres
    environment:
      POSTGRES_DB: TripEnjoy
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Run with:
```bash
docker-compose up -d
```

## Benefits of PostgreSQL

1. **Open Source**: No licensing costs
2. **ACID Compliant**: Full transactional integrity
3. **JSON Support**: Better handling of JSON data types
4. **Full-Text Search**: Built-in text search capabilities
5. **Cross-Platform**: Works on Linux, Windows, macOS
6. **Community**: Large, active community and ecosystem
7. **Performance**: Excellent performance for complex queries
8. **Standards Compliance**: Strong SQL standards compliance

## Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Entity Framework Core Provider](https://www.npgsql.org/efcore/)
- [Hangfire PostgreSQL Storage](https://github.com/frankhommers/Hangfire.PostgreSql)
- [PostgreSQL Tutorial](https://www.postgresqltutorial.com/)

## Support

For issues related to this migration, please check:
1. This documentation
2. PostgreSQL logs: `/var/log/postgresql/`
3. Application logs: `Logs/` directory
4. Hangfire dashboard: `https://localhost:7199/hangfire`
