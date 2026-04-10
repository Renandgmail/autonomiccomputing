# Screen: L3 Universal Search

**Route:** `/repos/:repoId/search` (repo-scoped) or `/search` (global)  
**Level:** L3  
**Access:** From global nav search bar (keyboard: `Ctrl+K` / `Cmd+K`). Available from every screen.  
**Primary question answered:** Find anything in the codebase using plain English.

---

## Access Patterns

Two modes — determined by scope toggle:

| Mode | Route | Default? | Behaviour |
|------|-------|----------|-----------|
| Repo-scoped | `/repos/:repoId/search` | Yes | Searches currently selected repository only |
| Global | `/search` | No | Searches across all connected repositories |

Clicking the global nav search bar defaults to repo-scoped if a repo is currently selected. Toggle switches to global mode.

**Keyboard shortcut:** `Ctrl+K` / `Cmd+K` opens search from any screen, auto-focuses the input.

---

## Layout

```
┌─────────────────────────────────────────────────────────┐
│  GLOBAL NAVIGATION                                      │
├─────────────────────────────────────────────────────────┤
│  CONTEXT BAR (if repo-scoped)                           │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │ 🔍 authentication methods in React components   │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  Scope: ● This repository  ○ All repositories           │
│  Filters: [Files ×] [Last 30 days ×]  [+ Add filter]  │
│                                                         │
│  Tabs: [Files (23)] [Contributors (5)] [Metrics (12)]  │
│                                                         │
│  ─── Files ──────────────────────────────────────────  │
│  ┌─────────────────────────────────────────────────┐   │
│  │ frontend-app/src/auth/AuthService.js             │   │
│  │ authenticateUser(), validateToken()              │   │
│  │ Quality: 92%   Last changed: 2 days ago          │   │
│  │ No issues                                        │   │
│  ├─────────────────────────────────────────────────┤   │
│  │ backend-api/Auth/AuthController.cs               │   │
│  │ Login(), RefreshToken(), ValidateUser()          │   │
│  │ Quality: 67%   Last changed: 1 week ago          │   │
│  │ 🔴 Critical: SQL injection  🟡 Medium: weak auth │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Search Input

- Full width, prominent
- Placeholder: "Search files, functions, patterns, or ask a question..."
- Accepts natural language queries
- Results appear as user types (debounced, 300ms)
- Search history: last 10 queries, shown in dropdown when input is focused and empty

---

## Scope Selector

- Radio toggle: "This repository" (default) | "All repositories"
- When "All repositories" selected: show skeleton loading state (cross-repo search is slower)
- Global search shows repository name as prefix on each result

---

## Query Types Supported

| Type | Example query | Result type |
|------|--------------|-------------|
| File discovery | "Show me authentication-related files" | Files tab |
| Quality queries | "Find files with complexity over 10" | Files tab (filtered) |
| Security queries | "Show me SQL injection vulnerabilities" | Files tab + Metrics tab |
| Change queries | "What changed in the payment module this week?" | Files tab (sorted by date) |
| Function search | "Where is the payment processing logic?" | Files tab |
| Cross-repo (global only) | "Compare authentication quality across all repos" | Metrics tab |

---

## Results Tabs

### Tab 1: Files (default)

Each result shows:
- File path (IBM Plex Mono, full path from repo root)
- Repository name prefix (global search only)
- Matched function/class names if applicable
- Quality score percentage
- Last modification date (relative)
- Issue chips: severity badge per active issue (max 3 visible, "+N more" if more)

Results ordered by: relevance score (default). Can be re-sorted by quality score, date modified.

Clicking a result → File Detail (L4) or relevant Analytics tab.

### Tab 2: Contributors

Each result shows:
- Contributor name + avatar
- Repository (global search only)
- Role label (Author, Reviewer, etc.)
- Relevance: "85% of commits in auth module"

Maximum 10 contributors. No individual metric drill-down from search results — navigates to Team Analytics.

### Tab 3: Metrics

Aggregated metric results:
- "Authentication files: Average complexity 7.2"
- "Security hotspots: 2 critical, 5 medium"
- Links to relevant Analytics tabs for full detail

---

## Filters

Available as horizontal chip row. Active filters are dismissable chips.

| Filter | Options |
|--------|---------|
| Result type | Files, Contributors, Metrics |
| File type | .cs, .js, .ts, .py, etc. (detected from repo) |
| Severity | Critical, High, Medium, Low |
| Date modified | Last 7d, 30d, 90d, custom |
| Language | Auto-populated from connected repos |
| Has issues | Boolean toggle |

---

## Empty State

```
No results for "[query]"
Try different keywords or broaden your scope.
[Search all repositories →]  [Clear filters →]
```

---

## Loading State

- Input: shows spinner inside input right side
- Results area: skeleton rows matching tab structure
- Do NOT show "no results" until loading is complete

---

## Responsive Behaviour

| Breakpoint | Changes |
|------------|---------|
| Mobile | Filters collapse to "Filters" button opening a bottom sheet. Tabs become scrollable. |
| Tablet | Full layout, narrower result cards. |
| Desktop | Full layout as above. |

---

## Acceptance Criteria

- [ ] Search accessible via `Ctrl+K` / `Cmd+K` from any screen
- [ ] Scope toggle defaults to "This repository" when a repo is selected
- [ ] Results appear within 400ms (target) / 800ms (max)
- [ ] Files tab is the default active tab
- [ ] Each file result shows path, quality score, date, and issue chips
- [ ] Filters apply without full page reload
- [ ] Empty state provides actionable next steps
- [ ] Global search shows repository name prefix on each result
- [ ] Search history shown when input focused and empty (last 10 queries)
- [ ] WCAG 2.1 AA passes (keyboard navigable results, screen reader labels)
