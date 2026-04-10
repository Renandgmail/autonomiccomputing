# CodeLens Platform - Flexible Deployment Options

**Multiple deployment strategies to suit different environments and requirements**

---

## 🎯 **Deployment Strategy Matrix**

| **Option** | **Docker Required** | **Kubernetes Required** | **Development Friendly** | **Production Ready** | **Complexity** |
|------------|--------------------|-----------------------|--------------------------|---------------------|----------------|
| **Local Development** | ❌ No | ❌ No | ✅ High | ⚠️ Limited | 🟢 Low |
| **Docker Compose** | ✅ Yes | ❌ No | ✅ High | ✅ Medium | 🟡 Medium |
| **Kubernetes** | ✅ Yes | ✅ Yes | ⚠️ Medium | ✅ High | 🔴 High |
| **Hybrid** | 🔵 Optional | ❌ No | ✅ High | ✅ High | 🟡 Medium |

---

## 🚀 **Option 1: Local Development (No Docker)**

### **Perfect for:**
- Local development and testing
- Quick prototyping
- Environments without Docker
- Learning and experimentation

### **Architecture:**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Backend API   │    │   Database      │
│   React:3000    │────│   .NET:5000     │────│   PostgreSQL    │
│                 │    │                 │    │   :5432         │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                ↓
                       ┌─────────────────┐
                       │   Worker        │
                       │   .NET:5001     │
                       └─────────────────┘
```

### **Setup Commands:**
```bash
# 1. Start PostgreSQL (local installation)
# Windows: Start PostgreSQL service
# Linux/Mac: sudo systemctl start postgresql

# 2. Start Backend API
cd CodeLens.Api
dotnet run --urls="https://localhost:5000"

# 3. Start Worker Service
cd CodeLens.Worker
dotnet run --urls="https://localhost:5001"

# 4. Start Frontend
cd repolens-ui
npm start  # Runs on http://localhost:3000
```

---

## 🐳 **Option 2: Docker Compose (Containerized)**

### **Perfect for:**
- Consistent development environments
- CI/CD pipelines
- Team collaboration
- Production-like local testing

### **Architecture:**
```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Network                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Frontend   │  │  Backend    │  │     Database        │  │
│  │  nginx:80   │──│  api:5000   │──│  postgres:5432      │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│                           │                                 │
│                  ┌─────────────┐                           │
│                  │   Worker    │                           │
│                  │ worker:5001 │                           │
│                  └─────────────┘                           │
└─────────────────────────────────────────────────────────────┘
```

---

## ⚡ **Option 3: Hybrid Microservices (Configurable)**

### **Perfect for:**
- Gradual migration to microservices
- Mix of containerized and native services
- Flexible deployment based on resources
- Easy scaling of specific services

### **Configuration Options:**
```yaml
# deployment-config.yaml
services:
  authentication:
    deployment_type: "native"    # native, docker, kubernetes
    port: 5100
    replicas: 1
    
  ast_analysis:
    deployment_type: "docker"    # High CPU - containerize for isolation
    port: 5200
    replicas: 2
    auto_scaling: true
    
  portfolio:
    deployment_type: "native"    # Fast startup for development
    port: 5300
    replicas: 1
    
  repository:
    deployment_type: "native"
    port: 5400
    replicas: 1
```

---

## 🔧 **Implementation: Configurable Deployment Scripts**

Let me create the flexible deployment scripts now...
