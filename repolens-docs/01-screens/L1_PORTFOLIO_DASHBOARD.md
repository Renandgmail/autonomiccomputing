# Screen: L1 Portfolio Dashboard

**Route:** `/`  
**Level:** L1  
**Primary user:** Engineering Manager, CTO  
**Primary question answered:** How healthy is my entire codebase?  
**Time target:** Answer the question within 10 seconds of opening.

---

## Layout — Three Zones

```
┌─────────────────────────────────────────────────────────┐
│  GLOBAL NAVIGATION                                      │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ZONE 1: Summary Strip (4 metric cards)                 │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │
│  │ 12       │ │ 92%      │ │ 3        │ │ 8        │   │
│  │ Repos    │ │ Avg      │ │ Critical │ │ Teams    │   │
│  │          │ │ Health   │ │ Issues   │ │          │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘   │
│                                                         │
│  ZONE 2: Repository List                [+ Add Repo]   │
│  ┌─────────────────────────────────────────────────┐   │
│  │ ● frontend-app   React   94% ↑   2 issues  2m   │   │
│  │ ● backend-api    .NET    78% ↓   5 issues  5m   │   │
│  │ ● mobile-app     Flutter 91% →   0 issues  1m   │   │
│  │ ● legacy-system  Java    45% ↓  12 issues  8m   │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
│  ZONE 3: Critical Issues Panel (conditional)            │
│  ┌─────────────────────────────────────────────────┐   │
│  │ ⚠ legacy-system: 3 critical security vulns      │   │
│  │ ⚠ backend-api: Technical debt exceeds 40h       │   │
│  │ ⚠ frontend-app: Test coverage below 80%         │   │
│  └─────────────────────────────────────────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## Zone 1: Summary Strip

**Exactly four metric cards. No more. No charts.**

| Card | Value type | What it links to |
|------|-----------|-----------------|
| Repository count | Integer | Settings > Repositories |
| Average health score | Percentage + trend arrow | Filtered repo list (no nav) |
| Open critical issues | Integer | Filtered repo list showing critical only |
| Active teams | Integer | No link (display only) |

Cards use the `MetricCard` component. See `02-components/METRIC_CARD.md`.

**Why no charts here:** Charts require interpretation time. Summary numbers tell the manager what to look at. Charts live at L3. Do not add charts to this zone under any circumstances.

---

## Zone 2: Repository List

### Default Sort
Health score **ascending** (worst first). This is deliberate — managers should see problems, not successes.

### Favourites behaviour
- Starred repositories appear at the top of the list, **above** sorted results
- Favourite state persists per user, not per organisation
- Star/unstar via icon on hover only (not a persistent column)

### Columns

| Column | Content | Sortable | Default |
|--------|---------|----------|---------|
| Status indicator | Coloured dot (health band colour) | No | — |
| Repository name | Name, linked to L2 dashboard | Yes (A–Z) | — |
| Language | Primary language badge | No | — |
| Health score | `RepositoryHealthChip` component | Yes | Ascending (worst first) |
| Open issues | Critical / High / Medium chips | Yes | — |
| Last sync | Relative time e.g. "3 minutes ago" | Yes | — |
| Actions | ⋮ menu (edit, unstar, remove) | No | — |

### Health Score Colour Bands

| Band | Score | Colour | Hex | Meaning |
|------|-------|--------|-----|---------|
| Excellent | 90–100% | Green | `#16A34A` | No action required |
| Good | 70–89% | Teal | `#0D9488` | Monitor, low priority |
| Fair | 50–69% | Amber | `#D97706` | Plan improvement sprint |
| Poor | 30–49% | Orange | `#EA580C` | Immediate attention needed |
| Critical | 0–29% | Red | `#DC2626` | Escalate to leadership |

### Add Repository Button
- Prominent (primary CTA button) on empty state
- Moves to a **secondary** position (top-right of Zone 2 header) once repositories exist
- Reason: onboarding must not compete with operational use

### Filtering
Filter chips above the table (not a sidebar). Active filters shown as dismissable chips.

Available filters:
- Health band (Excellent / Good / Fair / Poor / Critical)
- Language
- Team
- Has critical issues (boolean)

---

## Zone 3: Critical Issues Panel

**Conditional — only shown when ≥ 1 repository has a critical-severity issue.**

- Maximum 5 items displayed before "See all X critical issues" link
- Each item: repository name + issue description (one sentence) + severity chip
- Clicking an item navigates to the relevant repository dashboard (L2)
- Panel disappears entirely when all critical issues are resolved

---

## Responsive Behaviour

| Breakpoint | Changes |
|------------|---------|
| Mobile (< 768px) | Cards stack vertically. Repo list collapses to name + health score + arrow only. Critical issues panel always visible at top. |
| Tablet (768–1023px) | 2-column card grid. Repo list shows 4 columns max. |
| Desktop (1024px+) | Full layout as described above. |

---

## States

### Default (repositories exist)
Layout as above.

### Empty (no repositories connected)
Replace Zones 2 and 3 with:
```
[Illustration]
Connect your first repository to start analysing your codebase.
[Connect Repository →]
```

### Loading (initial page load)
- Zone 1: 4 skeleton cards
- Zone 2: 5 skeleton rows
- Zone 3: hidden

### Error (API unreachable)
- Zone 1: show last-known values with "Data may be outdated" warning banner
- Zone 2: show last-known list with stale indicator on each row
- Zone 3: show last-known critical issues

---

## Acceptance Criteria

- [ ] Page answers "how healthy is my codebase?" within 10 seconds of load
- [ ] Repository list defaults to health score ascending (worst first)
- [ ] Favourites always appear above sorted results
- [ ] Zone 3 is hidden when no critical issues exist
- [ ] Zone 3 shows maximum 5 items + "See all" link
- [ ] Clicking any repository row navigates to its L2 dashboard
- [ ] "Add Repository" button is prominent on empty state, secondary when repos exist
- [ ] All four breakpoints render correctly
- [ ] No charts present on this screen
- [ ] WCAG 2.1 AA passes (colour contrast, keyboard nav, screen reader labels)
