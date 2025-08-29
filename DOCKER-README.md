# Pioloop Microservices - Docker Setup

This document describes the Docker setup for Pioloop microservices using a single `docker-compose.yml` file.

## Architecture

The application consists of the following services:

### Microservices
- **API Gateway** (Port 5000) - Ocelot-based gateway for routing requests
- **Auth Microservice** (Port 5001) - User authentication and authorization
- **Property Microservice** (Port 5003) - Property management
- **Email Microservice** (Port 5002) - Email sending functionality

### Infrastructure
- **Auth Database** (Port 5433) - PostgreSQL database for authentication data
- **Property Database** (Port 5435) - PostgreSQL database for property data

## Quick Start

### Prerequisites
- Docker Desktop installed and running
- Docker Compose installed

### Starting All Services
```bash
# Start all services in background
./scripts/start-docker-compose.sh up --detach

# Start all services in foreground (to see logs)
./scripts/start-docker-compose.sh up
```

### Stopping All Services
```bash
./scripts/start-docker-compose.sh down
```

### Viewing Service Status
```bash
./scripts/start-docker-compose.sh status
```

## Service Management

### Available Commands

```bash
# Start services
./scripts/start-docker-compose.sh up [--detach]

# Stop services
./scripts/start-docker-compose.sh down

# Restart services
./scripts/start-docker-compose.sh restart

# View logs
./scripts/start-docker-compose.sh logs [SERVICE] [--follow]

# Build services
./scripts/start-docker-compose.sh build [SERVICE] [--force]

# Clean everything
./scripts/start-docker-compose.sh clean

# Show help
./scripts/start-docker-compose.sh --help
```

### Service URLs

Once started, the services are available at:

- **API Gateway**: http://localhost:5000
- **Auth API**: http://localhost:5001
- **Email API**: http://localhost:5002
- **Property API**: http://localhost:5003

### Database Connections

- **Auth Database**: localhost:5433
- **Property Database**: localhost:5435

## Individual Dockerfiles

Each microservice has its own Dockerfile:

### Auth Microservice
- **Location**: `Auth.Microservice/Dockerfile`
- **Features**: Multi-stage build, health checks, proper labels
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`

### Property Microservice
- **Location**: `Property.Microservice/Dockerfile`
- **Features**: Multi-stage build, health checks, proper labels
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`

### Email Microservice
- **Location**: `Email.Microservice/Dockerfile`
- **Features**: Multi-stage build, health checks, proper labels
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`

### API Gateway
- **Location**: `ApiGateway/Dockerfile`
- **Features**: Multi-stage build, health checks, non-root user, curl for health checks
- **Base Image**: `mcr.microsoft.com/dotnet/aspnet:8.0`

## Environment Variables

### API Gateway
- `ASPNETCORE_ENVIRONMENT`: Development
- `JwtSettings__SecretKey`: JWT secret key
- `JwtSettings__Issuer`: JWT issuer
- `JwtSettings__Audience`: JWT audience

### Auth Microservice
- `ASPNETCORE_ENVIRONMENT`: Development
- `ConnectionStrings__DefaultConnection`: Database connection string
- `JwtSettings__*`: JWT configuration
- `EmailApi__BaseUrl`: Email service URL
- `EmailSettings__*`: Email configuration
- `StripeSettings__*`: Stripe configuration

### Property Microservice
- `ASPNETCORE_ENVIRONMENT`: Development
- `ConnectionStrings__DefaultConnection`: Database connection string
- `JwtSettings__*`: JWT configuration
- `AuthApi__BaseUrl`: Auth service URL

### Email Microservice
- `ASPNETCORE_ENVIRONMENT`: Development
- `EmailSettings__*`: Email configuration

## Volumes

The following volumes are created for data persistence:

- `auth-db-data`: PostgreSQL data for authentication
- `property-db-data`: PostgreSQL data for properties

## Networks

All services are connected to the `pioloop-network` bridge network, allowing them to communicate using service names.

## Health Checks

Each microservice includes health checks:

- **API Gateway**: `http://localhost/health`
- **Auth Microservice**: `http://localhost/api/health`
- **Property Microservice**: `http://localhost/api/health`
- **Email Microservice**: `http://localhost/api/health`

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 5000-5003, 5433, and 5435 are available
2. **Database connection issues**: Check that databases are healthy before starting microservices
3. **Build failures**: Use `--force` flag to rebuild without cache

### Useful Commands

```bash
# View all container logs
./scripts/start-docker-compose.sh logs

# Follow specific service logs
./scripts/start-docker-compose.sh logs api-gateway --follow

# Check service status
./scripts/start-docker-compose.sh status

# Force rebuild all services
./scripts/start-docker-compose.sh build --force

# Clean everything and start fresh
./scripts/start-docker-compose.sh clean
./scripts/start-docker-compose.sh up --detach
```

### Database Access

To access the databases directly:

```bash
# Auth database
docker exec -it auth-db psql -U pioloop12345 -d pioloop_auth

# Property database
docker exec -it property-db psql -U pioloop12345 -d pioloop_property


```

## Migration from Individual Docker Compose Files

The previous setup used individual `docker-compose.yml` files for each microservice. The new setup consolidates everything into a single file for easier management.

### Benefits of Single Docker Compose

1. **Simplified management**: One command to start/stop all services
2. **Better dependency management**: Services start in the correct order
3. **Unified networking**: All services can communicate easily
4. **Centralized configuration**: All environment variables in one place
5. **Easier debugging**: View all logs from one place

## Development Workflow

1. **Start services**: `./scripts/start-docker-compose.sh up --detach`
2. **Make changes**: Edit code in your IDE
3. **Rebuild specific service**: `./scripts/start-docker-compose.sh build SERVICE --force`
4. **Restart service**: `./scripts/start-docker-compose.sh restart SERVICE`
5. **View logs**: `./scripts/start-docker-compose.sh logs SERVICE --follow`
6. **Stop all**: `./scripts/start-docker-compose.sh down`

## Production Considerations

For production deployment:

1. **Environment variables**: Use `.env` files or external configuration
2. **Secrets management**: Use Docker secrets or external secret management
3. **Resource limits**: Add memory and CPU limits to services
4. **Logging**: Configure external logging solutions
5. **Monitoring**: Add monitoring and alerting
6. **Backup**: Configure database backups
7. **SSL/TLS**: Configure HTTPS endpoints
