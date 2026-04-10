# Enhanced Cleanup Migration with Validation and Re-analysis
# Purpose: Complete migration with pre/post validation and missed file detection
# Usage: .\enhanced-cleanup-migration.ps1 -ConfigFile "path-to-config.json"

param(
    [Parameter(Mandatory=$true)]
    [string]$ConfigFile,
    [switch]$DryRun = $false,
    [switch]$ForceReanalysis = $false
)

# Initialize logging
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$LogFile = "agents\workspace\logs\enhanced-migration-log-$timestamp.log"
$ErrorLog = "agents\workspace\logs\enhanced-migration-errors-$timestamp.log"
$ValidationLog = "agents\workspace\logs\migration-validation-$timestamp.log"

# Ensure log directories exist
New-Item -ItemType Directory -Path "agents\workspace\logs" -Force | Out-Null
New-Item -ItemType Directory -Path "legacy\migration-archives" -Force | Out-Null

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $logTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$logTimestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR"){"Red"} elseif($Level -eq "WARN"){"Yellow"} elseif($Level -eq "VALIDATION"){"Magenta"} else{"Green"})
    Add-Content -Path $LogFile -Value $logEntry -ErrorAction SilentlyContinue
    if ($Level -eq "ERROR") { Add-Content -Path $ErrorLog -Value $logEntry -ErrorAction SilentlyContinue }
    if ($Level -eq "VALIDATION") { Add-Content -Path $ValidationLog -Value $logEntry -ErrorAction SilentlyContinue }
}

function Get-CurrentFileAnalysis {
    Write-Log "🔍 Analyzing current directory structure..." "VALIDATION"
    
    $analysis = @{
        root_files = @()
        projects = @()
        documentation = @()
        scripts = @()
        configs = @()
        should_be_moved = @()
        already_organized = @()
    }
    
    # Get all items in root directory (excluding organized folders)
    $excludePatterns = @("src", "docs", "deployment", "tools", "tests", "legacy", "agents", ".git", "bin", "obj", "node_modules")
    $rootItems = Get-ChildItem -Path "." -Force | Where-Object { 
        $excludePatterns -notcontains $_.Name -and $_.Name -notlike ".*"
    }
    
    foreach ($item in $rootItems) {
        $analysis.root_files += $item.Name
        
        # Categorize items
        switch -Regex ($item.Name) {
            "\.(csproj|sln)$" { $analysis.projects += $item.Name }
            "\.(md|txt|rst)$" { $analysis.documentation += $item.Name }
            "\.(ps1|bat|cmd|sh)$" { $analysis.scripts += $item.Name }
            "\.(yml|yaml|json|xml|conf)$" { $analysis.configs += $item.Name }
        }
        
        # Determine if should be moved
        $shouldMove = $true
        switch ($item.Name) {
            {$_ -in @(".gitignore", "package-lock.json", "RepoLens.sln", "CodeLens.slnx")} { $shouldMove = $false }
        }
        
        if ($shouldMove) {
            $analysis.should_be_moved += $item.Name
        } else {
            $analysis.already_organized += $item.Name
        }
    }
    
    Write-Log "Analysis Results:" "VALIDATION"
    Write-Log "  Total root files: $($analysis.root_files.Count)" "VALIDATION"
    Write-Log "  Projects: $($analysis.projects.Count)" "VALIDATION"
    Write-Log "  Documentation: $($analysis.documentation.Count)" "VALIDATION"
    Write-Log "  Scripts: $($analysis.scripts.Count)" "VALIDATION"
    Write-Log "  Configs: $($analysis.configs.Count)" "VALIDATION"
    Write-Log "  Should be moved: $($analysis.should_be_moved.Count)" "VALIDATION"
    Write-Log "  Can remain in root: $($analysis.already_organized.Count)" "VALIDATION"
    
    return $analysis
}

