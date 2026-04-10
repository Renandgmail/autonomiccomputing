# Phase 3: Advanced Features

**Timeline:** Weeks 9–12  
**Prerequisite:** Phase 2 fully complete and QA-passed.  
**Goal:** Deliver the power-user layer — code graph, team analytics with privacy controls, cross-repository comparison.

---

## Deliverables

### 1. Code Graph

**Route:** `/repos/:repoId/graph`  
**File:** `src/pages/CodeGraph.tsx`  
**Library:** D3.js

#### Data loading

```
GET /api/repositories/:repoId/graph?layout={hierarchical|force|circular}

Response: {
  nodes: Array<{
    id: string,
    filePath: string,
    label: string,         // filename only (not full path)
    complexityScore: number,
    qualityScore: number,
    hasIssues: boolean,
    issueMaxSeverity: 'critical' | 'high' | 'medium' | 'low' | null,
    type: 'file' | 'directory'
  }>,
  edges: Array<{
    source: string,        // node id
    target: string,        // node id
    isCircular: boolean    // part of a circular dependency
  }>,
  meta: {
    totalNodes: number,
    circularDependencyCount: number,
    orphanedNodeCount: number
  }
}
```

#### Layout implementations

**Hierarchical (default):**
- Use D3 `d3.hierarchy()` + `d3.tree()` layout
- Root = repository root directory
- Children = files/subdirectories
- Left-to-right orientation

**Force-directed:**
- Use D3 `d3.forceSimulation()`
- Link force: `d3.forceLink()` with edges as links
- Charge: `d3.forceManyBody()` with negative strength (repulsion)
- Center: `d3.forceCenter()`

**Circular:**
- Use D3 `d3.cluster()` with radial layout
- Nodes arranged on circle, edges as curved paths inside

#### Interactivity
- **Zoom + pan:** `d3.zoom()` on SVG container. Mouse wheel + drag. Pinch on touch.
- **Click node:** opens `NodeDetailPanel` (right panel, 320px). Graph remains interactive.
- **Hover node:** tooltip showing filename + health chip
- **Drag node** (force layout only): re-positions node, simulation continues
- **Escape:** closes node detail panel

#### Node Detail Panel
```tsx
interface NodeDetailPanelProps {
  node: GraphNode | null;
  onClose: () => void;
  onNavigateToFile: (fileId: string) => void;
}
```

Shows: full file path / complexity score / quality score / deps in count / deps out count / last modified date / active issues list / "View File Detail →" button.

#### Overlays (toggle switches in controls bar)

**Circular dependency detection:**
```tsx
// When enabled:
// - nodes involved in circular deps: show warning badge overlay
// - circular edges: stroke colour #DC2626
// - show count: "3 circular dependencies"
const circularNodeIds = edges.filter(e => e.isCircular).flatMap(e => [e.source, e.target]);
```

**Orphaned node detection:**
```tsx
// When enabled:
// - nodes with no incoming edges: opacity 0.3
// - show count: "7 orphaned nodes"
const nodeIdsWithIncoming = new Set(edges.map(e => e.target));
const orphans = nodes.filter(n => !nodeIdsWithIncoming.has(n.id));
```

#### Filters
Filters narrow which nodes are rendered. Applied client-side for performance.

```tsx
interface GraphFilters {
  fileTypes: string[];           // e.g. ['.cs', '.js']
  complexityMin: number | null;  // show nodes ABOVE this value
  issueSeverity: 'critical' | 'high' | 'any' | null;
  contributorId: string | null;  // show files owned by this contributor
}
```

#### Performance
- Target render for < 500 nodes: < 1500ms
- For repos > 500 nodes: paginate by directory tier
  - Show root + first-level directories
  - "Expand [DirectoryName] (42 files)" button per directory node
- Nodes off-screen are not rendered (virtualisation via `d3.zoom` viewport clipping)

#### Mobile fallback
On screens < 768px: render a flat file list with dependency counts instead of graph.
Show banner: "Interactive graph requires a desktop browser. Showing file list."

Full spec: `01-screens/L3_CODE_GRAPH.md`

---

### 2. Team Analytics

**Route:** `/repos/:repoId/team`  
**File:** `src/pages/TeamAnalytics.tsx`

**Privacy rule:** Team aggregate is the default. Individual view requires explicit click. This is enforced in the UI AND must be enforced at the API level (RBAC).

