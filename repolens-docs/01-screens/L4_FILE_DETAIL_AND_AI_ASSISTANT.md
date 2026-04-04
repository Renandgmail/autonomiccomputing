# Screen: L3 Team Analytics

**Route:** `/repos/:repoId/team`  
**Level:** L3  
**Primary users:** Engineering Manager, Team Lead  
**Privacy rule:** Individual data requires a second explicit click. Team aggregate is always the default.

---

## Team-Level View (Default)

Four aggregate panels:

### 1. Contribution volume over time
- Bar chart, grouped by week
- Y-axis: commit count
- No individual breakdown visible in this view
- Time range picker: 7d / 30d / 90d

### 2. Code review turnaround distribution
- Histogram: how many reviews resolved in < 1d / 1–3d / 3–7d / 7+ days
- Helps identify review bottlenecks without surfacing individuals

### 3. Module ownership distribution
- Horizontal bar per module showing ownership concentration
- "Concentrated" = < 2 contributors authored > 80% of module
- "Distributed" = wider spread
- High concentration = bus factor risk
- Clicking a module row → shows which module (not who), links to Code Graph filtered to that module

### 4. Team velocity trend
- Line chart: total effective lines changed per week
- Aggregate only
- Threshold line at team's configured velocity target (if set)

### CTA
"View by contributor →" button at bottom of page. This navigates to the individual view within the same route (query param `?view=individual`).

---

## Individual View

**Access:** Only via "View by contributor →" button. No deep links. No direct URL.

**URL:** `/repos/:repoId/team?view=individual`

### What is shown per contributor
- Commit volume trend (last 90 days) — bar chart
- Quality trend on files they authored — line chart
- Review activity count (reviews given + received) — numbers only
- Module focus areas — tags showing which parts of codebase they work in

### What is explicitly NOT shown
- Rankings or positions (no "top contributor" labels)
- Comparisons between individuals (no side-by-side individual charts)
- Performance scores or ratings
- Comparisons to team averages presented as evaluation

### Language rules
Use these terms: "areas of focus", "recent activity", "quality trends", "contribution pattern"  
Do NOT use: "performance", "productivity score", "rating", "rank", "output"

---

## Acceptance Criteria

- [ ] Team aggregate view is the default — individual view never shown on first load
- [ ] "View by contributor" requires an explicit click
- [ ] Individual view has no rankings, no comparisons, no performance labels
- [ ] Module ownership panel identifies bus factor risks
- [ ] All charts respect the time range picker
- [ ] WCAG 2.1 AA passes

---
---

# Screen: L4 File Detail

**Route:** `/repos/:repoId/files/:fileId`  
**Level:** L4  
**Primary users:** Senior Developer, QA Engineer  
**Primary question answered:** What exactly is wrong with this specific file, and what should I do about it?

---

## Layout

```
┌─────────────────────────────────────────────────────────┐
│  GLOBAL NAVIGATION                                      │
├─────────────────────────────────────────────────────────┤
│  CONTEXT BAR + extended breadcrumb:                     │
│  Portfolio > frontend-app > src/auth/AuthService.js     │
├──────────────────────────┬──────────────────────────────┤
│  METRICS PANEL           │  ISSUES LIST                 │
│                          │                              │
│  Quality: 92%   ↑        │  🔴 SQL injection · Line 47  │
│  Complexity: 7.2/10      │  Use parameterized queries   │
│  Tech debt: 2.4h         │                              │
│  Coverage: 88%           │  🟡 Weak password · Line 89  │
│  Security issues: 2      │  Add regex validation        │
│                          │                              │
│  Last changed: 2d ago    │  🟢 Missing logs · Line 123  │
│  Changed 8× this month   │  Add try-catch logging       │
│                          │                              │
│  Dependencies            │                              │
│  → UserService.cs (3)    │                              │
│  ← PaymentAPI.cs (1)     │                              │
│                          │                              │
└──────────────────────────┴──────────────────────────────┘
```

---

## Metrics Panel

