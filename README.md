# RepoLens - Enterprise Repository Analytics Platform

## 🎯 **Mission Statement**

RepoLens is an **enterprise-grade repository analytics platform** designed specifically for **Engineering Managers** to answer the critical question: *"Where does my team need to focus right now?"* in under 90 seconds.

## 📋 **Table of Contents**

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Development](#development)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Performance Targets](#performance-targets)
- [Contributing](#contributing)
- [License](#license)

## 🔍 **Overview**

### **What RepoLens Is**
- **Enterprise repository analytics platform**
- **Engineering Manager focused** - optimized for leadership decision-making
- **Progressive disclosure** interface (L1 → L2 → L3 → L4 navigation)
- **Real-time code quality monitoring** with actionable insights
- **Multi-language support** (TypeScript, C#, Python, JavaScript)

### **What RepoLens Is NOT**
- Code editor or IDE plugin
- AI chatbot for development
- Project management tool
- Generic dashboard builder

## ✨ **Key Features**

### **L1 Portfolio Dashboard**
- **10-second health assessment** across entire codebase portfolio
- **Critical issues detection** with priority-based alerts
- **Repository health scoring** (0-100% composite metric)
- **Engineering Manager optimized** layout and workflows

### **L2 Repository Dashboard**
- **Deep-dive repository analysis** with context preservation
- **Quality hotspots identification** ranked by urgency
- **Activity feed** with recent changes and trends
- **Quick actions** for common Engineering Manager tasks

### **L3 Analytics & Search**
- **Advanced analytics tabs**: Trends, Files, Team, Security, Compare
- **Universal search** with natural language queries
- **Code graph visualization** showing dependencies and architecture
- **Performance metrics** and technical debt analysis

### **L4 File Detail View**
- **File-level quality metrics** and issue tracking
- **AI-assisted code analysis** with improvement suggestions
- **Dependency mapping** with clickable navigation
- **Issue status management** with workflow integration

### **Advanced Capabilities**
- **Real-time monitoring** of code quality changes
- **AST-based code analysis** for deep insights
- **Security vulnerability scanning** (OWASP Top 10)
- **Technical debt calculation** with time estimates
- **Enterprise integrations** (CI/CD, Issue tracking, Communications)

## 🏗️ **Architecture**

### **Technology Stack**

#### **Frontend**
- **React 18** with TypeScript
- **Material-UI** with custom RepoLens design system
- **React Query** for state management and caching
- **React Router** for navigation
- **Custom AST visualization** components

#### **Backend**
- **.NET 8** Web API
- **Entity Framework Core** for database operations
- **PostgreSQL** for primary data storage
- **Elasticsearch** for advanced search capabilities
- **SignalR** for real-time updates

#### **Analysis Engine**
- **TypeScript AST Parser** (@typescript-eslint/parser)
- **C# AST Parser** (Microsoft.CodeAnalysis.Roslyn)
- **Python AST Parser** (ast module integration)
- **Custom complexity calculators**
- **Security scanning engine**

#### **Infrastructure**
- **Docker** containerization
- **Nginx** reverse proxy
- **Redis** for caching
- **Background job processing** for large repository analysis

### **Project Structure**
```
autonomiccomputing/
├── repolens-ui/                 # React frontend application
│   ├── src/
│   │   ├── components/          # React components organized by navigation level
│   │   │   ├── portfolio/       # L1 Portfolio Dashboard
│   │   │   ├── repository/      # L2 Repository Dashboard  
│   │   │   ├── analytics/       # L3 Analytics & Search
│   │   │   ├── files/           # L4 File Detail Views
│   │   │   └── layout/          # Navigation & UI framework
│   │   ├── services/            # API service integrations
│   │   ├── hooks/               # Custom React hooks
│   │   └── theme/               # RepoLens design system
│   └── public/                  # Static assets
├── RepoLens.Api/                # .NET Web API backend
│   ├── Controllers/             # API endpoints
│   ├── Services/                # Business logic services
│   ├── Models/                  # Data transfer objects
│   └── Hubs/                    # SignalR real-time communication
├── RepoLens.Core/               # Core business entities and interfaces
│   ├── Entities/                # Domain models
│   ├── Services/                # Service interfaces
│   └── Repositories/            # Repository interfaces
├── RepoLens.Infrastructure/     # Data access and external integrations
│   ├── Repositories/            # Entity Framework implementations
│   ├── Services/                # External service integrations
│   ├── Migrations/              # Database migrations
│   └── Providers/               # Git and CI/CD integrations
├── RepoLens.Tests/              # Comprehensive test suite
│   ├── Integration/             # Integration tests
│   ├── Controllers/             # API controller tests
│   └── Services/                # Service unit tests
├── RepoLens.Worker/             # Background job processing
├── docs/                        # Documentation
└── repolens-docs/               # Specification documents
    ├── 00-architecture/         # System architecture specs
    ├── 01-screens/              # Screen specifications (L1-L4)
    ├── 02-components/           # Component specifications
    ├── 03-interactions/         # UX interaction patterns
    ├── 04-design-system/        # Design system specification
    ├── 05-compliance/           # Security and compliance docs
    └── 06-implementation/       # Implementation phases and targets
```

## 🚀 **Quick Start**

### **Prerequisites**
- **Node.js** 18+ and npm
- **.NET 8 SDK**
- **PostgreSQL** 12+
- **Docker** and Docker Compose (optional)

### **Option 1: Docker Compose (Recommended)**
```bash
# Clone the repository
git clone https://github.com/Renandgmail/autonomiccomputing.git
cd autonomiccomputing

# Start all services
docker-compose up -d

# Access the application
# Frontend: http://localhost:3000
# API: http://localhost:5000
# API Documentation: http://localhost:5000/swagger
```

### **Option 2: Local Development**
```bash
# 1. Setup Backend
cd RepoLens.Api
dotnet restore
dotnet ef database update
dotnet run

# 2. Setup Frontend (new terminal)
cd repolens-ui
npm install
npm start

# 3. Access Application
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000
```

### **Initial Configuration**
1. **Database Setup**: Configure connection string in `appsettings.json`
2. **Authentication**: Configure JWT settings for secure API access
3. **Git Integration**: Add GitHub/GitLab tokens for repository access
4. **Elasticsearch**: Configure search cluster connection (optional)

## 💻 **Development**

### **Development Workflow**
```bash
# Backend development
cd RepoLens.Api
dotnet watch run              # Hot reload for API changes

# Frontend development  
cd repolens-ui
npm run dev                   # React development server with hot reload

# Database migrations
dotnet ef migrations add MigrationName
dotnet ef database update

# Run tests
cd RepoLens.Tests
dotnet test                   # Backend tests
cd ../repolens-ui
npm test                      # Frontend tests
```

### **Code Quality Standards**
- **TypeScript strict mode** for frontend development
- **ESLint + Prettier** for code formatting
- **C# style guidelines** following Microsoft conventions
- **100% test coverage** for critical business logic
- **AST-based complexity analysis** for all new code

### **Git Workflow**
- **Feature branches** for all new development
- **Pull request reviews** required for main branch
- **Automated CI/CD** pipeline with quality gates
- **Semantic versioning** for releases

## 📊 **Performance Targets**

### **Engineering Manager Success Metrics**
- **L1 Portfolio Load**: <10 seconds (target: 8 seconds achieved)
- **Critical Decision Time**: <90 seconds from login
- **Progressive Navigation**: L1→L4 in <15 seconds
- **Search Response**: <3 seconds for complex queries

### **Technical Performance**
- **Core Web Vitals**: >90/100 score
- **API Response Times**: <2 seconds average
- **Database Queries**: <50ms execution time
- **Memory Usage**: <100MB peak frontend usage
- **Bundle Size**: <2MB initial load

### **Scalability Targets**
- **Repository Size**: Support 10,000+ files
- **Concurrent Users**: 1,000+ simultaneous users
- **Analysis Speed**: <5 minutes for large repository analysis
- **Real-time Updates**: <1 second latency for live monitoring

## 🔧 **Configuration**

### **Environment Variables**

#### **Backend (RepoLens.Api)**
```bash
# Database
ConnectionStrings__DefaultConnection=
POSTGRES_CONNECTION_STRING=

# Authentication
JWT_SECRET_KEY=
JWT_EXPIRATION_HOURS=24

# External Integrations
GITHUB_TOKEN=
GITLAB_TOKEN=
ELASTICSEARCH_URL=
REDIS_CONNECTION_STRING=

# Feature Flags
ENABLE_AST_ANALYSIS=true
ENABLE_REAL_TIME_MONITORING=true
ENABLE_SECURITY_SCANNING=true
```

#### **Frontend (repolens-ui)**
```bash
REACT_APP_API_URL=http://localhost:5000
REACT_APP_ENVIRONMENT=development
REACT_APP_ENABLE_MOCK_DATA=false
REACT_APP_VERSION=1.0.0
```

### **Production Configuration**
- **SSL/TLS certificates** for HTTPS
- **Load balancer configuration** for high availability
- **Database connection pooling** for performance
- **CDN setup** for static asset delivery
- **Monitoring and logging** configuration

## 📚 **API Documentation**

### **Core API Endpoints**

#### **Portfolio Management**
- `GET /api/portfolio/summary` - Portfolio health overview
- `GET /api/portfolio/repositories` - Repository list with health scores
- `GET /api/portfolio/critical-issues` - Critical issues across portfolio

#### **Repository Analysis**
- `GET /api/repositories/{id}` - Repository details and metrics
- `GET /api/repositories/{id}/analytics` - Advanced analytics data
- `POST /api/repositories/{id}/analyze` - Trigger repository analysis
- `GET /api/repositories/{id}/code-graph` - AST-based code graph data

#### **Search & Discovery**
- `GET /api/search/universal` - Universal search across repositories
- `POST /api/search/natural-language` - Natural language code queries
- `GET /api/search/files` - File-specific search
- `GET /api/search/contributors` - Contributor search

#### **Real-time Updates**
- **SignalR Hub**: `/hubs/repository-updates` - Live quality monitoring
- **WebSocket Events**: Quality changes, analysis completion, issues detected

### **Authentication**
All API endpoints require JWT bearer token authentication:
```bash
Authorization: Bearer <jwt-token>
```

## 📈 **Monitoring & Analytics**

### **Application Monitoring**
- **Performance metrics** tracked in real-time
- **Error tracking** with detailed stack traces
- **User analytics** for Engineering Manager workflow optimization
- **System health monitoring** with automated alerting

### **Business Intelligence**
- **Quality trend analysis** across time periods
- **Team productivity metrics** based on code quality improvements
- **ROI calculations** for code quality investments
- **Compliance reporting** for enterprise requirements

## 🤝 **Contributing**

### **Development Setup**
1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create feature branch**: `git checkout -b feature/amazing-feature`
4. **Make changes** following code quality standards
5. **Add tests** for new functionality
6. **Run test suite**: `npm test && dotnet test`
7. **Commit changes**: `git commit -m 'Add amazing feature'`
8. **Push to branch**: `git push origin feature/amazing-feature`
9. **Create Pull Request** with detailed description

### **Code Review Process**
- **Automated testing** must pass
- **Code quality gates** enforced
- **Engineering Manager workflow** validation required
- **Performance impact** assessment
- **Security review** for new integrations

### **Issue Reporting**
- **Bug reports**: Use GitHub Issues with reproduction steps
- **Feature requests**: Include business justification and use cases
- **Performance issues**: Include profiling data when possible
- **Security issues**: Report privately to security@repolens.com

## 📄 **License**

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🙏 **Acknowledgments**

- **Engineering Management Community** for requirements and feedback
- **Open Source Contributors** for foundational libraries
- **Microsoft** for .NET and TypeScript ecosystems
- **React Team** for excellent frontend framework

## 📞 **Support & Contact**

- **Documentation**: [docs.repolens.com](https://docs.repolens.com)
- **Community**: [community.repolens.com](https://community.repolens.com)
- **Enterprise Support**: enterprise@repolens.com
- **GitHub Issues**: [Issues Page](https://github.com/Renandgmail/autonomiccomputing/issues)
- **Security Issues**: security@repolens.com

---

## 🎯 **Engineering Manager Quick Start**

### **First Time Login**
1. **Access RepoLens**: Navigate to your deployment URL
2. **Portfolio View**: See all repositories health at a glance (L1)
3. **Identify Issues**: Check critical issues panel for urgent items
4. **Drill Down**: Click any repository to investigate details (L2)
5. **Take Action**: Use analytics and file detail views for resolution (L3-L4)

### **Daily Workflow**
- **Morning Check**: Quick L1 portfolio scan (<30 seconds)
- **Issue Triage**: Review critical issues for team prioritization
- **Team Focus**: Use analytics to identify areas needing attention
- **Progress Tracking**: Monitor quality improvements over time

**Target Achievement**: Answer *"Where does my team need to focus?"* in under 90 seconds ✅

---

**RepoLens** - *Transforming Engineering Management through Actionable Code Analytics*

*Version 1.0.0 | Built for Engineering Leaders | Enterprise Ready*
