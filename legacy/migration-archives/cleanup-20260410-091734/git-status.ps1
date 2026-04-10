# Git Status Script - Quick Repository Overview
# Usage: .\git-status.ps1

Write-Host "Repository Status Overview" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan

# Basic repository info
Write-Host ""
Write-Host "Repository Information:" -ForegroundColor Yellow
Write-Host "   Location: $(Get-Location)" -ForegroundColor Gray
Write-Host "   Branch: $(git branch --show-current)" -ForegroundColor Gray

# Remote status
Write-Host ""
Write-Host "Remote Status:" -ForegroundColor Yellow
try {
    git fetch --quiet 2>$null
    $ahead = git rev-list origin/master..HEAD --count 2>$null
    $behind = git rev-list HEAD..origin/master --count 2>$null
    
    if ($ahead -eq "0" -and $behind -eq "0") {
        Write-Host "   Up to date with origin/master" -ForegroundColor Green
    } elseif ($ahead -gt 0 -and $behind -eq "0") {
        Write-Host "   $ahead commit(s) ahead of origin/master" -ForegroundColor Blue
    } elseif ($ahead -eq "0" -and $behind -gt 0) {
        Write-Host "   $behind commit(s) behind origin/master" -ForegroundColor Red
    } else {
        Write-Host "   $ahead ahead, $behind behind origin/master" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   Unable to check remote status" -ForegroundColor Yellow
}

# Working directory status
Write-Host ""
Write-Host "Working Directory:" -ForegroundColor Yellow

$status = git status --porcelain 2>$null
if (-not $status) {
    Write-Host "   Clean working directory" -ForegroundColor Green
} else {
    $modified = @($status | Where-Object { $_ -match "^ M" })
    $deleted = @($status | Where-Object { $_ -match "^ D" })
    $untracked = @($status | Where-Object { $_ -match "^\?\?" })
    $staged = @($status | Where-Object { $_ -match "^[AMD]" })
    
    if ($staged.Count -gt 0) {
        Write-Host "   Staged files: $($staged.Count)" -ForegroundColor Green
    }
    
    if ($modified.Count -gt 0) {
        Write-Host "   Modified files: $($modified.Count)" -ForegroundColor Yellow
    }
    
    if ($deleted.Count -gt 0) {
        Write-Host "   Deleted files: $($deleted.Count)" -ForegroundColor Red
    }
    
    if ($untracked.Count -gt 0) {
        Write-Host "   New files: $($untracked.Count)" -ForegroundColor Blue
    }
}

# Recent commits
Write-Host ""
Write-Host "Recent Commits:" -ForegroundColor Yellow
$commits = git log --oneline -5 2>$null
if ($commits) {
    $commits | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Gray
    }
} else {
    Write-Host "   No commits found" -ForegroundColor Gray
}

# File counts
Write-Host ""
Write-Host "Repository Stats:" -ForegroundColor Yellow
try {
    $totalFiles = (git ls-files | Measure-Object).Count
    Write-Host "   Total tracked files: $totalFiles" -ForegroundColor Gray
    
    $csFiles = (Get-ChildItem -Recurse -Include *.cs -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }).Count
    $tsFiles = (Get-ChildItem -Recurse -Include *.ts,*.tsx -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch "\\node_modules\\" }).Count
    
    Write-Host "   C# files: $csFiles" -ForegroundColor Gray
    Write-Host "   TypeScript files: $tsFiles" -ForegroundColor Gray
} catch {
    Write-Host "   Unable to count files" -ForegroundColor Yellow
}

# Quick actions
Write-Host ""
Write-Host "Quick Actions:" -ForegroundColor Yellow
Write-Host "   • View detailed status: git status" -ForegroundColor Gray
Write-Host "   • See all changes: git diff" -ForegroundColor Gray
Write-Host "   • Upload to Git: .\git-upload.ps1" -ForegroundColor Gray
Write-Host "   • View full log: git log --oneline" -ForegroundColor Gray

Write-Host ""
Write-Host "Status check complete!" -ForegroundColor Green
