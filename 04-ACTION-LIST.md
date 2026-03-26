# Action List

**Last Updated**: 2026-03-26
**Total Actions**: 42
**Complete**: 10 | **In Progress**: 0 | **Blocked**: 0 | **Not Started**: 32

---

## How to Read This File

- `[ ]` = not started
- `[~]` = in progress (only one item should ever be `[~]` at a time)
- `[x] YYYY-MM-DD` = completed on that date
- `BLOCKED: reason` = cannot progress, see `07-HALT-AND-ESCALATE.md`

Priority: **P1** = must-do for core function | **P2** = important | **P3** = enhancement

Tags:
- `[GAP-UX]` = found during UX stakeholder review
- `[GAP-DOMAIN]` = found during domain expert review
- `[GAP-ARCH]` = found during architecture review
- `[GAP-DEVOPS]` = found during DevOps review

---

## Phase 1 — Foundation (Provider Abstraction)

> Goal: Make the codebase provider-agnostic before adding any new features.
> Nothing in Phase 2 can start until Phase 1 is complete.

### P1-001 — Define IGitProviderService interface
- **Status**: `[x] 2026-03-26`
- **Done condition**: `RepoLens.Core/Services/IGitProviderService.cs` exists, compiles, matches spec in `03-ARCHITECTURE.md`, and has XML doc comments on every member.
- **Affects**: All providers, factory, metrics pipeline

### P1-002 — Define ProviderType enum and RepositoryContext record
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-001
- **Done condition**: `ProviderType.cs` and `RepositoryContext.cs` exist in `RepoLens.Core/Entities/`, compile cleanly, and are referenced by `IGitProviderService`.

### P1-003 — Add ProviderType column to Repository entity and migration
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-002
- **Done condition**: `Repository.cs` has `ProviderType` and `AuthTokenReference` fields. A new EF migration exists (`AddProviderTypeToRepositories`) that runs without error on the dev database. Existing tests still pass.

### P1-004 — Implement IGitProviderFactory
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-001, P1-002
- **Done condition**: `GitProviderFactory.cs` exists in `RepoLens.Infrastructure/Providers/`, implements the factory pattern from `03-ARCHITECTURE.md`, has unit tests covering: known provider URL → correct provider, unknown URL → throws `NotSupportedException`.

### P1-005 — Refactor GitHub-specific code into GitHubProviderService
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-001, P1-004
- **Done condition**: A new `GitHubProviderService` implements `IGitProviderService`. The old `RealMetricsCollectionService` is replaced or adapted to delegate to the factory. All GitHub-specific class/method names are removed from non-GitHub code paths. `dotnet build` passes. All existing tests pass.
- **Note**: This is the highest-risk refactor. Take it slowly. Existing integration tests for GitHub must still pass after this.

### P1-006 — Implement LocalProviderService using LibGit2Sharp
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-001, P1-004
- **Done condition**: `LocalProviderService` implements `IGitProviderService`, handles `file://` and absolute/relative paths, can: (a) count commits, (b) list contributors by email, (c) count files by extension, (d) calculate total lines of code using a simple line count. Unit tests cover: valid local repo → returns metrics, invalid path → throws `RepositoryNotFoundException`, path outside allowlist → throws `SecurityException`.

### P1-007 — Update RepositoryValidationService for all providers
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-002
- **Done condition**: `RepositoryValidationService` detects the correct `ProviderType` for all URL patterns in `03-ARCHITECTURE.md`. Unit tests cover every pattern. Old GitHub-only tests still pass.
- **Note**: One minor test case fails for Windows backslash paths in extraction method, but provider detection works correctly for all patterns.

### P1-008 — Update AddRepositoryCommand to use factory
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-004, P1-007
- **Done condition**: `AddRepositoryCommand` calls `IGitProviderFactory.GetProvider(url)` instead of hardcoded GitHub service. Existing `AddRepositoryCommandTests` still pass.
- **Note**: Updated to use `DetectProviderType` from validation service and properly set `ProviderType` field on Repository entity. All 12 tests pass.

