# Onboarding Flow

**Goal:** Get a new user from account creation to their first genuine quality insight in under 5 minutes.

**Rule:** Every step that is not necessary to reach the first insight must be deferred or removed. No feature tours. No walkthroughs. No "did you know?" modals.

---

## Step 1: Connect Repository

**Route:** `/onboarding/connect`  
**Shown:** When user has zero repositories connected. First screen after account creation.  
**File:** `src/pages/onboarding/ConnectRepository.tsx`

### Layout
Full-screen. Single purpose. Nothing else on screen.

```
┌─────────────────────────────────────────────────────────┐
│  RepoLens                                               │
│                                                         │
│  Connect your first repository                          │
│  We'll analyse your code and surface what to focus on.  │
│                                                         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐                │
│  │  GitHub  │ │  GitLab  │ │Bitbucket │                │
│  └──────────┘ └──────────┘ └──────────┘                │
│                                                         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐                │
│  │  Azure   │ │ Local Git│ │   Demo   │                │
│  │  DevOps  │ │          │ │   Repo   │                │
│  └──────────┘ └──────────┘ └──────────┘                │
│                                                         │
│  Already have repositories? Sign in →                  │
└─────────────────────────────────────────────────────────┘
```

### Provider buttons
Each button triggers the relevant OAuth flow or connection modal.

| Provider | Auth method |
|----------|------------|
| GitHub | OAuth App or GitHub App |
| GitLab | OAuth 2.0 |
| Bitbucket | OAuth 2.0 |
| Azure DevOps | OAuth 2.0 / PAT |
| Local Git | SSH key or HTTPS credentials |
| Demo repo | No auth — loads a pre-seeded demo repository instantly |

**Demo repo:** Pre-seeded with realistic data (mixed health scores, security issues, hotspots). Allows users to explore the full product without connecting a real repo. Does not require account creation.

### What is NOT on this screen
- No dashboard preview
- No feature list
- No pricing or plan information
- No "skip for now" option (connect is the only action)

---

## Step 2: Repository Connected — Configuration (Optional)

**Route:** `/onboarding/configure/:repoId`  
**Shown:** After OAuth/connection succeeds, before analysis starts.  
**This step is skippable** — "Start analysis" proceeds immediately with defaults.

```
┌─────────────────────────────────────────────────────────┐
│  frontend-app connected ✓                               │
│                                                         │
│  Analysis level:                                        │
│  ● Basic   (< 1 min)  — File metrics, complexity        │
│  ○ Advanced (< 5 min) — + Dependencies, vocabulary      │
│  ○ Expert  (< 15 min) — + Full AST, code graph          │
│                                                         │
│  [Start analysis]  [Change settings later]             │
└─────────────────────────────────────────────────────────┘
```

Default: **Basic** for first-time users. Reason: fastest path to first insight.

---

## Step 3: Analysis In Progress

**Route:** `/onboarding/analysing/:repoId`  
**File:** `src/pages/onboarding/AnalysisProgress.tsx`

### Layout
```
┌─────────────────────────────────────────────────────────┐
│  Analysing frontend-app...                              │
│                                                         │
│  ████████████████░░░░░░░░   62%                         │
│  Estimated time remaining: 45 seconds                   │
│                                                         │
│  We're calculating quality scores, identifying          │
│  hotspots, and mapping component relationships.         │
│                                                         │
│  [Go to dashboard — results will appear as ready]       │
└─────────────────────────────────────────────────────────┘
```

### Progress stages (shown sequentially as they complete)
1. Cloning repository
2. Parsing files
3. Calculating metrics
4. Detecting security patterns
5. Building dependency map
6. Indexing for search

Each stage: show a checkmark when complete, spinner for current stage.

### Progress API
```
// SignalR hub: /hubs/analysis
// Event: 'AnalysisProgress'
{
  repositoryId: string,
  stage: string,
  percentComplete: number,
  estimatedSecondsRemaining: number,
  isComplete: boolean
}
```

### User can navigate away
"Go to dashboard" link available immediately. If clicked:
- Navigate to Repository Dashboard (L2)
- Panels that haven't finished loading show skeleton + "Analysis in progress — results will appear here shortly"
- When analysis completes, SignalR event triggers panel data refresh automatically
- Browser notification offered: "Notify me when analysis is complete?" (uses Web Notifications API, with permission prompt)

---

## Step 4: Analysis Complete — First Landing

**Trigger:** Analysis complete event received (SignalR or page load after completion)  
**Destination:** Repository Dashboard (L2) at `/repos/:repoId`

### First-time contextual tooltip
A single tooltip, shown only on the first post-analysis visit to this repository's dashboard.

```
[Tooltip pointing at Quality Hotspots panel]
"Your most important files to look at are here."
[Dismiss — click anywhere]
```

Rules:
- Shown once only (stored in `localStorage: repo_{id}_tooltip_shown`)
- Dismissed by clicking anywhere on the page
- Does not block interaction — user can click into hotspots while tooltip is visible
- No "next", no tour, no additional steps

---

## Re-Onboarding (Adding More Repositories)

After the first repository is connected, subsequent repositories are added via **Settings > Repositories > Add Repository**.

The onboarding flow (`/onboarding/*`) is only used for the first repository.

For subsequent repositories:
- "Add Repository" button in Settings
- Same provider selection
- Same configuration step
- Analysis progress shown in a **notification** (not full-screen), since user has an existing dashboard to use

---

## Time to First Value

**Definition:** Elapsed time from clicking a provider button (Step 1) to viewing the Quality Hotspots panel populated with real data.

**Target:** Under 5 minutes for a repository under 100,000 lines of code at Basic analysis level.

**How measured:** Session analytics event `first_hotspot_viewed` minus session analytics event `repository_connect_initiated`.

---

## Acceptance Criteria

- [ ] Step 1 is a single-purpose screen — no other navigation visible
- [ ] Demo repo loads with pre-seeded data without authentication
- [ ] Analysis level defaults to Basic
- [ ] "Go to dashboard" link works during analysis — panels show skeleton while analysing
- [ ] SignalR updates progress bar in real time
- [ ] First-time tooltip appears on Quality Hotspots panel after analysis completes
- [ ] Tooltip appears once only per repository
- [ ] Tooltip dismisses on any click
- [ ] Web Notifications API permission prompted during analysis progress step
- [ ] Time to first value measurable via session analytics events
- [ ] Adding a second repository uses Settings flow, not `/onboarding/` route
