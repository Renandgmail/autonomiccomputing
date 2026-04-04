# L1 PORTFOLIO DASHBOARD - DETAILED ACTION PLAN

## 🎯 **MISSION: Transform Dashboard for Engineering Manager Success**

**Goal**: Replace current generic dashboard with L1 Portfolio Dashboard that serves Engineering Managers effectively.

**Success Criteria**: Engineering Manager can identify their most critical repository and action within 90 seconds.

---

## 📋 **PHASE 1: CRITICAL L1 DASHBOARD IMPLEMENTATION**

### **Step 1: Backend API Development (Priority 1)**

#### **1.1 Create Portfolio Controller**
```csharp
// File: RepoLens.Api/Controllers/PortfolioController.cs
// Purpose: Engineering Manager focused endpoints
```

**Required Endpoints**:
- `GET /api/portfolio/summary` - Zone 1 metrics
- `GET /api/portfolio/repositories` - Zone 2 repository list
- `GET /api/portfolio/critical-issues` - Zone 3 critical issues
- `POST /api/portfolio/repositories/{id}/star` - Repository starring
- `DELETE /api/portfolio/repositories/{id}/star` - Repository unstarring

#### **1.2 Create Portfolio Models**
```csharp
// File: RepoLens.Api/Models/PortfolioModels.cs
// Purpose: Engineering Manager specific data models
```

**Required Models**:
- `PortfolioSummary` - Zone 1 summary metrics
- `PortfolioRepository` - Zone 2 repository item
- `CriticalIssue` - Zone 3 critical issue item
- `RepositoryHealthBand` - Health categorization

#### **1.3 Create Portfolio Service**
```csharp
// File: RepoLens.Api/Services/PortfolioService.cs
// Purpose: Business logic for portfolio analytics
```

**Required Methods**:
- `GetPortfolioSummaryAsync()` - Calculate summary metrics
- `GetRepositoryListAsync()` - Get repositories sorted by health
- `GetCriticalIssuesAsync()` - Identify critical issues
- `ToggleRepositoryStarAsync()` - Manage favourites

---

### **Step 2: Frontend L1 Dashboard Implementation (Priority 1)**

#### **2.1 Create L1 Portfolio Dashboard Component**
```typescript
// File: repolens-ui/src/components/portfolio/L1PortfolioDashboard.tsx
// Purpose: Replace current Dashboard.tsx with Engineering Manager focused interface
```

**Required Zones**:
- **Zone 1**: Summary Strip (4 metric cards exactly)
- **Zone 2**: Repository List (sortable, filterable)
- **Zone 3**: Critical Issues Panel (conditional display)

#### **2.2 Create Portfolio Components**
```typescript
// Required new components:
// - RepositoryList.tsx (Zone 2)
// - RepositoryRow.tsx (Zone 2 items)
// - CriticalIssuesPanel.tsx (Zone 3)
// - PortfolioFilters.tsx (Zone 2 filters)
```

#### **2.3 Create Portfolio API Service**
```typescript
// File: repolens-ui/src/services/portfolioApiService.ts
// Purpose: Frontend API integration
```

---

### **Step 3: Navigation Integration (Priority 2)**

#### **3.1 Update Routing**
```typescript
// File: repolens-ui/src/App.tsx
// Update routes to use L1 Portfolio Dashboard as home
```

#### **3.2 Update Context Bar**
```typescript
// File: repolens-ui/src/components/layout/ContextBar.tsx
// Show proper L1/L2/L3/L4 breadcrumb hierarchy
```

---

## 🚀 **IMPLEMENTATION SEQUENCE - WEEK 1**

### **Day 1: Backend Foundation**
- [ ] Create PortfolioController.cs with basic structure
- [ ] Create PortfolioModels.cs with all required models
- [ ] Create PortfolioService.cs with method stubs
- [ ] Test compilation and basic endpoints

### **Day 2: Backend Implementation**
- [ ] Implement GetPortfolioSummaryAsync() method
- [ ] Implement GetRepositoryListAsync() with health sorting
- [ ] Implement GetCriticalIssuesAsync() with classification logic
- [ ] Add repository starring functionality

### **Day 3: Frontend Foundation**
- [ ] Create L1PortfolioDashboard.tsx with 3-zone layout
- [ ] Create RepositoryList.tsx component
- [ ] Create CriticalIssuesPanel.tsx component
- [ ] Create portfolioApiService.ts

### **Day 4: Frontend Implementation**
- [ ] Implement Zone 1: Summary Strip with correct metrics
- [ ] Implement Zone 2: Repository list with health sorting
- [ ] Implement Zone 3: Critical issues with conditional display
- [ ] Add filtering and starring functionality

### **Day 5: Integration & Testing**
- [ ] Replace current dashboard route with L1 Portfolio Dashboard
- [ ] Update navigation breadcrumbs
- [ ] Test Engineering Manager workflow
- [ ] Fix any integration issues

