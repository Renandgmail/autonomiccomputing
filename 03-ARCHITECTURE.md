# RepoLens Architecture

> This document describes the **target architecture** — what the system should look like
> after the current development programme completes. Where current code differs,
> treat this document as the authoritative specification.

---

## Core Design Principle: Provider Agnosticism

The original codebase assumed GitHub. The target architecture treats every repository source
as a pluggable provider behind a common interface. The rest of the system (metrics, storage,
UI, analytics) is entirely provider-neutral.

```
Any Git source → IRepositoryProvider → Common Metrics Model → Storage → UI
```

Supported providers (target):

| Provider | Type | Auth mechanism | Current status |
|----------|------|----------------|----------------|
| GitHub | Remote API | Personal Access Token / OAuth | Exists, needs refactor |
| GitLab | Remote API | Personal Access Token / OAuth | Not built |
| Bitbucket | Remote API | App password / OAuth | Not built |
| Azure DevOps | Remote API | PAT / Azure AD | Not built |
| Local | File system | None (path access) | Partially wired (LibGit2Sharp) |

---

## Layer Structure

```
┌────────────────────────────────────────────────────────────────┐
│  RepoLens.Api  (REST controllers, JWT auth, SignalR hub)       │
├────────────────────────────────────────────────────────────────┤
│  RepoLens.Core  (domain entities, interfaces, exceptions)      │
│  — no external dependencies; all other layers depend on this   │
├────────────────────────────────────────────────────────────────┤
│  RepoLens.Infrastructure  (EF Core, provider implementations)  │
│  ├── Providers/                                                │
│  │   ├── GitHub/    GitHubProviderService.cs                  │
│  │   ├── GitLab/    GitLabProviderService.cs                  │
│  │   ├── Bitbucket/ BitbucketProviderService.cs               │
│  │   ├── AzureDevOps/ AzureDevOpsProviderService.cs           │
│  │   └── Local/    LocalProviderService.cs                    │
│  ├── Repositories/ (EF Core implementations)                  │
│  ├── Services/     (MetricsCollection, Storage, Config)        │
│  └── Git/          (LibGit2Sharp wrapper)                      │
├────────────────────────────────────────────────────────────────┤
│  RepoLens.Worker  (background ingestion pipeline)              │
├────────────────────────────────────────────────────────────────┤
│  repolens-ui  (React 18 + TypeScript + MUI)                    │
└────────────────────────────────────────────────────────────────┘
```

---

## Core Domain Interfaces (RepoLens.Core)

### IGitProviderService

The single interface every provider must implement:

```csharp
public interface IGitProviderService
{
    /// <summary>Provider type this service handles</summary>
    ProviderType ProviderType { get; }

    /// <summary>True if this service can handle the given URL</summary>
    bool CanHandle(string repositoryUrl);

    /// <summary>Collect full repository metrics</summary>
    Task<RepositoryMetrics> CollectMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>Collect per-contributor metrics</summary>
    Task<IReadOnlyList<ContributorMetrics>> CollectContributorMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>Collect per-file metrics</summary>
    Task<IReadOnlyList<FileMetrics>> CollectFileMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>Validate that the repository URL is accessible</summary>
    Task<ValidationResult> ValidateAccessAsync(
        string repositoryUrl,
        CancellationToken ct = default);
}
```

### RepositoryContext

Carries all the data a provider needs to work with a specific repository:

```csharp
public record RepositoryContext(
    int RepositoryId,
    string Url,
    ProviderType ProviderType,
    string? AuthToken,          // null for public repos or local paths
    string? LocalClonePath,     // set after the repo is cloned locally
    string? Owner,              // null for local repos
    string? RepoName            // null for local repos (use path instead)
);
```

### ProviderType enum

```csharp
public enum ProviderType
{
    Unknown = 0,
    GitHub = 1,
    GitLab = 2,
    Bitbucket = 3,
    AzureDevOps = 4,
    Local = 5
}
```

---

## Provider Factory

A factory resolves the correct provider at runtime based on the repository URL:

```csharp
public interface IGitProviderFactory
{
    IGitProviderService GetProvider(string repositoryUrl);
    IGitProviderService GetProvider(ProviderType providerType);
    IEnumerable<ProviderType> SupportedProviders { get; }
}
```

Registration in `Program.cs`:

```csharp
builder.Services.AddScoped<IGitProviderService, GitHubProviderService>();
builder.Services.AddScoped<IGitProviderService, GitLabProviderService>();
builder.Services.AddScoped<IGitProviderService, LocalProviderService>();
// ... etc

builder.Services.AddScoped<IGitProviderFactory, GitProviderFactory>();
```

The factory uses constructor-injected `IEnumerable<IGitProviderService>` and calls
`CanHandle(url)` on each in registration order. First match wins.

---

## Repository Entity Changes

The `Repository` entity needs two new fields:

```csharp
public class Repository
{
    // existing fields ...
    public ProviderType ProviderType { get; set; }  // new
    public string? AuthTokenReference { get; set; } // new — reference to secrets store, NOT the token itself
}
```

Add a migration after adding these fields.

---

## URL Pattern Matching

Each provider's `CanHandle` implementation should use the following patterns:

