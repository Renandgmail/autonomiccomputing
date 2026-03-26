# Git Workflow Guide

**Repository**: https://github.com/Renandgmail/autonomiccomputing  
**Last Updated**: 2026-03-26

## Overview

This document establishes clear Git practices for the RepoLens project, ensuring proper version control with **mandatory human approval** for all Git operations.

---

## Repository Structure

### What's Properly Ignored (.gitignore)
✅ **Node.js/React**: `node_modules/`, build artifacts, environment files  
✅ **.NET**: `bin/`, `obj/`, build outputs, Visual Studio files  
✅ **Database**: `*.db`, `*.sqlite` files  
✅ **OS Files**: `.DS_Store`, `Thumbs.db`, etc.  
✅ **IDE Files**: `.vscode/*`, `.idea/`, etc.  
✅ **Secrets**: Production config files, local environment files

### Current Status (as of 2026-03-26)
- **Modified Files**: 22 files changed
- **Deleted Files**: 6 files removed
- **New Files**: Extensive Phase 1 implementation (Provider system, tests, documentation)
- **Major Addition**: Complete React UI (`repolens-ui/` directory)

---

## Git Workflow Commands

### 🔒 Human Approval Required
**ALL Git operations require explicit human approval before execution.**

### Daily Workflow

#### 1. Check Status
```bash
git status
git diff --name-only
```

#### 2. Pull Latest Changes
```bash
git pull origin master
```

#### 3. Stage Changes (Selective)
```bash
# Stage specific files
git add <file1> <file2>

# Stage all files (use carefully)
git add .

# Stage by category
git add *.cs          # Only C# files
git add RepoLens.*/   # Only specific projects
```

#### 4. Review Changes Before Commit
```bash
git diff --cached     # Review staged changes
git status           # Confirm what's being committed
```

#### 5. Commit with Clear Message
```bash
git commit -m "feat: implement provider abstraction system

- Add IGitProviderService interface and factory pattern
- Implement GitHub and Local providers
- Add comprehensive unit and integration tests
- Update database schema with ProviderType column
- Complete Phase 1 foundation requirements"
```

#### 6. Push Changes (Human Approval)
```bash
# Push to remote (requires approval)
git push origin master
```

---

## Commit Message Standards

### Format
```
<type>: <description>

<optional body>
<optional footer>
```

### Types
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Code style/formatting
- `refactor:` Code refactoring
- `test:` Adding/updating tests
- `chore:` Maintenance tasks

### Examples
```bash
git commit -m "feat: add local repository provider support"
git commit -m "fix: resolve provider factory null reference"
git commit -m "docs: update architecture documentation"
git commit -m "test: add integration tests for local repositories"
```

---

## Branch Strategy

### Current: Single Branch (Master)
- All development on `master` branch
- Direct commits with human approval
- Focus on code quality and testing

### Future: Feature Branch Strategy
```bash
# Create feature branch
git checkout -b feature/analytics-api
git push -u origin feature/analytics-api

# Merge back to master
git checkout master
git pull origin master
git merge feature/analytics-api
git push origin master
git branch -d feature/analytics-api
```

---

## Human Approval Process

### Simple Commands for Common Operations

#### "Upload to Git" - Full Commit & Push
Create a script: `commit-and-push.sh`
```bash
#!/bin/bash
echo "🔍 Reviewing changes..."
git status
echo ""
echo "📋 Files to be committed:"
git diff --name-only --cached
echo ""
read -p "Approve commit and push? (y/N): " confirm
if [[ $confirm == [yY] ]]; then
    echo "📝 Enter commit message:"
    read commit_msg
    git commit -m "$commit_msg"
    echo "🚀 Pushing to remote..."
    git push origin master
    echo "✅ Successfully pushed to Git!"
else
    echo "❌ Operation cancelled."
fi
```

#### "Quick Status" - Check Repository State
Create a script: `git-status.sh`
```bash
#!/bin/bash
echo "📊 Repository Status:"
echo "===================="
git status --short
echo ""
echo "🌐 Remote Status:"
git fetch
git status
```

