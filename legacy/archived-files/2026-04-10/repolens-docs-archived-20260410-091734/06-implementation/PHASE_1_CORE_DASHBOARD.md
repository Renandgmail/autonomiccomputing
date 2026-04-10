# Phase 1: Core Dashboard

**Timeline:** Weeks 1–4  
**Goal:** Deliver the structural foundation. Every screen in Phase 1 must be production quality — accessible, responsive, performant — before Phase 2 begins. No exceptions.

**Rule:** Phase 2 does not start until Phase 1 passes a full QA pass at all breakpoints.

---

## Deliverables

### 1. Global Navigation

**File:** `src/components/layout/GlobalNavigation.tsx`

Build the persistent top navigation bar that appears on every screen.

Elements (left to right):
- Logo linking to `/` (Portfolio Dashboard)
- Universal search bar — UI shell only in Phase 1 (input renders, submitting navigates to `/search` but returns empty results). Full functionality in Phase 2.
- Notification bell — renders with badge, opens empty panel with "No critical alerts" state. Full functionality in Phase 1.
- User profile avatar — opens dropdown: profile settings link, sign out button.

Height: 56px fixed.  
Keyboard shortcut: `Ctrl+K` / `Cmd+K` focuses the search input.  
Keyboard shortcut: `Ctrl+R` / `Cmd+R` opens the repository switcher.

See: `02-components/ALL_COMPONENTS.md#GlobalNavigation`

---

### 2. Repository Switcher Dropdown

**File:** `src/components/layout/RepositorySwitcherDropdown.tsx`

Triggered from the context bar. Required before L2 can function.

Behaviour:
- Opens on click of repository name in context bar
- Search input auto-focused on open
- Groups: Favourites → Recent → "All repositories" link
- `RepositoryHealthChip` shown per entry
- Closes on: selection / Escape / outside click
- Max visible height 400px, scrollable

See: `02-components/ALL_COMPONENTS.md#RepositorySwitcherDropdown`

API required:
```
GET /api/repositories?userId={id}
Response: { favourites: Repo[], recent: Repo[], all: Repo[] }
```

---

### 3. Context Bar

**File:** `src/components/layout/ContextBar.tsx`

Persistent below global nav on all screens at L2 and below.

Elements: breadcrumb / repo name trigger / health chip / sync status / refresh button.

Sync states: "Synced Xm ago" | "Syncing..." (spinner) | "Sync failed" (warning icon + text + retry button)

See: `02-components/ALL_COMPONENTS.md#ContextBar`

---

### 4. Portfolio Dashboard (L1)

**Route:** `/`  
**File:** `src/pages/PortfolioDashboard.tsx`

Three zones. Build in this order:

**Zone 1 — Summary Strip**
Four `MetricCard` components in a row. Data from:
```
GET /api/portfolio/summary
Response: {
  repositoryCount: number,
  averageHealthScore: number,
  averageHealthTrend: 'up' | 'down' | 'flat',
  criticalIssueCount: number,
  teamCount: number
}
```

**Zone 2 — Repository List**
Sortable, filterable table. Default sort: health score ascending.

```
GET /api/repositories
Response: {
  repositories: Array<{
    id: string,
    name: string,
    language: string,
    healthScore: number,
    healthTrend: 'up' | 'down' | 'flat',
    criticalIssues: number,
    highIssues: number,
    mediumIssues: number,
    lastSyncAt: string,    // ISO timestamp
    isFavourite: boolean
  }>
}
```

Favourites float to top regardless of sort. Star/unstar via `PATCH /api/repositories/:id/favourite`.

**Zone 3 — Critical Issues Panel**
Conditional: only renders when `criticalIssueCount > 0`.

```
GET /api/portfolio/critical-issues?limit=5
Response: {
  total: number,
  issues: Array<{
    repositoryId: string,
    repositoryName: string,
    description: string,
    severity: 'critical'
  }>
}
```

Full spec: `01-screens/L1_PORTFOLIO_DASHBOARD.md`

---

### 5. Repository Dashboard (L2)

**Route:** `/repos/:repoId`  
**File:** `src/pages/RepositoryDashboard.tsx`

Four zones. Build in this order: Zone 1 → Zone 2 → Zone 4 → Zone 3.

**Zone 1 — Summary Strip**
```
GET /api/repositories/:repoId/summary
Response: {
  healthScore: number,
  healthTrend: 'up' | 'down' | 'flat',
  activeContributors: number,
  criticalIssues: number,
  technicalDebtHours: number
}
```

