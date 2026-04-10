# RepoLens

## Multi-Technology Repository Analysis Platform

This repository contains a full-stack application for repository analysis and code intelligence:

- **Backend**: .NET 8 API and Services (`src/backend/`)
- **Frontend**: React TypeScript Application (`src/frontend/`)
- **Tests**: Technology-separated testing (`tests/`)

## 🚀 Quick Start

### Backend (.NET)
```bash
cd src/backend
dotnet restore
dotnet build
dotnet run --project RepoLens.Api
```

### Frontend (React)
```bash
cd src/frontend/repolens-ui
npm install
npm start
```

## 📁 Project Structure

```
├── src/
│   ├── backend/              # .NET 8 Backend Services
│   │   ├── RepoLens.sln     # Solution file for .NET projects
│   │   ├── RepoLens.Api/    # Web API
│   │   ├── RepoLens.Core/   # Domain models and interfaces
│   │   ├── RepoLens.Infrastructure/ # Data access and external services
│   │   └── RepoLens.Worker/ # Background processing
│   ├── frontend/            # React Frontend Application
│   │   └── repolens-ui/     # TypeScript React app
│   └── shared/              # Cross-technology shared resources
├── tests/
│   ├── backend/             # .NET testing
│   ├── frontend/            # React testing
│   └── shared/              # Integration testing
├── docs/                    # Documentation
├── deployment/              # Infrastructure configuration
├── automation/              # Build and deployment automation
└── tools/                   # Development utilities
```

## 🛠️ Technology Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- SignalR for real-time updates

### Frontend
- React 18
- TypeScript
- Modern React hooks and context

### Infrastructure
- Docker containerization
- Kubernetes deployment ready
- CI/CD automation

## 📚 Architecture

See `docs/architecture/` for detailed system design and architectural decisions.

## 🧪 Testing

### Run Backend Tests
```bash
cd tests/backend/unit
dotnet test
```

### Run Frontend Tests
```bash
cd src/frontend/repolens-ui
npm test
```

## 🚀 Deployment

See `deployment/` directory for infrastructure setup and deployment configurations.

## 📖 Documentation

- [Architecture Overview](docs/architecture/)
- [API Documentation](docs/api/)
- [Development Guide](docs/development/)
- [Deployment Guide](docs/deployment/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the technology-specific development patterns
4. Run tests before submitting
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.
