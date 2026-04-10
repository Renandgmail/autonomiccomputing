# Project Context Scanner - Automatic Discovery for New Sessions
# Purpose: Discover project type, structure, and available automation tools
# Location: agents/tools/discovery/
# Usage: Auto-executed by Cline in new sessions for context awareness

param(
    [string]$ProjectPath = ".",
    [switch]$Verbose = $false
)

# Initialize discovery results
$discovery = @{
    project_type = @()
    current_structure = ""
    organization_level = ""
    available_tools = @()
    cleanup_capabilities = @()
    recommendations = @()
    command_triggers = @()
    safety_features = @()
}

function Write-DiscoveryLog {
    param([string]$Message, [string]$Level = "INFO")
    if ($Verbose) {
        $color = if($Level -eq "FOUND") { "Green" } elseif($Level -eq "MISSING") { "Yellow" } else { "White" }
        Write-Host "🔍 $Message" -ForegroundColor $color
    }
}

Write-DiscoveryLog "Starting project context discovery..." "INFO"

# 1. PROJECT TYPE DETECTION
Write-DiscoveryLog "Detecting project types..." "INFO"

# Check for .NET projects
$dotnetFiles = @(
    Get-ChildItem -Path $ProjectPath -Filter "*.sln" -ErrorAction SilentlyContinue
    Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 3
)
if ($dotnetFiles.Count -gt 0) {
    $discovery.project_type += ".NET"
    Write-DiscoveryLog "Found .NET project (*.sln, *.csproj)" "FOUND"
}

# Check for Node/React projects
$nodeFiles = Get-ChildItem -Path $ProjectPath -Filter "package.json" -ErrorAction SilentlyContinue
foreach ($packageFile in $nodeFiles) {
    try {
        $packageContent = Get-Content $packageFile.FullName -Raw | ConvertFrom-Json
        if ($packageContent.dependencies.react -or $packageContent.devDependencies.react) {
            $discovery.project_type += "React"
            Write-DiscoveryLog "Found React project (package.json with react)" "FOUND"
        } else {
            $discovery.project_type += "Node.js"
            Write-DiscoveryLog "Found Node.js project (package.json)" "FOUND"
        }
    } catch {
        Write-DiscoveryLog "Found package.json but couldn't parse content" "MISSING"
    }
}

# Check for Docker projects
$dockerFiles = @(
    Get-ChildItem -Path $ProjectPath -Filter "docker-compose*.yml" -ErrorAction SilentlyContinue
    Get-ChildItem -Path $ProjectPath -Filter "Dockerfile" -ErrorAction SilentlyContinue
)
if ($dockerFiles.Count -gt 0) {
    $discovery.project_type += "Docker"
    Write-DiscoveryLog "Found Docker configuration" "FOUND"
}

# Check for Python projects
$pythonFiles = @(
    Get-ChildItem -Path $ProjectPath -Filter "requirements.txt" -ErrorAction SilentlyContinue
    Get-ChildItem -Path $ProjectPath -Filter "setup.py" -ErrorAction SilentlyContinue
    Get-ChildItem -Path $ProjectPath -Filter "pyproject.toml" -ErrorAction SilentlyContinue
)
if ($pythonFiles.Count -gt 0) {
    $discovery.project_type += "Python"
    Write-DiscoveryLog "Found Python project" "FOUND"
}

# 2. CURRENT STRUCTURE ASSESSMENT
Write-DiscoveryLog "Assessing current project structure..." "INFO"

$hasSourceFolder = Test-Path (Join-Path $ProjectPath "src")
$hasDocsFolder = Test-Path (Join-Path $ProjectPath "docs") 
$hasTestsFolder = Test-Path (Join-Path $ProjectPath "tests")
$hasDeploymentFolder = Test-Path (Join-Path $ProjectPath "deployment")
$hasToolsFolder = Test-Path (Join-Path $ProjectPath "tools")

$organizationScore = 0
if ($hasSourceFolder) { $organizationScore += 20 }
if ($hasDocsFolder) { $organizationScore += 20 }
if ($hasTestsFolder) { $organizationScore += 20 }
if ($hasDeploymentFolder) { $organizationScore += 20 }
if ($hasToolsFolder) { $organizationScore += 20 }

$discovery.organization_level = switch ($organizationScore) {
    {$_ -ge 80} { "Enterprise-Ready" }
    {$_ -ge 60} { "Well-Organized" }
    {$_ -ge 40} { "Partially-Organized" }
    {$_ -ge 20} { "Basic-Structure" }
    default { "Needs-Organization" }
}

Write-DiscoveryLog "Organization level: $($discovery.organization_level) (Score: $organizationScore/100)" "INFO"

# 3. AVAILABLE TOOLS DISCOVERY
Write-DiscoveryLog "Discovering available automation tools..." "INFO"

# Check for migration tools
$migrationEngine = Join-Path $ProjectPath "tools\utilities\migration\universal-migration-engine.ps1"
if (Test-Path $migrationEngine) {
    $discovery.available_tools += "Universal Migration Engine"
    $discovery.cleanup_capabilities += "Automated file migration with validation"
    Write-DiscoveryLog "Found Universal Migration Engine" "FOUND"
}

# Check for automation triggers
$autoTrigger = Join-Path $ProjectPath "agents\tools\automation\auto-cleanup-trigger.ps1"
if (Test-Path $autoTrigger) {
    $discovery.available_tools += "Auto-Cleanup Trigger"
    $discovery.cleanup_capabilities += "Intelligent project analysis and migration planning"
    Write-DiscoveryLog "Found Auto-Cleanup Trigger" "FOUND"
}