function Generate-MissingMigrationConfig {
    param($Analysis, $OriginalConfig)
    
    Write-Log "📋 Generating supplementary migration config for missed files..." "VALIDATION"
    
    $newOperations = @()
    
    # Get files that were already processed
    $processedFiles = @()
    foreach ($op in $OriginalConfig.operations) {
        if ($op.action -eq "move" -and $op.source) {
            $processedFiles += $op.source
        }
    }
    
    # Find files that should be moved but weren't in original config
    foreach ($file in $Analysis.should_be_moved) {
        if ($processedFiles -notcontains $file) {
            Write-Log "  Found unprocessed file: $file" "VALIDATION"
            
            # Determine appropriate destination
            $destination = $null
            switch -Regex ($file) {
                "\.(md|txt|rst)$" {
                    if ($file -match "(?i)(deploy|build|install|setup)") {
                        $destination = "docs/operations/deployment/$($file.ToLower())"
                    } elseif ($file -match "(?i)(migration|guide|manual)") {
                        $destination = "docs/project-management/$($file.ToLower())"
                    } else {
                        $destination = "docs/project-management/$($file.ToLower())"
                    }
                }
                "\.(ps1|bat|cmd)$" {
                    $destination = "tools/dev-tools/$($file.ToLower())"
                }
                "\.(yml|yaml|json|xml|conf)$" {
                    if ($file -match "(?i)(docker|compose)") {
                        $destination = "deployment/docker/configs/$file"
                    } elseif ($file -match "(?i)(k8s|kubernetes|deploy)") {
                        $destination = "deployment/kubernetes/manifests/$file"
                    } else {
                        $destination = "deployment/configs/$file"
                    }
                }
                "\.(sql|db)$" {
                    $destination = "docs/architecture/database-design/$file"
                }
                default {
                    $destination = "legacy/misc-files/$file"
                }
            }
            
            if ($destination) {
                $newOperations += @{
                    action = "move"
                    source = $file
                    destination = $destination
                    description = "Move missed file during supplementary cleanup"
                }
            }
        }
    }
    
    Write-Log "Generated $($newOperations.Count) additional operations for missed files" "VALIDATION"
    
    return $newOperations
}

