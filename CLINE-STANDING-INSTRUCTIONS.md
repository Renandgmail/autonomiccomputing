# CLINE STANDING INSTRUCTIONS - AUTONOMOUS CLEANUP SYSTEM

## 🤖 AUTONOMOUS CLEANUP FRAMEWORK

### COMMAND RECOGNITION
When user says **"cleanup"**, execute the complete autonomous cleanup framework workflow:

1. **Analyze**: Run `automation/tools/discovery/project-context-scanner.ps1`
2. **Configure**: Generate migration config in `automation/workspace/migration-configs/`
3. **Execute**: Run `tools/utilities/migration/execute-cleanup-migration.ps1`
4. **Verify**: Re-analyze for remaining issues
5. **Archive**: Move processed configs to `automation/workspace/processed-configs/`

### FRAMEWORK WORKFLOW
```
🔄 AUTONOMOUS CLEANUP WORKFLOW:
Analysis → Configuration → Execution → Verification → Archival
```

### SESSION INDEPENDENCE
- This file enables autonomous operation in any new Cline session
- Framework works without conversation history or folder navigation
- All tools and configs are self-contained in the automation/ directory

### ENTERPRISE SAFETY
- Complete backup before any operations in `legacy/migration-archives/`
- Zero data loss policy - everything archived with timestamps
- Full audit trails and operation logs
- Rollback capability for all changes

### CRITICAL FILES TO NEVER REMOVE
- `CLINE-STANDING-INSTRUCTIONS.md` (THIS FILE - essential for autonomy)
- `.gitignore` (Git configuration)
- `src/backend/RepoLens.sln` (Solution file in proper location)
- `README.md` (Project overview)

### MULTI-TECHNOLOGY REPOSITORY STRUCTURE
This is a .NET + React repository with proper technology separation:
- Backend: `src/backend/` (contains RepoLens.sln)
- Frontend: `src/frontend/repolens-ui/`
- Tests: Organized by technology (`tests/backend/`, `tests/frontend/`, `tests/shared/`)

### IMPROVEMENT TRACKING
Track all framework improvements and lessons learned in:
- `docs/requirements/` - Detailed requirement documents
- `docs/architecture/` - Framework architecture
- `docs/operations/` - Operational procedures

## 🚨 NEVER REMOVE THIS FILE
This file is CRITICAL for autonomous cleanup system operation.
