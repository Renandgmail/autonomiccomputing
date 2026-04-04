# RepoLens Implementation Action Plan

## Executive Summary

Based on comprehensive analysis of the repolens-docs specifications and existing codebase, this document provides a step-by-step action plan to transform the current RepoLens application into the specified enterprise repository analytics platform.

**Current State Analysis:**
- Existing React TypeScript frontend with basic routing and authentication
- Material-UI based components (needs migration to specified design system)
- Comprehensive .NET backend API with 15+ controllers
- Basic dashboard and repository management functionality
- Advanced search and analytics features partially implemented

**Target State:** Full RepoLens specification implementation across 4 phases

**Priority:** Maximum reuse of existing code while aligning with specifications

---

## Phase 1: Core Dashboard Foundation (Weeks 1-4)

### 1.1 Design System Migration

**CRITICAL: Must be completed first as foundation for all components**

#### File: `repolens-ui/src/theme/design-system.ts` (NEW)
**Action:** Create the complete design system following `04-design-system/DESIGN_SYSTEM.md`
**Dependencies:** None
**Reuse:** None - completely new implementation required

```typescript
// Color tokens, typography scale, spacing system, responsive breakpoints
// IBM Plex Sans + IBM Plex Mono font configuration
// Health band color mapping
// Semantic color system (status-success, status-warning, etc.)
```

#### File: `repolens-ui/src/App.tsx` (MODIFY)
**Action:** Replace Material-UI theme with RepoLens design system
**Dependencies:** design-system.ts
**Reuse:** Keep authentication logic and routing structure

**Changes Required:**
- Replace Material-UI `createTheme()` with custom design system
- Update color palette to match specification
- Change fonts to IBM Plex Sans/Mono
- Update component overrides for new design tokens

### 1.2 Global Navigation Implementation

#### File: `repolens-ui/src/components/layout/GlobalNavigation.tsx` (REPLACE)
**Action:** Complete rewrite to match specification
**Dependencies:** design-system.ts
**Reuse:** None - existing MainLayout doesn't match L1-L4 spec

**Current Issues:**
- Uses sidebar navigation instead of top global nav
- Missing universal search bar
- Missing repository context switcher
- Missing notification bell with badge
- Wrong layout structure for RepoLens specification

**New Implementation:**
```typescript
// Fixed height 56px top navigation bar
// Logo + Universal search + Repository switcher + Notifications + Profile
// Keyboard shortcuts: Ctrl+K for search, Ctrl+R for repo switcher
// Context-aware search (repo-scoped vs global)
```

#### File: `repolens-ui/src/components/layout/ContextBar.tsx` (NEW)
**Action:** Create persistent context bar for L2+ screens
**Dependencies:** RepositoryHealthChip, design-system.ts
**Reuse:** None - completely new component

```typescript
// Breadcrumb + Repository name/switcher + Health chip + Sync status + Refresh
// Shows below global nav on all L2-L4 screens
// Repository context switching functionality
```

#### File: `repolens-ui/src/components/layout/RepositorySwitcherDropdown.tsx` (NEW)
**Action:** Create repository switcher dropdown
**Dependencies:** RepositoryHealthChip, existing API service
**Reuse:** Repository data from existing `getRepositories()` API

```typescript
// Search input auto-focused
// Groups: Favourites -> Recent -> All repositories link
// Health chips per repository
// Keyboard navigation support
```

### 1.3 Reusable Components

#### File: `repolens-ui/src/components/MetricCard.tsx` (NEW)
**Action:** Create according to specification
**Dependencies:** design-system.ts
**Reuse:** None - completely new

**Implementation:**
```typescript
// 220px fixed width, clickable if onClick provided
// Trend arrow logic with positive/negative direction
// Loading skeleton state
// Accessibility compliance
```

#### File: `repolens-ui/src/components/RepositoryHealthChip.tsx` (NEW)
**Action:** Create according to specification
**Dependencies:** design-system.ts
**Reuse:** None

**Implementation:**
```typescript
// Colored dot + percentage + trend arrow
// Health band color mapping (0-29% Red, 30-49% Orange, etc.)
// Display only component - not interactive
// Color + text (never color alone)
```

