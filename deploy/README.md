# TripEnjoy Docker Deployment

This directory contains Docker configuration files for deploying the TripEnjoy application stack.

## Architecture

The deployment includes the following services:

1. **SQL Server** - Microsoft SQL Server 2022 for database
2. **Redis** - In-memory cache for session management
3. **Seq** - Structured logging and monitoring
4. **TripEnjoy API** - .NET 8 Web API backend
5. **TripEnjoy Client** - Blazor WebAssembly frontend

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- At least 4GB RAM available for containers
- Ports available: 1433, 6379, 5341, 7199, 7100

## Quick Start

1. **Configure Environment Variables**

   Copy the example environment file and fill in your credentials:
   ```bash
   cp .env.example .env
   ```

   Edit `.env` and provide:
   - Email credentials (Gmail with App Password)
   - Cloudinary credentials for image storage

2. **Start All Services**

   ```bash
   docker-compose up -d
   ```

   This will:
   - Pull required Docker images
   - Build API and Client containers
   - Start all services with health checks
   - Create necessary volumes for data persistence

3. **Check Service Status**

   ```bash
   docker-compose ps
   ```

4. **View Logs**

   ```bash
   # All services
   docker-compose logs -f

   # Specific service
   docker-compose logs -f api
   docker-compose logs -f client
   ```

5. **Access the Applications**

   - **Blazor Client**: http://localhost:7100
   - **API (Swagger)**: http://localhost:7199/swagger
   - **Seq Logs**: http://localhost:5341

## Database Initialization

The API will automatically:
1. Apply EF Core migrations on startup
2. Seed initial data (PropertyTypes, Roles, test accounts)
3. Create the database if it doesn't exist

## Service Configuration

### SQL Server
- **Port**: 1433
- **SA Password**: TripEnjoy@2024 (change in production)
- **Database**: TripEnjoy
- **Data Volume**: `sqlserver_data`

### Redis
- **Port**: 6379
- **Data Volume**: `redis_data`

### API
- **Port**: 7199
- **Health Check**: http://localhost:7199/health
- **Environment**: Production
- **Connection String**: Configured for SQL Server container

### Client
- **Port**: 7100
- **Served by**: Nginx (Alpine Linux)
- **API Base URL**: http://localhost:7199

## Environment Variables

Required variables in `.env` file:

| Variable | Description | Example |
|----------|-------------|---------|
| EMAIL_ADDRESS | Gmail address for OTP emails | `your-email@gmail.com` |
| EMAIL_PASSWORD | Gmail App Password | `abcd efgh ijkl mnop` |
| CLOUDINARY_CLOUD_NAME | Cloudinary cloud name | `your-cloud-name` |
| CLOUDINARY_API_KEY | Cloudinary API key | `123456789012345` |
| CLOUDINARY_API_SECRET | Cloudinary API secret | `your-api-secret` |

### Getting Gmail App Password

1. Go to Google Account Settings
2. Security → 2-Step Verification (enable if not already)
3. App Passwords → Generate new password
4. Use the 16-character password in `.env`

### Getting Cloudinary Credentials

1. Sign up at https://cloudinary.com
2. Dashboard shows your Cloud Name, API Key, and API Secret
3. Copy these values to `.env`

## Management Commands

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Volumes (deletes data)
```bash
docker-compose down -v
```

### Rebuild Containers
```bash
docker-compose up -d --build
```

### View Resource Usage
```bash
docker stats
```

### Access Container Shell
```bash
# API container
docker exec -it tripenjoy-api bash

# SQL Server
docker exec -it tripenjoy-sqlserver bash
```

## Troubleshooting

### API won't start
- Check if SQL Server is healthy: `docker-compose ps`
- View API logs: `docker-compose logs api`
- Verify connection string in docker-compose.yml

### Database connection failed
- Ensure SQL Server container is running
- Check health check status: `docker inspect tripenjoy-sqlserver`
- Try connecting manually: `docker exec -it tripenjoy-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P TripEnjoy@2024`

### Client can't reach API
- Verify API is healthy: `curl http://localhost:7199/health`
- Check network connectivity: `docker network inspect deploy_tripenjoy-network`
- Ensure CORS is configured in API for client origin

### Redis connection issues
- Check Redis is running: `docker exec -it tripenjoy-redis redis-cli ping`
- Should return `PONG`

### Out of memory
- Increase Docker memory limit in Docker Desktop settings
- Minimum 4GB recommended, 8GB preferred

## Production Deployment

For production deployments, consider:

1. **Change Passwords**: Update SA_PASSWORD in docker-compose.yml
2. **Use Secrets**: Replace environment variables with Docker secrets
3. **SSL/TLS**: Add reverse proxy (nginx) with SSL certificates
4. **Persistent Storage**: Use external volumes or cloud storage
5. **Monitoring**: Configure Seq or other monitoring solutions
6. **Backup**: Implement regular database backups
7. **Scaling**: Use Docker Swarm or Kubernetes for horizontal scaling

## Network Architecture

```
┌─────────────────────────────────────────────┐
│            tripenjoy-network                │
│                                             │
│  ┌──────────┐    ┌──────────┐             │
│  │  Client  │───→│   API    │             │
│  │ :7100    │    │ :7199    │             │
│  └──────────┘    └────┬─────┘             │
│                       │                     │
│                ┌──────┴──────┐             │
│                │             │             │
│           ┌────▼───┐    ┌───▼────┐        │
│           │ SQL    │    │ Redis  │        │
│           │ Server │    │ :6379  │        │
│           │ :1433  │    └────────┘        │
│           └────────┘                       │
│                                             │
│           ┌────────┐                       │
│           │  Seq   │                       │
│           │ :5341  │                       │
│           └────────┘                       │
└─────────────────────────────────────────────┘
```

## Support

For issues or questions:
1. Check logs: `docker-compose logs`
2. Review container health: `docker-compose ps`
3. Consult project documentation in `/docs` folder
