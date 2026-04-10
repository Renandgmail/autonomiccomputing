# Design System

Complete token system, typography, spacing, and responsive rules for RepoLens.

---

## Colours

### Core rule
**Colour encodes meaning only.** Never use colour decoratively. Every colour choice must have a semantic reason. Test: "why this colour?" — the answer must be semantic, not aesthetic.

**Colour alone never conveys information.** Every status colour (green, amber, red) must also have a text label or icon.

### Token reference

```css
:root {
  /* Brand */
  --brand-primary:    #1A2B4A;  /* Headers, primary CTAs, selected states */
  --interactive:      #0F7BFF;  /* Links, active states, focus rings, progress bars */
  
  /* Status — semantic only */
  --status-success:   #16A34A;  /* Excellent health, passing gates, resolved issues */
  --status-warning:   #D97706;  /* Fair health, approaching thresholds, medium severity */
  --status-danger:    #DC2626;  /* Critical health, security vulns, failed gates */
  --status-high:      #EA580C;  /* High severity (between warning and danger) */
  --feature:          #0D9488;  /* AI assistant accent, selected tabs, Good health band */
  
  /* Text */
  --text-primary:     #111827;  /* Headings, primary body text, table data */
  --text-secondary:   #6B7280;  /* Captions, metadata, placeholder text */
  
  /* Surfaces */
  --surface:          #F9FAFB;  /* Page background, table alternating rows */
  --card:             #FFFFFF;  /* Card surfaces, modal backgrounds, dropdowns */
  --border:           #E5E7EB;  /* Card borders, table dividers, input borders */
}
```

### Health band colours (used in RepositoryHealthChip and colour bands table)
| Band | Score | Token / Hex |
|------|-------|-------------|
| Excellent | 90–100% | `--status-success` / #16A34A |
| Good | 70–89% | `--feature` / #0D9488 |
| Fair | 50–69% | `--status-warning` / #D97706 |
| Poor | 30–49% | `--status-high` / #EA580C |
| Critical | 0–29% | `--status-danger` / #DC2626 |

### Severity badge colours
| Level | Background | Text |
|-------|-----------|------|
| Critical | #DC2626 | #FFFFFF |
| High | #EA580C | #FFFFFF |
| Medium | #D97706 | #FFFFFF |
| Low | #6B7280 | #FFFFFF |

### Chart colours
| Meaning | Colour |
|---------|--------|
| Meeting target | `--status-success` |
| Approaching threshold | `--status-warning` |
| Below threshold | `--status-danger` |
| Neutral trend | `--text-secondary` |
| Threshold line | `--interactive` (dashed) |

---

## Typography

**Font families:**
- **IBM Plex Sans** — all UI text (headings, body, labels, captions)
- **IBM Plex Mono** — all code references (file paths, repo names, code snippets, metric values)

Both are open-source and available from Google Fonts / IBM.

### Type scale

| Element | Font | Size | Weight | Line height | Usage |
|---------|------|------|--------|------------|-------|
| Page title (H1) | IBM Plex Sans | 28px | 600 | 1.3 | One per page, identifies the screen |
| Section heading (H2) | IBM Plex Sans | 20px | 600 | 1.4 | Major sections, analytics tabs |
| Panel heading (H3) | IBM Plex Sans | 16px | 500 | 1.4 | Card titles, panel headers |
| Body text | IBM Plex Sans | 14px | 400 | 1.6 | All descriptive text, labels, list items |
| Metric value | IBM Plex Mono | 32px | 500 | 1.2 | Health score, large KPIs on summary cards |
| Code / file path | IBM Plex Mono | 13px | 400 | 1.5 | File paths, repo names, code snippets |
| Caption / metadata | IBM Plex Sans | 12px | 400 | 1.5 | Timestamps, sync status, helper text |

### Rules
- Never go below 12px
- H1 appears once per page only
- Metric values always use IBM Plex Mono (not IBM Plex Sans) to maintain monospace alignment

---

## Spacing

**Base grid: 8px.** All spacing values are multiples of 8px (with 4px as the minimum exception).

```css
:root {
  --space-1: 4px;   /* Icon-to-label gaps, badge padding */
  --space-2: 8px;   /* List item spacing, chip padding, inline element gaps */
  --space-3: 16px;  /* Card internal padding, form field spacing */
  --space-4: 24px;  /* Between cards, section dividers, panel padding */
  --space-5: 32px;  /* Between major sections, page-level padding */
  --space-6: 48px;  /* Page margins, modal padding */
}
```

### Border radius
- Cards, panels, dropdowns: `8px`
- Chips, badges, small pills: `4px`
- Buttons: `6px`
- Input fields: `6px`

---

## Responsive Breakpoints

Four breakpoints. Mobile-first approach.

```css
/* Mobile */
@media (max-width: 767px) { ... }

/* Tablet */
@media (min-width: 768px) and (max-width: 1023px) { ... }

/* Desktop */
@media (min-width: 1024px) and (max-width: 1439px) { ... }

/* Wide desktop */
@media (min-width: 1440px) { ... }
```

### Behaviour per breakpoint

| Feature | Mobile (<768) | Tablet (768–1023) | Desktop (1024–1439) | Wide (1440+) |
|---------|--------------|-------------------|---------------------|--------------|
| Navigation | Bottom tab bar | Icon-only rail sidebar | Full sidebar | Full sidebar |
| Summary cards | Stacked 1-col | 2×2 grid | 4-in-a-row | 4-in-a-row |
| Repository list | Name + health + arrow | 4 columns | Full columns | Full columns (max-width capped) |
| Analytics charts | Full width, single col | 2-col grid | 2-col grid | 2-col grid |
| Code graph | List view instead | Simplified, no drag | Full interactive | Full interactive |
| AI assistant | Bottom sheet | Bottom sheet | Right panel (380px) | Right panel (380px) |
| Content max-width | 100% | 100% | 100% | 1280px centred |

### Mobile-excluded features

These features are **not available on mobile** (show "use desktop" message):
- Code graph interactive view (show list instead)
- Cross-repository comparison
- File detail full view (show summary + "View on desktop" prompt)
- Export functionality (deferred to desktop)

---

## Accessibility

### Colour contrast requirements (WCAG 2.1 AA)
| Text type | Minimum contrast ratio |
|-----------|----------------------|
| Body text (14px+) | 4.5:1 |
| Large text (18px+ or 14px bold) | 3:1 |
| Interactive components | 3:1 |
| Focus indicators | 3:1 |
| Graphical elements conveying info | 3:1 |

### Keyboard navigation
- All interactive elements reachable by `Tab`
- Focus order: top-to-bottom, left-to-right
- Focus ring: minimum 2px outline in `--interactive` colour
- `Escape` closes all modals, overlays, and panels
- `Ctrl+K` / `Cmd+K` → search
- `Ctrl+R` / `Cmd+R` → repository switcher
- Skip navigation link at top of every page

### Screen reader support
- All images and icons: descriptive `alt` or `aria-label`
- Dynamic updates: `aria-live="polite"` for data refreshes, `aria-live="assertive"` for critical errors
- Tables: proper `<th>` with `scope` attributes
- Charts: visually hidden `<table>` with same data as chart
- Status indicators: `role="status"` with descriptive text

### Reduced motion
```css
@media (prefers-reduced-motion: reduce) {
  /* Replace transitions with instant state changes */
  /* Replace spinners with static indicators */
  /* Replace progress bars with discrete step updates */
}
```
All animations respect `prefers-reduced-motion`.