#### File: `repolens-ui/src/components/QualityHotspotRow.tsx` (NEW)
**Action:** Create according to specification
**Dependencies:** SeverityBadge, design-system.ts
**Reuse:** None

#### File: `repolens-ui/src/components/SeverityBadge.tsx` (NEW)
**Action:** Create according to specification
**Dependencies:** design-system.ts
**Reuse:** None

### 1.4 Portfolio Dashboard (L1)

#### File: `repolens-ui/src/pages/PortfolioDashboard.tsx` (REPLACE)
**Action:** Complete rewrite of existing Dashboard.tsx
**Dependencies:** All Phase 1 components
**Reuse:** API service methods, basic React Query setup

**Current Issues with `components/dashboard/Dashboard.tsx`:**
- Wrong layout (not L1 specification)
- Missing three-zone layout structure
- No repository list with health sorting
- No critical issues panel
- Material-UI styling instead of design system

**New Implementation:**
- Zone 1: Four MetricCard components (repos, avg health, critical issues, teams)
- Zone 2: Repository table with favourites floating, health sorting
- Zone 3: Conditional critical issues panel
- Responsive grid layout per specification

**API Endpoints Required:**
```
GET /api/portfolio/summary -> 4 metric cards
GET /api/repositories -> Zone 2 table
GET /api/portfolio/critical-issues -> Zone 3 panel
PATCH /api/repositories/:id/favourite -> Star/unstar
```

### 1.5 Repository Dashboard (L2)

#### File: `repolens-ui/src/pages/RepositoryDashboard.tsx` (REPLACE)
**Action:** Rewrite existing RepositoryDetails.tsx to L2 spec
**Dependencies:** All Phase 1 components, ContextBar
**Reuse:** Existing API service methods, tab structure concept

**Current Issues with `RepositoryDetails.tsx`:**
- Has detailed analytics tabs (belongs in L3)
- Missing four-zone L2 layout
- No Quality Hotspots primary panel
- No activity feed
- Wrong navigation breadcrumb structure

**New Implementation:**
- Four zones: Summary Strip + Quality Hotspots + Activity Feed + Quick Actions
- Quality Hotspots as PRIMARY panel (left/main position)
- Activity feed shows quality events, not commits
- Six navigation buttons (non-action buttons)

**API Endpoints Required:**
```
GET /api/repositories/:id/summary -> Zone 1 metrics
GET /api/repositories/:id/hotspots -> Zone 2 ranked list
GET /api/repositories/:id/activity -> Zone 3 quality events
```

### 1.6 Onboarding Flow

#### File: `repolens-ui/src/pages/Onboarding.tsx` (NEW)
**Action:** Create empty state onboarding flow
**Dependencies:** Repository connection API
**Reuse:** Some logic from existing AddRepositoryDialog

**Implementation:**
- Step 1: Provider selection (GitHub/GitLab/etc.)
- Step 2: Analysis progress with SignalR
- Step 3: Redirect to L2 with tooltip

### 1.7 Route Structure Update

#### File: `repolens-ui/src/App.tsx` (MODIFY ROUTES)
**Action:** Update routing to match L1-L4 specification
**Dependencies:** All new page components
**Reuse:** Authentication wrapper logic

**New Route Structure:**
```
/ -> PortfolioDashboard (L1)
/repos/:repoId -> RepositoryDashboard (L2)
/repos/:repoId/analytics -> Analytics (L3) - Phase 2
/repos/:repoId/search -> Universal Search (L3) - Phase 2
/repos/:repoId/graph -> Code Graph (L3) - Phase 3
/repos/:repoId/team -> Team Analytics (L3) - Phase 3
/repos/:repoId/files/:fileId -> File Detail (L4) - Phase 2
/search -> Global Universal Search (L3) - Phase 2
```

---

## Phase 2: Analytics & Search (Weeks 5-8)

### 2.1 Universal Search

#### File: `repolens-ui/src/pages/UniversalSearch.tsx` (NEW)
**Action:** Create L3 universal search implementation
**Dependencies:** design-system.ts, existing NaturalLanguageSearch component
**Reuse:** Significant reuse from `components/search/NaturalLanguageSearch.tsx`

