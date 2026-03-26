# RepoLens - Repository Analytics Platform

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)]()
[![React Version](https://img.shields.io/badge/React-18.3.1-blue.svg)]()
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)]()

RepoLens is a comprehensive repository analytics platform that provides deep insights into your Git repositories through GitHub API integration. It offers repository metrics, contributor analytics, code quality assessment, and visual dashboards for development teams.

## ✨ Features

### 📊 Repository Analytics
- **Real GitHub Integration** - Connects directly to GitHub repositories via API
- **Comprehensive Metrics Collection** - Files, commits, contributors, languages
- **Repository Health Scoring** - Overall health assessment with detailed breakdowns
- **Code Quality Metrics** - Maintainability index, complexity analysis, technical debt
- **Performance Insights** - Build success rates, test coverage, bundle analysis

### 👥 Team Analytics  
- **Contributor Analysis** - Detailed contributor metrics and activity patterns
- **Collaboration Insights** - Team dynamics, knowledge distribution, bus factor
- **Activity Patterns** - Hourly/daily activity analysis, peak development times
- **Productivity Metrics** - Development velocity, contribution trends

### 🎯 Advanced Visualizations
- **Interactive Dashboards** - Professional Material-UI components
- **Real-time Charts** - Activity timelines, contribution matrices, trend analysis
- **Language Distribution** - Visual breakdown of codebase composition
- **Repository Comparisons** - Side-by-side repository analytics

### 🔍 Search & Discovery (Coming Soon)
- **Advanced Code Search** - Full-text search across repository contents
- **Function/Class Discovery** - Browse and search code structures
- **Dependency Analysis** - Visualize project dependencies
- **API Documentation** - Auto-generated endpoint documentation

## 🏗️ Architecture

### Backend (.NET 10)
- **RepoLens.Api** - REST API with JWT authentication
- **RepoLens.Core** - Business entities and interfaces
- **RepoLens.Infrastructure** - Data access, GitHub integration, Git services
- **RepoLens.Worker** - Background processing for metrics collection
- **RepoLens.Tests** - Comprehensive unit and integration tests

### Frontend (React + TypeScript)
- **Modern React 18.3.1** with TypeScript for type safety
- **Material-UI (MUI)** for professional, responsive design
- **React Router** for single-page application navigation
- **Comprehensive Component Library** - Reusable, accessible UI components

### Database
- **PostgreSQL 15+** - Production-ready relational database
- **Entity Framework Core** - Code-first migrations, LINQ queries
- **Optimized Schema** - Efficient indexing for analytics queries

