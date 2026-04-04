# Product Definition

## What RepoLens Is

RepoLens is an **enterprise repository analytics platform**. It turns complex code data into decisions — surfacing where to direct team attention, where technical risk is accumulating, and how quality is trending over time.

## What RepoLens Is NOT

Do not build or design any of these:
- Code editor or IDE plugin
- Inline code suggestion tool
- AI chatbot as primary interface
- Code review tool (replaces GitHub PRs / GitLab MRs)
- Automated code fix generator or PR creator
- Sprint tracking or project management tool
- Individual developer HR performance tool
- Real-time pair programming assistant

## Primary User

**Engineering Manager.** All design and architecture decisions default to their needs first.

The engineering manager needs to answer three questions every week without reading every pull request:
1. Where does my team need to focus right now?
2. Where is technical risk accumulating?
3. Is our quality improving or degrading over time?

## Secondary Users

Served by the same interface at different depth levels via progressive disclosure:
- Senior Developers (L3/L4)
- QA Engineers (L3/L4)
- CTOs / Technical Directors (L1 mainly)
- Product Managers (L1/L2 read-only)

## Primary Use Case

The **weekly planning and review cycle**:
1. Manager opens RepoLens
2. Reviews portfolio health
3. Selects the active repository
4. Identifies highest-priority quality issues
5. Acts — assigns work, schedules review, or escalates risk

Everything else — AI assistance, cross-repository comparison, code graph, team analytics — **serves this cycle**. Secondary features must not compete for attention in the primary interface.

## North Star Metric

**Time to actionable decision:** time from login to manager identifying their single most important quality action for the day.

**Target: under 90 seconds.**

## Explicit Non-Goals (Out of Scope)

These will not be built regardless of stakeholder requests:
- Real-time IDE integration
- Automated code fix generation
- Pull request creation
- Individual developer performance scoring for HR
- Real-time pair programming
- Sprint velocity tracking
- Replacing existing code review tools

## Design Principles (Priority Order)

When principles conflict, lower-numbered principles win.

| # | Principle | In practice |
|---|-----------|-------------|
| 1 | Dashboard-first, not chat-first | Primary interface is structured dashboard. AI is secondary overlay. Users never forced into chat for a core task. |
| 2 | Repository context always visible | Every screen at L2+ shows current repository. Context switch is one click. |
| 3 | Progressive disclosure | Portfolio → Repository → Feature → File. Never show file detail before repo is selected. |
| 4 | Insights over metrics | Every metric needs trend, threshold, or recommendation. Raw numbers alone = not acceptable. |
| 5 | Privacy by default | Individual dev data is aggregate-only by default. Per-person drill-down requires explicit navigation + GDPR compliance. |
