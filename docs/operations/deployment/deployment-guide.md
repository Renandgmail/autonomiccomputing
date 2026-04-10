# CodeLens Platform - Complete Deployment Guide

**🎯 Multiple deployment options to suit any environment**

---

## 📋 **Quick Start Options**

### **Option 1: Native Development (No Docker Required)**
```bash
# 1. Start PostgreSQL locally (Windows/Linux/Mac)
# Windows: Start PostgreSQL service
# Mac: brew services start postgresql

# 2. Start CodeLens Platform
.\start-codelens.ps1

# That's it! 🎉
# ✅ Frontend: http://localhost:3000
# ✅ API: http://localhost:5000/swagger
```

### **Option 2: Docker for Dependencies Only**
```bash
# 1. Start only database with Docker
docker-compose -f docker-compose.simple.yml up -d postgres

# 2. Start CodeLens Platform natively
.\start-codelens.ps1

# Best of both worlds! 🚀
```

### **Option 3: Full Docker (When Available)**
```bash
# 1. Start all dependencies
docker-compose -f docker-compose.simple.yml up -d

# 2. Start CodeLens Platform
.\start-codelens.ps1 -Mode docker

# Production-like environment! 💪
```

---

## 🛠️ **Deployment Configuration Matrix**

| **Scenario** | **Docker Required** | **Commands** | **Best For** |
|--------------|--------------------|--------------|--------------| 
| **Learning/Development** | ❌ No | `.\start-codelens.ps1` | Quick start, learning |
| **Team Development** | 🔵 Optional | `.\start-codelens.ps1` | Consistent environments |
| **CI/CD Pipeline** | ✅ Yes | `docker-compose up` | Automated testing |
| **Production** | ✅ Yes | `kubectl apply -f kong-gateway-setup.yaml` | Enterprise deployment |

---

## 🚀 **Detailed Deployment Options**

### **1. Native Development Mode (Recommended for Development)**

**Perfect when you want:**
- Fastest startup times
- Direct debugging in IDE
- No Docker installation required
- Maximum development productivity

**Prerequisites:**
- PostgreSQL installed locally
- .NET 8 SDK
- Node.js 18+

**Setup:**
```bash
# Option A: Use our script (Recommended)
.\start-codelens.ps1

# Option B: Manual startup
cd CodeLens.Api && dotnet run &
cd repolens-ui && npm start &
```

**Configuration:**
```yaml
# deployment-config.yaml
deployment:
  mode: "native"
services:
  api:
    deployment_type: "native"
    port: 5000
```

---

### **2. Hybrid Mode (Best of Both Worlds)**

**Perfect when you want:**
- Native performance for main application
- Docker for complex dependencies (Elasticsearch, ClickHouse)
- Easy switching between modes
- Gradual migration to containers

**Setup:**
```bash
# 1. Start heavy dependencies in Docker
docker-compose -f docker-compose.simple.yml up -d postgres elasticsearch

# 2. Start main services natively
.\start-codelens.ps1 -Mode hybrid

# 3. Enable search service when ready
# Edit deployment-config.yaml:
# search_service:
#   enabled: true
#   deployment_type: "docker"
```

**Configuration:**
```yaml
deployment:
  mode: "hybrid"
services:
  api:
    deployment_type: "native"  # Fast development
  search_service:
    deployment_type: "docker"  # Complex dependencies
    enabled: true
  ast_analysis_service:
    deployment_type: "docker"  # CPU isolation
    enabled: false
```

---

### **3. Full Docker Mode**

**Perfect when you want:**
- Consistent environments across team
- Production-like local development
- Easy cleanup and reset
- CI/CD pipeline compatibility

**Setup:**
```bash
# 1. Start all infrastructure
docker-compose -f docker-compose.simple.yml up -d

# 2. Start application services
.\start-codelens.ps1 -Mode docker
```

**Optional Components:**
```bash
# Start with search capabilities
docker-compose --profile search -f docker-compose.simple.yml up -d

# Start with analytics capabilities  
docker-compose --profile analytics -f docker-compose.simple.yml up -d

# Start with monitoring
docker-compose --profile monitoring -f docker-compose.simple.yml up -d
```

---

### **4. Kubernetes Production Mode**

**Perfect when you want:**
- Production deployment
- Auto-scaling capabilities
- High availability
- Enterprise-grade infrastructure

**Setup:**
```bash
# 1. Deploy infrastructure
.\deploy-infrastructure.ps1

# 2. Apply Kubernetes configurations
kubectl apply -f kong-gateway-setup.yaml
```

---

## 🎛️ **Configuration Management**

### **Environment-Based Configuration**

```bash
# Development (default)
.\start-codelens.ps1

# Staging
.\start-codelens.ps1 -Environment staging

# Production
.\start-codelens.ps1 -Environment production
```

### **Service-Specific Configuration**

```bash
# Start only API (for backend development)
.\start-codelens.ps1 -OnlyApi

# Skip frontend (for API-only development)
.\start-codelens.ps1 -SkipFrontend

# Skip worker service
.\start-codelens.ps1 -SkipWorker

# Skip database checks
.\start-codelens.ps1 -SkipDatabase
```