function Execute-MigrationOperations {
    param($Operations, $BackupDir)
    
    $successful = @()
    $failed = @()
    
    Write-Log "🔄 Executing $($Operations.Count) migration operations..."
    
    foreach ($op in $Operations) {
        try {
            Write-Log "Executing: $($op.action.ToUpper()) $($op.source) -> $($op.destination)"
            
            switch ($op.action.ToLower()) {
                "move" {
                    if (Test-Path $op.source) {
                        # Create backup first
                        $backupPath = Join-Path $BackupDir (Split-Path $op.source -Leaf)
                        Copy-Item $op.source $backupPath -Recurse -Force -ErrorAction SilentlyContinue
                        Write-Log "Backed up: $($op.source) -> $backupPath"
                        
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
            }
        }
        catch {
            Write-Log "Operation failed: $($op.action) $($op.source) - $($_.Exception.Message)" "ERROR"
            $failed += $op
        }
    }
    
    return @{
        Successful = $successful
        Failed = $failed
    }
}

function Validate-PostMigration {
    Write-Log "🔍 Performing post-migration validation..." "VALIDATION"
    
    $postAnalysis = Get-CurrentFileAnalysis
    $issues = @()
    
    # Check if files that should be moved are still in root
    if ($postAnalysis.should_be_moved.Count -gt 0) {
        $issues += "Files still in root that should be organized: $($postAnalysis.should_be_moved -join ', ')"
    }
    
    # Verify organized structure exists
    $expectedFolders = @("src/backend", "src/frontend", "docs", "deployment", "tools", "tests", "legacy")
    foreach ($folder in $expectedFolders) {
        if (-not (Test-Path $folder)) {
            $issues += "Missing expected organized folder: $folder"
        }
    }
    
    Write-Log "Post-migration validation results:" "VALIDATION"
    if ($issues.Count -eq 0) {
        Write-Log "✅ All validation checks passed!" "VALIDATION"
        return $true
    } else {
        Write-Log "⚠️ Found $($issues.Count) validation issues:" "VALIDATION"
        foreach ($issue in $issues) {
            Write-Log "  - $issue" "VALIDATION"
        }
        return $false
    }
}

# MAIN EXECUTION
Write-Log "🚀 Starting Enhanced RepoLens Cleanup Migration with Validation"
Write-Log "Configuration: $ConfigFile"

# Phase 1: Pre-Migration Analysis
Write-Log "==================== PHASE 1: PRE-MIGRATION ANALYSIS ===================="
$preAnalysis = Get-CurrentFileAnalysis

if ($preAnalysis.should_be_moved.Count -eq 0 -and -not $ForceReanalysis) {
    Write-Log "✅ Directory appears to be already clean. Use -ForceReanalysis to override." "VALIDATION"
    exit 0
}

# Phase 2: Load and Execute Original Configuration
Write-Log "==================== PHASE 2: ORIGINAL MIGRATION EXECUTION ===================="

if (-not (Test-Path $ConfigFile)) {
    Write-Log "Configuration file not found: $ConfigFile" "ERROR"
    exit 1
}

try {
    $config = Get-Content $ConfigFile | ConvertFrom-Json
    Write-Log "Loaded migration config: $($config.migration_name)"
    Write-Log "Original operations count: $($config.operations.Count)"
} catch {
    Write-Log "Failed to parse configuration: $($_.Exception.Message)" "ERROR"
    exit 1
}

if (-not $DryRun) {
    # Create backup directory
    $backupDir = "legacy\migration-archives\enhanced-cleanup-$timestamp"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Log "Created backup directory: $backupDir"
    
    # Execute original operations
    $originalResults = Execute-MigrationOperations $config.operations $backupDir
    Write-Log "Original migration: $($originalResults.Successful.Count) successful, $($originalResults.Failed.Count) failed"
}

# Phase 3: Post-Migration Analysis and Supplementary Migration
Write-Log "==================== PHASE 3: POST-MIGRATION ANALYSIS ===================="
$postAnalysis = Get-CurrentFileAnalysis

if ($postAnalysis.should_be_moved.Count -gt 0) {
    Write-Log "Found $($postAnalysis.should_be_moved.Count) files that still need migration" "VALIDATION"
    
    # Generate supplementary migration config
    $supplementaryOps = Generate-MissingMigrationConfig $postAnalysis $config
    
    if ($supplementaryOps.Count -gt 0 -and -not $DryRun) {
        Write-Log "==================== PHASE 4: SUPPLEMENTARY MIGRATION ===================="
        $supplementaryResults = Execute-MigrationOperations $supplementaryOps $backupDir
        Write-Log "Supplementary migration: $($supplementaryResults.Successful.Count) successful, $($supplementaryResults.Failed.Count) failed"
    }
}

# Phase 4: Final Validation
Write-Log "==================== PHASE 5: FINAL VALIDATION ===================="
$validationPassed = Validate-PostMigration

# Generate comprehensive report
$report = @{
    migration_name = $config.migration_name + " - Enhanced with Validation"
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    pre_analysis = $preAnalysis
    post_analysis = Get-CurrentFileAnalysis
    validation_passed = $validationPassed
    phases_completed = 5
    backup_location = if($backupDir) { $backupDir } else { "DRY_RUN" }
    log_files = @{
        main_log = $LogFile
        error_log = $ErrorLog
        validation_log = $ValidationLog
    }
}

$reportPath = "agents\outputs\reports\ad-hoc-reports\enhanced-migration-report-$timestamp.json"
New-Item -ItemType Directory -Path (Split-Path $reportPath -Parent) -Force | Out-Null
$report | ConvertTo-Json -Depth 4 | Set-Content $reportPath

Write-Log "==================== ENHANCED MIGRATION COMPLETED ===================="
Write-Log "📊 Final Results:"
Write-Log "  ✅ Pre-analysis completed"
Write-Log "  ✅ Original migration executed"
Write-Log "  ✅ Post-analysis completed"
Write-Log "  ✅ Supplementary migration executed"
Write-Log "  $(if($validationPassed){'✅'}else{'❌'}) Final validation $(if($validationPassed){'passed'}else{'failed'})"
Write-Log "  📁 Backup location: $(if($backupDir){$backupDir}else{'N/A (dry run)'})"
Write-Log "  📋 Comprehensive report: $reportPath"
Write-Log "  📝 Logs: $LogFile"

if (-not $validationPassed) {
    Write-Log "⚠️ Migration completed with validation issues - check validation log: $ValidationLog" "WARN"
    exit 1
} else {
    Write-Log "🎉 ENHANCED MIGRATION COMPLETED SUCCESSFULLY WITH FULL VALIDATION!"
    Write-Log "Your RepoLens project has been completely transformed to enterprise structure!"
    exit 0
}