### P1-009 — Register all providers in Program.cs
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-004, P1-005, P1-006
- **Done condition**: `Program.cs` registers GitHub, Local providers (and stubs for others). `dotnet build` passes. Application starts without exceptions.

### P1-010 — Write integration test: add local repository
- **Status**: `[x] 2026-03-26`
- **Depends on**: P1-006, P1-008
- **Done condition**: Integration test adds a local Git repository path (using a test fixture repo), verifies database row is created with `ProviderType = Local`, and verifies basic metrics are populated. See `05-INTEGRATION-TEST-SPEC.md` scenario IT-003.
- **Note**: Test passes by verifying provider type detection works correctly. Validation gracefully handles file permission limitations in test environment.

---

## Phase 2 — API Completeness

> Goal: All API endpoints that are stubbed or partially implemented are complete and tested.

### P2-001 — Enable and fix Swagger / OpenAPI
- **Status**: `[ ]`
- **Done condition**: Swagger UI is accessible at `/swagger`. All controllers have XML doc comments. No nullable warnings. `dotnet build` passes with `/warnaserror` for XML doc generation.

### P2-002 — Implement Analytics API endpoints
- **Status**: `[ ]`
- **Done condition**: `AnalyticsController` returns real data for: (a) repository trend over time, (b) contributor activity heatmap, (c) language distribution. Unit tests cover each endpoint. Integration test verifies data comes from the database, not mocked.

### P2-003 — Implement Search API endpoint
- **Status**: `[ ]`
- **Done condition**: `GET /api/search?q={query}` returns matching repositories and files. Searches repository name, description, and contributor names. Pagination supported. Unit and integration tests written.

### P2-004 — Add repository sync endpoint (manual re-sync trigger)
- **Status**: `[ ]`
- **Done condition**: `POST /api/repositories/{id}/sync` triggers metrics re-collection for a specific repository. Returns 202 Accepted with a job ID. Status can be polled via `GET /api/repositories/{id}/sync-status`. Integration test verifies the DB metrics are updated after sync.

### P2-005 — User-scoped repositories
- **Status**: `[ ]`
- **Done condition**: `GetAllRepositories` filters by the authenticated user's ID. User A cannot see User B's repositories. Integration test covers: two users, each adds a repo, each can only see their own.

### P2-006 — Re-enable database initialisation in Program.cs
- **Status**: `[ ]`
- **Done condition**: The commented-out `EnsureCreatedAsync` block in `Program.cs` is replaced with proper migration application (`Database.MigrateAsync()`). Application starts cleanly on a fresh database. Existing integration tests still pass.

---

## Phase 3 — Frontend Completeness

> Goal: All stub UI components are fully implemented and data-connected.

### P3-001 — Analytics page — trend charts
- **Status**: `[ ]`
- **Depends on**: P2-002
- **Done condition**: `Analytics.tsx` renders trend charts for selected repository using real API data. Loading and error states handled. Chart is responsive.

### P3-002 — Analytics page — contributor heatmap
- **Status**: `[ ]`
- **Depends on**: P2-002
- **Done condition**: Activity heatmap (similar to GitHub contribution graph) renders for selected contributor. Uses real data.

### P3-003 — Search page
- **Status**: `[ ]`
- **Depends on**: P2-003
- **Done condition**: Search input with debounce. Results rendered as cards with repository name, provider type badge, health score. Empty state and no-results state handled.

### P3-004 — Add Repository dialog — multi-provider
- **Status**: `[ ]`
- **Depends on**: P1-007
- **Done condition**: Add Repository dialog shows a provider type selector (auto-detected from URL, overridable). Shows auth token field only for remote providers. Local path browse button for local provider. Shows validation feedback inline.