### **Microservices Migration**

```yaml
# deployment-config.yaml - Enable services as you extract them
services:
  auth_service:
    enabled: true     # Set to true when extracted
    deployment_type: "native"
    port: 5100
    
  ast_analysis_service:
    enabled: true     # Set to true when extracted
    deployment_type: "docker"  # CPU intensive
    port: 5200
    auto_scaling:
      enabled: true
      min_replicas: 1
      max_replicas: 5
```

---

## 📊 **Service Management**

### **Check Status**
```bash
.\start-codelens.ps1 -ShowStatus
```

### **Stop All Services**
```bash
.\start-codelens.ps1 -StopServices
```

### **Restart Specific Service**
```bash
# Stop all first
.\start-codelens.ps1 -StopServices

# Start specific services
.\start-codelens.ps1 -OnlyApi
```

---

## 🔧 **Troubleshooting**

### **Common Issues & Solutions**

**1. PostgreSQL Connection Failed**
```bash
# Check if PostgreSQL is running
.\start-codelens.ps1 -ShowStatus

# Start PostgreSQL service
# Windows: Start-Service postgresql
# Mac: brew services start postgresql
# Linux: sudo systemctl start postgresql

# Or use Docker
docker-compose -f docker-compose.simple.yml up -d postgres
```

**2. Port Conflicts**
```bash
# Check what's using ports
netstat -ano | findstr :5000
netstat -ano | findstr :3000

# Kill conflicting processes or change ports in deployment-config.yaml
```

**3. Node.js Dependencies**
```bash
cd repolens-ui
npm install
npm start
```

**4. .NET Build Issues**
```bash
dotnet clean
dotnet restore
dotnet build
```

### **Dry Run Mode**
```bash
# Test configuration without starting services
.\start-codelens.ps1 -DryRun
```

---

## 🔄 **Migration Path: Monolith → Microservices**

### **Phase 1: Current State (100% Native)**
```yaml
services:
  api: { deployment_type: "native", enabled: true }
  worker: { deployment_type: "native", enabled: true }
  frontend: { deployment_type: "native", enabled: true }
  # All microservices: enabled: false
```

### **Phase 2: Extract Authentication (95% Native, 5% Docker)**
```yaml
services:
  api: { deployment_type: "native", enabled: true }
  auth_service: { deployment_type: "native", enabled: true }  # ✅ Extracted
  # Other services: enabled: false
```

### **Phase 3: Extract AST Analysis (90% Native, 10% Docker)**
```yaml
services:
  api: { deployment_type: "native", enabled: true }
  auth_service: { deployment_type: "native", enabled: true }
  ast_analysis_service: { deployment_type: "docker", enabled: true }  # ✅ Extracted
```

### **Phase 4: Full Microservices (Configurable Mix)**
```yaml
services:
  # Core services - native for performance
  portfolio_service: { deployment_type: "native", enabled: true }
  repository_service: { deployment_type: "native", enabled: true }
  
  # Resource-intensive - Docker for isolation
  search_service: { deployment_type: "docker", enabled: true }
  analytics_service: { deployment_type: "docker", enabled: true }
```

---

## 📈 **Performance Optimization**

### **Development Mode (Fastest)**
```bash
.\start-codelens.ps1 -OnlyApi -SkipWorker
# Startup time: ~10 seconds
```

### **Full Development Mode**
```bash
.\start-codelens.ps1
# Startup time: ~30 seconds
```

### **Production Mode**
```bash
.\start-codelens.ps1 -Environment production -Mode hybrid
# Startup time: ~60 seconds (with health checks)
```

---

## 🎯 **Next Steps Based on Your Needs**

### **🏃‍♂️ Quick Start (Just want to see it working)**
```bash
.\start-codelens.ps1
```

### **👥 Team Development (Consistent environments)**
```bash
# Each developer runs the same:
.\start-codelens.ps1 -Mode hybrid
```

### **🏢 Production Deployment (Enterprise ready)**
```bash
.\deploy-infrastructure.ps1
kubectl apply -f kong-gateway-setup.yaml
```

### **🔬 Microservices Migration (Step by step)**
1. Start with: `.\start-codelens.ps1`
2. Extract auth: Edit `deployment-config.yaml` → `auth_service: {enabled: true}`
3. Extract AST: Edit `deployment-config.yaml` → `ast_analysis_service: {enabled: true}`
4. Continue service by service...

---

## 📚 **Configuration Files Reference**

| **File** | **Purpose** | **Required** |
|----------|-------------|--------------|
| `deployment-config.yaml` | Main configuration | ✅ Auto-generated |
| `start-codelens.ps1` | Deployment script | ✅ Required |
| `docker-compose.simple.yml` | Docker dependencies | 🔵 Optional |
| `kong-gateway-setup.yaml` | Kubernetes production | 🔵 Optional |

---

**🎉 The CodeLens Platform is designed to grow with your needs - start simple, scale when ready!**

*Choose the deployment mode that fits your current requirements and easily upgrade when needed.*
