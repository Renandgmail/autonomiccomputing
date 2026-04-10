# Tech Stack

## Frontend

| Layer | Choice | Version | Reason |
|-------|--------|---------|--------|
| Framework | React | 18.x | Concurrent features, wide ecosystem |
| Language | TypeScript | 5.x | Strict mode, type safety across 200+ metrics |
| UI Components | Material-UI | v5 | Comprehensive, accessible, enterprise-grade |
| Data viz (charts) | Recharts | latest | Chart.js-compatible, React-native |
| Data viz (graph) | D3.js | latest | Code graph requires custom force/hierarchical layouts |
| State management | React Query + Zustand | latest | Server state (React Query) + UI state (Zustand) |
| Routing | React Router | v6 | URL-reflected filter state, nested routes |
| Fonts | IBM Plex Sans + IBM Plex Mono | — | Open licence, technical clarity, mono companion |

## Backend

| Layer | Choice | Version |
|-------|--------|---------|
| API | .NET | 8.0 |
| ORM | Entity Framework Core | latest |
| Real-time | SignalR | — |
| Logging | Serilog | — |
| Mapping | AutoMapper | — |

## Infrastructure

| Layer | Choice |
|-------|--------|
| Database | PostgreSQL 15+ |
| Containerisation | Docker |
| Reverse proxy | Nginx |
| Search indexing | Elasticsearch |

## Key Architecture Constraints

1. **Stateless API** — all session state lives in the frontend or DB. Required for horizontal scaling.
2. **SignalR for live updates** — analysis progress, sync status, and quality gate events are pushed via SignalR hub. Do not poll.
3. **Background Worker Service** — repository analysis runs in a separate .NET Worker Service, not the API process. Analysis jobs are queued.
4. **PostgreSQL full-text search** — GIN indexes on code content for natural language search. Elasticsearch supplements for complex semantic queries.
5. **React Query for data fetching** — all API calls go through React Query. No raw fetch calls in components. Cache invalidation on SignalR events.

## Performance Targets (Frontend)

| Interaction | Target | Max |
|-------------|--------|-----|
| Repository context switch | < 500ms | < 1000ms |
| Portfolio dashboard initial load | < 1500ms | < 2500ms |
| Repository dashboard load | < 1000ms | < 2000ms |
| Search results | < 400ms | < 800ms |
| Analytics chart render | < 600ms | < 1200ms |
| Code graph (< 500 nodes) | < 1500ms | < 3000ms |
| AI assistant first token | < 1000ms | < 2000ms |
| Filter / sort | < 200ms | < 400ms |

## API Response Targets

- All endpoints: < 500ms (currently achieving < 300ms)
- Database queries: < 100ms (currently achieving < 80ms)
- Frontend load: < 2s (currently achieving < 1.5s)

## Supported Git Providers

```typescript
enum ProviderType {
  GitHub = 0,
  GitLab = 1,
  Bitbucket = 2,
  AzureDevOps = 3,
  LocalGit = 4
}
```

## Database Schema Summary

30 tables total (23 core + 7 Identity):
- **Identity (7):** Organizations, Users, Roles, UserRoles, UserClaims, RoleClaims, UserTokens
- **Repository (3):** Repositories, RepositoryFiles, Commits
- **Code Intelligence (1):** CodeElements (AST-parsed classes, methods, functions)
- **Vocabulary (6):** VocabularyTerms, VocabularyLocations, VocabularyTermRelationships, BusinessConcepts, VocabularyStats
- **Metrics (3):** RepositoryMetrics (70+ fields), FileMetrics (80+ fields), ContributorMetrics (50+ fields)
- **Artifacts (2):** Artifacts, ArtifactVersions

## Analysis Levels

Three progressive analysis tiers, configurable per repository:

| Level | Features | RAM | Processing time | Target |
|-------|----------|-----|-----------------|--------|
| Basic | File metrics, basic complexity, security basics | < 50MB | < 1 min | Small repos, quick insights |
| Advanced | Full complexity, vocabulary analysis, dependencies | < 200MB | < 5 min | Medium repos, detailed analysis |
| Expert | AST analysis, graph construction, full indexing | < 500MB | < 15 min | Large repos, complete intelligence |
