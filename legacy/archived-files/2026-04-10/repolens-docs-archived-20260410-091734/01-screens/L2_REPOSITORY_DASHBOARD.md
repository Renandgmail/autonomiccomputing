# Screen: L2 Repository Dashboard

**Route:** `/repos/:repoId`  
**Level:** L2  
**Primary user:** Engineering Manager, Team Lead  
**Primary question answered:** What needs attention in this repository today?  
**Time target:** Enable a decision within 60 seconds of landing.

---

## Layout — Four Zones

```
┌─────────────────────────────────────────────────────────┐
│  GLOBAL NAVIGATION                                      │
├─────────────────────────────────────────────────────────┤
│  CONTEXT BAR                                            │
│  Portfolio > frontend-app   [94% ↑] [Switch ▼]  2m ago │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ZONE 1: Summary Strip                                  │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │ 94%      │ │ 5        │ │ 2        │ │ 12.4h    │   │
│  │ Health   │ │ Active   │ │ Critical │ │ Tech     │   │
│  │ Score    │ │ Contribs │ │ Issues   │ │ Debt     │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘   │
│                                                         │
│  ┌── ZONE 2: Quality Hotspots ──┐ ┌── ZONE 3: Activity─┐│
│  │ 🔴 PaymentService.cs         │ │ Build #234 passed  ││
│  │    Complexity · Critical     │ │ 5 minutes ago      ││
│  │    ~4h to fix                │ │                    ││
│  │                              │ │ Sarah updated      ││
│  │ 🟡 AuthValidator.js          │ │ auth.js · 12m ago  ││
│  │    Security · High · ~2h     │ │                    ││
│  │                              │ │ Quality gate warn  ││
│  │ 🟡 UserService.cs            │ │ user-svc.js · 1h   ││
│  │    Debt · High · ~3h         │ │                    ││
│  │                              │ │ Auto-sync: 23 new  ││
│  │ See all 18 hotspots →        │ │ commits · 2h ago   ││
│  └──────────────────────────────┘ └────────────────────┘│
│                                                         │
│  ZONE 4: Quick Actions                                  │
│  [Search] [Analytics] [Code Graph] [Team] [Security]   │
│  [Export]                                              │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Context Bar

Persistent on all screens at L2 and below. Specification: `02-components/CONTEXT_BAR.md`.

Contains:
- Breadcrumb: `Portfolio > [repository name]`
- Repository name (clickable — opens switcher dropdown)
- Health chip: current score + trend arrow
- Last sync time (relative, e.g. "2 minutes ago")
- Manual refresh button (icon only, with tooltip "Refresh now")

---

## Zone 1: Summary Strip

**Exactly four cards. No more. No charts.**

| Card | Metric | Link target |
|------|--------|------------|
| Health score | Current % + trend vs last 7 days | Analytics > Trends tab |
| Active contributors | Count of devs with commits in last 30 days | Analytics > Team tab |
| Open critical issues | Count of Critical severity issues | Analytics > Security tab |
| Technical debt | Total hours estimate across all files | Analytics > Files tab |

Cards are `MetricCard` components. Each is clickable and navigates to the relevant analytics view.

---

## Zone 2: Quality Hotspots (Primary Panel)

**This is the most important panel on the screen. It is always in the primary position.**

### Ranking logic
Files are ranked by composite hotspot score:
```
hotspot_score = complexity_score × churn_rate × (1 - quality_score)
```
Higher score = shown first.

### Each row shows
- File path (IBM Plex Mono, 13px, full path from repo root)
- Severity badge (Critical / High / Medium / Low)
- Issue type chip (Complexity / Security / Debt / Coverage)
- Estimated fix time (e.g. "~2h")
- Clicking the row → File Detail (L4)

### Truncation
Show top 5 by default. Always show "See all X hotspots →" link below. X = total count.

### Empty state
```
✓ No quality hotspots detected in the last 30 days.
  Your codebase is healthy.
  [View Full Analytics →]
```

---

## Zone 3: Recent Activity Feed (Secondary Panel)

**This is NOT a commit log.** Shows only meaningful quality events.

### Events that appear
- Quality gate failures
- New critical or high severity issues detected
- Security vulnerability flags
- Complexity spikes (file crosses threshold)
- Successful builds (build number + pass)
- Sync completions (commit count)

### Events that DO NOT appear
- Individual commit messages
- Branch creations / deletions
- PR opens / closes / merges
- Developer logins

### Display
- Maximum 10 items
- Each item: icon + description + relative timestamp
- No "See all" link — the activity feed does not have a deeper view

---

## Zone 4: Quick Actions (Tertiary Panel)

Six navigation shortcut buttons. These navigate — they do not perform actions with side effects.

| Button | Navigates to |
|--------|-------------|
| Search | Universal Search scoped to this repo |
| Analytics | Analytics > Trends tab |
| Code Graph | Code Graph (L3) |
| Team | Team Analytics (L3) |
| Security | Analytics > Security tab |
| Export Report | Triggers export modal (PDF/CSV choice) |

---

## What Is Deliberately Absent

Do not add these to this screen:

- **AI assistant panel** — accessible via floating button only, never shown by default
- **Individual developer names** in Zone 2 or Zone 4 — team-level metrics only
- **Sprint velocity / burndown widgets** — RepoLens is not a project management tool
- **Commit history** — not a git log viewer
- **Charts** in Zone 1 — charts live at L3, not here

---

## Responsive Behaviour

| Breakpoint | Changes |
|------------|---------|
| Mobile (< 768px) | Zones stack vertically (1 → 2 → 4 → 3). Zone 3 collapses to 5 items. Quick Actions become a horizontal scroll row. |
| Tablet (768–1023px) | Zones 2 and 3 side by side. Zone 1 is 2×2 grid. |
| Desktop (1024px+) | Full layout as above. |

---

## States

### Default
Full layout as described.

### Loading (context switch or initial load)
- Context bar: skeleton (repo name placeholder)
- Zone 1: 4 skeleton cards
- Zone 2: 5 skeleton rows
- Zone 3: 5 skeleton rows

### Sync in progress
- Context bar shows "Syncing..." with spinner
- Zone 2 and 3 show last-known data with "Results may not reflect latest changes" banner

### Analysis running (first-time or re-analysis)
- Zone 2: "Analysis in progress — hotspots will appear here shortly" with progress indicator
- Other zones: show whatever data is available

---

## Acceptance Criteria

- [ ] Quality Hotspots panel is always in the primary (left/main) position
- [ ] Hotspots are ranked by composite score (complexity × churn × quality deficit)
- [ ] Maximum 5 hotspots shown by default with "See all X" link
- [ ] Each hotspot row is clickable and navigates to L4 file detail
- [ ] Activity feed shows max 10 items, no commit messages
- [ ] All 4 summary cards are clickable and navigate to correct analytics view
- [ ] Context bar is visible and shows correct repo + health score
- [ ] No individual developer names on main dashboard panels
- [ ] No AI assistant panel visible by default
- [ ] All breakpoints render correctly
- [ ] WCAG 2.1 AA passes
