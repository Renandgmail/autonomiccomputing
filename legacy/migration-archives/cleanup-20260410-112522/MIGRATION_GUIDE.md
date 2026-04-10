# RepoLens Migration Guide

## 🚀 Quick Start Migration

This guide provides step-by-step instructions to migrate your RepoLens project to the new comprehensive folder structure while preserving all functionality.

## ⚠️ Pre-Migration Checklist

- [ ] **Backup Current Project**: Full backup of existing codebase
- [ ] **Team Notification**: Inform all team members of upcoming migration
- [ ] **Build Verification**: Ensure current project builds successfully
- [ ] **Test Suite**: Run complete test suite to establish baseline
- [ ] **Documentation Review**: Review existing documentation locations

## 📋 Phase 1: Infrastructure Setup (Week 1)

### Step 1.1: Create New Folder Structure

```powershell
# Create main directories
New-Item -ItemType Directory -Force -Path "docs"
New-Item -ItemType Directory -Force -Path "agents" 
New-Item -ItemType Directory -Force -Path "src"
New-Item -ItemType Directory -Force -Path "tests"
New-Item -ItemType Directory -Force -Path "deployment"
New-Item -ItemType Directory -Force -Path "tools"
New-Item -ItemType Directory -Force -Path "project-management"
New-Item -ItemType Directory -Force -Path "legacy"

# Create documentation structure
$docDirs = @(
    "docs\project-management\requirements",
    "docs\project-management\specifications", 
    "docs\project-management\planning",
    "docs\project-management\decisions",
    "docs\architecture\system-design",
    "docs\architecture\api-design",
    "docs\architecture\database-design", 
    "docs\architecture\integration-patterns",
    "docs\design\user-experience",
    "docs\design\user-interface",
    "docs\design\branding",
    "docs\security\security-policies",
    "docs\security\vulnerability-assessments",
    "docs\security\security-implementations",
    "docs\operations\deployment",
    "docs\operations\monitoring",
    "docs\operations\maintenance",
    "docs\operations\troubleshooting",
    "docs\testing\test-strategy",
    "docs\testing\test-cases",
    "docs\testing\test-reports",
    "docs\api\rest-endpoints",
    "docs\api\realtime-apis",
    "docs\api\integration-guides"
)

foreach ($dir in $docDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}

# Create agents structure
$agentDirs = @(
    "agents\workflows\development-workflows",
    "agents\workflows\maintenance-workflows",
    "agents\workflows\analytics-workflows",
    "agents\templates",
    "agents\tools\document-generators",
    "agents\tools\quality-checkers",
    "agents\tools\automation-scripts",
    "agents\outputs\reports\daily-reports",
    "agents\outputs\reports\weekly-reports", 
    "agents\outputs\reports\monthly-reports",
    "agents\outputs\reports\ad-hoc-reports",
    "agents\outputs\analysis\code-analysis",
    "agents\outputs\analysis\security-analysis",
    "agents\outputs\analysis\performance-analysis",
    "agents\outputs\analysis\trend-analysis",
    "agents\outputs\documentation\api-docs",
    "agents\outputs\documentation\code-docs",
    "agents\outputs\documentation\test-docs",
    "agents\outputs\documentation\deployment-docs",
    "agents\workspace\drafts",
    "agents\workspace\scratch",
    "agents\workspace\processing",
    "agents\workspace\archive"
)

foreach ($dir in $agentDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}

# Create source structure
$srcDirs = @(
    "src\backend",
    "src\frontend", 
    "src\shared\common",
    "src\shared\contracts",
    "src\shared\configurations",
    "src\shared\constants",
    "src\integrations\git-providers",
    "src\integrations\ci-cd",
    "src\integrations\notification-services",
    "src\integrations\analytics-services"
)

foreach ($dir in $srcDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}

# Create test structure
$testDirs = @(
    "tests\unit\backend",
    "tests\unit\frontend",
    "tests\integration\api-tests",
    "tests\integration\database-tests",
    "tests\integration\service-tests",
    "tests\e2e\user-workflows",
    "tests\e2e\performance-tests",
    "tests\e2e\security-tests",
    "tests\load\stress-tests",
    "tests\load\volume-tests",
    "tests\load\spike-tests",
    "tests\tools\test-data",
    "tests\tools\mocks",
    "tests\tools\fixtures"
)

foreach ($dir in $testDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}

# Create deployment structure  
$deployDirs = @(
    "deployment\docker\development",
    "deployment\docker\staging", 
    "deployment\docker\production",
    "deployment\docker\testing",
    "deployment\kubernetes\manifests",
    "deployment\kubernetes\helm-charts",
    "deployment\kubernetes\operators",
    "deployment\infrastructure\terraform",
    "deployment\infrastructure\ansible",
    "deployment\infrastructure\cloudformation",
    "deployment\scripts\deployment",
    "deployment\scripts\maintenance", 
    "deployment\scripts\monitoring",
    "deployment\scripts\backup",
    "deployment\environments"
)

foreach ($dir in $deployDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}

# Create tools and project management structure
$miscDirs = @(
    "tools\build",
    "tools\dev-tools",
    "tools\generators", 
    "tools\analyzers",
    "tools\migration",
    "project-management\requirements",
    "project-management\planning",
    "project-management\tracking\sprints",
    "project-management\tracking\milestones",
    "project-management\tracking\metrics",
    "project-management\governance\policies",
    "project-management\governance\procedures",
    "project-management\governance\standards",
    "legacy\original-structure",
    "legacy\deprecated",
    "legacy\migration-logs"
)

foreach ($dir in $miscDirs) {
    New-Item -ItemType Directory -Force -Path $dir
}
```

