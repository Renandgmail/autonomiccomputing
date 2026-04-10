# Migration Configuration Generator Prompt Template

## Purpose
This template helps generate JSON configuration files for the Universal Migration Engine without token-wasting back-and-forth operations.

## Usage Instructions

### Step 1: Analysis Prompt
```
Analyze the current project structure and generate a complete migration configuration JSON file to move:

**SOURCE ANALYSIS:**
- List all folders/files that need to be moved
- Identify file types (code, docs, config, deployment)
- Note dependencies and relationships

**TARGET STRUCTURE:**
- Backend code → src/backend/
- Frontend code → src/frontend/ 
- Tests → tests/[type]/[category]/
- Documentation → docs/[category]/[subcategory]/
- Deployment → deployment/[type]/[environment]/
- Tools/Scripts → tools/[category]/
- Legacy items → legacy/archived-files/

**VALIDATION REQUIREMENTS:**
- dotnet build for .NET projects
- npm build for Node.js projects  
- docker build for containers
- Custom test commands

**OUTPUT REQUIRED:**
Generate complete JSON config file with:
1. All move operations
2. File update operations (solution files, configs)
3. Archive operations for legacy items
4. Build validation targets
5. Cleanup operations

**FORMAT:**
Use the exact JSON schema from: agents/tools/migration/repolens-migration-config.json
```

### Step 2: Execution Prompt
```
Execute the migration using the Universal Migration Engine:

1. Save the generated config as: agents/workspace/migration-configs/[project-name]-migration.json
2. Run dry-run first: powershell agents/tools/migration/universal-migration-engine.ps1 -ConfigFile "agents/workspace/migration-configs/[project-name]-migration.json" -DryRun
3. If dry-run looks good, execute: powershell agents/tools/migration/universal-migration-engine.ps1 -ConfigFile "agents/workspace/migration-configs/[project-name]-migration.json"
4. Check the generated report in: agents/outputs/reports/ad-hoc-reports/

No additional verification needed - the engine handles validation, rollback, and reporting automatically.
```

## Configuration Schema Reference

### Basic Operation Types
```json
{
  "action": "move|copy|archive|delete|update_file",
  "source": "path/to/source",
  "destination": "path/to/destination", 
  "description": "Human readable description"
}
```

### Update File Operation
```json
{
  "action": "update_file",
  "source": "file/to/update",
  "replacements": [
    {
      "find": "regex_pattern_to_find",
      "replace": "replacement_text"
    }
  ]
}
```

### Validation Configuration
```json
{
  "validation": {
    "build_targets": [
      {
        "type": "dotnet|npm|docker",
        "path": "path/to/build",
        "description": "What this validates"
      }
    ],
    "custom_commands": [
      {
        "command": "command to run",
        "description": "What this validates",
        "expected_exit_code": 0
      }
    ]
  }
}
```

### Cleanup Configuration  
```json
{
  "cleanup": [
    {
      "action": "delete|archive",
      "path": "path/to/cleanup",
      "description": "Why this is being cleaned"
    }
  ]
}
```

## Benefits of This Approach

✅ **Zero Token Waste**: Single prompt generates complete config, no back-and-forth
✅ **Automatic Validation**: Engine tests builds and runs custom validation
✅ **Rollback Safety**: Auto-backup before changes, rollback on failure
✅ **Complete Logging**: Full audit trail with timestamps and error details
✅ **Batch Processing**: All operations in single execution
✅ **Enterprise Ready**: Handles complex multi-project migrations

## Example Workflow

1. **Agent receives migration request**
2. **Agent analyzes structure and generates JSON config using this template**
3. **Agent saves config and executes migration engine**
4. **Engine handles all validation, backup, execution, and reporting**
5. **Agent reports success/failure with log locations**

**Total interaction**: 1-2 messages instead of 15+ back-and-forth operations.