#### Team-level API
```
GET /api/repositories/:repoId/team/aggregate?range={7d|30d|90d}

Response: {
  contributionVolume: Array<{ week: string, commitCount: number }>,
  reviewTurnaround: {
    under1Day: number,
    oneTo3Days: number,
    threeTo7Days: number,
    over7Days: number
  },
  moduleOwnership: Array<{
    moduleName: string,
    isConcentrated: boolean,   // < 2 contributors own > 80%
    contributorCount: number
  }>,
  velocityTrend: Array<{ week: string, effectiveLinesChanged: number }>,
  velocityTarget: number | null
}
```

#### Individual-level API (requires RBAC check)
```
GET /api/repositories/:repoId/team/contributors/:contributorId?range=90d
// Returns 403 if requesting user lacks permission (see PRIVACY_AND_GDPR.md)

Response: {
  contributorId: string,
  displayName: string,
  commitVolume: Array<{ week: string, count: number }>,
  qualityTrend: Array<{ week: string, avgQualityScore: number }>,
  reviewActivity: { given: number, received: number },
  moduleFocus: string[]    // module/directory names
}
```

#### Individual view navigation
The "View by contributor" CTA is the only entry point to individual data.

```
URL: /repos/:repoId/team?view=individual&contributorId={id}
```

This URL is never shown in the UI (no copy-link button). Only navigable via the CTA button.

#### Language rules enforced in UI
The individual view component must use only these labels:
- ✅ "Areas of focus", "Recent activity", "Quality trends", "Contribution pattern"
- ❌ "Performance", "Productivity score", "Rating", "Rank", "Output", "Top contributor"

No side-by-side individual comparisons. No league tables. No ranking indicators.

Full spec: `01-screens/L4_FILE_DETAIL_AND_AI_ASSISTANT.md` (Team Analytics section)

---

### 3. Analytics — Team Tab

**Route:** `/repos/:repoId/analytics/team`  
**File:** `src/pages/analytics/TeamTab.tsx`

Same aggregate data as Team Analytics page but within the Analytics tab context. Links to full Team Analytics page for deeper exploration.

---

### 4. Analytics — Compare Tab

**Route:** `/repos/:repoId/analytics/compare`  
**File:** `src/pages/analytics/CompareTab.tsx`

#### Repository selector
- Current repo is always included (cannot be removed)
- Add up to 2 additional repos via dropdown (shows all connected repos)
- Selection stored in URL: `?compare=repoId1,repoId2`

#### Comparison API
```
GET /api/analytics/compare?repos={id1},{id2},{id3}

Response: {
  repositories: Array<{
    id: string,
    name: string,
    metrics: {
      healthScore: number,
      avgComplexity: number,
      debtHours: number,
      testCoverage: number,
      criticalSecurityIssues: number,
      activeContributors: number,
      linesOfCode: number
    }
  }>
}
```

#### Layout
Comparison table: repositories as columns, metrics as rows.  
Current repo column has a subtle highlight (`--surface` background).

#### Export
```
POST /api/analytics/compare/export
Body: { repos: string[], format: 'pdf' | 'csv' }
Response: { downloadUrl: string }
```

Full spec: `01-screens/L3_ANALYTICS.md#Compare Tab`

---

## API Endpoints Required in Phase 3

```
GET  /api/repositories/:id/graph
GET  /api/repositories/:id/team/aggregate
GET  /api/repositories/:id/team/contributors/:contributorId
GET  /api/analytics/compare
POST /api/analytics/compare/export
```

---

## Phase 3 Acceptance Criteria

- [ ] Code graph renders in < 1500ms for repos with < 500 nodes
- [ ] Hierarchical layout is the default
- [ ] Node detail panel opens on click without hiding the graph
- [ ] Circular dependency overlay shows warning badges + red edges
- [ ] Orphaned node overlay dims affected nodes to opacity 0.3
- [ ] Mobile route shows file list, not broken graph
- [ ] Graph export (PNG) captures current state including active overlays
- [ ] Team Analytics shows aggregate by default — no individual data visible on load
- [ ] "View by contributor" is the only entry point to individual data
- [ ] Individual view uses correct language (no "performance", "rank", "score")
- [ ] Individual view API returns 403 if user lacks RBAC permission
- [ ] Compare tab supports up to 3 repositories
- [ ] Compare export generates PDF and CSV
- [ ] All breakpoints correct for all Phase 3 screens
- [ ] WCAG 2.1 AA passes for all Phase 3 screens
