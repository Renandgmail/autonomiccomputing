# RepoLens — Agent Knowledge Base

This folder is the single source of truth for building RepoLens. Every file is written for a Cline agent to read and act on directly. Start here, then follow the task you are working on.

---

## What RepoLens Is

Enterprise repository analytics platform. Primary user: **Engineering Manager**. Primary job: answer "where does my team need to focus right now?" in under 90 seconds after login.

It is **not** a code editor, IDE plugin, AI chatbot, or project management tool.

---

## Folder Map

```
repolens-docs/
├── README.md                          ← You are here. Start here.
│
├── 00-architecture/
│   ├── PRODUCT_DEFINITION.md          ← What it is, who it's for, non-goals
│   ├── INFORMATION_ARCHITECTURE.md    ← Nav hierarchy, user flows, screen map
│   └── TECH_STACK.md                  ← Frontend/backend/infra decisions
│
├── 01-screens/
│   ├── L1_PORTFOLIO_DASHBOARD.md      ← Landing page spec
│   ├── L2_REPOSITORY_DASHBOARD.md     ← Main working screen spec
│   ├── L3_UNIVERSAL_SEARCH.md         ← Search screen spec
│   ├── L3_ANALYTICS.md                ← Analytics tabs spec
│   ├── L3_CODE_GRAPH.md               ← Code graph visualisation spec
│   ├── L3_TEAM_ANALYTICS.md           ← Team analytics spec
│   ├── L4_FILE_DETAIL.md              ← File detail view spec
│   └── AI_ASSISTANT_OVERLAY.md        ← AI assistant overlay spec
│
├── 02-components/
│   ├── METRIC_CARD.md
│   ├── REPOSITORY_HEALTH_CHIP.md
│   ├── QUALITY_HOTSPOT_ROW.md
│   ├── SEVERITY_BADGE.md
│   ├── REPOSITORY_SWITCHER.md
│   ├── CONTEXT_BAR.md
│   └── GLOBAL_NAVIGATION.md
│
├── 03-interactions/
│   ├── CONTEXT_SWITCHING.md
│   ├── PROGRESSIVE_DISCLOSURE.md
│   ├── FILTERING_AND_SORTING.md
│   ├── LOADING_STATES.md
│   ├── ERROR_STATES.md
│   ├── EMPTY_STATES.md
│   └── NOTIFICATIONS.md
│
├── 04-design-system/
│   ├── COLOURS.md
│   ├── TYPOGRAPHY.md
│   ├── SPACING.md
│   └── RESPONSIVE_BREAKPOINTS.md
│
├── 05-compliance/
│   ├── PRIVACY_AND_GDPR.md
│   ├── ACCESSIBILITY.md
│   └── DATA_ACCESS_MODEL.md
│
└── 06-implementation/
    ├── PHASE_1_CORE_DASHBOARD.md      ← Weeks 1–4: nav, context bar, L1, L2, components, onboarding shell
    ├── PHASE_2_ANALYTICS_SEARCH.md    ← Weeks 5–8: search, analytics tabs, file detail, export
    ├── PHASE_3_ADVANCED_FEATURES.md   ← Weeks 9–12: code graph, team analytics, compare tab
    ├── PHASE_4_AI_POLISH.md           ← Weeks 13–16: AI assistant, perf pass, a11y audit, i18n
    ├── ONBOARDING_FLOW.md             ← Step-by-step: connect → analyse → first insight
    ├── PERFORMANCE_TARGETS.md         ← All latency targets, caching strategy, remediation guide
    └── SUCCESS_METRICS.md             ← North star metric, all tracked events, analytics schema
```

---

## Quick Rules for Every Agent

1. **Dashboard-first, not chat-first.** The primary interface is a structured dashboard. AI is a secondary overlay. Never force users into a chat flow for a core task.
2. **Repository context is always visible.** Every screen at L2 and below shows the context bar with current repository name and health score.
3. **Progressive disclosure.** L1 → L2 → L3 → L4. Never show file-level data before the user has selected a repository.
4. **Insights over metrics.** Every metric must show a trend, threshold, or recommendation alongside it. Raw numbers alone are not acceptable.
5. **Privacy by default.** Individual developer data is aggregate-only by default. Per-person drill-down requires explicit navigation.
6. **Colour encodes meaning only.** Never use colour decoratively. Every colour must pass the "why this colour?" test with a semantic answer.

---

## North Star Metric

**Time to actionable decision:** median time from login to clicking a Quality Hotspot item. Target: **under 90 seconds**.

---

## Glossary

| Term | Definition |
|------|-----------|
| **L1 / L2 / L3 / L4** | Navigation depth levels: Portfolio / Repository / Feature / File |
| **Health score** | Composite 0–100% metric: complexity + test coverage + security + debt ratio |
| **Quality hotspot** | File ranked by: complexity × churn × quality deficit. Highest = most urgent. |
| **Technical debt** | Hours estimate to remediate a file to baseline quality (SQALE method) |
| **Context bar** | Persistent bar below global nav showing current repo, health, sync status, breadcrumb |
| **Bus factor** | Min team members whose departure would impair module maintenance. Higher = safer. |
