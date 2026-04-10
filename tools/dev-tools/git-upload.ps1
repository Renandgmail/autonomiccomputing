# Git Upload Script with Human Approval
# Usage: .\git-upload.ps1

Write-Host "Reviewing repository changes..." -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Show current status
git status

Write-Host ""
Write-Host "Summary of changes:" -ForegroundColor Yellow
$modified = git diff --name-only
$staged = git diff --name-only --cached
$untracked = git ls-files --others --exclude-standard

if ($modified) {
    Write-Host "   Modified files: $($modified.Count)" -ForegroundColor Red
    $modified | ForEach-Object { Write-Host "     - $_" -ForegroundColor Red }
}

if ($staged) {
    Write-Host "   Staged files: $($staged.Count)" -ForegroundColor Green
    $staged | ForEach-Object { Write-Host "     - $_" -ForegroundColor Green }
}

if ($untracked) {
    Write-Host "   New files: $($untracked.Count)" -ForegroundColor Blue
    $untracked | ForEach-Object { Write-Host "     - $_" -ForegroundColor Blue }
}

# Check if there are any changes to commit
if (-not ($modified -or $untracked)) {
    Write-Host ""
    Write-Host "No changes to commit." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "WARNING: This will commit and push all current changes to the remote repository!" -ForegroundColor Yellow
Write-Host "Make sure you've reviewed all changes and they follow the project standards." -ForegroundColor Yellow

# Human approval
$approval = Read-Host "`nDo you want to proceed with staging, committing, and pushing? (yes/no)"

if ($approval -ne "yes") {
    Write-Host "Operation cancelled. No changes were made to the repository." -ForegroundColor Red
    exit 1
}

# Get commit message
Write-Host ""
Write-Host "Enter commit message (or press Enter for default):" -ForegroundColor Cyan
$commitMessage = Read-Host "Commit message"

if ([string]::IsNullOrWhiteSpace($commitMessage)) {
    $commitMessage = "chore: update project files $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    Write-Host "Using default commit message: $commitMessage" -ForegroundColor Gray
}

try {
    Write-Host ""
    Write-Host "Staging all changes..." -ForegroundColor Cyan
    git add .
    
    Write-Host "Committing changes..." -ForegroundColor Cyan
    git commit -m "$commitMessage"
    
    Write-Host "Checking remote status..." -ForegroundColor Cyan
    git fetch
    
    $behind = git rev-list HEAD..origin/master --count 2>$null
    if ($behind -gt 0) {
        Write-Host "Your branch is $behind commit(s) behind origin/master." -ForegroundColor Yellow
        $pullFirst = Read-Host "Do you want to pull latest changes first? (yes/no)"
        
        if ($pullFirst -eq "yes") {
            Write-Host "Pulling latest changes..." -ForegroundColor Cyan
            git pull origin master
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Pull failed. Please resolve conflicts and try again." -ForegroundColor Red
                exit 1
            }
        }
    }
    
    Write-Host "Pushing to remote repository..." -ForegroundColor Cyan
    git push origin master
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Successfully uploaded to Git!" -ForegroundColor Green
        Write-Host "Repository URL: https://github.com/Renandgmail/autonomiccomputing" -ForegroundColor Blue
        Write-Host "Commit message: $commitMessage" -ForegroundColor Gray
    } else {
        Write-Host "Push failed. Please check the error messages above." -ForegroundColor Red
    }
    
} catch {
    Write-Host "An error occurred: $_" -ForegroundColor Red
    Write-Host "Please review the changes and try again." -ForegroundColor Yellow
}
