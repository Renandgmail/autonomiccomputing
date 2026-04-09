# Cline Standing Instructions - RepoLens Project

**Status**: ACTIVE - Apply to ALL development activities  
**Priority**: CRITICAL - Non-negotiable for all code changes  

---

## 🎯 PRIMARY DIRECTIVE: ZERO FUNCTIONALITY REGRESSION

Before ANY code change: **STOP → ASSESS → ANALYZE → CONFIRM → PROCEED**

### Mandatory Pre-Change Assessment
1. **Identify Current Functionality** - What features currently work?
2. **Map Dependencies** - What other components depend on this?
3. **Document Baseline** - Record current behavior and user experience
4. **Risk Classification**:
   - **GREEN**: No functionality affected (safe to proceed)
   - **YELLOW**: Potential minor impact (requires mitigation)
   - **RED**: Definite functionality impact (requires user approval)

### Impact Detection Protocol
When ANY functionality impact is detected:

```
⚠️  FUNCTIONALITY IMPACT DETECTED ⚠️

AFFECTED COMPONENTS: [List components]
CURRENT WORKING FEATURES AT RISK: [List features]
PROPOSED CHANGES: [Detail changes]
POTENTIAL DEGRADATION: [List risks]
USER IMPACT: [Describe user experience impact]
MITIGATION STRATEGY: [Preservation plan]
ROLLBACK PLAN: [Reversion steps]

⚠️ EXPLICIT USER APPROVAL REQUIRED ⚠️
```

## 🏗️ DEVELOPMENT WORKFLOW

### Action Item Format (Ultra-Concise)
```
**ID**: [Unique identifier]
**Action**: [Specific task - max 50 chars]
**Files**: [Exact file paths]
**Time**: [Estimated hours]
**Status**: [TODO/PROGRESS/DONE]
```

### Protected Domains (Never modify without approval)
- Repository Details Analytics Dashboard
- Working API endpoints
- Data visualization components
- Authentication flows
- Database schema (without migration)

### Quality Gates (All Required)
- [ ] Functionality impact assessed
- [ ] Risk classification determined
- [ ] User approval obtained (if RED risk)
- [ ] Existing functionality tested
- [ ] Regression tests passed

## 🎯 PROJECT-SPECIFIC RULES

### Current Status (Reference)
- **L1/L2 Dashboards**: Production ready
- **Backend**: 13 CS0854 compilation errors need fixing
- **L3 Code Graph**: Needs professional static layout (no animations)
- **AST Integration**: Database schema and services need implementation

### Immediate Priorities
1. **Fix Backend Compilation** - Replace `default` with `It.IsAny<CancellationToken>()` in tests
2. **AST Database Integration** - Implement missing schema and repository services
3. **Professional Code Graph** - Replace force-graph with 3-panel static layout

### Token Management Rules
- Keep action items under 100 words each
- Use bullet points instead of paragraphs
- Reference files by exact path only
- Consolidate related actions into single items

## 📋 MINIMAL ACTION FORMAT

```markdown
### Priority X: [Brief Description]
**Files**: `exact/file/path.ext`
**Action**: [One sentence description]
**Fix**: [Specific change needed]
**Time**: [Hours estimate]
**Blocker**: [Yes/No]
```

## 🚨 ESCALATION TRIGGERS
- ANY existing functionality might be affected
- User workflows could be disrupted
- API contract changes required
- Database schema breaking changes needed

**Remember**: Enhance the system while preserving ALL existing value. When in doubt, protect functionality first.
