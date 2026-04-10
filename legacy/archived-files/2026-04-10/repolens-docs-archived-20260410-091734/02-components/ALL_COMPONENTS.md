# Components: Complete Specification

All reusable UI components for RepoLens. Each component is built once and used consistently across screens.

---

## MetricCard

**Used on:** L1 Portfolio Dashboard (Zone 1), L2 Repository Dashboard (Zone 1)

### Anatomy
```
┌─────────────────────┐
│  Label              │  ← 12px caption, --text-secondary
│                     │
│  94%                │  ← 32px IBM Plex Mono, --text-primary
│  ↑ +3% vs last week │  ← 13px, trend colour (green/red/grey)
└─────────────────────┘
```

### Props
```typescript
interface MetricCardProps {
  label: string;
  value: string | number;
  unit?: string;           // e.g. "%" or "h"
  trend?: {
    direction: 'up' | 'down' | 'flat';
    delta: string;         // e.g. "+3%" or "-2h"
    context: string;       // e.g. "vs last week"
    positive: 'up' | 'down'; // is "up" good or bad for this metric?
  };
  onClick?: () => void;    // if present, card is clickable
  loading?: boolean;
}
```

### Styles
- Width: 220px fixed (desktop), 100% (mobile)
- Background: `--card` (#FFFFFF)
- Border: 1px solid `--border` (#E5E7EB)
- Border-radius: 8px
- Padding: 16px
- Cursor: pointer (if `onClick` present), default otherwise

### Trend colours
- `up` + `positive: 'up'` → green (#16A34A)
- `down` + `positive: 'up'` → red (#DC2626)
- `up` + `positive: 'down'` → red (#DC2626)  (e.g. "issues increased" is bad)
- `down` + `positive: 'down'` → green (#16A34A)
- `flat` → grey (#6B7280)

### Loading state
Render skeleton: label line (80px wide), value line (60px wide), trend line (100px wide). Animated shimmer.

---

## RepositoryHealthChip

**Used on:** L1 repository list, L2 context bar, repository switcher dropdown

### Anatomy
```
● 94% ↑
```
- Coloured dot (8px circle)
- Percentage value
- Trend arrow (↑ ↓ →)

### Props
```typescript
interface RepositoryHealthChipProps {
  score: number;           // 0–100
  trend: 'up' | 'down' | 'flat';
  size?: 'small' | 'default';
}
```

### Colour mapping
| Score | Dot colour | Hex |
|-------|-----------|-----|
| 90–100 | Green | #16A34A |
| 70–89 | Teal | #0D9488 |
| 50–69 | Amber | #D97706 |
| 30–49 | Orange | #EA580C |
| 0–29 | Red | #DC2626 |

### Rules
- This component is **display only** — not interactive
- Colour alone never conveys score — always show numeric value
- Trend arrow always present alongside colour and number

---

## QualityHotspotRow

**Used on:** L2 Quality Hotspots panel, L3 Analytics > Files tab

### Anatomy
```
┌─────────────────────────────────────────────────────────┐
│ src/auth/PaymentService.cs          🔴 Critical  ~4h   │
│ Complexity                                               │
└─────────────────────────────────────────────────────────┘
```

### Props
```typescript
interface QualityHotspotRowProps {
  filePath: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  issueType: 'complexity' | 'security' | 'debt' | 'coverage';
  estimatedFixTime: string;  // e.g. "~4h"
  onClick: () => void;       // navigates to L4
}
```

### Styles
- File path: IBM Plex Mono, 13px, `--text-primary`
- Full path from repository root
- On hover: background `--surface` (#F9FAFB)
- Cursor: pointer

### Severity badge
- Critical: red background (#DC2626) + white text
- High: orange background (#EA580C) + white text
- Medium: amber background (#D97706) + white text
- Low: grey background (#6B7280) + white text
- Always text + colour (never colour alone)

---

## SeverityBadge

**Used on:** QualityHotspotRow, search results, analytics security tab, file detail issues list

### Props
```typescript
interface SeverityBadgeProps {
  level: 'critical' | 'high' | 'medium' | 'low';
  size?: 'small' | 'default';
}
```

### Colours
| Level | Background | Text | 
|-------|-----------|------|
| Critical | #DC2626 | White |
| High | #EA580C | White |
| Medium | #D97706 | White |
| Low | #6B7280 | White |

**Rule:** Always show text label. Never colour alone.

---

## ContextBar

**Used on:** All screens at L2 and below. Appears below global navigation.

### Anatomy
```
┌─────────────────────────────────────────────────────────┐
│ Portfolio > [frontend-app ▼]  ● 94% ↑   Synced 2m ago 🔄│
└─────────────────────────────────────────────────────────┘
```

### Elements (left to right)
1. Breadcrumb: `Portfolio` (link to L1) `>` repository name (opens switcher)
2. Health chip (`RepositoryHealthChip`)
3. Sync status: "Synced 2m ago" or "Syncing..." or "Sync failed"
4. Manual refresh icon button (tooltip: "Refresh now")

### Rules
- Always visible on L2 and below — never hidden
- Repository name is always a link/button that opens the switcher
- Sync failure shows orange warning icon + "Sync failed" text
- Syncing shows spinner + "Syncing..."

---

## RepositorySwitcherDropdown

**Triggered by:** Clicking repository name in context bar  
**Keyboard shortcut:** `Ctrl+R` / `Cmd+R`

### Anatomy
```
┌─────────────────────────────────┐
│ 🔍 Search repositories...       │
├─────────────────────────────────┤
│ ⭐ FAVOURITES                   │
│  ● frontend-app  94% ↑          │ ← currently selected (highlighted)
│  ● backend-api   78% ↓          │
├─────────────────────────────────┤
│ 🕐 RECENT                       │
│  ● mobile-app    91% →          │
│  ● legacy-system 45% ↓          │
├─────────────────────────────────┤
│ All repositories (12) →         │ ← link to settings/repo list
└─────────────────────────────────┘
```

### Behaviour
- Opens on click of repository name in context bar
- Search input is **auto-focused** when dropdown opens
- Typing filters all groups in real time
- Currently selected repository is highlighted with a checkmark
- Selecting a repo triggers context switch (see `03-interactions/CONTEXT_SWITCHING.md`)
- Maximum visible height: 400px (scrollable)
- Closes on: selection made / Escape / click outside

### Groups
1. **Favourites** (starred) — always shown at top, max 5
2. **Recent** — last 5 accessed, excluding current
3. **All repositories** — link to full list (does not expand inline)

### Health chips
Each repository entry shows its `RepositoryHealthChip` to the right of the name.

---

## GlobalNavigation

**Appears on:** Every screen.  
**Height:** Fixed, 56px.

### Elements (left to right)
1. **Logo** — links to Portfolio Dashboard (L1). Always visible.
2. **Universal search bar** — takes up ~50% of nav width. Placeholder: "Search or ask anything...". Activates on click or `Ctrl+K` / `Cmd+K`.
3. **Notification bell** — icon button with badge count (critical alerts only). Opens notification panel.
4. **User avatar / profile** — opens profile dropdown (profile settings, organisation settings, sign out).

### Notification bell rules
- Badge shows count of **unread critical notifications only**
- Does NOT count: ambient updates, AI suggestions, sync completions
- Badge disappears when all critical notifications are viewed

### Mobile behaviour
- Logo only (no text)
- Search bar collapses to search icon (opens full-screen search on tap)
- Notification bell remains
- Profile avatar remains
- Navigation is bottom tab bar on mobile (not top)
