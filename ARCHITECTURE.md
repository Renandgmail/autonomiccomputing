# RepoLens Architecture

## System Overview

RepoLens is a semantic repository analysis engine designed to automatically ingest, analyze, and index Git repositories for intelligent codebase querying and analysis. The system provides content-addressable storage, semantic analysis, and a foundation for building intelligent development tools.

## Core Architecture

### Three-Layer Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    RepoLens.Worker                      │
│              (Application Layer - Services)             │
├─────────────────────────────────────────────────────────┤
│              RepoLens.Infrastructure                    │
│           (Data Access & External Services)             │
├─────────────────────────────────────────────────────────┤
│                    RepoLens.Core                        │
│                (Domain Layer - Entities)                │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### RepoLens.Core (Domain Layer)
- **Entities**: Repository, Commit, Artifact, ArtifactVersion
- **Interfaces**: Repository interfaces, service contracts
- **Exceptions**: Custom exception types
- **Pure Domain Logic**: No external dependencies

#### RepoLens.Infrastructure (Data Access Layer)
- **Data Access**: Entity Framework Core with PostgreSQL
- **Git Operations**: LibGit2Sharp integration for repository operations
- **Storage**: Content-addressable storage service
- **Repositories**: Entity Framework implementations
- **External Service Integration**: Database, Git, Storage

#### RepoLens.Worker (Application Layer)
- **Background Services**: Continuous repository ingestion
- **Business Logic**: Ingestion pipeline orchestration
- **Configuration**: Application settings and dependency injection
- **Entry Points**: Main program execution

## System Components

### 1. Repository Management

#### CodeRepository Entity
```csharp
public class CodeRepository
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string LocalPath { get; set; }
    public string LastSyncCommit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSyncAt { get; set; }
}
```

**Responsibilities:**
- Track repository metadata
- Manage synchronization state
- Store local working directory

### 2. Commit Processing

#### Commit Entity
```csharp
public class Commit
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    public string Sha { get; set; }
    public string Message { get; set; }
    public DateTime CommitDate { get; set; }
    public string Author { get; set; }
}
```

**Responsibilities:**
- Track commit history
- Enable incremental processing
- Support historical analysis

### 3. Artifact Management

#### Artifact Entity
```csharp
public class Artifact
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    public string Path { get; set; }
    public string FileType { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### ArtifactVersion Entity
```csharp
public class ArtifactVersion
{
    public Guid Id { get; set; }
    public Guid ArtifactId { get; set; }
    public Guid CommitId { get; set; }
    public string ContentHash { get; set; }
    public string StoredAt { get; set; }
    public string Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Responsibilities:**
- Track file versions across commits
- Enable content deduplication
- Store semantic metadata

### 4. Ingestion Pipeline

#### Pipeline Architecture
```
Repository → Commit Walker → Artifact Extractor → Storage → Index
```

**Pipeline Stages:**
1. **RepositoryFetcher**: Clone/update repositories
2. **CommitWalker**: Walk commit history
3. **ArtifactExtractor**: Extract files from commits
4. **ArtifactHasher**: Generate content hashes
5. **StorageWriter**: Write to content-addressable storage
6. **Indexer**: Build semantic index

### 5. Storage System

#### Content-Addressable Storage
```
/storage
   /objects
      /ab
        abcd1234567890...
      /cd
        cdef1234567890...
```

**Features:**
- SHA256-based content addressing
- Deduplication across repositories
- Immutable storage for version control
- Efficient retrieval by content hash

### 6. Semantic Analysis

#### C# Code Analysis
- **Roslyn Integration**: Parse C# syntax trees
- **Symbol Extraction**: Classes, methods, namespaces
- **Metadata Storage**: JSON-serialized semantic information
- **Cross-Reference**: Build dependency graphs

## Data Flow

### Repository Ingestion Flow
```
1. Repository URL → Git Clone
2. Commit History → Commit Walking
3. File Extraction → Content Hashing
4. Storage → Content-Addressable Storage
5. Analysis → Semantic Metadata
6. Indexing → Query Engine
```

### Query Flow
```
1. Query Input → Query Parser
2. AST Generation → Query AST
3. Index Lookup → Semantic Index
4. Result Processing → Query Engine
5. Response → API/CLI
```

## External Dependencies

### Required Dependencies
- **LibGit2Sharp**: Git repository operations
- **Microsoft.EntityFrameworkCore**: ORM and database access
- **Microsoft.CodeAnalysis**: C# semantic analysis
- **PostgreSQL**: Primary database storage

### Optional Dependencies (to be replaced)
- **Amazon.S3**: Cloud storage (planned for replacement with local storage)

## Configuration Architecture

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=repolens;Username=postgres;Password=TCEP"
  },
  "Storage": {
    "Type": "Local",
    "Path": "./storage"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "RepoLens": "Debug"
    }
  }
}
```

## Future Architecture Extensions

### Phase 2: Enhanced Analysis
- **Multi-Language Support**: Python, Java, JavaScript parsers
- **Advanced Semantic Analysis**: Type inference, control flow
- **Dependency Graph Generation**: Cross-artifact relationships
- **Code Quality Metrics**: Complexity, maintainability scores

### Phase 3: Query Engine
- **Custom Query Language**: Domain-specific language for code analysis
- **Pattern Matching**: Regular expressions and AST patterns
- **Historical Trend Analysis**: Code evolution over time
- **Integration APIs**: REST and GraphQL endpoints

### Phase 4: Autonomic Features
- **Self-Optimizing Pipelines**: Adaptive processing based on repository characteristics
- **Intelligent Resource Management**: Dynamic allocation based on workload
- **Predictive Analysis**: Anticipate developer needs
- **Automated Refactoring**: Suggest and apply code improvements

## Security Architecture

### Input Validation
- Repository URL validation
- File path sanitization
- Content size limits

### Access Control
- Repository access permissions
- Storage access controls
- API authentication and authorization

### Data Protection
- Secure hashing algorithms
- Encrypted storage options
- Audit logging for compliance

## Performance Architecture

### Caching Strategy
- Repository metadata caching
- Commit history caching
- Artifact metadata caching

### Parallel Processing
- Multi-repository ingestion
- Parallel artifact processing
- Background indexing

### Resource Management
- Memory usage monitoring
- Disk space management
- Database connection pooling

## Monitoring and Observability

### Logging Strategy
- Structured logging with Serilog
- Performance metrics collection
- Error tracking and alerting

### Health Monitoring
- Repository sync status
- Storage utilization
- Processing pipeline health

### Metrics Collection
- Ingestion performance metrics
- Storage efficiency metrics
- Query performance metrics

This architecture provides a solid foundation for building a sophisticated repository analysis system while maintaining flexibility for future enhancements and extensions.