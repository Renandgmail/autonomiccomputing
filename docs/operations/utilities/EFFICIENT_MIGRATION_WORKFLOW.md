# Efficient Migration Workflow - Universal Migration Engine

## Overview

This document describes the token-efficient migration workflow that eliminates back-and-forth operations and automates enterprise-grade file migrations with validation, rollback, and archiving.

## 🎯 Problem Solved

**Before**: 15+ back-and-forth messages, token waste, manual verification
**After**: 1-2 messages, complete automation, enterprise safety features

## 🔧 Components

### 1. Universal Migration Engine
**Location**: `tools/utilities/migration/universal-migration-engine.ps1`

**Features**:
- ✅ **JSON-driven operations** - No hardcoded paths
- ✅ **Automatic archiving** - All source files archived with timestamps
- ✅ **Build validation** - Tests .NET, NPM, Docker builds automatically
- ✅ **Rollback capability** - Auto-restore on validation failure
- ✅ **Complete logging** - Timestamped logs with error details
- ✅ **Metadata tracking** - File hashes and operation history
- ✅ **Dry-run mode** - Preview operations without changes

### 2. Configuration Schema
**Location**: `tools/utilities/migration/repolens-migration-config.json`

**Operation Types**:
- `move` - Move files/folders with automatic archiving
- `copy` - Copy files/folders 
- `archive` - Move to legacy with timestamp
- `delete` - Delete (requires force_delete=true)
- `update_file` - Text replacement with backup

### 3. Configuration Generator Template
**Location**: `agents/templates/migration-config-generator-prompt.md`

**Purpose**: Generates complete JSON config from single prompt

## 🚀 Workflow Usage

### Step 1: Generate Configuration
Use the template prompt to analyze project structure and generate JSON:

```
Analyze current project structure and generate complete migration config:

**SOURCE ANALYSIS**: [List files to move]
**TARGET STRUCTURE**: [Define enterprise structure]
**VALIDATION**: [Specify build/test requirements]
**OUTPUT**: Complete JSON config file
```

### Step 2: Execute Migration
```powershell
# Dry run first (recommended)
.\tools\utilities\migration\universal-migration-engine.ps1 -ConfigFile "config.json" -DryRun

# Execute actual migration
.\tools\utilities\migration\universal-migration-engine.ps1 -ConfigFile "config.json"
```

### Step 3: Review Results
- **Report**: `agents\outputs\reports\ad-hoc-reports\migration-report-[timestamp].json`
- **Logs**: `agents\workspace\logs\migration-log-[timestamp].log`
- **Archives**: `legacy\migration-archives\pre-migration-[timestamp]\`

## 📁 Directory Structure Created

```
tools/
├── utilities/
│   └── migration/
│       ├── universal-migration-engine.ps1    # Main migration engine
│       └── repolens-migration-config.json    # Sample configuration
└── dev-tools/                               # Development utilities

agents/
├── templates/
│   └── migration-config-generator-prompt.md  # Config generation template
├── workspace/
│   └── logs/                                 # Migration logs
└── outputs/
    └── reports/
        └── ad-hoc-reports/                   # Migration reports

legacy/
├── migration-archives/                       # Source file archives
├── execution-archives/                       # Config file archives
├── cleanup-archive/                          # Cleanup archives
└── archived-files/                          # General archives
```

## 🔒 Safety Features

### Automatic Archiving
- **Pre-migration backup** of all source files
- **Timestamped archives** with metadata
- **File integrity** tracking with SHA256 hashes
- **Operation history** for audit trails

### Validation & Rollback
- **Build validation** for .NET, NPM, Docker
- **Custom command validation** with exit code checking
- **Automatic rollback** on validation failure
- **Manual rollback** from archived files if needed

### Enterprise Compliance
- **Complete audit trail** with timestamps
- **Error logging** with detailed diagnostics
- **Configuration versioning** with archive retention
- **No data loss** policy with multiple backup layers

## 📊 Example Configuration

```json
{
  "migration_name": "Project Reorganization",
  "operations": [
    {
      "action": "move",
      "source": "src/OldProject",
      "destination": "src/backend/NewProject",
      "description": "Reorganize project structure"
    }
  ],
  "validation": {
    "build_targets": [
      {"type": "dotnet", "path": "src/backend", "description": "Test backend build"}
    ]
  },
  "cleanup": [
    {"action": "delete", "path": "temp", "description": "Remove temp files"}
  ]
}
```

## ⚡ Performance Benefits

| Aspect | Before | After |
|--------|--------|-------|
| Messages | 15-20 back-and-forth | 1-2 total |
| Tokens | 5000+ wasted | <500 efficient |
| Validation | Manual, error-prone | Automatic, reliable |
| Safety | Basic, risky | Enterprise-grade |
| Audit | None | Complete trail |
| Rollback | Manual/difficult | Automatic |

## 🎓 Best Practices

### For Agents
1. **Use the template** - Generate complete config in single prompt
2. **Test with dry-run** - Always preview operations first
3. **Monitor logs** - Check migration reports for issues
4. **Verify archives** - Confirm all files properly archived

### For Configuration
1. **Specific paths** - Use exact source/destination paths
2. **Clear descriptions** - Document each operation purpose
3. **Proper validation** - Include all build/test targets
4. **Safe cleanup** - Archive rather than delete when possible

### For Execution
1. **Backup important data** - Even though engine creates backups
2. **Test in dev environment** - Validate config before production
3. **Monitor disk space** - Archives require additional storage
4. **Review reports** - Check completion status and errors

## 🔧 Troubleshooting

### Common Issues
- **Path not found**: Check source paths exist before migration
- **Build failure**: Ensure all dependencies available in new locations
- **Permission denied**: Run PowerShell as administrator if needed
- **Archive full**: Monitor disk space for archive directories

### Recovery Procedures
1. **Validation failure**: Engine auto-restores from archives
2. **Manual recovery**: Restore from `legacy\migration-archives\pre-migration-[timestamp]`
3. **Config issues**: Review logs in `agents\workspace\logs\`
4. **Partial failure**: Use error logs to identify and fix specific operations

## 📈 Future Enhancements

- **GUI interface** for non-technical users
- **Integration with Git** for automatic commits
- **Cloud storage** archive support
- **Parallel processing** for large migrations
- **AI-powered** conflict resolution

---

**Location**: `docs/operations/utilities/EFFICIENT_MIGRATION_WORKFLOW.md`  
**Version**: 2.0  
**Last Updated**: 2026-04-10  
**Maintained by**: DevOps & Automation Team
