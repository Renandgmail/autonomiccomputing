# Universal Migration Engine - Enterprise Utility
# Purpose: Execute bulk file operations with validation, rollback, and automatic archiving
# Location: tools/utilities/migration/
# Usage: .\universal-migration-engine.ps1 -ConfigFile "migration-config.json"

param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile,
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

# Initialize logging with timestamp
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$LogFile = "agents\workspace\logs\migration-log-$timestamp.log"
$ErrorLog = "agents\workspace\logs\migration-errors-$timestamp.log"

# Ensure log directories exist
New-Item -ItemType Directory -Path "agents\workspace\logs" -Force | Out-Null
New-Item -ItemType Directory -Path "legacy\migration-archives" -Force | Out-Null
New-Item -ItemType Directory -Path "legacy\execution-archives" -Force | Out-Null

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$logTimestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR"){"Red"} elseif($Level -eq "WARN"){"Yellow"} else{"Green"})
    Add-Content -Path $LogFile -Value $logEntry
    if ($Level -eq "ERROR") { Add-Content -Path $ErrorLog -Value $logEntry }
}

function Archive-SourceFiles {
    param([array]$Operations, [string]$ArchiveReason = "migration")
    
    $archiveDir = "legacy\migration-archives\$ArchiveReason-$timestamp"
    New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null
    
    Write-Log "Archiving source files to: $archiveDir"
    $archivedFiles = @()
    
    foreach ($op in $Operations) {
        if ($op.action -eq "move" -and (Test-Path $op.source)) {
            try {
                $sourceItem = Get-Item $op.source
                $archiveName = "$($sourceItem.Name)-archived-$timestamp"
                $archivePath = Join-Path $archiveDir $archiveName
                
                # Create archive metadata
                $metadata = @{
                    original_path = $op.source
                    destination_path = $op.destination
                    archived_timestamp = $timestamp
                    archive_reason = $ArchiveReason
                    operation_description = $op.description
                    file_hash = if($sourceItem.PSIsContainer) { "DIRECTORY" } else { (Get-FileHash $op.source -Algorithm SHA256).Hash }
                }
                
                # Copy to archive (don't move yet, wait for validation)
                Copy-Item $op.source $archivePath -Recurse -Force
                
                # Save metadata
                $metadataPath = "$archivePath.metadata.json"
                $metadata | ConvertTo-Json | Set-Content $metadataPath
                
                $archivedFiles += @{
                    Source = $op.source
                    Archive = $archivePath
                    Metadata = $metadata
                }
                
                Write-Log "Archived: $($op.source) -> $archiveName"
            }
            catch {
                Write-Log "Failed to archive $($op.source): $($_.Exception.Message)" "ERROR"
            }
        }
    }
    
    return $archivedFiles
}

function Test-BuildAfterMove {
    param([array]$BuildTargets)
    Write-Log "Running build validation..."
    $buildResults = @()
    
    foreach ($target in $BuildTargets) {
        try {
            Write-Log "Testing build: $($target.type) at $($target.path)"
            switch ($target.type.ToLower()) {
                "dotnet" {
                    if (Test-Path $target.path) {
                        $result = dotnet build $target.path --no-restore --verbosity quiet 2>&1
                        $success = $LASTEXITCODE -eq 0
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$success; Output=$result}
                        Write-Log "Build result for $($target.path): $(if($success){'SUCCESS'}else{'FAILED'})"
                    } else {
                        Write-Log "Build target not found: $($target.path)" "WARN"
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$false; Output="Path not found"}
                    }
                }
                "npm" {
                    if (Test-Path $target.path) {
                        Push-Location $target.path
                        $result = npm run build 2>&1
                        $success = $LASTEXITCODE -eq 0
                        Pop-Location
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$success; Output=$result}
                        Write-Log "NPM build result for $($target.path): $(if($success){'SUCCESS'}else{'FAILED'})"
                    } else {
                        Write-Log "NPM build target not found: $($target.path)" "WARN"
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$false; Output="Path not found"}
                    }
                }
                "docker" {
                    if (Test-Path $target.path) {
                        $result = docker build -t "migration-test-$timestamp" $target.path 2>&1
                        $success = $LASTEXITCODE -eq 0
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$success; Output=$result}
                        Write-Log "Docker build result for $($target.path): $(if($success){'SUCCESS'}else{'FAILED'})"
                        # Clean up test image
                        if ($success) {
                            docker rmi "migration-test-$timestamp" 2>&1 | Out-Null
                        }
                    } else {
                        Write-Log "Docker build target not found: $($target.path)" "WARN"
                        $buildResults += @{Path=$target.path; Type=$target.type; Success=$false; Output="Path not found"}
                    }
                }
            }
        }
        catch {
            $buildResults += @{Path=$target.path; Type=$target.type; Success=$false; Output=$_.Exception.Message}
            Write-Log "Build test exception for $($target.path): $($_.Exception.Message)" "ERROR"
        }
    }
    
    $failedBuilds = $buildResults | Where-Object { -not $_.Success }
    if ($failedBuilds.Count -gt 0) {
        Write-Log "Build validation failed for $($failedBuilds.Count) targets" "ERROR"
        $failedBuilds | ForEach-Object { Write-Log "Failed: $($_.Path) - $($_.Output)" "ERROR" }
        return $false
    }
    
    Write-Log "All build targets validated successfully"
    return $true
}

