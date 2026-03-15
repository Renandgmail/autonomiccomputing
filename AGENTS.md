# RepoLens AI Agent Instructions

## Project: RepoLens

**Goal:** Build an autonomic repository analysis system that ingests git repositories and builds a semantic index of artifacts.

## Architecture

### RepoLens.Core
Domain entities and interfaces.

### RepoLens.Infrastructure
Git integration and storage implementations.

### RepoLens.Worker
Background ingestion worker.

## Rules for AI Agents

1. **Always run `dotnet build` before committing.**
2. **Do not introduce paid cloud dependencies.**
3. **Prefer local content-addressable storage.**
4. **Keep commits small and atomic.**
5. **Do not change public domain entities without migration planning.**

## Current Known Issues

1. **Repository entity conflicts with LibGit2Sharp.Repository**
   - Location: `RepoLens.Worker/Services/IngestionService.cs` line 43
   - Solution: Rename `Repository` entity to `CodeRepository`

2. **byte[] to string conversion error in ingestion**
   - Location: `RepoLens.Worker/Services/IngestionService.cs` line 144
   - Solution: Use proper stream reading or encoding

3. **Amazon S3 dependency should be removed**
   - Location: `RepoLens.Worker/Program.cs`
   - Solution: Replace with local file system storage

## Development Priorities

1. **Fix compilation errors** (Task 001)
2. **Replace S3 with local storage** (Task 002)
3. **Implement ingestion pipeline stages** (Task 003)
4. **Add semantic indexing** (Task 004)
5. **Implement query engine** (Task 005)

## Code Style Guidelines

- Use meaningful variable names
- Add XML documentation for public APIs
- Follow .NET naming conventions
- Use async/await patterns consistently
- Implement proper error handling with custom exceptions

## Testing Requirements

- Unit tests for all core services
- Integration tests for Git operations
- Mock external dependencies in tests
- Use xUnit for test framework

## Performance Considerations

- Use streaming for large file processing
- Implement caching for frequently accessed data
- Use background processing for long-running operations
- Monitor memory usage during repository ingestion

## Security Guidelines

- Validate all repository URLs
- Sanitize file paths to prevent directory traversal
- Use secure hashing algorithms
- Implement proper access controls for storage