# Check for agent workflows
$commandInterpreter = Join-Path $ProjectPath "agents\workflows\command-interpreter.md"
if (Test-Path $commandInterpreter) {
    $discovery.available_tools += "Command Interpreter System"
    $discovery.command_triggers += "cleanup, organize, enterprise structure"
    Write-DiscoveryLog "Found Command Interpreter definitions" "FOUND"
}

# Check for templates
$templatesFolder = Join-Path $ProjectPath "agents\templates"
if (Test-Path $templatesFolder) {
    $templateCount = (Get-ChildItem $templatesFolder -Filter "*.md" -ErrorAction SilentlyContinue).Count
    $discovery.available_tools += "Agent Templates ($templateCount available)"
    Write-DiscoveryLog "Found $templateCount agent templates" "FOUND"
}

# 4. SAFETY FEATURES ASSESSMENT
Write-DiscoveryLog "Checking safety and backup capabilities..." "INFO"

$legacyFolder = Join-Path $ProjectPath "legacy"
if (Test-Path $legacyFolder) {
    $discovery.safety_features += "Archive system (legacy folder exists)"
    Write-DiscoveryLog "Found legacy archive system" "FOUND"
}

$agentLogsFolder = Join-Path $ProjectPath "agents\workspace\logs"
if (Test-Path $agentLogsFolder) {
    $discovery.safety_features += "Audit logging system"
    Write-DiscoveryLog "Found audit logging system" "FOUND"
}

# 5. GENERATE RECOMMENDATIONS
Write-DiscoveryLog "Generating recommendations..." "INFO"

if ($discovery.organization_level -in @("Needs-Organization", "Basic-Structure", "Partially-Organized")) {
    if ($discovery.available_tools -contains "Auto-Cleanup Trigger") {
        $discovery.recommendations += "This project can benefit from automatic cleanup using the 'cleanup' command"
        $discovery.recommendations += "Available autonomous workflow: analyze → plan → execute → validate"
    } else {
        $discovery.recommendations += "Consider creating automation tools for project organization"
    }
}

if ($discovery.project_type -contains ".NET" -and -not $hasSourceFolder) {
    $discovery.recommendations += ".NET projects should be organized into src/backend/ structure"
}

if ($discovery.project_type -contains "React" -and -not $hasSourceFolder) {
    $discovery.recommendations += "React projects should be organized into src/frontend/ structure"
}

if ($dockerFiles.Count -gt 0 -and -not $hasDeploymentFolder) {
    $discovery.recommendations += "Docker configurations should be organized into deployment/ structure"
}

# 6. CREATE CONTEXT SUMMARY
$contextSummary = @{
    session_timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    discovery_results = $discovery
    autonomous_commands_available = ($discovery.available_tools -contains "Auto-Cleanup Trigger")
    cleanup_command_ready = ($discovery.command_triggers.Count -gt 0)
    safety_level = if ($discovery.safety_features.Count -ge 2) { "High" } elseif ($discovery.safety_features.Count -eq 1) { "Medium" } else { "Basic" }
    recommended_action = if ($discovery.recommendations.Count -gt 0) { $discovery.recommendations[0] } else { "Project appears well-organized" }
}

# 7. OUTPUT RESULTS
if ($Verbose) {
    Write-Host "`n🤖 PROJECT CONTEXT DISCOVERY COMPLETE" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Gray
    
    Write-Host "`n📊 PROJECT TYPE:" -ForegroundColor Yellow
    $discovery.project_type | ForEach-Object { Write-Host "  ✅ $_" -ForegroundColor Green }
    
    Write-Host "`n📁 ORGANIZATION LEVEL: $($discovery.organization_level)" -ForegroundColor Yellow
    
    Write-Host "`n🔧 AVAILABLE TOOLS:" -ForegroundColor Yellow
    if ($discovery.available_tools.Count -gt 0) {
        $discovery.available_tools | ForEach-Object { Write-Host "  ✅ $_" -ForegroundColor Green }
    } else {
        Write-Host "  ⚠️  No automation tools detected" -ForegroundColor Yellow
    }
    
    Write-Host "`n🎯 CLEANUP CAPABILITIES:" -ForegroundColor Yellow
    if ($discovery.cleanup_capabilities.Count -gt 0) {
        $discovery.cleanup_capabilities | ForEach-Object { Write-Host "  ✅ $_" -ForegroundColor Green }
    } else {
        Write-Host "  ⚠️  No automated cleanup capabilities" -ForegroundColor Yellow
    }
    
    Write-Host "`n💡 RECOMMENDATIONS:" -ForegroundColor Yellow
    if ($discovery.recommendations.Count -gt 0) {
        $discovery.recommendations | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "  ✅ Project appears well-organized" -ForegroundColor Green
    }
    
    if ($discovery.command_triggers.Count -gt 0) {
        Write-Host "`n🎮 AVAILABLE COMMANDS:" -ForegroundColor Magenta
        Write-Host "  Try saying: $($discovery.command_triggers -join ', ')" -ForegroundColor Cyan
    }
    
    Write-Host "`n🔒 SAFETY LEVEL: $($contextSummary.safety_level)" -ForegroundColor Green
}

# 8. SAVE CONTEXT FOR CLINE
$contextPath = Join-Path $ProjectPath "agents\workspace\discovery\project-context.json"
$contextDir = Split-Path $contextPath -Parent
if (-not (Test-Path $contextDir)) {
    New-Item -ItemType Directory -Path $contextDir -Force | Out-Null
}

$contextSummary | ConvertTo-Json -Depth 3 | Set-Content $contextPath
Write-DiscoveryLog "Context saved to: $contextPath" "INFO"

# Return the context for immediate use
return $contextSummary
