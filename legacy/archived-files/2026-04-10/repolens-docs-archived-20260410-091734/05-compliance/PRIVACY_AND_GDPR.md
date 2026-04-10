# Privacy, GDPR & Compliance

**This section is non-negotiable for enterprise deployment, particularly in the EU.**

All features involving individual developer data must be reviewed against these requirements before shipping. Non-compliance is a legal liability, not a UX preference.

---

## What Data RepoLens Processes

### Repository data (not personal)
- Code files and their content
- Commit history (timestamps, commit messages, file changes)
- Branch and tag structure
- Quality metrics derived from code analysis

### Personal data (GDPR-relevant)
- Developer usernames / display names (from Git provider)
- Individual commit counts and activity timestamps
- Code authorship (which files were authored/modified by whom)
- Review activity (reviews given/received)

---

## GDPR Requirements

### Legal basis
Processing of individual developer activity data requires a documented legal basis. For enterprise deployments this is typically **legitimate interests** (code quality management) or **contract performance**. Customers must document this in their DPA.

### Data Processing Agreement (DPA)
A DPA must be in place with each enterprise customer before individual contributor data is processed. This is a contractual requirement, not an optional add-on.

### Data subject rights
Developers (data subjects) must be able to:
- Request access to their own data
- Request deletion of their own data
- Object to processing

The platform must support data export and deletion per-user. Engineering must implement a user data deletion API endpoint.

### Data retention
Retention periods for individual metrics must be:
- Configurable by the organisation administrator
- Documented in the DPA
- Enforced automatically (data older than the retention period is purged)

---

## Works Council Requirements (Germany / EU)

In Germany and several other EU jurisdictions, deploying employee monitoring software requires **works council (Betriebsrat) approval** before rollout.

### What this means for enterprise sales
- Enterprise customers in Germany must be informed of this requirement during onboarding
- Sales/CS team must not promise deployment timelines without checking works council status

### What this means for the product
The platform must provide:

1. **A documented description** of:
   - What individual data is collected
   - How it is used
   - Who can access it
   - How long it is retained

2. **Aggregate-only mode** — a first-class deployment configuration where individual developer tracking is disabled entirely. Only team-level and repository-level metrics are shown. This mode must be fully functional, not a degraded fallback.

3. **Audit logs** of who has accessed individual developer data and when.

---

## Data Access Model (Role-Based)

Individual contributor data is restricted to a mandatory three-tier access model. This is enforced at the API level, not just the UI level.

| Role | Access to individual developer data | Notes |
|------|-------------------------------------|-------|
| Organisation admin | Full access | Can configure access controls and retention policies |
| Engineering manager | Their direct team members only | Cannot see data for developers outside their team |
| Team lead / senior dev | Self only (default) | Can see own data. Cannot see teammates' data unless elevated by admin. |
| Developer (standard) | Self only | Always can see own data. No access to others. |

### Implementation requirements
- RBAC enforced at API query level (not just frontend)
- `ContributorMetrics` queries filtered by team relationship before returning data
- Admin role can grant temporary elevation (with audit log entry)
- Audit log: every access to individual developer metrics records: user ID, accessed developer ID, timestamp, access reason (if provided)

---

## Data Never Collected

The following data points are **out of scope for RepoLens regardless of configuration**:
- Keylogging or IDE activity tracking
- Time-on-task or active hours monitoring
- Screenshots or screen recording
- Communication content (commits and code only — no email, chat, or PR comments)
- Biometric data of any kind
- Location data
- Device/browser tracking beyond what's needed for authentication

---

## Aggregate-Only Mode

A configuration flag at the organisation level: `individualMetricsEnabled: boolean`

When `false`:
- All `ContributorMetrics` API endpoints return 404 or aggregate-only responses
- Team Analytics screen shows aggregate view only — "View by contributor" button is hidden
- Search results do not show contributor tab
- File authorship data is not shown in file detail views
- Activity feed shows events without contributor attribution
- AI assistant cannot reference individual developers

This mode must be tested as a first-class configuration, not an afterthought.

---

## Acceptance Criteria

- [ ] DPA template exists and is available to enterprise customers
- [ ] Organisation admin can set data retention period (in months)
- [ ] User data deletion API endpoint exists and works
- [ ] RBAC enforced at API level (not just UI) for individual metrics
- [ ] Audit log captures: who accessed, whose data, when
- [ ] Aggregate-only mode disables all individual data views completely
- [ ] Works council documentation pack available for German enterprise customers
- [ ] "View by contributor" requires second click — never shown by default
- [ ] Individual view has no rankings, comparisons, or performance labels
