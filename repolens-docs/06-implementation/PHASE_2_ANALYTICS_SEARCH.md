# Phase 2: Analytics & Search

**Timeline:** Weeks 5–8  
**Prerequisite:** Phase 1 fully complete and QA-passed.  
**Goal:** Deliver the analytical depth layer — the screens that engineering managers and team leads use for weekly planning.

---

## Deliverables

### 1. Universal Search

**Route:** `/repos/:repoId/search` (scoped) and `/search` (global)  
**File:** `src/pages/UniversalSearch.tsx`

#### Search input
- Full-width, auto-focused on route load
- Debounced input: fires API call 300ms after last keystroke
- Keyboard shortcut `Ctrl+K` / `Cmd+K` navigates to search from any screen
- Search history: store last 10 queries in `localStorage`. Show in dropdown when input focused and empty.

#### Scope toggle
```tsx
type SearchScope = 'repository' | 'global';
// Default: 'repository' when repoId is in URL
// Default: 'global' when at /search
```

#### API
```
GET /api/search?q={query}&scope={repository|global}&repoId={id}&type={files|contributors|metrics}&page={n}

Response: {
  query: string,
  scope: 'repository' | 'global',
  results: {
    files: {
      total: number,
      items: Array<{
        fileId: string,
        filePath: string,
        repositoryName: string,   // only in global scope
        repositoryId: string,
        matchedSymbols: string[], // function/class names matched
        qualityScore: number,
        lastChangedAt: string,
        issues: Array<{ severity: string, type: string }>
      }>
    },
    contributors: {
      total: number,
      items: Array<{
        userId: string,
        displayName: string,
        relevanceDescription: string,  // e.g. "85% of commits in auth module"
        repositoryId: string
      }>
    },
    metrics: {
      total: number,
      items: Array<{
        label: string,       // e.g. "Authentication files: Average complexity"
        value: string,       // e.g. "7.2"
        linkTo: string       // route to full detail
      }>
    }
  }
}
```

#### Results tabs
Three tabs: Files (default) / Contributors / Metrics.  
Tab counts shown in brackets: `Files (23)`.  
Switching tabs does not re-query — all three result sets fetched in one call.

#### Filter chips
Filters applied as query params. Re-queries on filter change.

Available filters:
```
severity: 'critical' | 'high' | 'medium' | 'low'
fileType: string          // e.g. '.cs', '.js'
hasIssues: boolean
dateModified: '7d' | '30d' | '90d' | 'custom'
language: string
```

Full spec: `01-screens/L3_UNIVERSAL_SEARCH.md`

---

### 2. Analytics Screen — Trends Tab

**Route:** `/repos/:repoId/analytics`  
**File:** `src/pages/Analytics.tsx` with tab sub-components

#### Time range picker
```tsx
type TimeRange = '7d' | '30d' | '90d' | 'custom';
// Persisted in URL: ?range=30d
// Default: '30d'
```

#### API
```
GET /api/repositories/:repoId/analytics/trends?range={7d|30d|90d}&from={date}&to={date}

Response: {
  qualityScore: {
    current: number,
    previousPeriod: number,
    delta: number,
    target: number,         // configured threshold
    dataPoints: Array<{ date: string, value: number }>
  },
  technicalDebt: {
    current: number,        // hours
    previousPeriod: number,
    delta: number,
    dataPoints: Array<{ date: string, value: number }>
  },
  testCoverage: {
    current: number,
    previousPeriod: number,
    delta: number,
    target: number,
    dataPoints: Array<{ date: string, value: number }>
  },
  securityVulnerabilities: {
    current: { critical: number, high: number, medium: number, low: number },
    dataPoints: Array<{ date: string, critical: number, high: number, medium: number, low: number }>
  }
}
```

#### Charts
Use Recharts. One `LineChart` per metric. Rules:
- Threshold line: `ReferenceLine` in `--interactive` colour, dashed
- Tooltip: custom tooltip component, shown on hover only
- Colours: green if meeting target, amber if approaching, red if below
- No persistent data labels on lines

---

### 3. Analytics Screen — Files Tab

**Route:** `/repos/:repoId/analytics/files`

