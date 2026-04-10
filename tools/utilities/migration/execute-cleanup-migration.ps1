# Execute Cleanup Migration - Fixed Script
# Purpose: Execute the complete migration in one go
# Usage: .\execute-cleanup-migration.ps1 -ConfigFile "path-to-config.json"

param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile
)

# Initialize logging
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$LogFile = "agents\workspace\logs\migration-log-$timestamp.log"
$ErrorLog = "agents\workspace\logs\migration-errors-$timestamp.log"

# Ensure log directories exist
New-Item -ItemType Directory -Path "agents\workspace\logs" -Force | Out-Null
New-Item -ItemType Directory -Path "legacy\migration-archives" -Force | Out-Null

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$logTimestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR"){"Red"} elseif($Level -eq "WARN"){"Yellow"} else{"Green"})
    Add-Content -Path $LogFile -Value $logEntry -ErrorAction SilentlyContinue
    if ($Level -eq "ERROR") { Add-Content -Path $ErrorLog -Value $logEntry -ErrorAction SilentlyContinue }
}

Write-Log "🚀 Starting RepoLens Cleanup Migration"
Write-Log "Configuration: $ConfigFile"

# Load configuration
if (-not (Test-Path $ConfigFile)) {
    Write-Log "Configuration file not found: $ConfigFile" "ERROR"
    exit 1
}

try {
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Log "Loaded migration config: $($config.migration_name)"
    Write-Log "Operations count: $($config.operations.Count)"
} catch {
    Write-Log "Failed to parse configuration: $($_.Exception.Message)" "ERROR"
    exit 1
}

# Create backup directory
$backupDir = "legacy\migration-archives\cleanup-$timestamp"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
Write-Log "Created backup directory: $backupDir"

# Execute operations
$successful = @()
$failed = @()

Write-Log "🔄 Executing $($config.operations.Count) operations..."

foreach ($op in $config.operations) {
    try {
        Write-Log "Executing: $($op.action.ToUpper()) $($op.source) -> $($op.destination)"
        
        switch ($op.action.ToLower()) {
            "move" {
                if (Test-Path $op.source) {
                    # Create backup first
                    $backupPath = Join-Path $backupDir (Split-Path $op.source -Leaf)
                    if (Test-Path $op.source) {
                        Copy-Item $op.source $backupPath -Recurse -Force -ErrorAction SilentlyContinue
                        Write-Log "Backed up: $($op.source) -> $backupPath"
                    }
                    
                    # Ensure destination directory exists
                    $destDir = Split-Path $op.destination -Parent
                    if ($destDir -and -not (Test-Path $destDir)) {
                        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                        Write-Log "Created directory: $destDir"
                    }
                    
                    # Move the item
                    Move-Item $op.source $op.destination -Force
                    $successful += $op
                    Write-Log "✅ Moved: $($op.source) -> $($op.destination)"
                } else {
                    Write-Log "Source not found: $($op.source)" "WARN"
                }
            }
            "archive" {
                if (Test-Path $op.source) {
                    $archivePath = Join-Path "legacy\archived-files" (Get-Date -Format 'yyyy-MM-dd')
                    New-Item -ItemType Directory -Path $archivePath -Force | Out-Null
                    $archivedName = "$(Split-Path $op.source -Leaf)-archived-$timestamp"
                    $finalArchivePath = Join-Path $archivePath $archivedName
                    Move-Item $op.source $finalArchivePath -Force
                    $successful += $op
                    Write-Log "✅ Archived: $($op.source) -> $finalArchivePath"
                } else {
                    Write-Log "Source not found for archive: $($op.source)" "WARN"
                }
            }
            "update_file" {
                if (Test-Path $op.source) {
                    # Backup original file first
                    $backupPath = "$($op.source).backup-$timestamp"
                    Copy-Item $op.source $backupPath -Force
                    
                    $content = Get-Content $op.source -Raw
                    foreach ($replacement in $op.replacements) {
                        $content = $content -replace $replacement.find, $replacement.replace
                        Write-Log "Applied replacement in $($op.source): '$($replacement.find)' -> '$($replacement.replace)'"
                    }
                    Set-Content $op.source $content
                    $successful += $op
                    Write-Log "✅ Updated file: $($op.source) (backup: $backupPath)"
                }
            }
            "delete" {
                if (Test-Path $op.source) {
                    # Always backup before delete
                    $backupPath = Join-Path $backupDir (Split-Path $op.source -Leaf)
                    Copy-Item $op.source $backupPath -Recurse -Force -ErrorAction SilentlyContinue
                    Remove-Item $op.source -Recurse -Force
                    $successful += $op
                    Write-Log "✅ Deleted: $($op.source) (backed up to $backupPath)"
                }
            }
            default {
                Write-Log "Unknown action: $($op.action)" "ERROR"
                $failed += $op
            }
        }
    }
    catch {
        Write-Log "Operation failed: $($op.action) $($op.source) - $($_.Exception.Message)" "ERROR"
        $failed += $op
    }
}

# Generate completion report
$report = @{
    migration_name = $config.migration_name
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    operations_total = $config.operations.Count
    operations_successful = $successful.Count
    operations_failed = $failed.Count
    backup_location = $backupDir
    log_file = $LogFile
    error_log = $ErrorLog
}

$reportPath = "agents\outputs\reports\ad-hoc-reports\cleanup-migration-report-$timestamp.json"
New-Item -ItemType Directory -Path (Split-Path $reportPath -Parent) -Force | Out-Null
$report | ConvertTo-Json -Depth 3 | Set-Content $reportPath

Write-Log "==================== MIGRATION COMPLETED ===================="
Write-Log "📊 Results Summary:"
Write-Log "  ✅ Successful operations: $($successful.Count)"
Write-Log "  ❌ Failed operations: $($failed.Count)"
Write-Log "  📁 Backup location: $backupDir"
Write-Log "  📋 Report: $reportPath"
Write-Log "  📝 Logs: $LogFile"

if ($failed.Count -gt 0) {
    Write-Log "⚠️  Migration completed with $($failed.Count) failures" "WARN"
    Write-Log "Error details: $ErrorLog" "WARN"
    exit 1
} else {
    Write-Log "🎉 MIGRATION COMPLETED SUCCESSFULLY!"
    Write-Log "Your RepoLens project has been transformed to enterprise structure!"
    exit 0
}
