# Phase 4: AI Assistant & Polish

**Timeline:** Weeks 13–16  
**Prerequisite:** Phase 3 fully complete and QA-passed.  
**Goal:** AI assistant overlay, performance verification pass, third-party accessibility audit, internationalisation.

---

## Deliverables

### 1. AI Assistant Overlay

**File:** `src/components/AIAssistant/AIAssistantOverlay.tsx`

#### Trigger button
```tsx
// Persistent on every screen, bottom-right corner
// Position: fixed, bottom: 24px, right: 24px
// z-index: above all content, below modals

interface AIAssistantButtonProps {
  isFirstSession: boolean;   // shows pulsing indicator if true
  onOpen: () => void;
}
```

First session: pulsing CSS animation on the button border. Disappears permanently after first open (stored in `localStorage: ai_assistant_opened`).

#### Panel
```tsx
interface AIAssistantPanelProps {
  isOpen: boolean;
  onClose: () => void;
  context: AIAssistantContext;
}

// Desktop: slides in from right, 380px width
// Mobile: slides up from bottom, full width
// Primary content remains visible and interactive behind/beside the panel
// Closes: Escape / × button / outside click
// Focus: trapped inside panel when open (accessibility)
```

#### Context construction
The panel is context-aware. Build context automatically from current route.

```typescript
interface AIAssistantContext {
  screen: 'portfolio' | 'repository' | 'analytics' | 'search' | 'graph' | 'team' | 'file';
  repositoryId?: string;
  repositoryName?: string;
  repositoryHealthScore?: number;
  topHotspots?: Array<{ filePath: string; severity: string }>;
  fileId?: string;
  filePath?: string;
  fileMetrics?: {
    qualityScore: number;
    complexityScore: number;
    debtHours: number;
    issueCount: number;
  };
  selectedNode?: string;           // for code graph
  analyticsTab?: string;           // for analytics screen
}

// Build context hook:
function useAIAssistantContext(): AIAssistantContext {
  const { repoId, fileId } = useParams();
  const location = useLocation();
  const repoData = useRepository(repoId);
  const fileData = useFile(repoId, fileId);
  // derive context from current route and loaded data
}
```

#### API call
```
POST /api/ai/chat

Body: {
  message: string,
  context: AIAssistantContext,
  conversationHistory: Array<{ role: 'user' | 'assistant', content: string }>
}

Response: Server-sent events (streaming)
Content-Type: text/event-stream

// Each event:
data: { type: 'token', content: string }
data: { type: 'done' }
data: { type: 'error', message: string }
```

Stream tokens as they arrive — render progressively. Show typing indicator until first token.

#### Conversation state
- Stored in React state only (not persisted)
- Cleared when panel is closed
- History sent with each message (full conversation context)

#### What the AI is instructed to do (system prompt guidance)
Build the system prompt for the AI with these constraints. Send as part of the API call.

```
You are a code quality assistant for RepoLens. Your role is to help 
engineering managers and team leads understand their codebase quality data.

You MAY:
- Explain what quality metrics mean in plain language
- Summarise issues for non-technical stakeholders
- Suggest what to look at next based on the data provided
- Answer questions about the current screen's data

You MUST NOT:
- Generate code fixes or patches
- Create pull requests, branches, or issues in external systems
- Make HR or performance recommendations about individual developers
- Reference individual developer data unless it is included in the context
- Promise specific performance improvements from following your suggestions
- Speculate about data not present in the provided context

Keep responses concise: under 150 words. Offer to expand if needed.
Tone: direct and professional. No enthusiastic openers.
```

#### Appropriate use cases (surfaced in empty state)
When the conversation is empty, show example prompts relevant to current context:

- Repository Dashboard: "What should I prioritise this week?", "Summarise our top issues for a status report", "What does a complexity score of 8.2 mean?"
- File Detail: "What's wrong with this file?", "How long would it take to fix these issues?", "Explain this file's technical debt"
- Analytics > Trends: "Is our quality trend positive?", "How does our coverage compare to industry standards?"

#### Out-of-scope guardrails (enforced in system prompt + UI)
If the user asks the AI to generate code or create PRs, the UI should show a static refusal message before even making an API call:
```
Phrases that trigger client-side refusal:
- "generate a fix", "write the code", "create a PR", "open a pull request",
  "fix this for me", "apply this change"

Refusal message:
"I can explain the issue and suggest an approach, but I don't generate 
code or create pull requests. Would you like me to describe what a fix 
would involve?"
```

Full spec: `01-screens/L4_FILE_DETAIL_AND_AI_ASSISTANT.md`

---

### 2. Performance Verification Pass

Run against every screen in all four phases. Record actual measurements. All must pass before Phase 4 is considered complete.