### Step 1.2: Copy Files (Preserve Originals)

```powershell
# Copy backend projects to new location
Copy-Item "RepoLens.Api" -Destination "src\backend\" -Recurse -Force
Copy-Item "RepoLens.Core" -Destination "src\backend\" -Recurse -Force  
Copy-Item "RepoLens.Infrastructure" -Destination "src\backend\" -Recurse -Force
Copy-Item "RepoLens.Worker" -Destination "src\backend\" -Recurse -Force
Copy-Item "RepoLens.Tests" -Destination "tests\unit\backend\" -Recurse -Force
Copy-Item "DatabaseQuery" -Destination "src\backend\" -Recurse -Force
Copy-Item "SearchApiDemo" -Destination "src\backend\" -Recurse -Force

# Copy frontend to new location
Copy-Item "repolens-ui" -Destination "src\frontend\" -Recurse -Force

# Copy documentation
Copy-Item "repolens-docs\*" -Destination "docs\design\user-interface\" -Recurse -Force
Copy-Item "docs\operations\*" -Destination "docs\operations\maintenance\" -Recurse -Force

# Copy deployment files
Copy-Item "*.ps1" -Destination "deployment\scripts\deployment\" -Force
Copy-Item "*.bat" -Destination "deployment\scripts\deployment\" -Force
Copy-Item "docker-compose*.yml" -Destination "deployment\docker\development\" -Force
Copy-Item "*.yaml" -Destination "deployment\kubernetes\manifests\" -Force
Copy-Item "nginx.conf" -Destination "deployment\docker\development\" -Force

# Copy root documentation files
$rootDocs = @("README.md", "BUILD-AND-START-GUIDE.md", "DEPLOYMENT-GUIDE.md", "SystemAudit.md")
foreach ($doc in $rootDocs) {
    if (Test-Path $doc) {
        Copy-Item $doc -Destination "docs\project-management\" -Force
    }
}

# Create backup of original structure
Copy-Item "." -Destination "legacy\original-structure\" -Recurse -Force -Exclude @("legacy", "src", "docs", "agents", "tests", "deployment", "tools", "project-management")
```

### Step 1.3: Update Build Configurations

```powershell
# Update solution file to point to new locations
$solutionContent = Get-Content "RepoLens.sln" -Raw
$solutionContent = $solutionContent -replace "RepoLens\.Api\\", "src\backend\RepoLens.Api\"
$solutionContent = $solutionContent -replace "RepoLens\.Core\\", "src\backend\RepoLens.Core\"
$solutionContent = $solutionContent -replace "RepoLens\.Infrastructure\\", "src\backend\RepoLens.Infrastructure\"
$solutionContent = $solutionContent -replace "RepoLens\.Worker\\", "src\backend\RepoLens.Worker\"
$solutionContent = $solutionContent -replace "RepoLens\.Tests\\", "tests\unit\backend\RepoLens.Tests\"
$solutionContent | Set-Content "RepoLens.sln"

# Update Docker Compose files to use new paths
$dockerFiles = Get-ChildItem "deployment\docker\development\docker-compose*.yml"
foreach ($file in $dockerFiles) {
    $content = Get-Content $file.FullName -Raw
    $content = $content -replace "\.\/RepoLens\.Api", "..\..\..\..\src\backend\RepoLens.Api"
    $content = $content -replace "\.\/repolens-ui", "..\..\..\..\src\frontend\repolens-ui"
    $content | Set-Content $file.FullName
}
```

