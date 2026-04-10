# Simple Document Creator - RepoLens Agent Tool
Write-Host "Creating RepoLens documentation structure..." -ForegroundColor Green

# Create the main source structure first
Write-Host "Creating source structure..." -ForegroundColor Yellow
$dirs = @("src\backend", "src\frontend", "tests\unit\backend")
foreach ($dir in $dirs) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Gray
    }
}

# Create a simple document template
$docTemplate = "# {0}`n`n> **Status**: PLACEHOLDER - To be populated by agent analysis`n> **Last Updated**: {1}`n> **Owner**: [TEAM_NAME]`n`n## Document Purpose`n`nThis document will be populated by analyzing the existing RepoLens implementation.`n`n## Content To Be Added`n`n- [ ] Extract content from existing codebase`n- [ ] Analyze implementation patterns`n- [ ] Document best practices`n- [ ] Cross-reference with related documentation`n`n## Agent Instructions`n`n1. Analyze the existing codebase for relevant information`n2. Extract patterns and specifications`n3. Generate comprehensive documentation`n4. Follow established templates`n`n---`n`n> **Generated**: {1}`n> **Ready for**: Agent population"

# Document list
$documents = @{
    "docs\project-management\requirements\user-stories.md" = "User Stories"
    "docs\project-management\requirements\acceptance-criteria.md" = "Acceptance Criteria"
    "docs\architecture\system-design\component-diagrams.md" = "Component Diagrams"
    "docs\architecture\api-design\rest-api-design.md" = "REST API Design"
    "docs\design\user-experience\user-personas.md" = "User Personas"
    "docs\security\security-policies\data-protection-policy.md" = "Data Protection Policy"
    "docs\operations\deployment\environment-setup.md" = "Environment Setup Guide"
    "docs\testing\test-strategy\testing-approach.md" = "Testing Approach"
    "docs\api\rest-endpoints\portfolio-api.md" = "Portfolio API Documentation"
    "docs\user-experience\stories\feature-stories\dashboard-navigation.md" = "Feature Story: Dashboard Navigation"
    "docs\knowledge-base\patterns\code-patterns.md" = "Code Patterns"
}

$created = 0
foreach ($doc in $documents.GetEnumerator()) {
    $dir = Split-Path $doc.Key -Parent
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    
    $content = $docTemplate -f $doc.Value, (Get-Date -Format "yyyy-MM-dd")
    Set-Content -Path $doc.Key -Value $content -Force
    Write-Host "Created: $($doc.Key)" -ForegroundColor Gray
    $created++
}

Write-Host "`nCompleted! Created $created documents." -ForegroundColor Green
Write-Host "Next: Execute migration commands from CODE_MIGRATION_MAPPING.md" -ForegroundColor Cyan