#### API
```
GET /api/repositories/:repoId/analytics/files
    ?sort={field}&dir={asc|desc}
    &severity={critical|high|medium|low}
    &fileType={.cs|.js|...}
    &complexityMin={number}
    &coverageMax={number}
    &hasSecurityIssues={boolean}
    &changedWithin={7d|30d|90d}
    &page={n}&pageSize=50

Response: {
  total: number,
  page: number,
  pageSize: number,
  files: Array<{
    fileId: string,
    filePath: string,
    complexityScore: number,
    debtHours: number,
    coveragePercent: number,
    securityIssues: Array<{ severity: string, count: number }>,
    lastChangedAt: string
  }>
}
```

Default sort: `debtHours` descending.  
Pagination: 50 rows per page. Show "Showing 50 of 234 files".  
Each row clicks → File Detail (L4).

---

### 4. Analytics Screen — Security Tab

**Route:** `/repos/:repoId/analytics/security`

#### API
```
GET /api/repositories/:repoId/analytics/security
    ?severity={critical|high|medium|low}
    &type={sql_injection|xss|auth|encryption|config|other}
    &status={open|acknowledged|resolved}

Response: {
  summary: { critical: number, high: number, medium: number, low: number },
  issues: Array<{
    issueId: string,
    fileId: string,
    filePath: string,
    type: string,
    severity: string,
    description: string,
    lineNumber: number,
    detectedAt: string,
    status: 'open' | 'acknowledged' | 'resolved'
  }>
}
```

#### Inline status update
```
PATCH /api/repositories/:repoId/security-issues/:issueId
Body: { status: 'acknowledged' | 'resolved' }
```

Status update optimistic — update UI immediately, revert on error.

---

### 5. File Detail View (L4)

**Route:** `/repos/:repoId/files/:fileId`  
**File:** `src/pages/FileDetail.tsx`

#### API
```
GET /api/repositories/:repoId/files/:fileId

Response: {
  fileId: string,
  filePath: string,
  qualityScore: number,
  qualityTrend: 'up' | 'down' | 'flat',
  complexityScore: number,
  debtHours: number,
  coveragePercent: number,
  lastChangedAt: string,
  changeCountLast30Days: number,
  dependenciesOut: Array<{ fileId: string, filePath: string }>,
  dependenciesIn: Array<{ fileId: string, filePath: string }>,
  issues: Array<{
    issueId: string,
    severity: 'critical' | 'high' | 'medium' | 'low',
    type: string,
    description: string,
    lineNumber: number,
    recommendation: string,
    status: 'open' | 'acknowledged' | 'resolved'
  }>
}
```

Layout: two-column on desktop (metrics panel left, issues list right). Single column on mobile.  
Full spec: `01-screens/L4_FILE_DETAIL_AND_AI_ASSISTANT.md`

---

### 6. Export Functionality

**Files CSV export:**
```
GET /api/repositories/:repoId/analytics/files/export?format=csv&[same filters as files tab]
Response: Content-Type: text/csv
```

**Report PDF export:**
```
POST /api/repositories/:repoId/reports/export
Body: { format: 'pdf', sections: ['summary', 'hotspots', 'security', 'trends'] }
Response: { downloadUrl: string }  // pre-signed S3/blob URL, expires in 1h
```

Export triggers a modal asking format choice (CSV for tables, PDF for full report). Download starts immediately for CSV, shows progress for PDF generation.

---

## API Endpoints Required in Phase 2

```
GET  /api/search
GET  /api/repositories/:id/analytics/trends
GET  /api/repositories/:id/analytics/files
GET  /api/repositories/:id/analytics/security
GET  /api/repositories/:id/files/:fileId
PATCH /api/repositories/:id/security-issues/:issueId
GET  /api/repositories/:id/analytics/files/export
POST /api/repositories/:id/reports/export
```

---

## Phase 2 Acceptance Criteria

- [ ] Search results appear within 400ms (target) / 800ms (max)
- [ ] `Ctrl+K` / `Cmd+K` opens search from any screen
- [ ] Files tab is the default search results tab
- [ ] Search scope toggle works correctly (repo vs global)
- [ ] Analytics tab switching does not trigger full page reload
- [ ] Trends charts render within 600ms (target) / 1200ms (max)
- [ ] Threshold/target line appears on quality score and coverage charts
- [ ] Files tab defaults to debt descending, shows total count
- [ ] Security issues updatable inline (acknowledge / resolve)
- [ ] File detail shows all metrics, issues, and dependency links
- [ ] CSV export downloads immediately
- [ ] PDF export shows progress and delivers download link
- [ ] All breakpoints correct for all Phase 2 screens
- [ ] WCAG 2.1 AA passes for all Phase 2 screens