| Provider | Pattern examples |
|----------|-----------------|
| GitHub | `https://github.com/`, `git@github.com:`, `github.com/` |
| GitLab | `https://gitlab.com/`, `git@gitlab.com:`, custom GitLab host |
| Bitbucket | `https://bitbucket.org/`, `git@bitbucket.org:` |
| Azure DevOps | `https://dev.azure.com/`, `https://*.visualstudio.com/` |
| Local | `file://`, absolute path starting with `/` or `C:\`, relative path starting with `./` or `..\` |

Local paths are the fallback — if no remote provider matches, try local.

---

## Metrics Collection Pipeline

```
Repository URL
     │
     ▼
IGitProviderFactory.GetProvider(url)
     │
     ▼
IGitProviderService.ValidateAccessAsync(url)
     │  ← abort and return error if fails
     ▼
IGitProviderService.CollectMetricsAsync(context)
IGitProviderService.CollectContributorMetricsAsync(context)
IGitProviderService.CollectFileMetricsAsync(context)
     │
     ▼
Persist to PostgreSQL via IRepositoryMetricsRepository
     │
     ▼
Broadcast update via IMetricsNotificationService (SignalR)
```

The pipeline is orchestrated by `MetricsCollectionService` (new unified version),
which replaces the current split between `MetricsCollectionService` and
`RealMetricsCollectionService`.

---

## Local Repository Support

For local repos, LibGit2Sharp provides all data:

```csharp
public class LocalProviderService : IGitProviderService
{
    public ProviderType ProviderType => ProviderType.Local;

    public bool CanHandle(string url) =>
        url.StartsWith("file://") ||
        url.StartsWith("/") ||
        url.StartsWith("./") ||
        url.StartsWith("../") ||
        (url.Length >= 3 && url[1] == ':'); // Windows absolute path

    public async Task<RepositoryMetrics> CollectMetricsAsync(
        RepositoryContext context, CancellationToken ct)
    {
        var localPath = ResolveLocalPath(context.Url);
        using var repo = new LibGit2Sharp.Repository(localPath);

        // Walk commit history
        // Count contributors by commit author email
        // Analyse file tree
        // Compute metrics from local data only
    }
}
```

For local repos, auth tokens are not used. The `LocalClonePath` is the same as the URL.

---

## Database Schema

Key tables (simplified):

```
repositories
  id, name, url, provider_type, auth_token_reference,
  local_path, last_sync_commit, status, created_at, updated_at

repository_metrics
  id, repository_id, measurement_date,
  [all existing metric columns],
  provider_specific_metadata JSONB

contributor_metrics
  id, repository_id, contributor_name, contributor_email,
  period_start, period_end, [all metric columns]

file_metrics
  id, repository_id, file_path, file_type,
  [all metric columns]

commits
  id, repository_id, sha, message, author, commit_date

artifacts
  id, repository_id, path, file_type, created_at

artifact_versions
  id, artifact_id, commit_id, content_hash, stored_at, metadata JSONB, created_at
```

---

## Frontend Architecture

```
src/
├── components/
│   ├── auth/          Login.tsx, Register.tsx
│   ├── dashboard/     Dashboard.tsx
│   ├── repositories/  Repositories.tsx, RepositoryDetails.tsx, AddRepositoryDialog.tsx
│   ├── analytics/     Analytics.tsx (currently stub — needs building)
│   ├── search/        Search.tsx (currently stub — needs building)
│   ├── charts/        TrendChart.tsx, ContributorChart.tsx, LanguageChart.tsx
│   └── layout/        MainLayout.tsx, Sidebar.tsx, Header.tsx
├── services/
│   ├── apiService.ts  (existing — centralised Axios instance)
│   └── signalrService.ts  (new — SignalR connection management)
├── hooks/
│   ├── useRepositories.ts
│   ├── useMetrics.ts
│   └── useSignalR.ts
├── types/
│   └── api.ts         (existing — extend with ProviderType enum)
└── config/
    └── app-config.json
```

---

## Security Constraints

1. Auth tokens for remote providers are stored as **references** in the database, not the tokens themselves. The actual tokens go in environment variables or a secrets manager keyed by the reference.
2. Local repository paths must be validated against a configurable allowlist in `appsettings.json` (`AllowedLocalPaths`). A request to analyse `/etc/passwd` must be rejected.
3. JWT secret key must be at least 32 characters. The default dev key in `Program.cs` must be replaced in any non-local environment.
4. CORS is currently open to `localhost:3000`. In production, this must be replaced with an explicit allowed-origins list.

---

## Configuration Structure (target appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=repolens_db;Username=postgres;Password=CHANGE_ME"
  },
  "JwtSettings": {
    "SecretKey": "SET_VIA_ENV_VAR_REPOLENS_JWT_SECRET",
    "Issuer": "RepoLens.Api",
    "Audience": "RepoLens.Web",
    "ExpiryMinutes": 60
  },
  "GitHub": {
    "ApiToken": "SET_VIA_ENV_VAR_GITHUB_TOKEN"
  },
  "GitLab": {
    "ApiToken": "SET_VIA_ENV_VAR_GITLAB_TOKEN"
  },
  "Storage": {
    "Type": "Local",
    "LocalPath": "./storage"
  },
  "AllowedLocalPaths": [
    "C:/repos",
    "/home/user/repos"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "RepoLens": "Debug"
    }
  }
}
```
