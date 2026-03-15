# RepoLens Development Tasks

## Task 001: Fix Compilation Errors

**Priority:** 🔴 Critical
**Estimated Time:** 30 minutes

### Description
Resolve all compilation errors preventing the solution from building successfully.

### Steps
1. **Fix Repository Entity Conflict**
   - Rename `RepoLens.Core.Entities.Repository` to `CodeRepository`
   - Update all references in the codebase
   - Update database schema if needed

2. **Fix byte[] to string Conversion**
   - Locate line 144 in `IngestionService.cs`
   - Replace direct byte[] to string conversion with proper encoding
   - Use `Encoding.UTF8.GetString()` or stream reader

3. **Verify Build Success**
   - Run `dotnet build RepoLens.sln`
   - Ensure no compilation errors remain

### Expected Result
- Solution builds successfully with no errors
- All tests pass (if any exist)

### Files to Modify
- `RepoLens.Core/Entities/Repository.cs`
- `RepoLens.Core/Entities/Repository.cs` (rename to `CodeRepository.cs`)
- `RepoLens.Infrastructure/Repositories/RepositoryRepository.cs`
- `RepoLens.Worker/Services/IngestionService.cs`
- Any other files referencing the old entity name

---

## Task 002: Remove S3 Dependency

**Priority:** 🟡 High
**Estimated Time:** 45 minutes

### Description
Replace Amazon S3 dependency with local content-addressable storage.

### Steps
1. **Remove AWS S3 References**
   - Remove `Amazon.S3` package references
   - Remove S3 configuration from `appsettings.json`
   - Remove S3 client setup from `Program.cs`

2. **Implement Local Storage Service**
   - Create local file system storage implementation
   - Implement content-addressable storage structure:
     ```
     /storage
        /objects
           /ab
             abcd1234...
     ```

3. **Update Storage Interface**
   - Modify `StorageService` to use local storage
   - Ensure backward compatibility with existing code

### Expected Result
- No paid cloud dependencies
- Local storage working with content-addressable structure
- All existing functionality preserved

### Files to Modify
- `RepoLens.Worker/Program.cs`
- `RepoLens.Infrastructure/Storage/StorageService.cs`
- `appsettings.json`
- `RepoLens.Worker/RepoLens.Worker.csproj`

---

## Task 003: Refactor Ingestion Worker

**Priority:** 🟡 Medium
**Estimated Time:** 60 minutes

### Description
Split the monolithic ingestion worker into pipeline stages for better maintainability and testing.

### Steps
1. **Design Pipeline Architecture**
   - RepositoryFetcher: Clone/Update repositories
   - CommitWalker: Walk through commit history
   - ArtifactExtractor: Extract files from commits
   - ArtifactHasher: Generate content hashes
   - StorageWriter: Write to storage

2. **Create Pipeline Interfaces**
   - Define interfaces for each pipeline stage
   - Implement dependency injection for pipeline components

3. **Refactor Existing Code**
   - Extract logic from `IngestionService` into pipeline stages
   - Maintain existing functionality while improving structure

### Expected Result
- Modular pipeline architecture
- Better testability and maintainability
- Clear separation of concerns

### Files to Create/Modify
- `RepoLens.Worker/Pipeline/` (new directory)
- `RepoLens.Worker/Services/IngestionService.cs` (refactor)
- `RepoLens.Worker/Program.cs` (update DI)

---

## Task 004: Implement Semantic Index

**Priority:** 🟢 Medium
**Estimated Time:** 90 minutes

### Description
Build semantic indexing capabilities for extracted artifacts.

### Steps
1. **Design Index Entities**
   - `ArtifactSymbol`: Classes, methods, functions
   - `ArtifactReference`: Cross-file references
   - `ArtifactDependency`: Module dependencies

2. **Implement Indexing Service**
   - Extract semantic information from artifacts
   - Build dependency graphs
   - Store indexed data in database

3. **Integrate with Ingestion Pipeline**
   - Add indexing stage to pipeline
   - Process artifacts after storage

### Expected Result
- Semantic index of repository artifacts
- Dependency graph visualization
- Foundation for advanced querying

### Files to Create
- `RepoLens.Core/Entities/ArtifactSymbol.cs`
- `RepoLens.Core/Entities/ArtifactReference.cs`
- `RepoLens.Core/Entities/ArtifactDependency.cs`
- `RepoLens.Infrastructure/Repositories/` (index repositories)
- `RepoLens.Worker/Pipeline/SemanticIndexer.cs`

---

## Task 005: Create Query Engine Skeleton

**Priority:** 🟢 Low
**Estimated Time:** 120 minutes

### Description
Implement the foundation for a semantic query engine.

### Steps
1. **Design Query Components**
   - `QueryParser`: Parse query strings into AST
   - `QueryAST`: Abstract syntax tree for queries
   - `ExecutionEngine`: Execute queries against index

2. **Implement Basic Query Language**
   - Support for basic filters (file type, date ranges)
   - Simple pattern matching
   - Cross-artifact relationship queries

3. **Create Query API**
   - REST API endpoints for queries
   - Query result serialization
   - Pagination and filtering

### Expected Result
- Basic query engine infrastructure
- REST API for semantic queries
- Foundation for advanced query language

### Files to Create
- `RepoLens.Core/Query/` (new directory)
- `RepoLens.Worker/Controllers/QueryController.cs`
- `RepoLens.Worker/Services/QueryService.cs`

---

## Task 006: Add Comprehensive Testing

**Priority:** 🟡 Medium
**Estimated Time:** 180 minutes

### Description
Implement comprehensive test coverage for all components.

### Steps
1. **Unit Tests**
   - Test all core services and repositories
   - Mock external dependencies (Git, Storage)
   - Use xUnit framework

2. **Integration Tests**
   - Test complete ingestion pipeline
   - Test with real Git repositories
   - Test storage and retrieval

3. **Performance Tests**
   - Benchmark large repository ingestion
   - Memory usage monitoring
   - Storage efficiency tests

### Expected Result
- Comprehensive test coverage
- Automated testing pipeline
- Performance benchmarks

### Files to Create
- `RepoLens.Tests/` (new project)
- Unit test files for all components
- Integration test scenarios
- Performance test suite

---

## Development Guidelines

### Commit Messages
Use conventional commits:
- `fix:` for bug fixes
- `feat:` for new features
- `refactor:` for code restructuring
- `test:` for test additions
- `docs:` for documentation

### Code Review
- All changes require review
- Run `dotnet build` and `dotnet test` before committing
- Ensure backward compatibility
- Update documentation for breaking changes

### Testing Strategy
- Unit tests for all new code
- Integration tests for pipeline components
- End-to-end tests for complete workflows
- Performance tests for critical paths