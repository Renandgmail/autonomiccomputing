# Success Metrics

Metrics tied to observable user behaviour. Each has a measurement method. Metrics without a measurement method are not tracked.

**Rule:** We do not track metrics that incentivise the wrong product behaviours. See "Metrics Not Tracked" section.

---

## North Star Metric

**Time to actionable decision**

Definition: Median time from login to the user clicking on a Quality Hotspot item.

| Target | Measurement |
|--------|------------|
| < 90 seconds (P50) | Session analytics: `first_hotspot_clicked.msFromLogin` |
| Baseline | Established in first 30 days post-launch |

Instrumentation:
```typescript
// On login / session start:
analytics.identify(userId, { sessionStartedAt: Date.now() });

// On first hotspot click in session:
analytics.track('first_hotspot_clicked', {
  msFromLogin: Date.now() - sessionStartedAt,
  repositoryId,
  sessionId
});
```

---

## Onboarding Metrics

| Metric | Target | Measurement |
|--------|--------|------------|
| Time to first repository connected | < 3 minutes | `repository_connect_confirmed.timestamp` minus `onboarding_started.timestamp` |
| Time to first quality hotspot viewed | < 5 minutes post-analysis | `first_hotspot_clicked.timestamp` minus `analysis_complete.timestamp` |
| 7-day retention after onboarding | > 65% | Return session within 7 days of `onboarding_complete` event |
| Onboarding completion rate | > 80% | % of users who reach `analysis_complete` from `onboarding_started` |

---

## Core Weekly Usage Metrics

| Metric | Target | Measurement |
|--------|--------|------------|
| Weekly active sessions per manager | > 3 sessions/week | Session count for users with role=manager, per week |
| L2 → L4 navigation rate | > 60% of sessions | % of sessions containing both `repository_dashboard_viewed` and `file_detail_viewed` |
| Search usage | > 2 searches/session (active users) | Search events per session, median across users with > 1 session/week |
| AI assistant invocation rate | 10–30% of sessions | % of sessions where `ai_assistant_opened` fires |

**AI assistant rate interpretation:**
- Below 10%: feature is undiscoverable → investigate placement, first-session indicator
- Above 30%: over-reliance → investigate if dashboard is failing to answer questions unaided

---

## Feature Adoption Milestones

Tracked as one-time events per user (first time each feature is used):

| Feature | Event name | Target: used within first 2 weeks |
|---------|-----------|----------------------------------|
| Universal search | `search_first_used` | > 70% of active users |
| Analytics screen | `analytics_first_viewed` | > 60% of active users |
| Code graph | `code_graph_first_viewed` | > 30% of active users |
| Cross-repo comparison | `compare_first_used` | > 20% of manager users |
| AI assistant | `ai_assistant_first_opened` | > 50% of active users |
| Export | `export_first_used` | > 25% of manager users |

---

## Retention Metrics

| Metric | Target | Measurement |
|--------|--------|------------|
| 7-day retention | > 65% | Users with session in days 1–7 after first session |
| 30-day retention | > 50% | Users with session in days 8–30 after first session |
| Weekly return rate (steady state) | > 70% | % of active users with session each calendar week (after 4-week ramp) |

---

## Quality Gate Metrics (Product Quality)

Measure whether the product itself is performing to its design intent:

| Metric | Target | Measurement |
|--------|--------|------------|
| North star (P50) | < 90 seconds | `first_hotspot_clicked.msFromLogin` |
| Portfolio dashboard load (P95) | < 2500ms | `performance.measure('portfolio_load')` |
| Context switch (P95) | < 1000ms | `performance.measure('context_switch')` |
| Search latency (P95) | < 800ms | `search_executed.durationMs` |
| Zero critical axe violations | 0 | Lighthouse CI per PR |

---

## Metrics Explicitly Not Tracked

These are not tracked because tracking them creates perverse incentives:

| Metric | Why not tracked |
|--------|----------------|
| AI suggestion acceptance rate | Optimising for this creates pressure to make suggestions more frequent and less relevant |
| Individual developer engagement | We do not monitor how much individual contributors use the platform — this is a privacy boundary |
| Number of notifications sent | Optimising for this creates notification spam |
| Feature click-through rate on tooltips/callouts | Optimising for this creates intrusive UI |
| Session duration | Longer sessions may mean users are confused, not engaged |
| Page views per session | More page views may mean worse navigation, not better discovery |

---

## Analytics Implementation

### Event schema (all events)
```typescript
interface AnalyticsEvent {
  event: string;
  userId: string;
  sessionId: string;
  timestamp: number;       // Unix ms
  properties: Record<string, unknown>;
}
```

### Required events (minimum viable instrumentation)

```typescript
// Session
analytics.track('session_started', { source: 'direct' | 'link' | 'notification' });

// Onboarding funnel
analytics.track('onboarding_started', {});
analytics.track('repository_connect_initiated', { provider: string });
analytics.track('repository_connect_confirmed', { repositoryId: string });
analytics.track('analysis_started', { repositoryId: string, analysisLevel: string });
analytics.track('analysis_complete', { repositoryId: string, durationMs: number });
analytics.track('onboarding_complete', { repositoryId: string });

// North star
analytics.track('first_hotspot_clicked', {
  msFromLogin: number,
  msFromAnalysisComplete: number,
  repositoryId: string,
  severity: string
});

// Navigation
analytics.track('repository_dashboard_viewed', { repositoryId: string });
analytics.track('file_detail_viewed', { repositoryId: string, fileId: string });
analytics.track('analytics_viewed', { repositoryId: string, tab: string });

// Context switch
analytics.track('repository_context_switched', {
  fromRepositoryId: string,
  toRepositoryId: string,
  durationMs: number
});

// Search
analytics.track('search_executed', {
  scope: 'repository' | 'global',
  queryLength: number,      // length only, not the query itself (privacy)
  resultCount: number,
  durationMs: number
});

// AI assistant
analytics.track('ai_assistant_opened', { screen: string });
analytics.track('ai_message_sent', { screen: string });   // no message content

// Feature first-use
analytics.track('feature_first_used', { feature: string });
```

### Privacy in analytics
- **Never log search query text** — only query length and result count
- **Never log AI message content**
- **Never log file paths or code content**
- Analytics events contain only IDs and behavioural metadata
- Analytics data is subject to the same retention policy as other personal data
