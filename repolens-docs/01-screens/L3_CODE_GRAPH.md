# Screen: L3 Code Graph

**Route:** `/repos/:repoId/graph`  
**Level:** L3  
**Primary users:** Team Lead, Senior Developer  
**Primary question answered:** How do the components in this repository connect?  
**Primary use case:** Architecture comprehension, NOT operational monitoring.

---

## Layout

```
┌─────────────────────────────────────────────────────────┐
│  GLOBAL NAVIGATION                                      │
├─────────────────────────────────────────────────────────┤
│  CONTEXT BAR                                            │
├─────────────────────────────────────────────────────────┤
│  Controls Bar:                                          │
│  Layout: [Hierarchical ▼]  Filter: [All files ▼]       │
│  🔍 Search nodes...        [⊙ Circular deps] [⊙ Orphans]│
│  [Zoom In] [Zoom Out] [Center] [Export PNG]             │
├──────────────────────────────────┬──────────────────────┤
│                                  │  NODE DETAIL PANEL   │
│         GRAPH CANVAS             │  (opens on click)    │
│                                  │                      │
│   [interactive D3 graph]         │  AuthService.js      │
│                                  │  Complexity: 7.2/10  │
│                                  │  Quality: 92%        │
│                                  │  Deps in: 3          │
│                                  │  Deps out: 5         │
│                                  │  Last changed: 2d    │
│                                  │  Issues: none        │
│                                  │                      │
│                                  │  [View File Detail]  │
└──────────────────────────────────┴──────────────────────┘
```

---

## Default State

- Layout: **Hierarchical** (directory structure as root, first-level dependencies as children)
- Filter: All files
- Overlays: Off
- Node detail panel: Hidden (no node selected)

---

## Layout Options

| Layout | Description | Use case |
|--------|-------------|----------|
| Hierarchical | Tree structure from directory root | Understanding directory organisation |
| Force-directed | Nodes attracted by dependency relationships | Finding tightly coupled clusters |
| Circular | Nodes arranged in circle by module | Seeing cross-module dependencies |

Selected layout persists per session.

---

## Node Detail Panel

Opens on left side when any node is clicked. Graph remains visible and interactive.

Shows:
- File path (full)
- Complexity score (0–10)
- Quality score (%)
- Dependency count: in / out
- Last modification date
- Active issues list (if any)
- "View File Detail →" button → navigates to L4

Panel closes when clicking empty canvas area or pressing `Escape`.

---

## Filters (Dropdown)

| Filter | Options |
|--------|---------|
| File type | .cs / .js / .ts / .py / All |
| Complexity threshold | Show only nodes above value (slider 0–10) |
| Issue severity | Highlight nodes with Critical / High / Any issues |
| Contributor | Show files owned by specific contributor |

Active filters are shown as dismissable chips below the controls bar.

---

## Issue Detection Overlays (Toggle Switches)

**Circular dependency detection**
- When ON: nodes involved in circular dependencies show a warning badge
- Circular dependency edges highlighted in red
- Count shown: "3 circular dependencies detected"

**Orphaned node detection**
- When ON: nodes with no incoming dependencies are dimmed (opacity 0.3)
- Count shown: "7 orphaned nodes detected"

Both overlays can be on simultaneously.

---

## Mobile Behaviour

Code graph is **not available on mobile**. On mobile, the route renders a list view:
- Flat list of all files
- Dependency count (in/out) as text
- Link to File Detail for each

Message shown: "Interactive graph requires a desktop browser."

---

## Performance

- Target render time for < 500 nodes: < 1500ms
- Maximum target: < 3000ms
- For repositories > 500 nodes: progressive reveal by directory tier, with "Load more" per tier
- Nodes are virtualised — off-screen nodes are not rendered

---

## Acceptance Criteria

- [ ] Graph opens in hierarchical layout by default
- [ ] Clicking a node opens the detail panel without hiding the graph
- [ ] Circular dependency overlay draws warning badges and red edges correctly
- [ ] Orphaned node overlay dims affected nodes to 0.3 opacity
- [ ] Mobile renders a list view, not a broken graph
- [ ] Export PNG captures current graph state including active overlays
- [ ] Zoom and pan work with mouse scroll + drag (desktop) and pinch + drag (touch)
- [ ] Node detail panel closes on Escape or empty canvas click
- [ ] WCAG: graph has text alternative (the file list accessible to screen readers)
