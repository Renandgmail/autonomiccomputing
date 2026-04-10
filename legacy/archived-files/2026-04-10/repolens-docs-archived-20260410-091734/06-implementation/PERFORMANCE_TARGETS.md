# Performance Targets

All performance targets are stated as UX thresholds — the boundaries at which an interface feels responsive. These are requirements, not aspirations.

**Measurement method:** Browser `PerformanceObserver` API + Lighthouse CI in the build pipeline. Results documented in Phase 4 performance report.

---

## Interaction-Level Targets

| Interaction | Target (P50) | Maximum (P95) | Fallback behaviour |
|-------------|-------------|--------------|-------------------|
| Repository context switch | < 500ms | < 1000ms | Skeleton loader covers entire content area immediately |
| Portfolio Dashboard initial load | < 1500ms | < 2500ms | Skeleton zones 1–3, progressive reveal |
| Repository Dashboard load | < 1000ms | < 2000ms | Skeleton all four zones |
| Universal search results | < 400ms | < 800ms | Spinner inside search input |
| Analytics chart render | < 600ms | < 1200ms | Skeleton rectangle at chart dimensions |
| Code graph render (< 500 nodes) | < 1500ms | < 3000ms | Progressive tier reveal |
| Code graph render (500–2000 nodes) | < 3000ms | < 6000ms | Directory-tier pagination with "Load more" |
| AI assistant first token | < 1000ms | < 2000ms | Typing indicator visible immediately |
| Filter / sort application | < 200ms | < 400ms | Spinner in table header |
| Repository switcher open | < 100ms | < 200ms | Instant (cached data) |
| Page navigation (client-side) | < 150ms | < 300ms | Instant route transition + skeleton |

---

## API-Level Targets

| Endpoint category | Target | Maximum |
|-------------------|--------|---------|
| Summary / aggregate endpoints | < 300ms | < 500ms |
| List endpoints (paginated) | < 300ms | < 500ms |
| Search queries | < 300ms | < 600ms |
| Analytics/trends queries | < 400ms | < 700ms |
| Code graph data | < 500ms | < 1000ms |
| Export generation (PDF) | < 5s | < 15s |
| Export generation (CSV) | < 1s | < 3s |
| AI chat response (first token) | < 800ms | < 1500ms |

---

## Frontend Bundle Targets

| Asset | Target | Maximum |
|-------|--------|---------|
| Initial JS bundle (gzipped) | < 150KB | < 250KB |
| Initial CSS (gzipped) | < 30KB | < 50KB |
| Largest Contentful Paint (LCP) | < 1.5s | < 2.5s |
| First Input Delay (FID) | < 50ms | < 100ms |
| Cumulative Layout Shift (CLS) | < 0.05 | < 0.1 |
| Time to Interactive (TTI) | < 2s | < 3.5s |

**Techniques required:**
- Code splitting per route (React.lazy + Suspense)
- D3.js loaded only on the code graph route
- Recharts loaded only on analytics routes
- IBM Plex fonts loaded via `font-display: swap`
- Images and avatars lazy-loaded

---

## Database Query Targets

| Query type | Target | Maximum |
|-----------|--------|---------|
| Single repository summary | < 50ms | < 100ms |
| Repository list (all repos for org) | < 80ms | < 150ms |
| Hotspots ranked query | < 80ms | < 150ms |
| Trend data (30-day aggregation) | < 100ms | < 200ms |
| Full-text search | < 150ms | < 300ms |
| Code graph query | < 200ms | < 400ms |
| Analytics files list (paginated) | < 80ms | < 150ms |

**Indexes required:**
- `RepositoryFiles`: index on `repositoryId`, `healthScore`, `lastModifiedAt`
- `FileMetrics`: index on `repositoryId`, `debtHours`, `complexityScore`, `coveragePercent`
- `ContributorMetrics`: index on `repositoryId`, `contributorId`
- `Commits`: index on `repositoryId`, `authorId`, `committedAt`
- GIN index on `RepositoryFiles.content` for full-text search

---

## Caching Strategy

### Client-side (React Query)
```typescript
const queryConfig = {
  // Portfolio summary — changes infrequently
  portfolioSummary: { staleTime: 5 * 60 * 1000 },       // 5 minutes

  // Repository dashboard — check freshness every 2 minutes
  repositoryDashboard: { staleTime: 2 * 60 * 1000 },    // 2 minutes

  // Hotspots — moderately dynamic
  hotspots: { staleTime: 2 * 60 * 1000 },               // 2 minutes

  // Analytics trends — can be stale for longer
  analyticsTrends: { staleTime: 10 * 60 * 1000 },       // 10 minutes

  // Search results — short cache (queries change)
  search: { staleTime: 30 * 1000 },                      // 30 seconds

  // Code graph — expensive to compute, cache aggressively
  codeGraph: { staleTime: 15 * 60 * 1000 },             // 15 minutes
};
```

### Cache invalidation
React Query cache is invalidated on SignalR events:
```typescript
// On 'AnalysisComplete' event for repoId:
queryClient.invalidateQueries(['repository', repoId]);
queryClient.invalidateQueries(['hotspots', repoId]);
queryClient.invalidateQueries(['analytics', repoId]);

// On 'SyncComplete' event for repoId:
queryClient.invalidateQueries(['repository', repoId, 'summary']);
queryClient.invalidateQueries(['hotspots', repoId]);
```

### Server-side
- Repository summary and portfolio data: cached in Redis with 2-minute TTL
- Code graph data: cached in Redis with 15-minute TTL, invalidated on new commits
- Trend aggregations: pre-computed nightly, stored in `RepositoryMetrics` table
- Analysis results: stored permanently in DB, never recomputed unless triggered

---

## Performance Monitoring in Production

Instrument these events via your analytics provider:

```typescript
// Measure context switch
analytics.track('repository_context_switch', {
  fromRepoId: string,
  toRepoId: string,
  durationMs: number,
  wasCached: boolean
});

// Measure dashboard load
analytics.track('dashboard_load', {
  screen: 'portfolio' | 'repository',
  durationMs: number,
  repositoryId?: string
});

// Measure search
analytics.track('search_executed', {
  scope: 'repository' | 'global',
  resultCount: number,
  durationMs: number
});

// Track time to first hotspot click (north star metric)
analytics.track('first_hotspot_clicked', {
  sessionId: string,
  msFromLogin: number,
  repositoryId: string
});
```

The `first_hotspot_clicked` event is the primary instrument for the **north star metric** (time to actionable decision, target < 90 seconds).

---

## Remediation Decision Tree

If a measurement exceeds its maximum target:

```
Is it a frontend rendering issue?
  → Profile with React DevTools
  → Check for unnecessary re-renders (React.memo, useMemo)
  → Check bundle size (webpack-bundle-analyzer)
  → Check if library is loaded eagerly (should be lazy)

Is it an API response time issue?
  → Check database query execution plan (EXPLAIN ANALYZE)
  → Check if index is missing
  → Check if query can be pre-aggregated
  → Check Redis cache hit rate

Is it a network issue?
  → Check payload size (compress large responses)
  → Check if multiple sequential API calls can be parallelised
  → Check if React Query is making redundant requests
```
