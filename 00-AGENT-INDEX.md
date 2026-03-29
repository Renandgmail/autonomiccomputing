# RepoLens Agent Development System — Master Index

> **Read this document first before reading anything else.**
> This index tells you what every document is for, in what order to read them,
> and the exact rules that govern every decision you make.

---

## What You Are Building

**RepoLens** is a repository analytics platform that ingests, analyses, and visualises metrics from *any* Git repository — GitHub, GitLab, Bitbucket, Azure DevOps, local file-system clones, and bare Git repos. It is NOT limited to GitHub. The current codebase already has GitHub-specific API code; your job is to extend it to be provider-agnostic while keeping everything that already works.

Tech stack:
- **Backend**: .NET 10, ASP.NET Core, Entity Framework Core, PostgreSQL 15+, SignalR, LibGit2Sharp
- **Frontend**: React 18 + TypeScript, Material UI v6, Chart.js, React Router v6, Axios
- **Testing**: xUnit, Moq, FluentAssertions (backend); React Testing Library, Jest (frontend)
- **Background**: .NET Worker Service (hosted background service)

---

## Document Map

Read documents in this exact order for each phase of work:

| # | File | Purpose | When to Read |
|---|------|---------|--------------|
| 00 | `00-AGENT-INDEX.md` | **This file** — system overview and rules | First, always |
| 01 | `01-AGENT-RULES.md` | Immutable operating rules for every loop iteration | Before starting any task |
| 02 | `02-DEVELOPMENT-LOOP.md` | The step-by-step loop: implement → compile → regress → integrate → review → prioritise | Before every implementation cycle |
| 03 | `01-SYSTEM-ARCHITECTURE-AND-DESIGN.md` | Full system architecture, provider-agnostic design, extension points | When designing or changing structure |
| 04 | `04-ACTION-LIST.md` | The living backlog — tasks, statuses, priorities, stakeholder gaps | To find what to do next and to record what was done |
| 05 | `05-INTEGRATION-TEST-SPEC.md` | Exact specification for every integration test scenario | When writing or reviewing integration tests |
| 06 | `06-STAKEHOLDER-REVIEW-TEMPLATE.md` | Review checklist for UX, Domain, Architecture, DevOps perspectives | When conducting end-of-phase stakeholder reviews |
| 07 | `07-HALT-AND-ESCALATE.md` | Rules for what to do when stuck, when to stop, how to document blockers | When you hit a problem you cannot solve |

---

## Current Project State

| Layer | Status | Notes |
|-------|--------|-------|
| Backend compile | ✅ Passing | As of project creation |
| GitHub metrics collection | ✅ Working | `RealMetricsCollectionService` |
| JWT auth | ✅ Working | ASP.NET Identity + JWT |
| PostgreSQL schema | ✅ Migrated | `InitialCreatePostgreSQL` migration exists |
| SignalR hub | ✅ Wired | `MetricsHub` — auth-gated |
| Multi-provider support | ❌ Missing | Only GitHub API today |
| File-based repo support | ❌ Missing | LibGit2Sharp exists but not surfaced |
| Analytics UI | ❌ Stub | Component exists, no data |
| Search UI | ❌ Stub | Component exists, no data |
| Integration tests | ⚠️ Partial | PostgreSQL + VSCode integration tests exist |
| Docker / deployment | ❌ Missing | Only `.bat` script exists |
| Swagger | ⚠️ Disabled | Commented out in `Program.cs` |
| Redis cache | ❌ Not implemented | Mentioned in README |

---

## The One Rule Above All Others

> **Never break a passing test. Never leave the codebase in a state that does not compile.**
>
> Every single loop iteration ends with `dotnet build` returning exit code 0
> and `dotnet test` not introducing any *new* failures compared to the state
> at the start of that iteration.

---

## How to Use the Action List

`04-ACTION-LIST.md` is the single source of truth for what has been done and what comes next.

After every loop iteration, update it:
1. Mark completed actions as `[x]` with the date
2. Add any newly discovered tasks (discovered during implementation, compile errors, review gaps)
3. Add the reason for any blocked task with a `BLOCKED:` prefix
4. After updating, re-read the priority order and identify the single highest-priority unblocked task

The action list is also the audit trail. Every time you change something, it must appear in the action list.

---

## Glossary

| Term | Meaning |
|------|---------|
| **Loop iteration** | One complete cycle of: pick task → implement → compile → regress-check → integration-test → stakeholder-review → update action list |
| **Provider** | A source-control platform: GitHub, GitLab, Bitbucket, Azure DevOps, or Local (file-system) |
| **Integration test** | An automated test that exercises the full stack end-to-end: HTTP request → business logic → database write → database read → assertion |
| **Regression** | A previously-passing test that now fails because of a change made in the current iteration |
| **Stakeholder review** | A structured checklist pass from four perspectives: UX, Domain Expert, Architect, DevOps — gaps are recorded, NOT solved in the same iteration |
| **Action item** | A single, atomic piece of work with a clear done-condition |
| **Blocked** | A task that cannot be progressed because of a dependency, ambiguity, or unresolved error after 3 attempts |
