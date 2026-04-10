# Agent Command Interpreter - Automated Workflow Triggers

## Overview

This document defines how agents can recognize specific commands and automatically execute complex workflows without manual intervention.

## 🎯 Command Recognition System

### Trigger Keywords for Migration/Cleanup
When users say any of these phrases, the agent should automatically initiate the migration workflow:

#### **Primary Cleanup Commands**
- `"cleanup"` 
- `"reorganize"`
- `"migrate structure"`
- `"clean up the codebase"`
- `"organize files"`
- `"restructure the project"`

#### **Structural Analysis Commands**
- `"analyze structure"`
- `"check organization"`
- `"review file structure"`
- `"assess project layout"`

#### **Enterprise Readiness Commands**
- `"enterprise structure"`
- `"production ready"`
- `"professional organization"`
- `"standardize structure"`

## 🤖 Agent Workflow Automation

### When Agent Detects Cleanup Command

#### **Step 1: Automatic Analysis**
Agent should immediately execute:
```markdown
**ANALYZING PROJECT STRUCTURE...**

Scanning current directory structure:
- Source files: [Count and location]
- Documentation: [Current organization]
- Deployment configs: [Location analysis]  
- Legacy items: [Items that need archiving]

**RECOMMENDATION**: Execute enterprise structure migration
```

#### **Step 2: Auto-Generate Configuration**
Agent should use the template from `agents/templates/migration-config-generator-prompt.md` to:
1. Analyze current file structure
2. Generate complete JSON migration config
3. Save to `agents/workspace/migration-configs/auto-cleanup-[timestamp].json`

#### **Step 3: Execute Migration**
Agent should automatically run:
```powershell
# Auto-generated command
powershell -ExecutionPolicy Bypass -File "tools\utilities\migration\universal-migration-engine.ps1" -ConfigFile "agents\workspace\migration-configs\auto-cleanup-[timestamp].json" -DryRun
```

#### **Step 4: Present Results**
Agent should show:
```markdown
**CLEANUP ANALYSIS COMPLETE**

🔍 **Found Issues**:
- [X] files in wrong locations
- [Y] legacy items need archiving
- [Z] missing documentation structure

📋 **Proposed Changes**:
- Move [list of moves]
- Archive [list of archives]  
- Organize [list of organization changes]

🚀 **Ready to Execute**:
Would you like me to:
1. Execute this cleanup now
2. Modify the plan first
3. Save for later execution

**Command**: Say "execute cleanup" to proceed automatically.
```

## 📋 Agent Decision Tree

### User Input Processing
```
User Input → Keyword Detection → Workflow Selection → Auto-Execution

Examples:
"cleanup" → MIGRATE → Auto-analyze + Generate config + Present plan
"organize files" → MIGRATE → Auto-analyze + Generate config + Present plan  
"execute cleanup" → EXECUTE → Run migration engine with config
"rollback" → ROLLBACK → Restore from latest archive
```

## 🛠 Agent Implementation Commands

### For Agents to Recognize

#### **Cleanup Detection Pattern**
```regex
(?i)(cleanup|clean\s*up|reorganize|migrate|structure|organize|enterprise|standardize|production\s*ready).*?(files?|structure|project|codebase|code)
```

#### **Execution Detection Pattern**  
```regex
(?i)(execute|run|start|do|perform).*?(cleanup|migration|reorganization)
```

#### **Analysis Detection Pattern**
```regex
(?i)(analyze|check|review|assess|scan).*?(structure|organization|files?)
```

## 🎯 Automatic Workflow Responses

### When User Says "cleanup"

**Agent Response Template**:
```markdown
🔍 **INITIATING AUTOMATIC PROJECT CLEANUP**

**Step 1: Structure Analysis**
Scanning project for optimization opportunities...

**Step 2: Migration Plan Generation**  
Creating enterprise-grade structure plan...

**Step 3: Safety Validation**
Generating backup and rollback strategy...

**ANALYSIS COMPLETE** ✅

📊 **Current State**:
- [X] source files need reorganization
- [Y] documentation requires structuring  
- [Z] deployment configs scattered

🎯 **Target State**:
- Enterprise-ready folder structure
- Organized documentation hierarchy
- Standardized deployment configuration
- Complete backup and audit trail

**Ready to proceed?** 
- Say **"execute"** to run automatic cleanup
- Say **"preview"** to see detailed changes first
- Say **"customize"** to modify the plan

*Estimated time: 2-3 minutes*
*Risk level: LOW (full rollback available)*
```

### When User Says "execute cleanup"

**Agent Response**:
```markdown
🚀 **EXECUTING AUTOMATIC CLEANUP**

**Phase 1: Creating Backups** ✅
- Archived all source files to legacy/migration-archives/
- Generated metadata and integrity hashes

**Phase 2: File Migration** ✅  
- Moved [X] source files to organized structure
- Updated [Y] configuration references
- Archived [Z] legacy items

**Phase 3: Validation** ✅
- Build tests: PASSED
- Dependency checks: PASSED  
- Integration tests: PASSED

**CLEANUP COMPLETED SUCCESSFULLY** ✅

📊 **Results**:
- [X] files reorganized
- [Y] GB disk space optimized
- [Z] configuration files standardized

📁 **New Structure**:
- `src/backend/` - All .NET projects
- `src/frontend/` - React applications  
- `docs/` - Organized documentation
- `deployment/` - Infrastructure configs
- `tools/` - Development utilities

🔒 **Safety**:
- Backup location: `legacy/migration-archives/cleanup-[timestamp]/`
- Rollback available: `agents/workflows/rollback-cleanup.ps1`
- Audit report: `agents/outputs/reports/cleanup-report-[timestamp].json`

**Project is now enterprise-ready!** 🎉
```

## 🔄 Rollback Command Support

### When User Says "rollback" or "undo cleanup"

**Agent Response**:
```markdown
🔄 **INITIATING ROLLBACK**

Restoring from latest migration archive...

**Rollback Complete** ✅
- Restored [X] files to original locations
- Reverted [Y] configuration changes
- Project returned to pre-cleanup state

Archive preserved at: `legacy/rollback-archives/[timestamp]/`
```

## 📋 Configuration Templates for Auto-Generation

### Smart Analysis Prompts
```markdown
**AUTO-CLEANUP ANALYSIS**

Current directory scan results:
[Agent scans current directory and identifies patterns]

Enterprise structure requirements:
- Source code organization
- Documentation hierarchy  
- Deployment standardization
- Legacy item archival

Generating optimal migration plan...
```

## 🎯 Implementation Instructions for Agents

### Recognition Logic
1. **Parse user input** for cleanup-related keywords
2. **Auto-trigger** structure analysis workflow
3. **Generate** migration configuration automatically
4. **Present** clear summary with execution options
5. **Execute** upon confirmation with full automation

### Safety Protocols
1. **Always create backups** before any changes
2. **Validate builds** after migration
3. **Provide rollback** commands and instructions
4. **Generate reports** for audit trails
5. **Confirm success** with clear status indicators

### User Experience
1. **Single command trigger** - "cleanup" starts everything
2. **Clear progress indicators** - Show what's happening
3. **Safety messaging** - Explain backup and rollback options
4. **Immediate feedback** - Confirm completion with results
5. **Professional output** - Enterprise-ready structure achieved

This system allows users to simply say "cleanup" and have the agent automatically analyze, plan, and execute a complete project reorganization with enterprise-grade safety and automation.