**Measurement method:** Use Lighthouse CI and browser Performance API `PerformanceObserver`.

```typescript
// Instrument context switch measurement
performance.mark('context-switch-start');
// ... switch repository ...
performance.mark('context-switch-end');
performance.measure('context-switch', 'context-switch-start', 'context-switch-end');
```

**Targets (all must pass — see full table in `PERFORMANCE_TARGETS.md`):**

| Interaction | Target | Max |
|-------------|--------|-----|
| Repository context switch | < 500ms | < 1000ms |
| Portfolio dashboard load | < 1500ms | < 2500ms |
| Repository dashboard load | < 1000ms | < 2000ms |
| Search results | < 400ms | < 800ms |
| Analytics charts | < 600ms | < 1200ms |
| Code graph (< 500 nodes) | < 1500ms | < 3000ms |
| AI first token | < 1000ms | < 2000ms |
| Filter / sort | < 200ms | < 400ms |

**Remediation before ship:**
- Any interaction exceeding its max target: investigate and fix before Phase 4 sign-off
- Document measurement results in a performance report

---

### 3. Full Accessibility Audit

**Third-party audit required** — not self-audited. Use an external accessibility auditor or automated tool (axe-core, Deque) + manual screen reader testing.

**Scope:** All screens across all four phases.

**Checklist:**

#### Automated (axe-core in CI)
- [ ] Zero critical or serious axe violations across all routes
- [ ] Integrate axe-core into Playwright test suite

#### Manual testing
- [ ] Full keyboard navigation: every interactive element reachable by Tab
- [ ] Focus order: logical top-to-bottom, left-to-right
- [ ] Focus ring visible: 2px outline, `--interactive` colour
- [ ] Skip navigation link present and functional
- [ ] Escape closes all modals, panels, and overlays
- [ ] Screen reader (NVDA/JAWS on Windows, VoiceOver on Mac):
  - [ ] All page headings announced in correct order
  - [ ] All table headers announced correctly
  - [ ] All charts have accessible text alternatives
  - [ ] Dynamic updates announced via aria-live regions
  - [ ] Form inputs have associated labels
- [ ] `prefers-reduced-motion`: all animations disabled correctly
- [ ] Colour contrast: all combinations pass WCAG 2.1 AA minimums
- [ ] No information conveyed by colour alone (every status has text label)

---

### 4. Internationalisation (i18n)

**Framework:** `react-i18next`

**Phase 4 scope:** Infrastructure setup + English (en-GB) as baseline. Full translation work is post-Phase 4.

#### What to implement in Phase 4

**Date formatting** — use `Intl.DateTimeFormat` with locale:
```typescript
// Relative dates ("3 minutes ago")
// Use date-fns formatDistanceToNow with locale parameter

// Absolute dates
new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(date)
```

**Number formatting** — use `Intl.NumberFormat`:
```typescript
// Health scores: "94%" not "94%"
// Technical debt: "4.2h" (not localised in Phase 4 — consistent unit)
// Large numbers: "1,234" vs "1.234" (locale-specific thousand separator)
new Intl.NumberFormat(locale).format(number)
```

**String externalisation:** Extract all UI strings to `src/locales/en-GB.json`. No hardcoded UI strings in components.

```json
{
  "portfolio.summary.repositories": "{{count}} Repositories",
  "portfolio.summary.avgHealth": "Average Health",
  "repository.hotspots.title": "Quality Hotspots",
  "repository.hotspots.seeAll": "See all {{count}} hotspots",
  "common.severity.critical": "Critical",
  "common.severity.high": "High",
  "common.severity.medium": "Medium",
  "common.severity.low": "Low"
}
```

**RTL layout** — add `dir="rtl"` support at root level using CSS logical properties (`margin-inline-start` instead of `margin-left`). Test with Arabic locale. Full RTL language support is post-Phase 4.

---

## Phase 4 Acceptance Criteria

- [ ] AI assistant floating button visible on every screen
- [ ] Panel opens from right (desktop) and bottom (mobile)
- [ ] Context is correct per screen (test against context mapping table)
- [ ] Streaming tokens render progressively in the panel
- [ ] Client-side refusal fires for code generation / PR creation requests
- [ ] Conversation cleared when panel closes
- [ ] All performance targets pass (documented results in performance report)
- [ ] Zero critical or serious axe-core violations in CI
- [ ] All manual accessibility checks pass (screen reader, keyboard, contrast)
- [ ] All UI strings in `en-GB.json` locale file (zero hardcoded strings)
- [ ] Dates and numbers formatted via `Intl` APIs
- [ ] RTL layout correct at root level with `dir="rtl"`