## 📋 Phase 2: Content Migration (Week 2)

### Step 2.1: Move Source Code

```powershell
# Stop any running services first
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force

# Move backend projects
Move-Item "RepoLens.Api" -Destination "src\backend\" -Force
Move-Item "RepoLens.Core" -Destination "src\backend\" -Force
Move-Item "RepoLens.Infrastructure" -Destination "src\backend\" -Force  
Move-Item "RepoLens.Worker" -Destination "src\backend\" -Force
Move-Item "DatabaseQuery" -Destination "src\backend\" -Force
Move-Item "SearchApiDemo" -Destination "src\backend\" -Force

# Move frontend
Move-Item "repolens-ui" -Destination "src\frontend\" -Force

# Move tests
Move-Item "RepoLens.Tests" -Destination "tests\unit\backend\" -Force
```

### Step 2.2: Update All References

```powershell
# Update package.json scripts in frontend
$packageJsonPath = "src\frontend\repolens-ui\package.json"
if (Test-Path $packageJsonPath) {
    $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
    # Update any relative paths if needed
    $packageJson | ConvertTo-Json -Depth 10 | Set-Content $packageJsonPath
}

# Update .csproj files with correct relative paths
$csprojFiles = Get-ChildItem "src\backend\*\*.csproj" -Recurse
foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    # Update any relative references to other projects
    $content = $content -replace "\.\.\\RepoLens\.", "..\RepoLens."
    $content | Set-Content $file.FullName
}
```

### Step 2.3: Update Environment Files

```powershell
# Create environment-specific configuration files
$envConfigs = @{
    "development.env" = @"
# Development Environment Configuration
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=RepoLens_Dev;Trusted_Connection=true;
FRONTEND_URL=http://localhost:3000
API_URL=http://localhost:5000
"@
    "staging.env" = @"
# Staging Environment Configuration  
ASPNETCORE_ENVIRONMENT=Staging
ConnectionStrings__DefaultConnection=Server=staging-db;Database=RepoLens_Staging;
FRONTEND_URL=https://staging.repolens.com
API_URL=https://api-staging.repolens.com
"@
    "production.env" = @"
# Production Environment Configuration
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-db;Database=RepoLens_Prod;
FRONTEND_URL=https://repolens.com  
API_URL=https://api.repolens.com
"@
}

foreach ($env in $envConfigs.Keys) {
    $envConfigs[$env] | Set-Content "deployment\environments\$env"
}
```

## 📋 Phase 3: Agent Setup (Week 3)

### Step 3.1: Create Agent Workflow Templates

This step will create the agentic workflow templates and rules defined in the reorganization plan.

### Step 3.2: Create Document Templates

```powershell
# This will be handled by the agent template creation script
```

### Step 3.3: Set up Agent Tools

```powershell
# Create placeholder Python scripts for agent tools
$agentTools = @{
    "agents\tools\document-generators\requirement-extractor.py" = @"
#!/usr/bin/env python3
"""
Requirement Extractor Agent Tool
Extracts requirements from code and creates documentation
"""

def extract_requirements(source_path):
    # TODO: Implement requirement extraction logic
    pass

if __name__ == "__main__":
    # TODO: Implement CLI interface
    pass
"@
    "agents\tools\quality-checkers\code-quality-checker.py" = @"
#!/usr/bin/env python3
"""
Code Quality Checker Agent Tool
Analyzes code quality and generates reports
"""

def check_code_quality(project_path):
    # TODO: Implement code quality checking logic
    pass

if __name__ == "__main__":
    # TODO: Implement CLI interface
    pass
"@
}

foreach ($tool in $agentTools.Keys) {
    $agentTools[$tool] | Set-Content $tool -Encoding UTF8
}
```

## 📋 Phase 4: Cleanup (Week 4)

### Step 4.1: Remove Old Structure

```powershell
# Only run this after confirming everything works in new structure!
$oldFiles = @(
    "repolens-docs", 
    "system-audit",
    "bin",
    "obj"
)

foreach ($file in $oldFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Recurse -Force
    }
}
```