| Metric | Display |
|--------|---------|
| Quality score | Large % with trend arrow |
| Complexity | Score / 10 with colour band |
| Technical debt | Hours estimate |
| Test coverage | % |
| Security issues | Count with severity breakdown |
| Last changed | Relative date |
| Change frequency | "Changed X times in last 30 days" |
| Dependencies out | Count + list (linked to target files) |
| Dependencies in | Count + list (linked to source files) |

---

## Issues List

Each issue row:
- Severity badge
- Issue type + description (one sentence)
- Line number reference
- Recommended action (one sentence)
- Status: Open / Acknowledged / Resolved (updateable inline)

---

## Acceptance Criteria

- [ ] Breadcrumb shows full path including file name
- [ ] All metrics visible without scrolling on desktop
- [ ] Each issue is actionable (line number + recommendation)
- [ ] Issue status updatable inline
- [ ] Dependency links navigate to the linked file's L4 view
- [ ] WCAG 2.1 AA passes

---
---

# Screen: AI Assistant Overlay

**Access:** Persistent floating button, bottom-right corner, every screen.  
**Type:** Overlay panel (not a page, not a permanent panel).  
**Width:** 380px on desktop. Full-width bottom sheet on mobile.

---

## Access Behaviour

1. User clicks the floating AI button
2. Panel slides in from the right (desktop) or slides up from bottom (mobile)
3. Primary content remains visible and interactive behind/beside the panel
4. Panel closes on: Escape key / clicking × / clicking outside panel area

**First session only:** A single pulsing indicator on the button to signal it exists. Disappears permanently after first open.

---

## Context Awareness

The assistant knows the current screen context and uses it automatically:

| Current screen | Context passed to assistant |
|---------------|---------------------------|
| Repository Dashboard (L2) | Current repository name + health score + top 3 hotspots |
| Analytics > Trends | Current repo + selected time range + current quality trend |
| Analytics > Files | Current repo + currently visible files list |
| File Detail (L4) | Current file path + all current metrics + issues list |
| Code Graph | Current repo + selected node (if any) |
| Portfolio Dashboard (L1) | All repositories + health scores |

---

## Appropriate Use Cases

The assistant is designed for these tasks:

- Explaining what a quality metric means ("what does a complexity score of 8.2 mean?")
- Summarising a set of issues into plain language for a non-technical stakeholder
- Suggesting what to look at next based on current screen context
- Answering "what should I prioritise this week?" based on visible hotspots
- Generating a summary paragraph suitable for a status report

---

## Out-of-Scope Use Cases

The assistant explicitly does NOT:
- Generate code fixes or patches
- Create pull requests or branches
- Send notifications or messages to team members
- Make promotion or performance recommendations
- Access individual developer data unless the user has explicitly navigated to that view
- Proactively interrupt, push notifications, or pop up suggestions

**Proactive suggestions constraint:** The assistant never shows content unless the user opens it. No push notifications from the assistant. No pop-ups. The only exception: a one-time, dismissable onboarding tooltip the first time a user opens a screen (not from the assistant itself — from the onboarding system).

---

## Conversation Design

- Context is automatically set from current screen (user does not need to explain where they are)
- Conversation history persists within a session only (not across sessions)
- Responses use plain language — no jargon unless the user uses it first
- Maximum response length: ~150 words before offering "tell me more" expansion
- Tone: direct, professional, not enthusiastic or congratulatory

---

## Acceptance Criteria

- [ ] Floating button visible on every screen (all L1–L4)
- [ ] Panel opens from right on desktop, bottom on mobile
- [ ] Primary content remains visible and interactive when panel is open
- [ ] Panel closes on Escape or outside click
- [ ] Context is correct for each screen (verified against context mapping table above)
- [ ] Assistant does not proactively push content, notifications, or suggestions
- [ ] Pulsing indicator shows on first session only
- [ ] No code generation, PR creation, or individual HR data in responses
- [ ] WCAG 2.1 AA: panel is keyboard navigable, focus trapped inside when open, Escape closes it