---

## 📊 **DETAILED COMPONENT SPECIFICATIONS**

### **Zone 1: Summary Strip**
```typescript
// Exactly 4 metric cards (no more, no less):
// 1. Repository Count (integer)
// 2. Average Health Score (percentage + trend)
// 3. Critical Issues Count (integer, clickable filter)
// 4. Active Teams Count (integer, display only)

// MUST NOT include:
// - Code files count
// - Storage metrics 
// - System health
// - Charts (charts belong in L3)
```

### **Zone 2: Repository List**
```typescript
// Required columns:
// - Status indicator (colored dot)
// - Repository name (linked to L2)
// - Language (primary language badge)
// - Health score (RepositoryHealthChip)
// - Open issues (Critical/High/Medium chips)
// - Last sync (relative time)
// - Actions (⋮ menu: edit, star, remove)

// Required features:
// - Default sort: health score ascending (worst first)
// - Starred repositories at top (above sorted results)
// - Filtering: health band, language, team, has critical issues
// - Actions: star/unstar, navigate to L2, remove repository
```

### **Zone 3: Critical Issues Panel**
```typescript
// Conditional display: only when >= 1 repository has critical issues
// Maximum 5 items, then "See all X critical issues" link
// Each item: repository name + issue description + severity chip
// Clicking item navigates to L2 repository dashboard
// Panel disappears when all critical issues resolved
```

---

## 🎯 **SUCCESS METRICS FOR VALIDATION**

### **L1 Portfolio Dashboard Requirements**
- [ ] Page answers "how healthy is my codebase?" within 10 seconds
- [ ] Repository list defaults to health score ascending (worst first)
- [ ] Favourites always appear above sorted results  
- [ ] Zone 3 hidden when no critical issues exist
- [ ] Zone 3 shows maximum 5 items + "See all" link
- [ ] Clicking repository navigates to L2 dashboard
- [ ] No charts on L1 (charts belong in L3)
- [ ] All breakpoints render correctly
- [ ] WCAG 2.1 AA compliance

### **Engineering Manager Value Test**
- [ ] Can identify worst repository in under 30 seconds
- [ ] Can see critical issues requiring immediate action
- [ ] Can navigate to detailed analysis in 1-2 clicks
- [ ] Gets clear trending indicators (up/down/flat)
- [ ] Total decision time under 90 seconds (North Star metric)

---

## 🔧 **TECHNICAL IMPLEMENTATION DETAILS**

### **Health Score Color Bands (From Requirements)**
```typescript
// Exact color bands from L1_PORTFOLIO_DASHBOARD.md:
const HEALTH_BANDS = {
  EXCELLENT: { range: [90, 100], color: '#16A34A', meaning: 'No action required' },
  GOOD: { range: [70, 89], color: '#0D9488', meaning: 'Monitor, low priority' },
  FAIR: { range: [50, 69], color: '#D97706', meaning: 'Plan improvement sprint' },
  POOR: { range: [30, 49], color: '#EA580C', meaning: 'Immediate attention needed' },
  CRITICAL: { range: [0, 29], color: '#DC2626', meaning: 'Escalate to leadership' }
};
```

### **Critical Issues Classification Logic**
```csharp
// Business rules for critical issue identification:
// - Security vulnerabilities (any severity)
// - Technical debt > 40 hours
// - Test coverage < 80%
// - Health score in CRITICAL band (0-29%)
// - No commits in 30+ days (stale repository)
```

### **Repository Sorting Algorithm**
```csharp
// Primary sort: Starred repositories first
// Secondary sort: Health score ascending (worst first)
// Tertiary sort: Repository name alphabetical
```

---

## 🚀 **IMMEDIATE NEXT STEPS**

### **Start Implementation NOW**:
1. **Create PortfolioController.cs** - Backend foundation
2. **Create PortfolioModels.cs** - Data structures  
3. **Create L1PortfolioDashboard.tsx** - Frontend foundation
4. **Update routing** - Make L1 the home page

### **Files to Create/Modify**:
- `RepoLens.Api/Controllers/PortfolioController.cs` (NEW)
- `RepoLens.Api/Models/PortfolioModels.cs` (NEW) 
- `RepoLens.Api/Services/PortfolioService.cs` (NEW)
- `repolens-ui/src/components/portfolio/L1PortfolioDashboard.tsx` (NEW)
- `repolens-ui/src/components/portfolio/RepositoryList.tsx` (NEW)
- `repolens-ui/src/components/portfolio/CriticalIssuesPanel.tsx` (NEW)
- `repolens-ui/src/services/portfolioApiService.ts` (NEW)
- `repolens-ui/src/App.tsx` (UPDATE)

**Ready to begin implementation! Let's start with the backend foundation.**
