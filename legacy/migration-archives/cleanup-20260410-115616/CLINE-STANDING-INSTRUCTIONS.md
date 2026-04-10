# Cline Standing Instructions - Autonomous Cleanup Commands

## 🤖 Auto-Discovery on Session Start

When you start a new session in this directory, automatically:

1. **Read this file** to understand available capabilities
2. **Scan current directory** for project type (*.sln, package.json, docker-compose.yml)  
3. **Check for cleanup tools** (agents/tools/, tools/utilities/)
4. **Assess organization level** (src/, docs/, tests/ folders exist?)

## 🎯 Autonomous Commands Available

### When User Says: **"cleanup"**
**Execute this workflow automatically**:

```powershell
# 1. Auto-analyze project structure
powershell -ExecutionPolicy Bypass -File "agents\tools\automation\auto-cleanup-trigger.ps1"

# 2. Present results and offer execution
# 3. If user approves, execute migration engine
```

### When User Says: **"organize"** or **"enterprise structure"**
**Same as cleanup** - these are synonymous commands

### When User Says: **"execute cleanup"** 
**Run the migration immediately**:
```powershell
powershell -ExecutionPolicy Bypass -File "agents\tools\automation\auto-cleanup-trigger.ps1" -Execute
```

## 📊 Project Context (Auto-Detected)

**Project Type**: .NET + React Enterprise Application (RepoLens)
**Current State**: Mixed organization (needs enterprise structure)
**Available Tools**: 
- ✅ Universal Migration Engine (`tools/utilities/migration/`)
- ✅ Auto-Cleanup Trigger (`agents/tools/automation/`)
- ✅ Project Context Scanner (`agents/tools/discovery/`)

**Cleanup Capabilities**:
- ✅ Intelligent project analysis
- ✅ Enterprise-grade file organization  
- ✅ Automatic build validation
- ✅ Complete backup and rollback
- ✅ Audit trails and reporting

## 🚀 Autonomous Workflow Response

### When user says "cleanup", respond with:

```markdown
🤖 **INITIATING AUTOMATIC PROJECT CLEANUP**

**Analyzing current structure...**
- .NET projects: [X] found (need src/backend/ organization)
- React projects: [Y] found (need src/frontend/ organization)  
- Documentation: [Z] files (need docs/ categorization)
- Deployment configs: [A] files (need deployment/ organization)

**Enterprise structure plan generated** ✅
**Safety measures confirmed** ✅ (Complete backup + rollback available)

**Ready to execute?**
- Say "execute" to run automatic cleanup
- Say "preview" to see detailed changes first
- Say "customize" to modify the plan

*Estimated time: 2-3 minutes | Risk: LOW (full rollback available)*
```

### When user says "execute", respond with:

```markdown
🚀 **EXECUTING AUTOMATIC CLEANUP**

**Phase 1: Backup** ✅ - Created: legacy/migration-archives/cleanup-[timestamp]/
**Phase 2: Migration** ✅ - Moved [X] projects to organized structure  
**Phase 3: Validation** ✅ - All builds passing (dotnet + npm + docker)
**Phase 4: Cleanup** ✅ - Removed temporary files

**CLEANUP COMPLETED SUCCESSFULLY** 🎉

**Results**: 
- [X] files reorganized into enterprise structure
- [Y] GB disk space optimized  
- All builds validated and working
- Complete audit trail generated

**New Structure**:
```
src/backend/     # All .NET projects
src/frontend/    # React applications
docs/           # Organized documentation  
deployment/     # Infrastructure configs
tools/          # Development utilities
tests/          # Organized testing
legacy/         # Archived items + backups
```

**Rollback available**: Run `agents/tools/automation/rollback-cleanup.ps1` if needed
```

## 🔍 Detection Patterns

**Trigger cleanup workflow when user input matches**:
```regex
(?i)(cleanup|clean\s*up|reorganize|organize|enterprise|structure|migrate|standardize)
```

**Execute immediately when user says**:
```regex
(?i)(execute|run|start|do|perform).*(cleanup|migration|reorganization)
```

## 🔒 Safety Protocols

**Always before any file operations**:
1. ✅ Create timestamped backup in `legacy/migration-archives/`
2. ✅ Generate file integrity hashes  
3. ✅ Test all builds after changes
4. ✅ Provide clear rollback instructions
5. ✅ Generate detailed audit reports

**Never**:
- ❌ Delete files without archiving first
- ❌ Execute destructive operations without user approval
- ❌ Skip build validation after structural changes
- ❌ Proceed if any safety checks fail

## 📁 Tool Locations (Relative to Current Directory)

- **Migration Engine**: `tools/utilities/migration/universal-migration-engine.ps1`
- **Auto-Cleanup Trigger**: `agents/tools/automation/auto-cleanup-trigger.ps1`  
- **Project Scanner**: `agents/tools/discovery/project-context-scanner.ps1`
- **Configuration Templates**: `agents/templates/`
- **Workflow Documentation**: `agents/workflows/`

## 🎯 Expected User Experience

**User**: "cleanup"
**Agent**: *Automatically analyzes project and presents enterprise organization plan*

**User**: "execute"  
**Agent**: *Automatically reorganizes entire project with validation and reporting*

**Result**: Professional enterprise structure in 2-3 minutes with complete safety

---

## 📖 Quick Reference

**Available Commands**: `cleanup`, `organize`, `enterprise structure`, `execute cleanup`
**Safety Level**: Enterprise-grade (complete backup + validation + rollback)
**Time**: 2-3 minutes for full reorganization
**Risk**: Minimal (full rollback available)

This file enables autonomous cleanup workflows in any new Cline session by providing context and command recognition directly in the working directory.