**Existing Asset Analysis:**
- `NaturalLanguageSearch.tsx` has sophisticated search interface
- Includes intent processing and demo fallback logic
- Has search suggestions and filters functionality
- API service includes comprehensive search methods

**Modification Required:**
- Adapt to three-tab structure (Files/Contributors/Metrics)
- Add scope toggle (repository vs global)
- Implement filter chips pattern
- Add keyboard shortcuts (Ctrl+K)
- Update styling to design system

#### File: `repolens-ui/src/pages/Analytics.tsx` (MODIFY)
**Action:** Transform existing Analytics component to L3 specification
**Dependencies:** Recharts, existing analytics components
**Reuse:** High reuse - existing component structure mostly compatible

**Existing Asset Analysis:**
- Current `Analytics.tsx` exists with some structure
- Has file metrics dashboard via `FileMetricsDashboard`
- Has contributor analytics via `ContributorAnalytics`
- Has security analytics via `SecurityAnalytics`

**Required Changes:**
- Add five-tab structure (Trends/Files/Team/Security/Compare)
- Implement Trends tab with Recharts
- Ensure no page reload on tab switching
- Add time range picker
- Privacy controls for Team tab

### 2.2 File Detail View (L4)

#### File: `repolens-ui/src/pages/FileDetail.tsx` (NEW)
**Action:** Create L4 file detail page
**Dependencies:** Metrics components, issue components
**Reuse:** Some metric display logic from existing analytics

**Implementation:**
- Two-column layout (metrics panel + issues list)
- Extended breadcrumb with file path
- Dependency links to other files
- Inline issue status updates

### 2.3 Export Functionality

#### File: `repolens-ui/src/services/exportService.ts` (NEW)
**Action:** Create export service for CSV/PDF reports
**Dependencies:** API service
**Reuse:** None

**Implementation:**
- CSV export for data tables
- PDF export for comprehensive reports
- Progress indicators for long exports

---

## Phase 3: Advanced Features (Weeks 9-12)

### 3.1 Code Graph

#### File: `repolens-ui/src/pages/CodeGraph.tsx` (REPLACE)
**Action:** Enhance existing CodeGraphVisualization to L3 spec
**Dependencies:** D3.js, existing code graph component
**Reuse:** High reuse - existing `CodeGraphVisualization.tsx` is sophisticated

**Existing Asset Analysis:**
- `CodeGraphVisualization.tsx` already exists with D3 implementation
- Has interactive features and good performance
- Includes filtering and layout options

**Required Enhancements:**
- Add three layout options (hierarchical/force/circular)
- Implement node detail panel
- Add circular dependency detection overlay
- Add orphaned node detection overlay
- Mobile fallback to list view
- Performance optimization for 500+ nodes

### 3.2 Team Analytics with Privacy

#### File: `repolens-ui/src/pages/TeamAnalytics.tsx` (NEW)
**Action:** Create privacy-compliant team analytics
**Dependencies:** Contributor analytics API, RBAC service
**Reuse:** Logic from existing `ContributorAnalytics.tsx`

**Implementation:**
- Aggregate view by default
- Individual view requires explicit click
- RBAC enforcement at API level
- No rankings or performance labels
- Audit logging for individual data access

### 3.3 Analytics Compare Tab

#### File: `repolens-ui/src/pages/analytics/CompareTab.tsx` (NEW)
**Action:** Create cross-repository comparison
**Dependencies:** Repository metrics API
**Reuse:** Some display logic from existing analytics

---

## Phase 4: AI Assistant & Polish (Weeks 13-16)

### 4.1 AI Assistant Overlay

#### File: `repolens-ui/src/components/AIAssistant/AIAssistantOverlay.tsx` (NEW)
**Action:** Create context-aware AI assistant
**Dependencies:** AI API endpoints, context hooks
**Reuse:** None - completely new feature

**Implementation:**
- Floating button on all screens
- Right panel on desktop, bottom sheet on mobile
- Context awareness per screen
- Streaming token rendering
- Client-side refusal for code generation

