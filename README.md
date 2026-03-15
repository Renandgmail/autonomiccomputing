# RepoLens - Autonomic Computing Repository Analysis System

## Overview

RepoLens is an autonomic computing system designed to automatically ingest, analyze, and index Git repositories for intelligent codebase querying and analysis. The system provides content-addressable storage, semantic analysis, and a foundation for building intelligent development tools.

## Architecture

### Core Components

#### 1. RepoLens.Core
- **Domain Entities**: Repository, Commit, Artifact, ArtifactVersion
- **Repositories**: IRepositoryRepository, IArtifactRepository
- **Exceptions**: GitException, RepoLensException, StorageException

#### 2. RepoLens.Infrastructure
- **Data Access**: Entity Framework Core with PostgreSQL
- **Git Operations**: LibGit2Sharp integration for repository cloning and commit walking
- **Storage**: Content-addressable storage service (currently configured for S3/MinIO)
- **Repositories**: Entity Framework implementations

#### 3. RepoLens.Worker
- **Background Service**: Continuous repository ingestion
- **Ingestion Service**: Main processing pipeline
- **Dependency Injection**: Service registration and configuration

## Key Features

### Content-Addressable Storage
- Files are stored using SHA256 hash as identifier
- Deduplication across repositories
- Immutable storage for version control

### Semantic Analysis
- Roslyn-based C# code analysis
- Extracts classes, methods, namespaces
- Metadata storage for intelligent querying

### Commit Walking
- Incremental repository processing
- Efficient change detection
- Historical analysis support

### Autonomic Operation
- Self-configuring repository ingestion
- Automatic error handling and recovery
- Configurable processing pipelines

## Setup Instructions

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL database
- Git (for LibGit2Sharp)
- Optional: MinIO or S3-compatible storage

### Configuration

1. **Database Setup**:
   ```json
   {
     "ConnectionStrings": {
       "Postgres": "Host=localhost;Database=repolens;Username=postgres;Password=yourpassword"
     }
   }
   ```

2. **Storage Configuration**:
   ```json
   {
     "S3": {
       "Endpoint": "http://localhost:9000",
       "AccessKey": "minioadmin",
       "SecretKey": "minioadmin",
       "Bucket": "repolens-storage"
     }
   }
   ```

### Building and Running

```bash
# Build the solution
dotnet build RepoLens.sln

# Run the worker service
dotnet run --project RepoLens.Worker/RepoLens.Worker.csproj
```

## Current Status

### ✅ Completed
- [x] Core domain model and entities
- [x] Entity Framework Core integration
- [x] LibGit2Sharp Git operations
- [x] Content-addressable storage service
- [x] Roslyn semantic analysis
- [x] Background worker service
- [x] Dependency injection setup
- [x] Error handling and logging

### ⚠️ Known Issues
- **Compilation Error**: Ambiguous reference between `RepoLens.Core.Entities.Repository` and `LibGit2Sharp.Repository`
- **Type Conversion**: `byte[]` to `string` conversion error in storage service
- **Amazon S3 Dependency**: Currently configured for paid AWS S3 service

### 🔧 To Be Fixed
- Fix ambiguous reference in `IngestionService.cs` (line 43)
- Fix type conversion in `IngestionService.cs` (line 144)
- Replace Amazon S3 with free alternative (local file system or MinIO)

## Project Structure

```
RepoLens/
├── RepoLens.Core/           # Domain layer
│   ├── Entities/           # Repository, Commit, Artifact, ArtifactVersion
│   ├── Exceptions/         # Custom exception types
│   └── Repositories/       # Repository interfaces
├── RepoLens.Infrastructure/ # Data access layer
│   ├── Git/               # LibGit2Sharp integration
│   ├── Storage/           # Content-addressable storage
│   ├── Repositories/      # EF Core implementations
│   └── RepoLensDbContext.cs
├── RepoLens.Worker/        # Application layer
│   ├── Services/          # IngestionService
│   ├── Program.cs         # Main worker entry point
│   └── appsettings.json   # Configuration
└── README.md              # This file
```

## Future Development

### Phase 1: Core Stability
- [ ] Fix compilation errors
- [ ] Replace paid services with free alternatives
- [ ] Add comprehensive unit tests
- [ ] Implement proper error recovery

### Phase 2: Enhanced Analysis
- [ ] Multi-language support (Python, Java, JavaScript)
- [ ] Advanced semantic analysis
- [ ] Dependency graph generation
- [ ] Code quality metrics

### Phase 3: Query Engine
- [ ] Custom query language for code analysis
- [ ] Pattern matching and detection
- [ ] Historical trend analysis
- [ ] Integration with development tools

### Phase 4: Autonomic Features
- [ ] Self-optimizing ingestion pipelines
- [ ] Intelligent resource management
- [ ] Predictive analysis
- [ ] Automated refactoring suggestions

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Technical Notes

### Dependencies
- **LibGit2Sharp**: Git repository operations
- **Microsoft.EntityFrameworkCore**: ORM and database access
- **Microsoft.CodeAnalysis**: C# semantic analysis
- **Amazon.S3**: Cloud storage (to be replaced with free alternative)

### Performance Considerations
- Content-addressable storage reduces storage requirements
- Incremental processing minimizes redundant work
- Background processing prevents blocking operations
- Database indexing for efficient querying

### Security Considerations
- Repository URLs should be validated
- Storage credentials should be secured
- Database connections should use proper authentication
- Input validation for all external data

## Contact

For questions or contributions, please open an issue or submit a pull request.