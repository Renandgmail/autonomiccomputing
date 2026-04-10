# Auto-Cleanup Trigger - Intelligent Agent Automation
# Purpose: Automatically analyze project structure and generate migration configs
# Location: agents/tools/automation/
# Usage: Called by agents when cleanup commands are detected

param(
    [string]$ProjectPath = ".",
    [string]$OutputConfigPath = "",
    [switch]$Execute = $false,
    [switch]$DryRun = $true
)

# Initialize
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
if (-not $OutputConfigPath) {
    $OutputConfigPath = "agents\workspace\migration-configs\auto-cleanup-$timestamp.json"
}

Write-Host "🤖 AGENT AUTO-CLEANUP INITIATED" -ForegroundColor Cyan
Write-Host "Analyzing project structure at: $ProjectPath" -ForegroundColor Gray

# Scan project structure
function Analyze-ProjectStructure {
    param([string]$Path)
    
    Write-Host "🔍 Scanning project structure..." -ForegroundColor Yellow
    
    $analysis = @{
        source_files = @()
        documentation = @()
        deployment_configs = @()
        legacy_items = @()
        scripts = @()
        recommendations = @()
    }
    
    # Scan for .NET projects
    $dotnetProjects = Get-ChildItem -Path $Path -Filter "*.csproj" -Recurse | Where-Object { 
        $_.Directory.Name -notmatch "^(src|tests|tools)" 
    }
    foreach ($project in $dotnetProjects) {
        $analysis.source_files += @{
            type = "dotnet"
            current = $project.Directory.FullName.Replace((Resolve-Path $Path).Path + "\", "")
            recommended = "src/backend/$($project.Directory.Name)"
            reason = ".NET project should be in src/backend/"
        }
        $analysis.recommendations += "Move .NET project $($project.Directory.Name) to organized backend structure"
    }
    
    # Scan for React/Node projects
    $reactProjects = Get-ChildItem -Path $Path -Filter "package.json" | Where-Object {
        $content = Get-Content $_.FullName -Raw | ConvertFrom-Json
        $content.dependencies -and ($content.dependencies.react -or $content.devDependencies.react)
    }
    foreach ($project in $reactProjects) {
        if ($project.Directory.Name -notmatch "^(src|frontend)") {
            $analysis.source_files += @{
                type = "react"
                current = $project.Directory.Name
                recommended = "src/frontend/$($project.Directory.Name)"
                reason = "React project should be in src/frontend/"
            }
            $analysis.recommendations += "Move React project $($project.Directory.Name) to frontend structure"
        }
    }
    
    # Scan for documentation
    $docFiles = Get-ChildItem -Path $Path -Filter "*.md" | Where-Object { 
        $_.Directory.Name -notmatch "^docs" -and $_.Name -notmatch "^(README|CHANGELOG)" 
    }
    foreach ($doc in $docFiles) {
        $category = switch -Regex ($doc.Name) {
            "deploy|build|install" { "operations/deployment" }
            "api|endpoint" { "api" }
            "security|auth" { "security" }
            "test|spec" { "testing" }
            default { "project-management" }
        }
        $analysis.documentation += @{
            current = $doc.Name
            recommended = "docs/$category/$($doc.Name.ToLower())"
            reason = "Documentation should be organized by category"
        }
        $analysis.recommendations += "Organize documentation file $($doc.Name)"
    }
    
    # Scan for deployment configs
    $deployConfigs = @("docker-compose*.yml", "*.dockerfile", "nginx.conf", "*.yaml", "*.yml") | ForEach-Object {
        Get-ChildItem -Path $Path -Filter $_ | Where-Object { $_.Directory.Name -notmatch "^deployment" }
    }
    foreach ($config in $deployConfigs) {
        $category = switch -Regex ($config.Name) {
            "docker-compose\.yml$" { "docker/production" }
            "docker-compose.*dev" { "docker/development" }
            "\.dockerfile$" { "docker/images" }
            "nginx" { "docker/production" }
            "k8s|kubernetes" { "kubernetes/manifests" }
            default { "configs" }
        }
        $analysis.deployment_configs += @{
            current = $config.Name
            recommended = "deployment/$category/$($config.Name)"
            reason = "Deployment configs should be organized by environment"
        }
        $analysis.recommendations += "Organize deployment config $($config.Name)"
    }
    
    # Scan for scripts
    $scripts = Get-ChildItem -Path $Path -Filter "*.ps1" | Where-Object { 
        $_.Directory.Name -notmatch "^(tools|scripts|deployment)" 
    }
    foreach ($script in $scripts) {
        $category = switch -Regex ($script.Name) {
            "deploy|infrastructure" { "deployment" }
            "monitor|health" { "monitoring" }
            "git|version" { "dev-tools" }
            "test|build" { "dev-tools" }
            default { "dev-tools" }
        }
        $analysis.scripts += @{
            current = $script.Name
            recommended = "tools/$category/$($script.Name.ToLower())"
            reason = "Scripts should be organized by purpose"
        }
        $analysis.recommendations += "Organize script $($script.Name)"
    }
    
    # Scan for legacy items
    $legacyPatterns = @("system-audit", "old-*", "*-backup", "temp", "tmp", "cache")
    foreach ($pattern in $legacyPatterns) {
        $legacyItems = Get-ChildItem -Path $Path -Filter $pattern -Directory
        foreach ($item in $legacyItems) {
            $analysis.legacy_items += @{
                current = $item.Name
                recommended = "legacy/archived-files/$($item.Name)-archived-$timestamp"
                reason = "Legacy items should be archived"
            }
            $analysis.recommendations += "Archive legacy item $($item.Name)"
        }
    }
    
    return $analysis
}

# Generate migration configuration
function Generate-MigrationConfig {
    param($Analysis)
    
    Write-Host "⚙️  Generating migration configuration..." -ForegroundColor Yellow
    
    $operations = @()
    
    # Add source file operations
    foreach ($file in $Analysis.source_files) {
        $operations += @{
            action = "move"
            source = $file.current
            destination = $file.recommended
            description = $file.reason
        }
    }
    
    # Add documentation operations
    foreach ($doc in $Analysis.documentation) {
        $operations += @{
            action = "move"
            source = $doc.current
            destination = $doc.recommended
            description = $doc.reason
        }
    }
    
    # Add deployment operations
    foreach ($config in $Analysis.deployment_configs) {
        $operations += @{
            action = "move"
            source = $config.current
            destination = $config.recommended
            description = $config.reason
        }
    }
    
    # Add script operations
    foreach ($script in $Analysis.scripts) {
        $operations += @{
            action = "move"
            source = $script.current
            destination = $script.recommended
            description = $script.reason
        }
    }
    
    # Add legacy operations
    foreach ($legacy in $Analysis.legacy_items) {
        $operations += @{
            action = "archive"
            source = $legacy.current
            description = $legacy.reason
        }
    }
    
    # Add solution file update if exists
    $solutionFile = Get-ChildItem -Path $ProjectPath -Filter "*.sln" | Select-Object -First 1
    if ($solutionFile) {
        $operations += @{
            action = "update_file"
            source = $solutionFile.Name
            description = "Update solution file paths for new structure"
            replacements = @(
                @{ find = "([^\\\\]+)\\.csproj"; replace = "src\\backend\\`$1\\`$1.csproj" },
                @{ find = "([^\\\\]+)\\.Tests\\\\"; replace = "tests\\unit\\backend\\`$1.Tests\\" }
            )
        }
    }
    
    # Build validation targets
    $buildTargets = @()
    if ($solutionFile) {
        $buildTargets += @{
            type = "dotnet"
            path = $solutionFile.Name
            description = "Validate entire solution builds"
        }
    }
    
    # Check for React projects
    $reactPackageJson = Get-ChildItem -Path $ProjectPath -Filter "package.json" | Where-Object {
        $content = Get-Content $_.FullName -Raw | ConvertFrom-Json
        $content.dependencies.react -or $content.devDependencies.react
    } | Select-Object -First 1
    
    if ($reactPackageJson) {
        $buildTargets += @{
            type = "npm"
            path = "src/frontend/$($reactPackageJson.Directory.Name)"
            description = "Validate React frontend builds"
        }
    }
    
    $config = @{
        migration_name = "Auto-Generated Project Cleanup"
        description = "Intelligent agent-generated migration to enterprise structure"
        version = "auto-1.0"
        generated_by = "agents/tools/automation/auto-cleanup-trigger.ps1"
        generated_timestamp = $timestamp
        analysis_summary = @{
            total_recommendations = $Analysis.recommendations.Count
            source_files_found = $Analysis.source_files.Count
            documentation_files = $Analysis.documentation.Count
            deployment_configs = $Analysis.deployment_configs.Count
            scripts_found = $Analysis.scripts.Count
            legacy_items = $Analysis.legacy_items.Count
        }
        operations = $operations
        validation = @{
            build_targets = $buildTargets
            custom_commands = @()
        }
        cleanup = @(
            @{ action = "delete"; path = "bin"; description = "Remove build outputs" },
            @{ action = "delete"; path = "obj"; description = "Remove build cache" },
            @{ action = "archive"; path = "node_modules"; description = "Archive root node_modules" }
        )
    }
    
    return $config
}

# Execute the analysis
$analysis = Analyze-ProjectStructure $ProjectPath

# Display analysis results
Write-Host "`n📊 ANALYSIS COMPLETE" -ForegroundColor Green
Write-Host "Found issues requiring cleanup:" -ForegroundColor Yellow
foreach ($recommendation in $analysis.recommendations) {
    Write-Host "  • $recommendation" -ForegroundColor Gray
}

Write-Host "`n📋 SUMMARY:" -ForegroundColor Cyan
Write-Host "  Source files to organize: $($analysis.source_files.Count)" -ForegroundColor White
Write-Host "  Documentation to structure: $($analysis.documentation.Count)" -ForegroundColor White  
Write-Host "  Deployment configs to organize: $($analysis.deployment_configs.Count)" -ForegroundColor White
Write-Host "  Scripts to organize: $($analysis.scripts.Count)" -ForegroundColor White
Write-Host "  Legacy items to archive: $($analysis.legacy_items.Count)" -ForegroundColor White

# Generate configuration
$config = Generate-MigrationConfig $analysis

# Ensure output directory exists
$configDir = Split-Path $OutputConfigPath -Parent
New-Item -ItemType Directory -Path $configDir -Force | Out-Null

# Save configuration
$config | ConvertTo-Json -Depth 4 | Set-Content $OutputConfigPath
Write-Host "`n💾 Configuration saved: $OutputConfigPath" -ForegroundColor Green

# Execute if requested
if ($Execute) {
    Write-Host "`n🚀 EXECUTING MIGRATION..." -ForegroundColor Yellow
    
    $enginePath = "tools\utilities\migration\universal-migration-engine.ps1"
    $params = @("-ConfigFile", $OutputConfigPath)
    if ($DryRun) { $params += "-DryRun" }
    
    & powershell -ExecutionPolicy Bypass -File $enginePath @params
}
else {
    Write-Host "`n✅ READY FOR EXECUTION" -ForegroundColor Green
    Write-Host "To execute this cleanup:" -ForegroundColor White
    Write-Host "  powershell -File `"tools\utilities\migration\universal-migration-engine.ps1`" -ConfigFile `"$OutputConfigPath`"" -ForegroundColor Cyan
    Write-Host "`nOr for dry-run:" -ForegroundColor White  
    Write-Host "  powershell -File `"tools\utilities\migration\universal-migration-engine.ps1`" -ConfigFile `"$OutputConfigPath`" -DryRun" -ForegroundColor Cyan
}

Write-Host "`n🤖 Agent auto-cleanup analysis complete!" -ForegroundColor Magenta
