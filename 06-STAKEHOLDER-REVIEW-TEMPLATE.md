# Stakeholder Review Template

Run this review at the end of each phase, or after any significant batch of completed actions.

**Instructions**:
1. Go through each perspective's checklist
2. For every item marked ❌ or ⚠️, create an action item in `04-ACTION-LIST.md`
   with the `[GAP-UX]`, `[GAP-DOMAIN]`, `[GAP-ARCH]`, or `[GAP-DEVOPS]` tag
3. Do **not** fix gaps during this review session — log and move on
4. Mark this template with the date and phase after completing it

---

## Review Record

| Review | Date | Phase | Conducted by |
|--------|------|-------|--------------|
| *(first review)* | | Phase 1 | Agent |

---

## Perspective 1 — UX Expert

*Ask: "Would a developer with no knowledge of this codebase be able to use this product effectively?"*

### Onboarding & Discoverability
- [ ] Is there an empty-state UI for every page? (Not a blank white box when no data exists)
- [ ] Does the Add Repository flow indicate which URL formats are accepted?
- [ ] Is the provider type auto-detected and shown to the user before they submit?
- [ ] Are loading states shown for every async operation (adding a repo, collecting metrics)?
- [ ] Are error messages human-readable? (Not "500 Internal Server Error" but "Could not connect to the GitHub API. Check your token.")

### Feedback & Status
- [ ] Can a user tell if a metrics collection is in progress, complete, or failed?
- [ ] Does the dashboard reflect real-time updates without a manual page refresh?
- [ ] Is there a way to see the last time metrics were collected for each repository?

### Navigation
- [ ] Is the sidebar navigation consistent across all pages?
- [ ] Can a user reach any page in ≤ 3 clicks from the dashboard?
- [ ] Are back-navigation flows handled correctly (browser back button)?

### Accessibility
- [ ] Are all interactive elements keyboard-accessible?
- [ ] Are colour choices functional for colour-blind users? (Health score should not rely on red/green alone)
- [ ] Are form labels associated with their inputs (not just placeholder text)?

### Mobile / Responsiveness
- [ ] Does the layout degrade gracefully at 768px width?
- [ ] Are charts readable on small screens?

---

## Perspective 2 — Domain Expert

*Ask: "Are the metrics calculated correctly? Do they mean what they say they mean?"*

### Metric Accuracy
- [ ] Is the health score formula documented and explainable to an end user?
- [ ] Are complexity metrics (cyclomatic, cognitive, Halstead) computed from actual AST parsing, or estimated from line counts?
  - ⚠️ Currently these appear to be estimated. This should be flagged as a limitation in the UI.
- [ ] Is "technical debt hours" computed from a known standard (SQALE, SonarQube model) or is it a heuristic? Is it labelled clearly?
- [ ] Does "bus factor" correctly identify the single contributor whose departure would most impact the project?
- [ ] Is test coverage sourced from actual test runner output, or inferred from the presence of test files?

### Provider Coverage
- [ ] Are local repositories treated as first-class citizens (not just a fallback)?
- [ ] For remote providers without commit-level API access, are metrics clearly marked as "estimated" vs "measured"?
- [ ] Are private repositories supported? Is the user clearly told what permissions the auth token needs?

### Time Ranges
- [ ] Is the user able to see metrics for different time periods (last week, last month, last quarter)?
- [ ] Are trends computed correctly when the measurement window changes?

### Edge Cases
- [ ] What happens with an empty repository (no commits)?
- [ ] What happens with a repository that has a single contributor?
- [ ] What happens with a very large repository (100k+ files)?
- [ ] What happens with a monorepo (multiple projects in one repo)?

---

## Perspective 3 — Architect

*Ask: "Is the system correctly layered, extensible, and free of anti-patterns?"*