**Zone 2 — Quality Hotspots**
Ranked by: `complexity × churn × (1 - quality)`. Backend handles ranking.
```
GET /api/repositories/:repoId/hotspots?limit=5
Response: {
  total: number,
  hotspots: Array<{
    fileId: string,
    filePath: string,
    severity: 'critical' | 'high' | 'medium' | 'low',
    issueType: 'complexity' | 'security' | 'debt' | 'coverage',
    estimatedFixHours: number
  }>
}
```

**Zone 3 — Activity Feed**
Quality events only, not commits.
```
GET /api/repositories/:repoId/activity?limit=10
Response: {
  events: Array<{
    type: 'quality_gate_fail' | 'new_critical_issue' | 'security_flag' |
          'complexity_spike' | 'build_pass' | 'sync_complete',
    description: string,
    timestamp: string
  }>
}
```

**Zone 4 — Quick Actions**
Six static navigation buttons. No API calls. Pure navigation links.

Full spec: `01-screens/L2_REPOSITORY_DASHBOARD.md`

---

### 6. MetricCard Component

**File:** `src/components/MetricCard.tsx`

See full spec: `02-components/ALL_COMPONENTS.md#MetricCard`

Include: default state, loading skeleton state, hover state (if clickable).

---

### 7. RepositoryHealthChip Component

**File:** `src/components/RepositoryHealthChip.tsx`

See full spec: `02-components/ALL_COMPONENTS.md#RepositoryHealthChip`

---

### 8. QualityHotspotRow Component

**File:** `src/components/QualityHotspotRow.tsx`

See full spec: `02-components/ALL_COMPONENTS.md#QualityHotspotRow`

---

### 9. SeverityBadge Component

**File:** `src/components/SeverityBadge.tsx`

See full spec: `02-components/ALL_COMPONENTS.md#SeverityBadge`

---

### 10. Responsive Layout System

All Phase 1 screens must pass at all four breakpoints:

| Breakpoint | Min width | Max width |
|------------|-----------|-----------|
| Mobile | 320px | 767px |
| Tablet | 768px | 1023px |
| Desktop | 1024px | 1439px |
| Wide | 1440px | — |

See breakpoint behaviour: `04-design-system/DESIGN_SYSTEM.md#Responsive Breakpoints`

---

### 11. Onboarding Flow (Empty State)

**File:** `src/pages/Onboarding.tsx`

Shown when user has zero repositories connected.

Step 1 — Connect repository:
- Full-screen, single-purpose
- Six provider buttons: GitHub / GitLab / Bitbucket / Azure DevOps / Local Git / Demo repo
- No dashboard visible yet

Step 2 — Analysis in progress:
- Progress bar (live via SignalR)
- Estimated time remaining
- One sentence explanation: "We are analysing your code to calculate quality scores, identify hotspots, and map relationships."
- User can navigate away; analysis continues in background

Step 3 — Analysis complete:
- Redirect to Repository Dashboard (L2)
- Single contextual tooltip on Quality Hotspots panel: "Your most important files to look at are here."
- Tooltip dismissed by clicking anywhere

Full spec: `06-implementation/ONBOARDING_FLOW.md`

---

## API Endpoints Required in Phase 1

```
GET    /api/portfolio/summary
GET    /api/portfolio/critical-issues
GET    /api/repositories
PATCH  /api/repositories/:id/favourite
GET    /api/repositories/:id/summary
GET    /api/repositories/:id/hotspots
GET    /api/repositories/:id/activity
POST   /api/repositories/connect
GET    /api/repositories/:id/analysis-status   (SSE or SignalR)
```

---

## Phase 1 Acceptance Criteria

- [ ] Portfolio Dashboard loads in < 1500ms (target) / < 2500ms (max)
- [ ] Repository Dashboard loads in < 1000ms (target) / < 2000ms (max)
- [ ] Repository context switch completes in < 1 second for cached data
- [ ] Skeleton loaders shown during all data fetches (never blank screen)
- [ ] Zone 3 (critical issues) hidden when no critical issues exist
- [ ] Hotspots default to 5 items + "See all X" link
- [ ] Favourites float above sorted results in repo list
- [ ] All 4 breakpoints render correctly for all Phase 1 screens
- [ ] WCAG 2.1 AA passes: colour contrast, keyboard navigation, ARIA labels
- [ ] Error states handled: connectivity error, auth error, analysis error
- [ ] Empty states render correctly: no repos, no hotspots
- [ ] Onboarding flow: connect → progress → dashboard completes end-to-end
- [ ] SignalR connection for analysis progress works and updates in real time
