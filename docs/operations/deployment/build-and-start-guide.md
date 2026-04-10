# CodeLens Platform - Build and Start Guide

This guide explains how to use the `compile-and-start-all.bat` script to build and start all services in your CodeLens Platform.

## 🚀 Quick Start

### Option 1: With Docker (Recommended)
```bash
.\compile-and-start-all.bat --docker
```

### Option 2: Manual Database Setup
1. Set up PostgreSQL database (see Database Setup below)
2. Run the batch file:
```bash
.\compile-and-start-all.bat
```

## 📋 Prerequisites

The script automatically checks for these requirements:

- ✅ **.NET SDK** (version 8.0 or later)
- ✅ **Node.js** (version 18 or later)  
- ✅ **npm** (comes with Node.js)
- ✅ **Docker Desktop** (optional, for `--docker` mode)

## 🗄️ Database Setup

The CodeLens Platform requires PostgreSQL with these credentials:

- **Host**: localhost
- **Port**: 5432
- **Database**: CodeLens_db
- **Username**: postgres
- **Password**: TCEP

### Option 1: Docker Setup (Automatic)
```bash
.\compile-and-start-all.bat --docker
```
This automatically starts PostgreSQL and Redis containers with the correct configuration.

### Option 2: Manual PostgreSQL Setup

1. **Install PostgreSQL** from https://www.postgresql.org/download/

2. **Start PostgreSQL service** and ensure it's running on port 5432

3. **Create database and user** using psql or pgAdmin:
```sql
CREATE DATABASE "CodeLens_db";
CREATE USER postgres WITH PASSWORD 'TCEP';
GRANT ALL PRIVILEGES ON DATABASE "CodeLens_db" TO postgres;
```

## 🎛️ Command Options

### Basic Usage
```bash
.\compile-and-start-all.bat [OPTIONS]
```

### Available Options

| Option | Description |
|--------|-------------|
| `--help`, `-h` | Show detailed help message |
| `--dry-run`, `-d` | Test configuration without starting services |
| `--clean` | Clean build before compilation |
| `--docker` | Use Docker for infrastructure services |
| `--skip-dependencies` | Skip dependency installation |
| `--skip-frontend` | Skip frontend build and start |
| `--skip-worker` | Skip worker service start |

### Example Commands

```bash
# Full build and start (all services)
.\compile-and-start-all.bat

# Clean build with Docker infrastructure
.\compile-and-start-all.bat --clean --docker

# Backend services only (no React frontend)
.\compile-and-start-all.bat --skip-frontend

# Test configuration without starting
.\compile-and-start-all.bat --dry-run

# Backend API only (no worker or frontend)
.\compile-and-start-all.bat --skip-frontend --skip-worker
```

## 🌐 Service Endpoints

After successful startup, these services will be available:

| Service | URL | Description |
|---------|-----|-------------|
| **API Health** | http://localhost:5000/health | Health check endpoint |
| **API Swagger** | http://localhost:5000/swagger | Interactive API documentation |
| **React Frontend** | http://localhost:3000 | Web application interface |

## 📝 Build Process Steps

The script follows these steps:

1. **Prerequisites Check** - Validates .NET, Node.js, npm, Docker
2. **Project Structure** - Verifies all required directories exist  
3. **Dependency Management** - Starts Docker services or prompts for manual setup
4. **Build .NET Solutions** - Compiles all C# projects
5. **Frontend Dependencies** - Installs npm packages and builds React app
6. **Start Backend Services** - Launches API and Worker services
7. **Start Frontend** - Launches React development server
8. **Status Summary** - Shows running services and URLs

## 🔧 Troubleshooting

### Common Issues

#### Build Failures
- Ensure .NET SDK 8.0+ is installed
- Try running with `--clean` flag
- Check for port conflicts (5000, 5001, 3000)

#### Database Connection Errors
- Verify PostgreSQL is running on localhost:5432
- Check database credentials (postgres/TCEP)
- Ensure database "CodeLens_db" exists
- Use `--docker` flag for automatic setup

#### Frontend Issues
- Ensure Node.js 18+ is installed
- Try deleting `node_modules` and running again
- Use `--skip-frontend` to start without frontend

#### Port Conflicts
```bash
# Check what's using ports
netstat -an | findstr "5000\|5001\|3000"

# Kill processes if needed
taskkill /f /pid <process_id>
```

### Getting Help

1. **Dry Run Test**: `.\compile-and-start-all.bat --dry-run`
2. **View Help**: `.\compile-and-start-all.bat --help`
3. **Check Logs**: Look at individual service windows for detailed error messages

## 🏃‍♂️ Service Management

### Starting Services
- Run the batch file with your preferred options
- Each service starts in a separate console window
- Monitor logs in individual windows

### Stopping Services
- Close the console windows for each service
- Or press `Ctrl+C` in each service window
- Services will stop gracefully

### Restarting Services
- Simply re-run the batch file
- It will automatically stop existing services and start fresh ones

## 🐳 Docker Integration

When using `--docker` flag:

```bash
# View running containers
docker ps --filter "name=repolens"

# Stop all containers
docker-compose down

# View container logs
docker logs repolens-postgres
docker logs repolens-redis
```

## 📊 Development Workflow

### Daily Development
```bash
# Start with clean build and Docker
.\compile-and-start-all.bat --clean --docker
```

### Backend Development Only
```bash
# Skip frontend for faster startup
.\compile-and-start-all.bat --skip-frontend --docker
```

### Frontend Development Only
```bash
# Frontend developers can use existing API
cd repolens-ui
npm start
```

### Testing Configuration
```bash
# Test without actually starting services
.\compile-and-start-all.bat --dry-run
```

This batch file provides a comprehensive solution for building and starting the entire CodeLens Platform with proper error handling, dependency management, and flexible configuration options.
