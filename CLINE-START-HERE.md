# Cline Quick Start

> This document is specifically for the Cline agent.
> Read this before reading anything else in this folder.

---

## Your Role

You are the implementation agent for the RepoLens project.
You work in autonomous loops, reading one document at a time, implementing one action at a time.

---

## First Session — Startup Sequence

Run these steps exactly, in this order, before writing a single line of code:

```
1. Read 00-AGENT-INDEX.md         → understand the system
2. Read 01-AGENT-RULES.md         → internalize the non-negotiable rules
3. Read 02-DEVELOPMENT-LOOP.md    → understand how every work cycle operates
4. Read 04-ACTION-LIST.md         → find the first P1 unblocked action
5. Read 03-ARCHITECTURE.md        → understand the target design
```

Only then begin implementing.

---

## Subsequent Sessions — Resume Sequence

At the start of every new session:

```
1. Read 04-ACTION-LIST.md         → see what was done, find the next action
2. Read 01-AGENT-RULES.md         → refresh the rules (they never change)
3. Check 07-HALT-AND-ESCALATE.md  → see if any blockers were resolved
```

Then continue the loop.

---

## Key Commands

```bash
# Build the backend
dotnet build Repolense/RepoLens.sln

# Run all tests
dotnet test Repolense/RepoLens.sln --logger "console;verbosity=normal"

# Run only integration tests
dotnet test Repolense/RepoLens.Tests --filter "Category=Integration"

# Run only unit tests
dotnet test Repolense/RepoLens.Tests --filter "Category!=Integration"

# Add a new migration (run from solution root)
dotnet ef migrations add {MigrationName} \
  --project Repolense/RepoLens.Infrastructure \
  --startup-project Repolense/RepoLens.Api

# Apply migrations
dotnet ef database update \
  --project Repolense/RepoLens.Infrastructure \
  --startup-project Repolense/RepoLens.Api

# Build the frontend
cd repolens-ui && npm install && npm run build
```

---

## The Single Most Important Rule

> Every session ends with `dotnet build` returning exit code 0.
> Never commit broken code. Never leave broken code.
> If you broke it, fix it before stopping.

---

## What "Done" Means

An action item is done when **all of the following are true**:

1. `dotnet build` exits with code 0
2. `dotnet test` has no new failures compared to the session start
3. The integration test for this feature passes (or was written and passes)
4. The database state after the integration test matches the expected SQL in `05-INTEGRATION-TEST-SPEC.md`
5. The action is marked `[x] YYYY-MM-DD` in `04-ACTION-LIST.md`

If any of these is false, the action is not done.

---

## File Locations in This Project

```
Repolense/
├── RepoLens.Api/              → REST API
├── RepoLens.Core/             → Domain (interfaces, entities)
├── RepoLens.Infrastructure/   → Data access, providers, services
├── RepoLens.Worker/           → Background processing
├── RepoLens.Tests/            → All tests
│   ├── Integration/           → Integration tests (Category=Integration)
│   ├── Commands/              → Unit tests for commands
│   ├── Controllers/           → Unit tests for controllers
│   └── Services/              → Unit tests for services
└── repolens-ui/               → React frontend
    └── src/
        ├── components/        → React components
        ├── services/          → API + SignalR services
        ├── types/             → TypeScript types
        └── hooks/             → Custom React hooks (to be created)
```

---

## Common Pitfalls to Avoid

- **Do not** use `!` (null-forgiving) to silence nullable warnings — fix the null check properly
- **Do not** modify existing EF migrations — always add a new one
- **Do not** store API tokens or passwords in source code — use `appsettings.Development.json` or env vars
- **Do not** use `Thread.Sleep` — use `await Task.Delay` or proper async patterns
- **Do not** catch `Exception` broadly without logging — always log before swallowing
- **Do not** add a `[Fact]` test and leave it empty — an empty test always passes and is worse than no test
- **Do not** mark an action `[x]` until the integration test for it passes

---

## When You Are Unsure

If you are unsure how to implement something:

1. First check `03-ARCHITECTURE.md` for design guidance
2. Then check existing code for patterns to follow (e.g., how `GitHubApiService` is structured)
3. If still unsure, implement the simplest thing that satisfies the done condition, add a `// TODO:` comment, and add an action item for the improvement
4. If the uncertainty is about a fundamental design decision, use the halt protocol in `07-HALT-AND-ESCALATE.md`
