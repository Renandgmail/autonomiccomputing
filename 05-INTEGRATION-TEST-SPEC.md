# Integration Test Specification

Every integration test in this project must conform to this specification.
Integration tests live in `RepoLens.Tests/Integration/`.

---

## Test Anatomy

Every integration test must include:

```csharp
/// <summary>
/// IT-XXX: [One sentence describing the user journey being tested]
/// </summary>
[Fact(DisplayName = "IT-XXX: ...")]
[Trait("Category", "Integration")]
public async Task Scenario_WhenCondition_ThenExpectedOutcome()
{
    // ARRANGE — set up the system state
    // ACT — perform the operation via HTTP or direct service call
    // ASSERT HTTP — verify the response
    // ASSERT DB — query the database directly and verify state
    // ASSERT LOGS — verify expected log entries were emitted
}
```

The three assert sections are mandatory. An integration test that only checks the HTTP
response is labelled `[Trait("Category", "ApiSmoke")]`, not `Integration`.

---

## Test Infrastructure

### Database

Integration tests use a dedicated test database, separate from the development database.

```json
// appsettings.Test.json (git-ignored)
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=repolens_test;Username=postgres;Password=CHANGE_ME"
  }
}
```

Use a test fixture that:
1. Applies all migrations before the test class runs
2. Truncates all user-data tables before each test (not drops — truncation is faster)
3. Does NOT drop and recreate the schema between tests (too slow)

```csharp
public class IntegrationTestFixture : IAsyncLifetime
{
    public RepoLensDbContext DbContext { get; private set; }
    public HttpClient ApiClient { get; private set; }

    public async Task InitializeAsync()
    {
        // Start test API host
        // Apply migrations
        // Seed base data (roles, etc.)
    }

    public async Task DisposeAsync()
    {
        // Truncate tables
        // Dispose context
    }
}
```

### Log capture

Use a test-scoped logger sink that collects log entries:

```csharp
public class TestLoggerSink : ILogger
{
    public List<LogEntry> Entries { get; } = new();
    // implementation captures LogLevel, message, and state
}
```

---

## Test Fixture Repository (Local Tests)

For tests that require a local Git repository, use a bundled test fixture.

Location: `RepoLens.Tests/Fixtures/TestRepo/`

This is a real, minimal Git repository committed into the test project:
- 3 commits
- 2 contributors (fixture-user-a@test.com, fixture-user-b@test.com)
- Files: `README.md`, `src/Main.cs`, `src/Helper.cs`, `tests/MainTests.cs`
- 2 branches: `main`, `feature/test-branch`

The fixture repo path is resolved at test time via:
```csharp
var fixturePath = Path.Combine(
    AppContext.BaseDirectory,
    "..", "..", "..", "Fixtures", "TestRepo");
```

---

## Scenario Specifications

---

### IT-001 — Add GitHub Repository (requires GITHUB_TOKEN)

**User journey**: A user adds a public GitHub repository by URL.
The system validates it, creates the DB record, collects metrics, and returns a health score.

**Skip condition**: `Environment.GetEnvironmentVariable("GITHUB_TOKEN") == null`

**Arrange**:
- Authenticated user (call `POST /api/auth/login` first, store JWT)
- URL: `https://github.com/microsoft/vscode` (or a small public repo for speed)

**Act**:
```
POST /api/repositories
Authorization: Bearer {jwt}
{
  "url": "https://github.com/microsoft/vscode",
  "name": "vscode"
}
```

**Assert HTTP**:
- Status: 201 Created
- Body contains `id`, `name`, `url`, `providerType: "GitHub"`
- Body `status` is `"Pending"` or `"Processing"` or `"Completed"` (not `"Failed"`)

**Assert DB**:
```sql
SELECT id, name, url, provider_type, status
FROM repositories
WHERE url = 'https://github.com/microsoft/vscode'
ORDER BY created_at DESC
LIMIT 1;
```
Expected: 1 row, `provider_type = 1` (GitHub), `status != 'Failed'`

```sql
SELECT COUNT(*) FROM repository_metrics
WHERE repository_id = {returned_id};
```
Expected: at least 1 row (metrics were collected)

```sql
SELECT total_contributors, total_files, health_score
FROM repository_metrics
WHERE repository_id = {returned_id}
ORDER BY measurement_date DESC
LIMIT 1;
```
Expected: `total_contributors > 0`, `total_files > 0`, `health_score BETWEEN 0 AND 100`

**Assert Logs**:
- Contains: `"Starting metrics collection"` at `Information` level
- Contains: `"Metrics collection completed"` or `"Collected"` at `Information` level
- Does NOT contain: any `Error` or `Critical` level entries for this repository ID

---

### IT-002 — Duplicate Repository Rejected

**User journey**: A user tries to add a repository that already exists.
The system returns a 409 Conflict without creating a duplicate.

**Arrange**:
- Authenticated user
- Repository already exists in DB (inserted directly via `DbContext` in arrange)

**Act**:
```
POST /api/repositories
Authorization: Bearer {jwt}
{ "url": "{existing url}" }
```

**Assert HTTP**:
- Status: 409 Conflict
- Body contains `"already exists"` (case-insensitive)

**Assert DB**:
```sql
SELECT COUNT(*) FROM repositories WHERE url = '{url}';
```
Expected: exactly 1 (no duplicate created)

**Assert Logs**:
- Contains: `"Repository already exists"` at `Warning` or `Information` level

---

### IT-003 — Add Local Repository

**User journey**: A user adds a local file-system Git repository by path.
The system analyses it using LibGit2Sharp and stores basic metrics.

**Skip condition**: None (uses bundled fixture repo)

