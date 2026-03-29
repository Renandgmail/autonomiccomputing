# Docker Setup Guide - RepoLens

This guide covers how to run RepoLens using Docker Compose for both development and production environments.

## 📋 Prerequisites

- Docker Desktop (20.10+) or Docker Engine with Docker Compose
- At least 4GB of available RAM
- At least 10GB of available disk space

## 🚀 Quick Start (Production)

```bash
# Clone the repository
git clone <repository-url>
cd autonomiccomputing

# Start all services
docker compose up -d

# Check service status
docker compose ps

# View logs
docker compose logs -f api
docker compose logs -f ui
docker compose logs -f worker
```

**Access Points:**
- **Application**: http://localhost (via nginx reverse proxy)
- **API Direct**: http://localhost:5000
- **UI Direct**: http://localhost:3000
- **API Documentation**: http://localhost:5000/swagger

## 🛠️ Development Setup

```bash
# Use development configuration
docker compose -f docker-compose.dev.yml up -d

# Or build and start
docker compose -f docker-compose.dev.yml up --build
```

**Development Access Points:**
- **Application**: http://localhost:3001
- **API**: http://localhost:5001
- **Database**: localhost:5433
- **Redis**: localhost:6380

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Docker Network                           │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────── │
│  │    nginx    │  │     ui      │  │     api     │  │   worker  │ │
│  │   (proxy)   │  │  (React)    │  │ (.NET API)  │  │(.NET Svc) │ │
│  │   Port 80   │  │  Port 3000  │  │ Port 5000   │  │           │ │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────── │ │
│         │               │               │               │        │
│         └───────────────┼───────────────┼───────────────┘        │
│                         │               │                        │
│  ┌─────────────┐       │               │        ┌─────────────┐  │
│  │ PostgreSQL  │       │               │        │    Redis    │  │
│  │ Port 5432   │       │               │        │  Port 6379  │  │
│  └─────────────┘       │               │        └─────────────┘  │
│         │               │               │               │        │
│         └───────────────┴───────────────┴───────────────┘        │
└─────────────────────────────────────────────────────────────────┘
```

## 🔧 Service Configuration

### API Service
- **Framework**: .NET 8 ASP.NET Core
- **Database**: PostgreSQL with Entity Framework Core
- **Cache**: Redis for sessions and caching
- **Features**: SignalR, Swagger/OpenAPI, JWT authentication

### Worker Service
- **Purpose**: Background processing for repository analysis
- **Features**: Code metrics, pattern detection, vocabulary extraction
- **Tools**: Git, Python, Node.js for multi-language analysis

### UI Service
- **Framework**: React 18.3.1 with TypeScript
- **Server**: Nginx with optimized configuration
- **Features**: SPA routing, asset caching, proxy configuration

### Database Services
- **PostgreSQL**: Primary data storage with JSON support
- **Redis**: Caching and real-time features

## 📝 Environment Configuration

### Production Environment Variables

```bash
# Database
POSTGRES_DB=repolens
POSTGRES_USER=repolens_user
POSTGRES_PASSWORD=your_secure_password

# API Configuration
JWT__Secret=your_jwt_secret_key_here
JWT__Issuer=RepoLens
JWT__Audience=RepoLensUsers

# Code Intelligence
CodeIntelligence__EnablePatternMining=true
CodeIntelligence__MaxConcurrentAnalysis=3

# Logging
Serilog__MinimumLevel=Information
```

### Development Overrides

```bash
# Use development database
POSTGRES_DB=repolens_dev
POSTGRES_PASSWORD=repolens_dev_password

# Development JWT
JWT__Secret=DevelopmentJWTSecretKey123456789

# Debugging
Serilog__MinimumLevel=Debug
CodeIntelligence__EnablePatternMining=false
```

## 🗃️ Data Persistence

### Volumes

```bash
# Production volumes
postgres_data     # Database files
redis_data        # Cache data
./logs           # Application logs
./repositories   # Repository analysis data

# Development volumes
postgres_dev_data # Development database
redis_dev_data    # Development cache
```

### Backup Commands

```bash
# Database backup
docker compose exec postgres pg_dump -U repolens_user -d repolens > backup.sql

# Restore database
docker compose exec -i postgres psql -U repolens_user -d repolens < backup.sql

# Volume backup
docker run --rm -v repolens_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres-data.tar.gz -C /data .
```

## 🚀 Deployment Commands

### Build and Deploy

```bash
# Build all images
docker compose build

# Deploy to production
docker compose up -d

# Update single service
docker compose up -d --no-deps api

# Scale worker service
docker compose up -d --scale worker=3

# Rolling update
docker compose pull
docker compose up -d
```

### Health Monitoring

```bash
# Check all services
docker compose ps

# View service health
docker compose exec api curl -f http://localhost/health

# Monitor logs
docker compose logs -f --tail=100

# Resource usage
docker stats
```

## 🔍 Troubleshooting

### Common Issues

**Service won't start:**
```bash
# Check logs
docker compose logs service_name

# Rebuild image
docker compose build --no-cache service_name
docker compose up -d service_name
```

**Database connection issues:**
```bash
# Verify database is running
docker compose exec postgres pg_isready -U repolens_user

# Check connection from API
docker compose exec api curl -f http://localhost/health
```

**Port conflicts:**
```bash
# Use development configuration with different ports
docker compose -f docker-compose.dev.yml up

# Or change ports in docker-compose.yml
```

### Performance Optimization

```bash
# Limit resource usage
docker compose --compatibility up -d

# Monitor performance
docker compose exec api dotnet-dump collect -n RepoLens.Api

# Database performance
docker compose exec postgres pg_stat_activity
```

## 🔒 Security Considerations

### Production Hardening

1. **Change default passwords**
2. **Use environment files for secrets**
3. **Enable SSL certificates**
4. **Configure firewall rules**
5. **Regular security updates**

### SSL Configuration

```bash
# Generate self-signed certificate (development)
mkdir -p ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/repolens.key \
  -out ssl/repolens.crt

# Update nginx.conf with proper SSL paths
# Mount SSL directory in docker-compose.yml
```

## 📊 Integration Testing

```bash
# Run integration tests against Docker environment
docker compose -f docker-compose.test.yml up --build

# Test specific scenarios
docker compose exec api dotnet test --filter "Category=Integration"

# Performance testing
docker compose exec api ab -n 1000 -c 10 http://localhost/health
```

## 🎯 Next Steps

1. **Configure monitoring** (Prometheus/Grafana)
2. **Set up CI/CD pipeline**
3. **Configure backup automation**
4. **Implement log aggregation**
5. **Security scanning integration**

For more detailed configuration options, see the individual service documentation and configuration files.