### 4.2 Performance Optimization

**Action:** Full performance audit and optimization
**Dependencies:** All existing components
**Focus Areas:**
- Repository context switching < 500ms
- Chart rendering performance
- Code graph virtualization
- React Query cache optimization

### 4.3 Accessibility Audit

**Action:** Third-party accessibility audit and remediation
**Dependencies:** All components
**Requirements:**
- WCAG 2.1 AA compliance
- Screen reader testing
- Keyboard navigation
- Focus management

### 4.4 Internationalization

#### File: `repolens-ui/src/locales/` (NEW)
**Action:** Set up i18n infrastructure
**Dependencies:** react-i18next
**Phase 4 Scope:**
- English baseline (en-GB.json)
- Date/number formatting with Intl
- RTL layout support
- String externalization

---

## API Endpoint Requirements

### Phase 1 APIs (Backend Priority)
```
GET /api/portfolio/summary
GET /api/portfolio/critical-issues
GET /api/repositories (EXISTING - enhance with health scores)
PATCH /api/repositories/:id/favourite (NEW)
GET /api/repositories/:id/summary
GET /api/repositories/:id/hotspots
GET /api/repositories/:id/activity
```

### Phase 2 APIs
```
GET /api/search (EXISTING - enhance)
GET /api/repositories/:id/analytics/trends
GET /api/repositories/:id/analytics/files
GET /api/repositories/:id/analytics/security
GET /api/repositories/:id/files/:fileId
```

### Phase 3 APIs
```
GET /api/repositories/:id/graph (EXISTING - enhance)
GET /api/repositories/:id/team/aggregate
GET /api/repositories/:id/team/contributors/:id
GET /api/analytics/compare
```

### Phase 4 APIs
```
POST /api/ai/chat (NEW)
```

---

## Reusable Assets Inventory

### High Reuse Potential
1. **API Service** (`apiService.ts`) - Comprehensive, well-structured
2. **Search Components** - `NaturalLanguageSearch.tsx` very sophisticated
3. **Code Graph** - `CodeGraphVisualization.tsx` advanced D3 implementation
4. **Analytics Components** - Good foundation for enhancement
5. **Authentication System** - Solid foundation
6. **Backend Controllers** - 15 controllers provide good API coverage

### Moderate Reuse Potential
1. **Repository Details** - Structure usable but needs L2 alignment
2. **Dashboard** - Basic concepts but needs L1 redesign
3. **Main Layout** - Navigation concept but wrong structure

### No Reuse (Replace Required)
1. **Design System** - Complete Material-UI to custom system migration
2. **Global Navigation** - Sidebar vs top nav fundamental difference
3. **Portfolio Dashboard** - Current dashboard not L1 specification

---

## Risk Mitigation

### High Risk Items
1. **Design System Migration** - Affects all components, must be done first
2. **Navigation Architecture** - Fundamental change from sidebar to top nav
3. **Performance Targets** - Aggressive sub-second loading requirements

### Mitigation Strategies
1. **Incremental Migration** - Phase-based approach with QA gates
2. **Parallel Development** - Design system first, then components
3. **Fallback Patterns** - Maintain existing APIs during transition
4. **Performance Budget** - Continuous monitoring throughout development

---

## Success Metrics

### Phase 1
- [ ] Portfolio Dashboard loads < 1500ms
- [ ] Repository context switch < 1000ms
- [ ] All 4 breakpoints working
- [ ] WCAG 2.1 AA compliance

### Phase 2
- [ ] Search results < 400ms
- [ ] Analytics charts render < 600ms
- [ ] File detail view complete

### Phase 3
- [ ] Code graph renders < 1500ms for 500 nodes
- [ ] Team analytics privacy compliance
- [ ] Cross-repo comparison functional

### Phase 4
- [ ] AI assistant context awareness working
- [ ] Performance targets all met
- [ ] Full accessibility audit pass
- [ ] i18n infrastructure complete

---

This action plan prioritizes maximum reuse of existing sophisticated components while ensuring alignment with the RepoLens specification. The phased approach allows for incremental delivery and risk mitigation while building toward the complete enterprise repository analytics platform.