### Dependency Direction
- [ ] Does `RepoLens.Core` have zero dependencies on `RepoLens.Infrastructure`?
- [ ] Does `RepoLens.Api` depend only on `RepoLens.Core` interfaces (not Infrastructure implementations directly)?
- [ ] Are there any circular dependencies between projects?

### Provider Abstraction
- [ ] Is `IGitProviderService` the only interface API-layer code talks to? (No `GitHubApiService` references in controllers)
- [ ] Is the provider factory the only place where `ProviderType` is used to branch logic?
- [ ] Can a new provider be added by implementing one interface and registering it — without modifying existing code?

### Data Model
- [ ] Are auth tokens stored as references (not raw values) in the database?
- [ ] Is the `ProviderType` column indexed for query performance?
- [ ] Are JSONB columns (`LanguageDistribution`, `HourlyActivityPattern`, etc.) typed strongly on the application side?

### Error Handling
- [ ] Are custom exception types used throughout (`RepositoryNotFoundException`, `ProviderAuthException`, etc.)?
- [ ] Are all unhandled exceptions caught at the controller level and returned as structured error responses?
- [ ] Are transient errors (network timeout, rate limit) retried with backoff?

### Scalability
- [ ] Could two workers process metrics for two different repositories simultaneously without conflict?
- [ ] Is the metrics collection pipeline safe to run as a background job without blocking the API?
- [ ] Is there a maximum page size on list endpoints to prevent unbounded queries?

### Testing
- [ ] Is the test project independent enough that it could be run against any environment (dev, CI, staging)?
- [ ] Are integration tests using test transactions or table truncation (not polluting shared data)?

---

## Perspective 4 — DevOps

*Ask: "Can this system be deployed, monitored, maintained, and rolled back safely?"*

### Deployment
- [ ] Does a `docker-compose.yml` exist?
- [ ] Are all environment-specific values (DB passwords, JWT secrets, API tokens) injected via environment variables?
- [ ] Is there a documented startup order (DB must be ready before API starts)?
- [ ] Does the API use `Database.MigrateAsync()` (not `EnsureCreated`) so migrations are applied automatically on start?

### Health and Observability
- [ ] Is there a `/health` endpoint returning structured JSON?
- [ ] Does the health check test: database connectivity, storage path write access, and provider configuration?
- [ ] Are logs structured (JSON format) and include a correlation ID per request?
- [ ] Are metrics (request count, latency p99, error rate) exported to a monitoring system?

### Security
- [ ] Is the JWT secret key at least 32 characters and injected via env var (not hardcoded)?
- [ ] Is CORS restricted to known origins in production?
- [ ] Are SQL queries using parameterised values everywhere (EF Core: yes by default, but verify raw SQL)?
- [ ] Is HTTPS enforced (not just redirected)?

### Backup and Recovery
- [ ] Is there a documented backup strategy for PostgreSQL?
- [ ] Is the storage path (for artifact content-addressable storage) included in the backup?
- [ ] Is there a documented rollback plan for database migrations?

### CI/CD
- [ ] Does a CI pipeline exist that runs `dotnet build` and `dotnet test` on every push?
- [ ] Are integration tests separated from unit tests in CI (different pipeline stage or job)?
- [ ] Is the Docker image built and pushed to a registry as part of CI?
- [ ] Are failing tests a hard block on merge (not just warnings)?

---

## Post-Review Actions

After completing the review, append to `04-ACTION-LIST.md`:

```markdown
## Gaps Found — [Phase Name] Review — [Date]

### [GAP-UX-001] [Short title]
- **Found during**: UX review, Phase X
- **Description**: [What is wrong or missing]
- **Priority**: P2
- **Done condition**: [Specific, testable outcome]
- **Status**: `[ ]`

### [GAP-ARCH-001] [Short title]
...
```

Assign priorities:
- **P1** if the gap is a correctness or security issue
- **P2** if the gap significantly degrades the user experience or system reliability
- **P3** if it's a polish or enhancement item
