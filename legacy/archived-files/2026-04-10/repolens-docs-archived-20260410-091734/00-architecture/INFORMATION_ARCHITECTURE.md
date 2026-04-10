# Information Architecture

## Navigation Hierarchy

Four levels. Always navigated top-down. Users can jump levels via context bar and breadcrumb.

```
L1: Portfolio Dashboard
    └── L2: Repository Dashboard
            ├── L3: Universal Search      (cross-cutting, accessible from global nav)
            ├── L3: Analytics
            │       ├── Trends tab
            │       ├── Files tab
            │       ├── Team tab
            │       ├── Security tab
            │       └── Compare tab
            ├── L3: Code Graph
            ├── L3: Team Analytics
            └── L4: File / Component Detail
```

| Level | Screen | Primary question answered | Primary user |
|-------|--------|--------------------------|--------------|
| L1 | Portfolio Dashboard | How healthy is my entire codebase? | Engineering Manager, CTO |
| L2 | Repository Dashboard | What needs attention in this repo today? | Engineering Manager, Team Lead |
| L3 | Feature views (analytics / search / graph / team) | What do I need to know about this specific aspect? | Team Lead, Senior Dev, QA |
| L4 | File / component detail | What is wrong with this specific file? | Senior Dev, QA Engineer |

## Global Navigation (Persistent — All Screens)

Five elements, always present:

1. **Logo / home** — returns to Portfolio Dashboard (L1)
2. **Universal search bar** — searches across all repositories. Keyboard shortcut: `Ctrl+K` / `Cmd+K`
3. **Repository context switcher** — shows current repo name + health chip. Click to switch. Shortcut: `Ctrl+R` / `Cmd+R`
4. **Notification bell** — critical alerts only (not ambient updates)
5. **User profile / settings**

**AI assistant:** Persistent floating button, bottom-right corner. Opens as right-side overlay panel (380px wide on desktop). Is NOT part of global navigation. Is NOT shown as a panel by default.

## Context Bar (L2 and below — Persistent)

Appears below global nav. Contains:
- Repository name + health score badge (value + trend arrow)
- Repository switcher dropdown trigger
- Last sync time + manual refresh button
- Breadcrumb: `Portfolio > [Repository name]` or `Portfolio > [Repo] > [Feature]`

## Primary User Flows

### Flow A — Daily Planning (Engineering Manager)
```
Login
  → Portfolio Dashboard (L1)
    → Review health summary — spot repo health drop
      → Click flagged repo → Repository Dashboard (L2)
        → Review Quality Hotspots panel — identify top file
          → Click file → File Detail (L4)
            → Assign ticket or tag team member
```
**Target time: under 90 seconds to reach step 5.**

### Flow B — Sprint Planning (Team Lead)
```
Login
  → Repository Dashboard (L2) for active repo
    → Analytics → Trends tab — review quality over 30 days
      → Quality Hotspots ranked list — identify debt items
        → Universal Search — "payment processing complexity"
          → Export shortlist for upcoming sprint
```

### Flow C — Executive Review (CTO)
```
Login
  → Portfolio Dashboard (L1)
    → Review cross-repo health summary
      → Analytics → Compare tab
        → Export portfolio health report
```

### Flow D — First-Time Onboarding (Any User)
```
Account creation
  → Connect repository screen (single-purpose, no dashboard yet)
    → Analysis in progress (progress bar + estimated time)
      → Repository Dashboard (L2) — analysis complete
        → Contextual tooltip on Quality Hotspots panel (dismissed on click)
```

## Screen Ownership by Role

| Screen | Eng Manager | Team Lead | Senior Dev | QA | CTO |
|--------|-------------|-----------|------------|-----|-----|
| L1 Portfolio | Primary | Read | Occasional | Rarely | Primary |
| L2 Repo Dashboard | Primary | Primary | Daily | Daily | Rarely |
| L3 Analytics | Weekly | Weekly | Occasional | Weekly | Monthly |
| L3 Search | Occasional | Daily | Daily | Daily | Rarely |
| L3 Code Graph | Rarely | Weekly | Weekly | Occasional | Rarely |
| L3 Team Analytics | Weekly | Weekly | Self only | Rarely | Monthly |
| L4 File Detail | Occasional | Weekly | Daily | Daily | Never |
| AI Overlay | Occasional | Occasional | Occasional | Occasional | Rarely |

## URL Structure

```
/                              → Portfolio Dashboard (L1)
/repos/:repoId                 → Repository Dashboard (L2)
/repos/:repoId/analytics       → Analytics (L3) — defaults to Trends tab
/repos/:repoId/analytics/files → Analytics Files tab
/repos/:repoId/analytics/team  → Analytics Team tab
/repos/:repoId/analytics/security → Analytics Security tab
/repos/:repoId/analytics/compare  → Analytics Compare tab
/repos/:repoId/search          → Universal Search scoped to repo (L3)
/repos/:repoId/graph           → Code Graph (L3)
/repos/:repoId/team            → Team Analytics (L3)
/repos/:repoId/files/:fileId   → File Detail (L4)
/search                        → Universal Search across all repos
/settings                      → Settings
/settings/repositories         → Repository management
```

Filter and sort state is reflected in URL query params for shareability.