**Arrange**:
- Authenticated user
- Local fixture repo path resolved (see Test Fixture section above)

**Act**:
```
POST /api/repositories
Authorization: Bearer {jwt}
{
  "url": "file://{fixturePath}",
  "name": "test-fixture-repo"
}
```

**Assert HTTP**:
- Status: 201 Created
- Body `providerType` is `"Local"`

**Assert DB**:
```sql
SELECT provider_type, status, local_path FROM repositories
WHERE name = 'test-fixture-repo';
```
Expected: `provider_type = 5` (Local), `status = 'Completed'` or `'Processing'`

```sql
SELECT total_contributors, total_commits, total_files
FROM repository_metrics
WHERE repository_id = {returned_id};
```
Expected: `total_contributors = 2`, `total_commits = 3`, `total_files = 4`
(matches the fixture repo exactly)

**Assert Logs**:
- Contains: `"Analysing local repository"` at `Information` level
- Contains: `"Local metrics collection completed"` at `Information` level

---

### IT-004 — Invalid Repository URL Rejected

**User journey**: A user submits a URL that is not a valid Git repository.
The system returns a 400 Bad Request with a clear error message.

**Arrange**:
- Authenticated user

**Act**:
```
POST /api/repositories
{ "url": "https://not-a-git-repo.example.com/foo" }
```

**Assert HTTP**:
- Status: 400 Bad Request
- Body contains a human-readable error message (not a stack trace)

**Assert DB**:
```sql
SELECT COUNT(*) FROM repositories WHERE url = 'https://not-a-git-repo.example.com/foo';
```
Expected: 0 (no row created for an invalid URL)

---

### IT-005 — Authentication Flow

**User journey**: Register → Login → Access protected endpoint → Verify token expiry → Logout

**Test A — Register**:
```
POST /api/auth/register
{ "email": "test-{guid}@example.com", "password": "TestPassword123!" }
```
Assert: 200 OK, body contains `token`
DB: `SELECT COUNT(*) FROM asp_net_users WHERE email = '...'` → 1

**Test B — Login with wrong password**:
```
POST /api/auth/login
{ "email": "test-{guid}@example.com", "password": "WrongPassword" }
```
Assert: 401 Unauthorized

**Test C — Access protected endpoint without token**:
```
GET /api/repositories
(no Authorization header)
```
Assert: 401 Unauthorized

**Test D — Access protected endpoint with valid token**:
```
GET /api/repositories
Authorization: Bearer {token from Test A}
```
Assert: 200 OK

**Assert Logs**:
- Registration: contains `"User registered"` at `Information`
- Failed login: contains `"Failed login attempt"` at `Warning`

---

### IT-006 — User Data Isolation

**User journey**: User A adds a repository. User B cannot see or access it.

**Arrange**:
- Register User A, login, add a repository, store repo ID
- Register User B, login, store User B's JWT

**Act**:
```
GET /api/repositories
Authorization: Bearer {User B's JWT}
```

**Assert HTTP**:
- Status: 200 OK
- Body: empty list (User B has no repositories)

```
GET /api/repositories/{User A's repo ID}
Authorization: Bearer {User B's JWT}
```

**Assert HTTP**:
- Status: 404 Not Found (not 403 — do not reveal existence)

**Assert DB**:
```sql
SELECT user_id FROM repositories WHERE id = {User A's repo ID};
```
Expected: `user_id` matches User A, not User B

---

### IT-007 — Manual Sync Triggers Metric Update and SignalR Push

**User journey**: User triggers a manual re-sync. Metrics update in DB. SignalR message received.

**Arrange**:
- Existing repository with metrics from a previous collection
- Record the `measurement_date` of the existing metrics row
- Open a SignalR test client connected to `/hubs/metrics`
- Call `JoinRepositoryGroup(repoId)`

**Act**:
```
POST /api/repositories/{id}/sync
Authorization: Bearer {jwt}
```

**Assert HTTP**:
- Status: 202 Accepted
- Body contains `{ "jobId": "..." }`

**Poll** `GET /api/repositories/{id}/sync-status` until status is `Completed` or timeout 30s

**Assert DB**:
```sql
SELECT measurement_date FROM repository_metrics
WHERE repository_id = {id}
ORDER BY measurement_date DESC
LIMIT 1;
```
Expected: `measurement_date > {original measurement_date}` (a newer record was created)

**Assert SignalR**:
- The test SignalR client received at least one `MetricsUpdate` message for this repository ID
- The message contains a `healthScore` field

**Assert Logs**:
- Contains: `"Sync triggered for repository {id}"` at `Information`
- Contains: `"Sync completed for repository {id}"` at `Information`

---

### IT-008 — Local Path Outside Allowlist Rejected

**User journey**: A user submits a local path that is not in the `AllowedLocalPaths` config.
The system rejects it as a security violation.

**Arrange**:
- `AllowedLocalPaths` in test config set to `["./TestFixtures"]` only

**Act**:
```
POST /api/repositories
{ "url": "/etc/passwd" }
```

**Assert HTTP**:
- Status: 400 Bad Request
- Body contains `"not in the allowed paths"` or similar (NOT the actual filesystem error)

**Assert DB**:
```sql
SELECT COUNT(*) FROM repositories WHERE url LIKE '%etc%';
```
Expected: 0

---

## Test Naming Convention

`[Scenario]_[Condition]_[ExpectedResult]`

Examples:
- `AddRepository_WithValidGitHubUrl_CreatesRepositoryAndCollectsMetrics`
- `AddRepository_WithDuplicateUrl_Returns409Conflict`
- `AddRepository_WithLocalPathOutsideAllowlist_Returns400BadRequest`
- `Login_WithWrongPassword_Returns401Unauthorized`