#### "Safe Backup" - Stash Changes
Create a script: `backup-changes.sh`
```bash
#!/bin/bash
echo "💾 Backing up current changes..."
git stash push -m "Auto-backup $(date)"
echo "✅ Changes backed up. Use 'git stash pop' to restore."
```

---

## Pre-Commit Checklist

Before ANY commit, ensure:

### ✅ Code Quality
- [ ] Code compiles without errors: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] No sensitive data in files
- [ ] Code follows project standards

### ✅ Git Hygiene
- [ ] Reviewed all staged files: `git diff --cached`
- [ ] Commit message follows standards
- [ ] No unnecessary files included
- [ ] Large files properly excluded

### ✅ Documentation
- [ ] Update relevant documentation
- [ ] Update action list if completing tasks
- [ ] Add comments for complex changes

---

## Emergency Procedures

### Undo Last Commit (Local Only)
```bash
git reset --soft HEAD~1  # Keep changes staged
git reset --hard HEAD~1  # Discard changes completely
```

### Revert Pushed Commit
```bash
git revert <commit-hash>
git push origin master
```

### Recover Lost Changes
```bash
git reflog              # Find lost commits
git checkout <commit>   # Restore specific commit
```

---

## Phase 1 Commit Strategy

### Recommended Commit Structure for Current Changes

#### Commit 1: Core Infrastructure
```bash
git add .gitignore RepoLens.Core/ RepoLens.Infrastructure/ RepoLens.sln
git commit -m "feat: implement core provider abstraction system

- Add IGitProviderService interface and factory pattern
- Implement ProviderType enum and RepositoryContext
- Add database migration for provider support
- Update project structure and dependencies"
```

#### Commit 2: Provider Implementations
```bash
git add RepoLens.Infrastructure/Providers/
git commit -m "feat: implement Git provider services

- Add GitHub provider with full API integration
- Add Local provider using LibGit2Sharp
- Add stub providers for GitLab, Bitbucket, Azure DevOps
- Implement comprehensive provider factory"
```

#### Commit 3: API and Commands
```bash
git add RepoLens.Api/
git commit -m "feat: add API layer with provider integration

- Implement repository validation service
- Update AddRepositoryCommand to use provider factory
- Add comprehensive controller endpoints
- Configure dependency injection for all providers"
```

#### Commit 4: Testing
```bash
git add RepoLens.Tests/
git commit -m "test: add comprehensive test suite

- Add unit tests for all provider services
- Implement integration test with local repository fixture
- Add validation service tests with all provider patterns
- Test provider factory with comprehensive scenarios"
```

#### Commit 5: Documentation
```bash
git add *.md 00-* 01-* 02-* 03-* 04-* 05-* 06-* 07-* CONTRIBUTING.md
git commit -m "docs: add comprehensive project documentation

- Add architecture documentation and specifications
- Create detailed action list and development workflow
- Add integration test specifications
- Document stakeholder review processes and escalation procedures"
```

#### Commit 6: React UI
```bash
git add repolens-ui/
git commit -m "feat: add React frontend application

- Complete React TypeScript application structure
- Implement components for dashboard, repositories, analytics
- Add authentication and search functionality
- Configure build system and development environment"
```

---

## Security Considerations

### Secrets Management
- **NEVER commit**: API keys, passwords, connection strings
- **Use**: Environment variables and local config files
- **Exclude**: `appsettings.Production.json`, `.env.local`

### File Size Limits
- **Maximum**: 100MB per file
- **Watch**: Database files, compiled binaries, large assets
- **Use**: Git LFS for large files if needed

---

## Getting Help

### Commands Reference
```bash
git help <command>      # Detailed help for specific command
git status             # Current repository state
git log --oneline      # Commit history
git diff              # Changes not yet staged
git diff --cached     # Changes staged for commit
```

### Emergency Contact
If Git operations fail or repository becomes corrupted:
1. **Stop all operations**
2. **Create backup**: Copy entire working directory
3. **Document issue**: What commands were run
4. **Seek help**: Before attempting recovery

---

## Next Steps

1. **Review current uncommitted changes**
2. **Plan structured commits for Phase 1**
3. **Execute commits with human approval**
4. **Verify remote repository state**
5. **Proceed with Phase 2 development**

---

*This workflow ensures proper version control while maintaining explicit human control over all repository operations.*
