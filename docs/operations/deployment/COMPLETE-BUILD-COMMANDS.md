# RepoLens - Complete Build Commands Guide

This document provides a comprehensive list of commands to build and compile all projects in the RepoLens system, covering both UI (React) and backend (.NET) components.

## 🏗️ Prerequisites

Ensure you have the following installed:
- **.NET SDK 8.0+** - `dotnet --version`
- **Node.js 18+** - `node --version`
- **npm** - `npm --version`
- **Docker Desktop** (optional) - `docker --version`
- **Git** - `git --version`

## 📋 Quick Build Commands Summary

### Option 1: Use Existing Build Script (Recommended)
```bash
# Full build and start (recommended for development)
.\compile-and-start-all.bat --docker

# Clean build with Docker infrastructure
.\compile-and-start-all.bat --clean --docker

# Backend only (skip frontend)
.\compile-and-start-all.bat --skip-frontend --docker

# Test configuration without starting
.\compile-and-start-all.bat --dry-run
```

### Option 2: Manual Build Commands (Step by Step)

## 🔧 Manual Backend Build Commands

### Build All .NET Projects
```bash
# Restore NuGet packages for all projects
dotnet restore RepoLens.sln

# Build entire solution in Debug mode
dotnet build RepoLens.sln --configuration Debug

# Build entire solution in Release mode
dotnet build RepoLens.sln --configuration Release

# Clean and rebuild everything
dotnet clean RepoLens.sln
dotnet build RepoLens.sln --configuration Release
```

### Build Individual .NET Projects
```bash
# Build Core project
dotnet build RepoLens.Core/RepoLens.Core.csproj --configuration Release

# Build Infrastructure project
dotnet build RepoLens.Infrastructure/RepoLens.Infrastructure.csproj --configuration Release

# Build API project
dotnet build RepoLens.Api/RepoLens.Api.csproj --configuration Release

# Build Worker project
dotnet build RepoLens.Worker/RepoLens.Worker.csproj --configuration Release

# Build Tests project
dotnet build RepoLens.Tests/RepoLens.Tests.csproj --configuration Release
```

### Publish .NET Projects for Deployment
```bash
# Publish API for production deployment
dotnet publish RepoLens.Api/RepoLens.Api.csproj --configuration Release --output ./publish/api

# Publish Worker for production deployment
dotnet publish RepoLens.Worker/RepoLens.Worker.csproj --configuration Release --output ./publish/worker

# Publish with specific runtime (e.g., Linux)
dotnet publish RepoLens.Api/RepoLens.Api.csproj --configuration Release --runtime linux-x64 --output ./publish/api-linux
```

## 🌐 Frontend Build Commands

### React UI Build Commands
```bash
# Navigate to frontend directory
cd repolens-ui

# Install dependencies
npm install

# Build for development (with hot reload)
npm start

# Build for production
npm run build

# Run tests
npm test

# Install dependencies and build (one command)
npm install && npm run build

# Clean install (delete node_modules first)
rm -rf node_modules package-lock.json
npm install

# Go back to root directory
cd ..
```

### Frontend Production Build
```bash
cd repolens-ui
npm ci --production
npm run build
cd ..
```

## 🐳 Docker Build Commands

### Build All Services with Docker
```bash
# Build and start all services
docker-compose up --build

# Build and start in detached mode
docker-compose up --build -d

# Build specific services
docker-compose build api
docker-compose build worker
docker-compose build ui

# Rebuild without cache
docker-compose build --no-cache
```

### Individual Docker Builds
```bash
# Build API Docker image
docker build -f RepoLens.Api/Dockerfile -t repolens-api .

# Build Worker Docker image
docker build -f RepoLens.Worker/Dockerfile -t repolens-worker .

# Build UI Docker image
cd repolens-ui
docker build -f Dockerfile -t repolens-ui .
cd ..
```

## ⚡ Development Workflow Commands

### Full Development Setup (From Scratch)
```bash
# 1. Clone and setup (if needed)
git clone <repository-url>
cd autonomiccomputing

# 2. Setup database with Docker
docker-compose up postgres redis -d

# 3. Restore and build backend
dotnet restore RepoLens.sln
dotnet build RepoLens.sln --configuration Debug

# 4. Setup and build frontend
cd repolens-ui
npm install
npm run build
cd ..

# 5. Start services manually
dotnet run --project RepoLens.Api/RepoLens.Api.csproj &
dotnet run --project RepoLens.Worker/RepoLens.Worker.csproj &
cd repolens-ui && npm start
```

