# Cline Agent Standing Instructions - Project Context Discovery

## Overview

This document provides Cline agents with automatic context discovery capabilities to understand project state and available commands in any new session.

## 🎯 New Session Initialization

### When Cline Starts a New Session

#### **Step 1: Automatic Context Discovery**
Upon starting, Cline should automatically scan for and read these key files to understand the project:

1. **Project Type Detection**
   ```
   Look for: package.json, *.sln, *.csproj, docker-compose.yml
   Purpose: Understand what type of project this is
   ```

2. **Agent Capability Discovery**
   ```
   Look for: agents/workflows/command-interpreter.md
   Purpose: Understand available autonomous commands
   ```

3. **Project Structure Assessment**
   ```
   Look for: PROJECT_STRUCTURE_REORGANIZATION.md, REPOLENS_STRUCTURE_GUIDE.md
   Purpose: Understand current organization state and standards
   ```

4. **Available Tools Discovery**
   ```
   Look for: tools/utilities/migration/, agents/tools/automation/
   Purpose: Understand available automation capabilities
   ```

#### **Step 2: Context Briefing**
After scanning, Cline should present a brief overview:

```markdown
🤖 **Cline Agent Initialized**

**Project Type**: .NET + React Enterprise Application (RepoLens)
**Current Structure**: Mixed (needs enterprise organization)
**Available Commands**: 
- `cleanup` - Intelligent project reorganization
- `organize files` - Same as cleanup
- `enterprise structure` - Professional organization

**Autonomous Capabilities Detected**:
✅ Universal Migration Engine
✅ Automatic Structure Analysis  
✅ Enterprise Organization Templates
✅ Build Validation & Rollback
✅ Complete Audit Trails

Ready to assist! Try saying "cleanup" for automatic project organization.
```

## 🔍 Context Discovery Implementation

### Cline's Automatic File Reading Sequence

When Cline starts a new session in ANY directory, it should automatically read:

#### **Priority 1: Agent Instructions (if exists)**
```
1. CLINE-STANDING-INSTRUCTIONS.md (this file)
2. agents/workflows/command-interpreter.md
3. agents/workflows/AUTONOMOUS_CLEANUP_WORKFLOW.md
```

#### **Priority 2: Project Understanding**
```
1. README.md (project overview)
2. PROJECT_STRUCTURE_REORGANIZATION.md (organization plans)
3. REPOLENS_STRUCTURE_GUIDE.md (structure standards)
```

#### **Priority 3: Available Tools**
```
1. tools/utilities/migration/ (check for migration tools)
2. agents/tools/automation/ (check for automation scripts)
3. agents/templates/ (check for available templates)
```

#### **Priority 4: Project State**
```
1. *.sln (check for .NET solution)
2. package.json (check for Node/React projects)
3. docker-compose*.yml (check for containerized setup)
4. Current directory structure (assess organization level)
```

## 🤖 Command Recognition Without Prior Context

### Universal Command Detection

Cline should recognize these commands in ANY new session by:

1. **Keyword Pattern Matching**
   ```regex
   (?i)(cleanup|clean\s*up|reorganize|organize|enterprise|structure|migrate)
   ```

2. **Automatic Tool Discovery**
   ```
   If user says "cleanup" → Check for agents/tools/automation/auto-cleanup-trigger.ps1
   If found → Execute autonomous workflow
   If not found → Offer to create migration tools
   ```

3. **Context-Aware Responses**
   ```
   IF (project has *.csproj OR *.sln OR package.json) 
   AND (files scattered in root)
   AND (cleanup tools exist)
   THEN → Offer autonomous cleanup
   
   ELSE → Explain what cleanup would involve
   ```

### Self-Discovering Command System

#### When User Says "cleanup" in New Session:

**Cline's Response Pattern**:
```markdown
🔍 **Analyzing Project for Cleanup Opportunities...**

**Project Scan Results**:
- Project Type: [Auto-detected from files]
- Current Organization: [Assessment of structure]
- Available Tools: [Check for automation scripts]

**Cleanup Capabilities Found**:
✅ Autonomous Migration Engine: tools/utilities/migration/
✅ Intelligent Analysis: agents/tools/automation/
✅ Enterprise Templates: agents/templates/

**Recommendation**: This project can benefit from enterprise reorganization.

Would you like me to:
1. **Execute automatic cleanup** (analyze + reorganize + validate)
2. **Show what would change** (dry-run analysis)  
3. **Explain the process** (how it works)

*This is safe - complete backup and rollback available*
```

## 📋 Cline Standing Instructions Checklist

### Every New Session - Cline Should:

#### **✅ Phase 1: Discovery (Automatic)**
- [ ] Read `CLINE-STANDING-INSTRUCTIONS.md` (this file) for context
- [ ] Scan for `agents/workflows/command-interpreter.md` for commands
- [ ] Check for `agents/tools/automation/` for available tools
- [ ] Assess current directory structure and project type

#### **✅ Phase 2: Capability Assessment**
- [ ] Identify if cleanup/organization tools are available
- [ ] Understand project type (.NET, React, etc.)
- [ ] Assess current organization level (enterprise-ready or needs work)
- [ ] Load available templates and workflows

#### **✅ Phase 3: User Interaction**
- [ ] Present brief capability summary
- [ ] Be ready to respond to cleanup commands
- [ ] Offer autonomous workflows when appropriate
- [ ] Execute tools with proper safety measures

### Command Response Patterns

#### **When User Says "cleanup"**:
```
1. Execute: agents/tools/automation/auto-cleanup-trigger.ps1
2. Present: Analysis results and recommendations
3. Offer: Execute, Preview, or Customize options
4. Execute: Migration with user approval
5. Report: Complete results with audit trails
```

#### **When User Says "organize" or "enterprise structure"**:
```
Same as cleanup - these are synonymous commands
```

#### **When User Says "execute" (after cleanup analysis)**:
```
1. Execute: auto-cleanup-trigger.ps1 -Execute
2. Monitor: Progress and validation
3. Report: Success with detailed results
```

## 🎯 Project-Agnostic Intelligence

### Works in Any Directory Structure

This system is designed to work regardless of:
- **Project type** (detects .NET, React, Python, etc.)
- **Current organization** (messy root or already organized)
- **Session history** (no prior context needed)
- **File locations** (discovers tools and capabilities)

### Intelligent Defaults

If automation tools aren't found, Cline should:
1. **Offer to create them** based on project type
2. **Explain the benefits** of enterprise organization
3. **Provide manual steps** if user prefers

## 🔒 Safety in New Sessions

### Always Maintain Safety Standards
- **Never execute destructive operations** without explicit approval
- **Always create backups** before any file operations
- **Validate builds** after any structural changes
- **Provide rollback options** for all operations
- **Generate audit trails** for compliance

### User Trust Building
- **Be transparent** about what operations will be performed
- **Explain safety measures** (backups, validation, rollback)
- **Show low risk** with complete recovery options
- **Demonstrate value** with before/after comparisons

---

## 📖 Summary for Cline

**In any new session**:
1. **Auto-discover** project type and available tools
2. **Load context** from key documentation files  
3. **Be ready** to respond to cleanup commands
4. **Execute autonomously** when tools are available
5. **Maintain safety** with backups and validation

**Core principle**: Make complex project organization as simple as saying "cleanup" regardless of session history or prior context.

This enables powerful autonomous workflows while maintaining complete safety and user control.