function Execute-Migration {
    Write-Log "Starting Universal Migration Engine v2.0"
    Write-Log "Log file: $LogFile"
    
    # Load configuration
    if (-not (Test-Path $ConfigFile)) {
        Write-Log "Configuration file not found: $ConfigFile" "ERROR"
        exit 1
    }
    
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Log "Loaded migration config: $($config.migration_name)"
    Write-Log "Operations count: $($config.operations.Count)"
    
    if ($DryRun) {
        Write-Log "DRY RUN MODE - No actual changes will be made" "WARN"
        Write-Log "Operations that would be executed:"
        foreach ($op in $config.operations) {
            Write-Log "  $($op.action.ToUpper()): $($op.source) -> $($op.destination)" "WARN"
        }
        
        # Archive the config file used for dry run
        $dryRunArchive = "legacy\execution-archives\dryrun-config-$timestamp.json"
        Copy-Item $ConfigFile $dryRunArchive
        Write-Log "Dry-run config archived: $dryRunArchive"
        return
    }
    
    # Pre-migration archive of source files
    Write-Log "Creating pre-migration archive..."
    $moveOperations = $config.operations | Where-Object { $_.action -eq "move" }
    $archivedFiles = Archive-SourceFiles $moveOperations "pre-migration"
    
    # Execute operations
    $successful = @()
    $failed = @()
    
    Write-Log "Executing $($config.operations.Count) operations..."
    foreach ($op in $config.operations) {
        try {
            Write-Log "Executing: $($op.action.ToUpper()) $($op.source) -> $($op.destination)"
            
            # Ensure destination directory exists
            if ($op.destination -and $op.action -ne "delete") {
                $destDir = Split-Path $op.destination -Parent
                if ($destDir -and -not (Test-Path $destDir)) {
                    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                    Write-Log "Created directory: $destDir"
                }
            }
            
            switch ($op.action.ToLower()) {
                "move" {
                    if (Test-Path $op.source) {
                        Move-Item $op.source $op.destination -Force
                        $successful += $op
                        Write-Log "✓ Moved: $($op.source) -> $($op.destination)"
                    } else {
                        Write-Log "Source not found: $($op.source)" "WARN"
                    }
                }
                "copy" {
                    if (Test-Path $op.source) {
                        Copy-Item $op.source $op.destination -Recurse -Force
                        $successful += $op
                        Write-Log "✓ Copied: $($op.source) -> $($op.destination)"
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
                        Write-Log "✓ Archived: $($op.source) -> $finalArchivePath"
                    } else {
                        Write-Log "Source not found for archive: $($op.source)" "WARN"
                    }
                }
                "delete" {
                    if ($op.force_delete -eq $true) {
                        if (Test-Path $op.source) {
                            Remove-Item $op.source -Recurse -Force
                            $successful += $op
                            Write-Log "✓ Deleted: $($op.source)"
                        }
                    } else {
                        Write-Log "Delete operation requires force_delete=true in config" "WARN"
                    }
                }
                "update_file" {
                    if (Test-Path $op.source) {
                        # Backup original file first
                        $backupPath = "$($op.source).backup-$timestamp"
                        Copy-Item $op.source $backupPath
                        
                        $content = Get-Content $op.source -Raw
                        foreach ($replacement in $op.replacements) {
                            $content = $content -replace $replacement.find, $replacement.replace
                            Write-Log "Applied replacement: '$($replacement.find)' -> '$($replacement.replace)'"
                        }
                        Set-Content $op.source $content
                        $successful += $op
                        Write-Log "✓ Updated file: $($op.source) (backup: $backupPath)"
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
    
    Write-Log "Operations completed: $($successful.Count) successful, $($failed.Count) failed"
    
    # Validation phase
    if ($config.validation -and $failed.Count -eq 0) {
        Write-Log "Starting validation phase..."
        
        # Build validation
        if ($config.validation.build_targets) {
            $buildSuccess = Test-BuildAfterMove $config.validation.build_targets
            if (-not $buildSuccess) {
                Write-Log "Build validation failed - initiating rollback" "ERROR"
                
                # Restore from archive
                foreach ($archivedFile in $archivedFiles) {
                    try {
                        if (Test-Path $archivedFile.Archive) {
                            Move-Item $archivedFile.Archive $archivedFile.Source -Force
                            Write-Log "Restored: $($archivedFile.Archive) -> $($archivedFile.Source)"
                        }
                    }
                    catch {
                        Write-Log "Failed to restore $($archivedFile.Source): $($_.Exception.Message)" "ERROR"
                    }
                }
                exit 1
            }
        }
        
        # Custom validation commands
        if ($config.validation.custom_commands) {
            foreach ($cmd in $config.validation.custom_commands) {
                try {
                    Write-Log "Running validation: $($cmd.description)"
                    $result = Invoke-Expression $cmd.command 2>&1
                    if ($cmd.expected_exit_code -and $LASTEXITCODE -ne $cmd.expected_exit_code) {
                        throw "Command failed with exit code $LASTEXITCODE"
                    }
                    Write-Log "✓ Validation passed: $($cmd.description)"
                }
                catch {
                    Write-Log "Validation failed: $($cmd.description) - $($_.Exception.Message)" "ERROR"
                    exit 1
                }
            }
        }
    }
    
    # Cleanup phase
    if ($config.cleanup -and $failed.Count -eq 0) {
        Write-Log "Starting cleanup phase..."
        foreach ($cleanup in $config.cleanup) {
            if (Test-Path $cleanup.path) {
                try {
                    switch ($cleanup.action.ToLower()) {
                        "delete" {
                            Remove-Item $cleanup.path -Recurse -Force
                            Write-Log "✓ Cleaned up (deleted): $($cleanup.path)"
                        }
                        "archive" {
                            $cleanupArchive = Join-Path "legacy\cleanup-archive" (Get-Date -Format 'yyyy-MM-dd')
                            New-Item -ItemType Directory -Path $cleanupArchive -Force | Out-Null
                            $cleanupName = "$(Split-Path $cleanup.path -Leaf)-cleanup-$timestamp"
                            Move-Item $cleanup.path (Join-Path $cleanupArchive $cleanupName) -Force
                            Write-Log "✓ Cleaned up (archived): $($cleanup.path)"
                        }
                    }
                }
                catch {
                    Write-Log "Cleanup failed for $($cleanup.path): $($_.Exception.Message)" "WARN"
                }
            }
        }
    }
    
    # Archive the successful execution config
    if ($failed.Count -eq 0) {
        $successArchive = "legacy\execution-archives\successful-migration-$($config.migration_name.Replace(' ', '-'))-$timestamp.json"
        Copy-Item $ConfigFile $successArchive
        Write-Log "Migration config archived: $successArchive"
    }
    
    # Generate completion report
    $report = @{
        migration_name = $config.migration_name
        timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        operations_successful = $successful.Count
        operations_failed = $failed.Count
        validation_passed = $failed.Count -eq 0
        archive_location = "legacy\migration-archives\pre-migration-$timestamp"
        config_archive = if($failed.Count -eq 0) { $successArchive } else { "NOT_ARCHIVED_DUE_TO_FAILURES" }
        log_file = $LogFile
        error_log = $ErrorLog
        execution_summary = @{
            total_operations = $config.operations.Count
            successful_operations = $successful | ForEach-Object { @{ action = $_.action; source = $_.source; destination = $_.destination } }
            failed_operations = $failed | ForEach-Object { @{ action = $_.action; source = $_.source; error = "See error log" } }
        }
    }
    
    $reportPath = "agents\outputs\reports\ad-hoc-reports\migration-report-$timestamp.json"
    New-Item -ItemType Directory -Path (Split-Path $reportPath -Parent) -Force | Out-Null
    $report | ConvertTo-Json -Depth 3 | Set-Content $reportPath
    
    Write-Log "==================== MIGRATION COMPLETED ===================="
    Write-Log "Migration: $($config.migration_name)"
    Write-Log "Report: $reportPath"
    Write-Log "Archives: legacy\migration-archives\pre-migration-$timestamp"
    Write-Log "Logs: $LogFile"
    
    if ($failed.Count -gt 0) {
        Write-Log "⚠️  Migration completed with $($failed.Count) failures" "WARN"
        Write-Log "Error details: $ErrorLog" "WARN"
        exit 1
    } else {
        Write-Log "✅ Migration completed successfully!"
        Write-Log "All source files have been archived with timestamp: $timestamp"
        exit 0
    }
}

# Execute migration
Execute-Migration
