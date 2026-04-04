# Interaction Patterns

All interaction patterns used consistently across every screen in RepoLens.

---

## 1. Repository Context Switching

Context switching is the most frequent action. It must be fast, obvious, and reversible.

### Steps
1. User clicks repository name in context bar (or presses `Ctrl+R` / `Cmd+R`)
2. Switcher dropdown opens — search input is **auto-focused**
3. User types to filter OR selects from Favourites/Recent
4. On selection:
   - Context bar updates immediately (optimistic UI)
   - All panels show skeleton loading state
   - New repository data loads
   - Breadcrumb updates
   - Page title updates
5. On completion: skeleton replaced with live data

### What does NOT carry over on switch
- Scroll position
- Selected tab
- Active filters
- User returns to the default state of the new repository's dashboard

### Performance constraint
**Must complete in < 1 second for cached data.** Never leave the user viewing data from the previous repository while new data loads. The skeleton loader covers the entire content area immediately on selection.

### Error during switch
If the new repository fails to load: show error state in content area, keep previous repository in context bar with an error indicator. Do not silently stay on old repo without telling the user.

---

## 2. Progressive Disclosure

The primary mechanism for managing complexity. Applies at every level.

### Rules
- Default to showing **summary** at the current level
- Always provide a **visible, labelled** path to more detail (never hide it)
- Never show file-level data before the user has selected a repository
- Never auto-expand panels or accordions — user controls expansion
- Lists are **truncated** at 5 or 10 items with a "See all X [items]" link
  - The count is always shown: "See all 47 files" not just "See all"

### Level summary
| Level | Shows by default | More detail available via |
|-------|-----------------|--------------------------|
| L1 | Portfolio health, repo count, issues count | Click repo row → L2 |
| L2 | Top 5 hotspots, 10 activity items, 4 summary cards | Click hotspot → L4, click card → L3 |
| L3 | First page of results / default tab | Pagination, tab switching, drill-down |
| L4 | All metrics + all issues for that file | No deeper level |

---

## 3. Filtering and Sorting

Consistent across all table and list views.

### Sort
- Triggered by clicking column headers
- Active sort column: arrow icon in header (↑ ascending, ↓ descending)
- Clicking active sort column reverses direction
- Default sorts are defined per screen (see individual screen specs)

### Filters
- Shown as **horizontal chip row** above the table
- Active filters: coloured chips with × dismiss button
- "Reset filters" link appears when any filter is active
- Filter state is preserved during the session

### URL reflection
Filter and sort state is reflected in URL query params:
```
/repos/123/analytics/files?sort=debt&dir=desc&severity=critical&type=security
```
This enables shareable filtered views.

### No sidebar filters
Filters are always chips above the content, never a sidebar. Sidebars add layout complexity and hide options.

---

## 4. Loading States

Three distinct loading states. Each used in a specific context.

### Skeleton loader
**When:** Content area loading for the first time OR after a context switch.

- Skeleton shapes match the shape of the content they precede
- MetricCard skeleton: rectangle blocks at label/value/trend positions
- List row skeleton: full-width rectangle at text height
- Chart skeleton: rectangle at chart dimensions
- Animated shimmer effect (left-to-right light sweep)
- Duration: shown until real data arrives, no timeout

### Spinner
**When:** In-place refresh triggered by user action (manual sync, applying a filter, tab switch).

- Appears within the panel being refreshed, not full-screen
- Never use a full-screen spinner
- Size: 24px, positioned at centre of refreshing panel or inside the triggering control

### Progress bar
**When:** Long-running background operations (initial analysis, full re-analysis).

- Shown in notification area
- Shows: operation name + percentage complete + estimated time remaining
- Does not block the UI — user can continue using the platform

### Rule: No empty + loading confusion
During analysis, all panels show **skeleton** (not empty state) until data arrives. This prevents the false impression that there are no issues.

---

## 5. Error States

Three error categories, each handled differently.

### Connectivity errors (sync failed, repo unreachable)
- Shown in context bar: warning icon + "Last sync: failed" text
- Does NOT remove existing data from the screen (show last-known data)
- Does NOT show a full-page error
- Retry button available in context bar

### Analysis errors (parsing failed for a specific file)
- Shown inline in the relevant list row: error chip "Analysis failed"
- Other files and data remain visible
- Tooltip on error chip: "This file could not be analysed. Check file encoding."

### Authentication errors (token expired, access revoked)
- Shown as a full-panel message replacing the content area
- Other repositories (if not affected) remain accessible
- Single CTA: "Re-authenticate with [Provider]"
- Does not log the user out entirely

---

## 6. Empty States

Three-part structure for all empty states:
1. **Illustration** — simple, relevant (icon or minimal graphic, not decorative)
2. **Explanation** — one sentence, plain language, no jargon
3. **Action** — single CTA button

### Standard empty states

**No repositories connected:**
```
[Icon: repo]
Connect your first repository to start analysing your codebase.
[Connect Repository →]
```

**No quality issues found:**
```
[Icon: checkmark]
No quality issues detected in the last 30 days. Your codebase is healthy.
[View Full Analytics →]
```

**Search returned no results:**
```
[Icon: search]
No results for "[query]". Try different keywords or broaden your scope.
[Search all repositories →]  or  [Clear filters →]
```

**Analysis in progress (first-time):**
```
[Skeleton loader — not empty state]
"Analysis in progress — results will appear here shortly"
(shown as subtitle beneath skeleton, not as empty state)
```

---

## 7. Notifications

Notifications are restricted to critical events only. This is not negotiable — notification spam destroys trust.

### Events that trigger a notification (complete list)
1. A repository health score drops below the configured threshold (default: 50%)
2. A new critical-severity security vulnerability is detected
3. A repository sync fails and remains unresolved for > 30 minutes
4. A quality gate is configured and fails on a new commit (if quality gates are enabled)

### Events that do NOT trigger notifications
Everything else, including:
- New medium or low severity issues
- Health score improvements
- Sync completions (success)
- Team member activity
- AI assistant suggestions
- Analysis completions
- Sprint or planning reminders

All non-critical events appear in the **activity feed** on the repository dashboard only, when the user is already on that screen.

### Notification panel (bell click)
- Shows last 20 critical notifications
- Each shows: icon / description / repository name / relative timestamp / "Mark read" action
- "Mark all read" button at top
- Empty state: "No critical alerts" with green checkmark

### Notification badge
Shows count of unread critical notifications only. No count = no badge.
