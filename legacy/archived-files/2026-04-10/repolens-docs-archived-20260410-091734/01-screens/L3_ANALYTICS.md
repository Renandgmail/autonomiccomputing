# Screen: L3 Analytics

**Route:** `/repos/:repoId/analytics`  
**Level:** L3  
**Primary users:** Engineering Manager, Team Lead, QA  
**Primary question answered:** What is the state of this repository across quality, team, and security dimensions?

---

## Tab Structure

Five tabs. Always visible. Switching between tabs does NOT trigger a full page reload (client-side navigation only).

| Tab | Route | Primary question | Default? |
|-----|-------|-----------------|----------|
| Trends | `/analytics` | Is our quality improving over time? | Yes |
| Files | `/analytics/files` | Which files need attention? | No |
| Team | `/analytics/team` | How is the team working together? | No |
| Security | `/analytics/security` | Where are our security risks? | No |
| Compare | `/analytics/compare` | How does this repo compare to others? | No |

---

## Trends Tab

### Purpose
Show quality trajectory over time. Primary use case: sprint retrospectives and weekly check-ins.

### Time range picker
Options: Last 7 days / Last 30 days / Last 90 days / Custom date range.  
Default: Last 30 days.  
Persists per user session.

### Charts — one per metric

Each chart is a separate card with:
- Chart title
- Current value (large, prominent)
- Change vs previous period (e.g. "+3% vs last 30 days")
- Line chart showing trend over selected period
- Threshold line (where a target exists)

**Charts displayed:**
1. **Overall quality score** — line chart. Threshold line at configured target (default 85%).
2. **Technical debt (hours)** — line chart. No threshold line.
3. **Test coverage %** — line chart. Threshold line at configured target (default 80%).
4. **Security vulnerability count** — line chart by severity (Critical/High/Medium as stacked area).

### Chart design rules
- One chart type per metric. No combined line + bar.
- Always show threshold line where one exists.
- Tooltips on hover only — no persistent data labels on lines.
- Colour = meaning: green = meeting target, amber = approaching threshold, red = below threshold.
- No decorative colours.

---

## Files Tab

### Purpose
Find and sort every file in the repository by quality metrics. Primary use case: identifying which files to prioritise in a sprint.

### Layout
Filterable, sortable table. Same component as Quality Hotspots but showing all files.

### Columns

| Column | Content | Sortable | Default |
|--------|---------|----------|---------|
| File | Path (IBM Plex Mono) + repository-root-relative | Yes (A–Z) | — |
| Complexity | Score (0–10 scale) | Yes | — |
| Debt | Hours estimate (e.g. "4.2h") | Yes | Descending |
| Coverage % | Test coverage (%) | Yes | — |
| Security | Count of open issues (chips by severity) | Yes | — |
| Last changed | Relative date | Yes | — |

**Default sort:** Debt descending (highest debt first).

### Filters (chip row above table)
- File type (.cs, .js, .ts, etc.)
- Complexity threshold (above X)
- Coverage below threshold (below X%)
- Has security issues (boolean)
- Changed in last N days

### Each row is clickable → File Detail (L4)

### Pagination
50 rows per page. Page controls at bottom. Total count always shown: "Showing 50 of 234 files".

---

## Team Tab

### Privacy rule — enforced in UI
**Team-level data is the default.** Individual data requires a second explicit click.

### Team-level view (default)

Metrics shown:
- **Contribution volume over time** — bar chart, grouped by week, showing total commits. No individual breakdown visible.
- **Code review turnaround** — distribution histogram (how many reviews resolved in < 1 day, 1–3 days, 3–7 days, 7+ days).
- **Module ownership distribution** — horizontal bar showing what % of each module was authored by "concentrated" vs "distributed" ownership. Identifies knowledge silos.
- **Team velocity trend** — lines of effective code change per sprint/week (aggregate).

CTA button: "View by contributor →" (navigates to individual view within same tab).

### Individual view (second click from team view)

**Accessed by clicking "View by contributor".**

NOT linked from any other screen. No deep link. Requires intentional navigation.

Shows for each contributor:
- Commit volume trend (last 90 days)
- Quality trend on files they authored
- Review activity count
- Module focus areas (what parts of the codebase they work in)

**UI language rules:**
- No rankings or league tables
- No comparisons between individuals
- No performance score labels
- Labels use: "areas of focus", "recent activity", "quality trends" — NOT "performance", "productivity score", "rating"

---

## Security Tab

### Purpose
Identify and track security vulnerabilities. Primary use case: security reviews and compliance checks.

### Layout
Severity-ranked list with filter controls.

### Filters
- Severity: Critical / High / Medium / Low
- Type: SQL Injection / XSS / Auth / Encryption / Config / Other
- Status: Open / Acknowledged / Resolved

### Each issue row shows
- File path (linked to L4)
- Issue type
- Severity badge
- Brief description (one sentence)
- Date detected (relative)
- Status chip (Open / Acknowledged / Resolved)
- "Acknowledge" and "Mark Resolved" action buttons (inline)

### Counts summary strip
At top of tab: `2 Critical · 5 High · 12 Medium · 3 Low` — these are clickable chips that apply a filter.

---

## Compare Tab

### Purpose
Side-by-side comparison of up to 3 repositories. Primary use case: CTO/executive reviews and portfolio decisions.

### Layout
- Repository selector: add up to 2 additional repositories to compare against current
- Comparison table: current repo + selected repos as columns, metrics as rows

### Metrics compared

| Metric | Unit |
|--------|------|
| Overall health score | % |
| Average complexity | 0–10 scale |
| Technical debt | Hours |
| Test coverage | % |
| Critical security issues | Count |
| Active contributors | Count |
| Lines of code | Count |

### Export
"Export comparison" button → generates PDF or CSV. See `06-implementation/PHASE_2_ANALYTICS_SEARCH.md` for export spec.

---

## Responsive Behaviour

| Breakpoint | Changes |
|------------|---------|
| Mobile | Tabs become scrollable horizontal list. Charts resize to full width, single column. Table switches to card view (one card per file). Compare tab shows 2 repos max. |
| Tablet | Two-column chart grid. Full table. |
| Desktop | Full layout as above. |

---

## Acceptance Criteria

- [ ] Tab switching does not trigger full page reload
- [ ] Trends tab is the default active tab
- [ ] All charts show threshold line where one is configured
- [ ] Tooltips appear on chart hover, no persistent labels
- [ ] Files tab defaults to sorting by debt descending
- [ ] Files tab table is paginated at 50 rows, shows total count
- [ ] Team tab shows aggregate data by default
- [ ] "View by contributor" requires a click — not the default
- [ ] Individual view has no rankings, no comparisons, no performance labels
- [ ] Security tab status can be updated inline (acknowledge / resolve)
- [ ] Compare tab supports up to 3 repositories
- [ ] Export generates PDF or CSV from Compare tab
- [ ] WCAG 2.1 AA passes on all five tabs