### Step 4.2: Update Documentation

```powershell
# Update README.md with new structure references
$readmePath = "docs\project-management\README.md"
if (Test-Path $readmePath) {
    $content = Get-Content $readmePath -Raw
    $content = $content -replace "repolens-ui/", "src/frontend/repolens-ui/"
    $content = $content -replace "RepoLens\.Api/", "src/backend/RepoLens.Api/"
    $content | Set-Content $readmePath
}
```

## 🧪 Testing & Validation

### Test New Structure

```powershell
# Test backend build
Push-Location "src\backend\RepoLens.Api"
dotnet build
dotnet test ..\..\..\tests\unit\backend\RepoLens.Tests\
Pop-Location

# Test frontend build  
Push-Location "src\frontend\repolens-ui"
npm install
npm run build
Pop-Location

# Test Docker Compose
Push-Location "deployment\docker\development"
docker-compose up --build -d
docker-compose down
Pop-Location
```

### Validation Checklist

- [ ] **Backend API**: All endpoints respond correctly
- [ ] **Frontend UI**: Application loads and functions properly
- [ ] **Database**: All migrations apply successfully
- [ ] **Authentication**: Login/logout works as expected
- [ ] **Real-time Features**: SignalR connections work
- [ ] **Search**: Elasticsearch integration functions
- [ ] **File Operations**: All file paths resolve correctly
- [ ] **Build Process**: Solution builds without errors
- [ ] **Tests**: All unit and integration tests pass
- [ ] **Deployment**: Docker containers start successfully

## 🚨 Rollback Plan

If issues arise during migration:

```powershell
# Stop all services
docker-compose -f "deployment\docker\development\docker-compose.yml" down

# Restore from backup
Remove-Item "src" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "docs" -Recurse -Force -ErrorAction SilentlyContinue  
Remove-Item "agents" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "tests" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "deployment" -Recurse -Force -ErrorAction SilentlyContinue

Copy-Item "legacy\original-structure\*" -Destination "." -Recurse -Force

# Restore original solution file
git checkout HEAD -- RepoLens.sln
```

## 📊 Post-Migration Verification

### Performance Benchmarks

```powershell
# Frontend bundle size check
Push-Location "src\frontend\repolens-ui"
npm run build
Get-ChildItem "build\static\js\*.js" | Measure-Object -Property Length -Sum
Pop-Location

# Backend startup time  
Measure-Command {
    Push-Location "src\backend\RepoLens.Api"
    $process = Start-Process "dotnet" -ArgumentList "run" -PassThru
    Start-Sleep 10
    Stop-Process $process
    Pop-Location
}
```

### Success Criteria

- [ ] **Build Time**: ≤ 2 minutes for full solution
- [ ] **Startup Time**: ≤ 30 seconds for backend API
- [ ] **Frontend Load**: ≤ 5 seconds initial page load
- [ ] **Test Execution**: ≤ 5 minutes for complete test suite
- [ ] **Memory Usage**: ≤ 100MB peak frontend usage
- [ ] **Docker Build**: ≤ 10 minutes for all containers

## 🤝 Team Onboarding

### New Developer Setup

1. **Clone Repository**: `git clone <repository-url>`
2. **Install Dependencies**: Run `tools\dev-tools\setup-dev-environment.ps1`
3. **Review Structure**: Read `PROJECT_STRUCTURE_REORGANIZATION.md`
4. **Run Application**: Follow `docs\operations\deployment\deployment-guide.md`
5. **Explore Docs**: Browse `docs\` folder for comprehensive information

### Documentation Updates

- [ ] Update onboarding documentation
- [ ] Update deployment guides
- [ ] Update development workflows  
- [ ] Update CI/CD pipelines
- [ ] Update IDE configurations
- [ ] Update team communication channels

## 📝 Migration Log Template

```markdown
# Migration Log - [Date]

## Phase Completed: [Phase Number/Name]

### Actions Taken:
- [ ] Action 1
- [ ] Action 2  
- [ ] Action 3

### Issues Encountered:
- Issue 1: [Description and Resolution]
- Issue 2: [Description and Resolution]

### Next Steps:
- [ ] Next action 1
- [ ] Next action 2

### Team Notes:
[Any important information for team members]
```

Save migration logs to: `legacy\migration-logs\migration-log-[YYYY-MM-DD].md`
