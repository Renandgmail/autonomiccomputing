# Universal Migration Engine - RepoLens Agent Tool
# Purpose: Execute bulk file operations with validation and rollback capability
# Usage: .\universal-migration-engine.ps1 -ConfigFile "migration-config.json"

param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile,
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

# Initialize logging
$LogFile = "agents\workspace\scratch\migration-log-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$ErrorLog = "agents\workspace\scratch\migration-errors-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR"){"Red"} elseif($Level -eq "WARN"){"Yellow"} else{"Green"})
    Add-Content -Path $LogFile -Value $logEntry
    if ($Level -eq "ERROR") { Add-Content -Path $ErrorLog -Value $logEntry }
}

function Test-BuildAfterMove {
    param([array]$BuildTargets)
    Write-Log "Running build validation..."
    $buildResults = @()
    
    foreach ($target in $BuildTargets) {
        try {
            switch ($target.type) {
                "dotnet" {
                    $result = dotnet build $target.path --no-restore 2>&1
                    $buildResults += @{Path=$target.path; Type=$target.type; Success=$LASTEXITCODE -eq 0; Output=$result}
                }
                "npm" {
                    Push-Location $target.path
                    $result = npm run build 2>&1
                    Pop-Location
                    $buildResults += @{Path=$target.path; Type=$target.type; Success=$LASTEXITCODE -eq 0; Output=$result}
                }
                "docker" {
                    $result = docker build -t test-build $target.path 2>&1
                    $buildResults += @{Path=$target.path; Type=$target.type; Success=$LASTEXITCODE -eq 0; Output=$result}
                }
            }
        }
        catch {
            $buildResults += @{Path=$target.path; Type=$target.type; Success=$false; Output=$_.Exception.Message}
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

function Backup-Files {
    param([array]$Operations)
    $backupDir = "legacy\migration-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    
    Write-Log "Creating backup in $backupDir"
    foreach ($op in $Operations) {
        if (Test-Path $op.source) {
            $backupPath = Join-Path $backupDir (Split-Path $op.source -Leaf)
            Copy-Item $op.source $backupPath -Recurse -Force
            Write-Log "Backed up: $($op.source) -> $backupPath"
        }
    }
    return $backupDir
}

function Execute-Migration {
    Write-Log "Starting Universal Migration Engine"
    
    # Load configuration
    if (-not (Test-Path $ConfigFile)) {
        Write-Log "Configuration file not found: $ConfigFile" "ERROR"
        exit 1
    }
    
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Log "Loaded configuration with $($config.operations.Count) operations"
    
    if ($DryRun) {
        Write-Log "DRY RUN MODE - No actual changes will be made" "WARN"
        foreach ($op in $config.operations) {
            Write-Log "WOULD EXECUTE: $($op.action) $($op.source) -> $($op.destination)"
        }
        return
    }
    
    # Create backup
    $backupDir = Backup-Files $config.operations
    
    # Execute operations
    $successful = @()
    $failed = @()
    
    foreach ($op in $config.operations) {
        try {
            Write-Log "Executing: $($op.action) $($op.source) -> $($op.destination)"
            
            # Ensure destination directory exists
            $destDir = Split-Path $op.destination -Parent
            if ($destDir -and -not (Test-Path $destDir)) {
                New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                Write-Log "Created directory: $destDir"
            }
            
            switch ($op.action.ToLower()) {
                "move" {
                    if (Test-Path $op.source) {
                        Move-Item $op.source $op.destination -Force
                        $successful += $op
                        Write-Log "Moved: $($op.source) -> $($op.destination)"
                    } else {
                        Write-Log "Source not found: $($op.source)" "WARN"
                    }
                }
                "copy" {
                    if (Test-Path $op.source) {
                        Copy-Item $op.source $op.destination -Recurse -Force
                        $successful += $op
                        Write-Log "Copied: $($op.source) -> $($op.destination)"
                    } else {
                        Write-Log "Source not found: $($op.source)" "WARN"
                    }
                }
                "archive" {
                    if (Test-Path $op.source) {
                        $archivePath = Join-Path "legacy\archived-files" (Get-Date -Format 'yyyy-MM-dd')
                        New-Item -ItemType Directory -Path $archivePath -Force | Out-Null
                        Move-Item $op.source $archivePath -Force
                        $successful += $op
                        Write-Log "Archived: $($op.source) -> $archivePath"
                    } else {
                        Write-Log "Source not found for archive: $($op.source)" "WARN"
                    }
                }
                "delete" {
                    if ($op.force_delete -eq $true) {
                        if (Test-Path $op.source) {
                            Remove-Item $op.source -Recurse -Force
                            $successful += $op
                            Write-Log "Deleted: $($op.source)"
                        }
                    } else {
                        Write-Log "Delete operation requires force_delete=true in config" "WARN"
                    }
                }
                "update_file" {
                    if (Test-Path $op.source) {
                        $content = Get-Content $op.source -Raw
                        foreach ($replacement in $op.replacements) {
                            $content = $content -replace $replacement.find, $replacement.replace
                        }
                        Set-Content $op.source $content
                        $successful += $op
                        Write-Log "Updated file: $($op.source)"
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
        Write-Log "Starting validation phase"
        
        # Build validation
        if ($config.validation.build_targets) {
            $buildSuccess = Test-BuildAfterMove $config.validation.build_targets
            if (-not $buildSuccess) {
                Write-Log "Build validation failed - initiating rollback" "ERROR"
                # Restore from backup
                foreach ($op in $successful) {
                    $backupPath = Join-Path $backupDir (Split-Path $op.source -Leaf)
                    if (Test-Path $backupPath) {
                        Move-Item $backupPath $op.source -Force
                        Write-Log "Restored: $backupPath -> $($op.source)"
                    }
                }
                exit 1
            }
        }
        
        # Custom validation commands
        if ($config.validation.custom_commands) {
            foreach ($cmd in $config.validation.custom_commands) {
                try {
                    $result = Invoke-Expression $cmd.command
                    if ($cmd.expected_exit_code -and $LASTEXITCODE -ne $cmd.expected_exit_code) {
                        throw "Command failed with exit code $LASTEXITCODE"
                    }
                    Write-Log "Validation passed: $($cmd.description)"
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
        Write-Log "Starting cleanup phase"
        foreach ($cleanup in $config.cleanup) {
            if (Test-Path $cleanup.path) {
                switch ($cleanup.action) {
                    "delete" {
                        Remove-Item $cleanup.path -Recurse -Force
                        Write-Log "Cleaned up: $($cleanup.path)"
                    }
                    "archive" {
                        $archivePath = Join-Path "legacy\cleanup-archive" (Get-Date -Format 'yyyy-MM-dd')
                        New-Item -ItemType Directory -Path $archivePath -Force | Out-Null
                        Move-Item $cleanup.path $archivePath -Force
                        Write-Log "Archived for cleanup: $($cleanup.path)"
                    }
                }
            }
        }
    }
    
    # Generate completion report
    $report = @{
        timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        operations_successful = $successful.Count
        operations_failed = $failed.Count
        validation_passed = $failed.Count -eq 0
        backup_location = $backupDir
        log_file = $LogFile
        error_log = $ErrorLog
    }
    
    $reportPath = "agents\outputs\reports\ad-hoc-reports\migration-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    New-Item -ItemType Directory -Path (Split-Path $reportPath -Parent) -Force | Out-Null
    $report | ConvertTo-Json | Set-Content $reportPath
    
    Write-Log "Migration completed. Report saved to: $reportPath"
    Write-Log "Backup available at: $backupDir"
    
    if ($failed.Count -gt 0) {
        Write-Log "Migration completed with $($failed.Count) failures. Check error log: $ErrorLog" "WARN"
        exit 1
    } else {
        Write-Log "Migration completed successfully!"
        exit 0
    }
}

# Execute migration
Execute-Migration