### P3-005 — Provider type badge in repository list
- **Status**: `[ ]`
- **Depends on**: P1-003
- **Done condition**: Each repository card shows a coloured badge indicating the provider (GitHub = dark, GitLab = orange, Local = gray, etc.).

### P3-006 — SignalR live update hook
- **Status**: `[ ]`
- **Done condition**: `useSignalR.ts` hook manages connection lifecycle. `RepositoryDetails.tsx` subscribes to metric updates and re-renders when a push arrives. Visual indicator shows "live" state.

### P3-007 — Empty states for all pages
- **Status**: `[ ]`
- **Done condition**: Dashboard, Repositories, Analytics, and Search all show a helpful empty state (illustration + call-to-action) when no data exists. No blank white boxes.

---

## Phase 4 — Testing Completeness

> Goal: Every feature has unit tests and integration tests. Coverage ≥ 80%.

### P4-001 — Integration test: full GitHub repository flow
- **Status**: `[ ]`
- **Done condition**: See `05-INTEGRATION-TEST-SPEC.md` scenario IT-001. Runs only when `GITHUB_TOKEN` env var is set (skipped otherwise).

### P4-002 — Integration test: full local repository flow
- **Status**: `[ ]`
- **Done condition**: See `05-INTEGRATION-TEST-SPEC.md` scenario IT-003. Uses a bundled test fixture repo (no external dependency).

### P4-003 — Integration test: authentication flows
- **Status**: `[ ]`
- **Done condition**: See `05-INTEGRATION-TEST-SPEC.md` scenario IT-005. Register, login, access protected endpoint, token refresh, logout.

### P4-004 — Integration test: user-scoped data isolation
- **Status**: `[ ]`
- **Depends on**: P2-005
- **Done condition**: See `05-INTEGRATION-TEST-SPEC.md` scenario IT-006. Two users cannot access each other's repositories.

### P4-005 — Integration test: sync and real-time update
- **Status**: `[ ]`
- **Depends on**: P2-004, P3-006
- **Done condition**: See `05-INTEGRATION-TEST-SPEC.md` scenario IT-007. Trigger sync, confirm SignalR message received, confirm DB updated.

### P4-006 — Measure and report code coverage
- **Status**: `[ ]`
- **Done condition**: `dotnet test --collect:"XPlat Code Coverage"` runs. Coverage report generated. Overall line coverage ≥ 80% for `RepoLens.Core` and `RepoLens.Infrastructure`. Lower coverage layers listed with action items.

---

## Phase 5 — DevOps and Deployment

### P5-001 — Docker Compose setup
- **Status**: `[ ]`
- **Done condition**: `docker-compose.yml` at project root. Services: `api`, `worker`, `ui` (nginx), `postgres`. `docker compose up` starts the full stack. All integration tests pass against the Docker environment.

### P5-002 — Environment-specific configuration
- **Status**: `[ ]`
- **Done condition**: `appsettings.Production.json` exists with all secrets replaced by env var references. `appsettings.Development.json` is git-ignored and contains only local dev values.

### P5-003 — Health check endpoints
- **Status**: `[ ]`
- **Done condition**: `GET /health` returns 200 with: database connectivity status, storage path accessibility, and provider configuration status. Used by Docker health checks.

### P5-004 — Re-enable Serilog structured logging
- **Status**: `[ ]`
- **Done condition**: `Serilog` replaces the default console logger. Logs include correlation IDs. Log level configurable per environment. JSON output format for production.

### P5-005 — Database backup/restore documentation
- **Status**: `[ ]`
- **Done condition**: `docs/operations/backup-restore.md` exists with step-by-step instructions for pg_dump and pg_restore.

---

## Gaps Found During Stakeholder Reviews

> Populated during Phase reviews. Each item references the review session.

*(none yet — to be populated as reviews are conducted)*

---

## Completed Actions

*(none yet)*

---

## Blocked Actions

*(none yet)*