### Daily Development Commands
```bash
# Quick start for development (using existing script)
.\compile-and-start-all.bat --docker

# Or manual quick start
docker-compose up postgres redis -d
dotnet run --project RepoLens.Api/RepoLens.Api.csproj &
cd repolens-ui && npm start
```

### Backend Only Development
```bash
# Start infrastructure
docker-compose up postgres redis -d

# Build and run API
dotnet build RepoLens.sln --configuration Debug
dotnet run --project RepoLens.Api/RepoLens.Api.csproj --configuration Debug

# Run Worker (in separate terminal)
dotnet run --project RepoLens.Worker/RepoLens.Worker.csproj --configuration Debug
```

### Frontend Only Development
```bash
cd repolens-ui
npm install
npm start
```

## 🧪 Testing Commands

### Backend Testing
```bash
# Run all tests
dotnet test RepoLens.Tests/RepoLens.Tests.csproj

# Run tests with detailed output
dotnet test RepoLens.Tests/RepoLens.Tests.csproj --verbosity normal

# Run tests with coverage
dotnet test RepoLens.Tests/RepoLens.Tests.csproj --collect:"XPlat Code Coverage"
```

### Frontend Testing
```bash
cd repolens-ui
npm test
npm test -- --coverage
cd ..
```

## 🔄 Clean and Reset Commands

### Clean Everything
```bash
# Clean .NET builds
dotnet clean RepoLens.sln

# Clean frontend
cd repolens-ui
rm -rf node_modules package-lock.json build
cd ..

# Clean Docker (careful - removes all containers and images)
docker-compose down --volumes --rmi all
docker system prune -af
```

### Reset Development Environment
```bash
# Stop all services
docker-compose down

# Clean builds
dotnet clean RepoLens.sln
cd repolens-ui && rm -rf node_modules package-lock.json build && cd ..

# Rebuild everything
dotnet restore RepoLens.sln
dotnet build RepoLens.sln --configuration Debug
cd repolens-ui && npm install && npm run build && cd ..

# Start fresh
.\compile-and-start-all.bat --clean --docker
```

## 🚀 Production Deployment Commands

### Build for Production
```bash
# Build backend for production
dotnet publish RepoLens.Api/RepoLens.Api.csproj --configuration Release --output ./publish/api
dotnet publish RepoLens.Worker/RepoLens.Worker.csproj --configuration Release --output ./publish/worker

# Build frontend for production
cd repolens-ui
npm ci --production
npm run build
cd ..

# Build Docker images for production
docker-compose -f docker-compose.yml build
```

### Deploy with Docker
```bash
# Deploy all services
docker-compose -f docker-compose.yml up -d

# Deploy with custom environment file
docker-compose -f docker-compose.yml --env-file .env.production up -d
```

## 📊 Monitoring Commands

### Check Build Status
```bash
# Check .NET build
dotnet build RepoLens.sln --verbosity quiet --nologo

# Check frontend build
cd repolens-ui && npm run build && cd ..

# Check Docker builds
docker-compose config --quiet
```

### Service Health Checks
```bash
# Check running services
docker-compose ps

# Check logs
docker-compose logs api
docker-compose logs worker
docker-compose logs ui

# Check API health
curl http://localhost:5000/health
```

## 🛠️ Troubleshooting Commands

### Common Issues
```bash
# Port conflicts - check what's running
netstat -an | findstr "3000\|5000\|5432\|6379"

# Kill processes on specific ports (Windows)
taskkill /f /pid <process_id>

# Check Docker status
docker ps
docker-compose ps

# Rebuild problematic services
docker-compose down
docker-compose build --no-cache api
docker-compose up api
```

### Database Issues
```bash
# Reset database
docker-compose down postgres
docker volume rm autonomiccomputing_postgres_data
docker-compose up postgres -d

# Check database connection
docker-compose exec postgres psql -U repolens_user -d repolens -c "\l"
```

## 🎯 Service Endpoints After Build

After successful compilation and startup:

| Service | URL | Description |
|---------|-----|-------------|
| **API Health** | http://localhost:5000/health | Health check endpoint |
| **API Swagger** | http://localhost:5000/swagger | API documentation |
| **React Frontend** | http://localhost:3000 | Web application |
| **PostgreSQL** | localhost:5432 | Database (repolens/repolens_user) |
| **Redis** | localhost:6379 | Cache service |

## 📝 Notes

- Use the `compile-and-start-all.bat` script for the easiest development experience
- The script handles dependency checking, database setup, and service orchestration
- For production, use Docker Compose with the production configuration
- Frontend builds create static files in `repolens-ui/build/`
- Backend builds create executables in `bin/` directories or specified output paths
- Always ensure database is running before starting backend services