### External Integrations
- **GitHub API** - Repository data, commits, contributors, languages
- **Real-time Updates** - SignalR for live metric updates
- **JWT Authentication** - Secure API access

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm
- PostgreSQL 15+
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/repolens.git
   cd repolens
   ```

2. **Setup Database**
   ```bash
   # Create PostgreSQL database
   createdb repolens_db
   
   # Update connection string in appsettings.json
   ```

3. **Configure GitHub API**
   ```bash
   # Add your GitHub token to appsettings.json
   "GitHub": {
     "ApiToken": "your-github-token-here"
   }
   ```

4. **Start Services**
   ```bash
   # Use the optimized startup script
   .\start-services-optimized.bat
   
   # Or start manually:
   # Backend: cd RepoLens.Api && dotnet run
   # Frontend: cd repolens-ui && npm start
   ```

5. **Access the Application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5179
   - API Documentation: http://localhost:5179/swagger (when enabled)

## 📚 Usage Guide

### Adding a Repository
1. Navigate to the **Repositories** page
2. Click **Add Repository**
3. Enter a GitHub repository URL (supports HTTPS, SSH, GitLab, Azure DevOps)
4. The system will validate and sync the repository
5. Access comprehensive metrics via the **Metrics** button

### Understanding Metrics

#### Repository Health Score (85%)
- **Code Quality (92%)** - Maintainability, complexity, technical debt
- **Activity Level (75%)** - Development frequency and consistency  
- **Maintenance (88%)** - Recent updates, issue resolution

#### Key Metrics Displayed
- **Files Analyzed** - Total repository files processed
- **Total Commits** - Complete commit history analysis
- **Contributors** - Active and historical contributors
- **Programming Languages** - Language distribution and percentages
- **Technical Debt** - Estimated hours for code improvements
- **Test Coverage** - Code coverage percentage and trends
- **Security Assessment** - Vulnerability scanning results

### Dashboard Navigation
- **Dashboard** - Overview of all repositories
- **Repositories** - Repository management and metrics access
- **Analytics** - Advanced analytics and reporting (coming soon)
- **Search** - Code search and discovery tools (coming soon)

## 🧪 Testing

### Run All Tests
```bash
cd RepoLens.Tests
dotnet test
```

### Test Categories
- **Unit Tests** - Business logic, validation, services
- **Integration Tests** - Database operations, GitHub API integration
- **Performance Tests** - Response time and throughput validation

### Test Coverage
Current test coverage includes:
- Repository validation and URL parsing
- GitHub API integration
- Metrics collection and calculation
- Database operations and migrations
- Authentication and authorization flows

## 🔧 Development

### Project Structure
```
repolens/
├── RepoLens.Api/              # REST API controllers and configuration
├── RepoLens.Core/             # Business entities, interfaces, exceptions
├── RepoLens.Infrastructure/   # Data access, external services, Git integration
├── RepoLens.Worker/           # Background processing services
├── RepoLens.Tests/            # Unit and integration tests
├── repolens-ui/               # React frontend application
│   ├── src/components/        # React components
│   ├── src/services/          # API integration services  
│   └── src/types/             # TypeScript type definitions
├── start-services-optimized.bat  # Development startup script
└── README.md                  # This file
```

### Adding New Metrics
1. **Define Entity** - Add to `RepoLens.Core/Entities/`
2. **Create Repository Interface** - Add to `RepoLens.Core/Repositories/`
3. **Implement Repository** - Add to `RepoLens.Infrastructure/Repositories/`
4. **Update DbContext** - Modify `RepoLensDbContext.cs`
5. **Create Migration** - `dotnet ef migrations add NewMetric`
6. **Update UI** - Add components to display new metrics

### API Development
- **Controllers** - RESTful endpoints in `RepoLens.Api/Controllers/`
- **Authentication** - JWT-based authentication with Identity
- **Validation** - Model validation and business rule enforcement
- **Documentation** - Swagger/OpenAPI integration

### Frontend Development
- **Component Library** - Material-UI components with custom styling
- **State Management** - React hooks and context for state management
- **Routing** - React Router for SPA navigation
- **API Integration** - Axios-based API service layer

## 🔒 Security

### Authentication
- **JWT Tokens** - Secure API authentication
- **Identity Framework** - User management and role-based access
- **Token Refresh** - Automatic token renewal

### Data Protection
- **Input Validation** - Comprehensive input sanitization
- **SQL Injection Protection** - Entity Framework parameterized queries
- **CORS Configuration** - Secure cross-origin resource sharing

### GitHub Integration
- **Token Security** - Encrypted GitHub API token storage
- **Rate Limiting** - Respect GitHub API rate limits
- **Permission Validation** - Repository access verification

## 📈 Performance

### Optimization Features
- **Lazy Loading** - Component and data lazy loading
- **Caching Strategy** - Redis caching for frequently accessed data
- **Database Indexing** - Optimized indexes for analytics queries
- **Background Processing** - Async metrics collection via worker services

### Monitoring
- **Health Checks** - Service health monitoring endpoints
- **Logging** - Comprehensive application logging with Serilog
- **Metrics Collection** - Performance metrics and monitoring

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes with tests
4. Run the test suite (`dotnet test`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Code Standards
- **C#** - Follow Microsoft C# coding conventions
- **TypeScript/React** - ESLint and Prettier configuration
- **Testing** - Maintain >80% code coverage
- **Documentation** - Update README and inline documentation

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft** - .NET and Entity Framework Core
- **Meta** - React and create-react-app
- **Material-UI Team** - Excellent component library
- **GitHub** - API integration and platform
- **PostgreSQL** - Robust database platform
- **LibGit2Sharp** - Git integration library

## 📞 Support

- **Issues** - Report bugs and feature requests via GitHub Issues
- **Documentation** - Comprehensive guides in the `docs/` directory
- **Community** - Join discussions in GitHub Discussions

---

**RepoLens** - Bringing clarity to your codebase 🔍✨
