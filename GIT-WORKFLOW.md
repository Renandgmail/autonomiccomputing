# Git Workflow and Best Practices

## Current Repository Status

✅ **Successfully pushed latest improvements:**
- Repository addition and analytics functionality fixes
- Comprehensive integration test system
- Frontend runtime error fixes
- Authorization management for testing

## Git Workflow Best Practices

### 1. Branch Management

#### Main Branches
- **`master`** - Production-ready code
- **`develop`** - Integration branch for features
- **`feature/feature-name`** - Individual feature development
- **`hotfix/issue-name`** - Critical production fixes

#### Branch Commands
```bash
# Create and switch to new feature branch
git checkout -b feature/analytics-improvements

# Switch between branches
git checkout develop
git checkout master

# List all branches
git branch -a

# Delete merged branch
git branch -d feature/analytics-improvements
```

### 2. Commit Message Standards

#### Format
```
<type>(<scope>): <subject>

<body>

<footer>
```

#### Types
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code formatting (no logic changes)
- **refactor**: Code restructuring
- **test**: Adding or updating tests
- **chore**: Maintenance tasks

#### Examples
```bash
# Good commit messages
git commit -m "feat(analytics): add comprehensive file metrics dashboard"
git commit -m "fix(auth): resolve 401 unauthorized errors in API endpoints"
git commit -m "test(integration): add database validation service tests"

# Multi-line commit with description
git commit -m "feat(analytics): implement real-time contributor analytics

- Add contributor collaboration patterns analysis
- Implement team productivity metrics
- Add knowledge sharing assessment
- Include bus factor risk analysis
- Support hourly and daily activity patterns"
```

### 3. Pre-Commit Workflow

#### Before Committing
```bash
# 1. Check status and review changes
git status
git diff

# 2. Stage specific files (preferred over git add .)
git add RepoLens.Api/Controllers/AnalyticsController.cs
git add repolens-ui/src/components/analytics/

# 3. Run tests before committing
dotnet test RepoLens.Tests/
npm test --prefix repolens-ui

# 4. Commit with descriptive message
git commit -m "feat(analytics): implement contributor analytics dashboard"
```

### 4. Working with Remote Repository

#### Sync with Remote
```bash
# Fetch latest changes from remote
git fetch origin

# Pull latest changes into current branch
git pull origin master

# Push current branch to remote
git push origin feature/analytics-improvements

# Push and set upstream for new branch
git push -u origin feature/new-feature
```

#### Before Pushing to Master
```bash
# 1. Ensure you're on the correct branch
git branch

# 2. Pull latest changes
git pull origin master

# 3. Run full test suite
dotnet test
npm test --prefix repolens-ui

# 4. Push changes
git push origin master
```

### 5. Feature Development Workflow

#### Standard Feature Development
```bash
# 1. Create feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/contributor-analytics

# 2. Develop and commit incrementally
git add <files>
git commit -m "feat(analytics): add basic contributor metrics"

git add <files>
git commit -m "feat(analytics): implement team collaboration analysis"

# 3. Push feature branch
git push -u origin feature/contributor-analytics

# 4. Create Pull Request (on GitHub)
# 5. After review and approval, merge to develop
# 6. Eventually merge develop to master for release
```

### 6. Hotfix Workflow

#### Critical Production Fixes
```bash
# 1. Create hotfix branch from master
git checkout master
git pull origin master
git checkout -b hotfix/critical-analytics-bug

# 2. Fix the issue
git add <fixed-files>
git commit -m "fix(analytics): resolve null reference in dashboard"

# 3. Push hotfix
git push -u origin hotfix/critical-analytics-bug

# 4. Merge to both master AND develop
git checkout master
git merge hotfix/critical-analytics-bug
git push origin master

git checkout develop
git merge hotfix/critical-analytics-bug
git push origin develop
```

### 7. Code Review Guidelines

#### Pull Request Best Practices
- **Small, focused PRs** - Easier to review
- **Clear descriptions** - Explain what and why
- **Link issues** - Reference related GitHub issues
- **Test evidence** - Show tests pass
- **Screenshots** - For UI changes

#### PR Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Documentation update
- [ ] Refactoring

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
```

### 8. Release Management

#### Version Tagging
```bash
# Create version tag
git tag -a v1.2.0 -m "Release version 1.2.0 - Analytics improvements"

# Push tags to remote
git push origin --tags

# List tags
git tag -l
```

#### Release Process
1. **Feature freeze** on develop branch
2. **Create release branch** from develop
3. **Final testing** and bug fixes
4. **Merge to master** and tag version
5. **Deploy to production**
6. **Merge back to develop**

### 9. Emergency Procedures

#### Undo Last Commit (Not Pushed)
```bash
# Undo last commit but keep changes
git reset --soft HEAD~1

# Undo last commit and discard changes
git reset --hard HEAD~1
```

#### Revert Pushed Commit
```bash
# Create a new commit that reverses changes
git revert <commit-hash>
git push origin master
```

#### Reset to Previous State
```bash
# Reset local branch to remote state
git fetch origin
git reset --hard origin/master
```

### 10. Useful Git Commands

#### Investigation
```bash
# View commit history
git log --oneline --graph

# Show changes in specific commit
git show <commit-hash>

# Find when bug was introduced
git bisect start
git bisect bad
git bisect good <good-commit>

# Search commit messages
git log --grep="analytics"

# Show file history
git log --follow <filename>
```

#### Cleanup
```bash
# Remove untracked files
git clean -fd

# Prune deleted remote branches
git remote prune origin

# Delete merged local branches
git branch --merged | grep -v master | xargs -n 1 git branch -d
```

## Current Project Setup

### Repository Structure
```
autonomiccomputing/
├── RepoLens.Api/          # Backend API
├── RepoLens.Core/         # Business logic
├── RepoLens.Infrastructure/ # Data access
├── RepoLens.Tests/        # Test suites
├── RepoLens.Worker/       # Background services
├── repolens-ui/          # React frontend
├── docs/                 # Documentation
└── database/             # Database scripts
```

### Development Commands
```bash
# Start all services
.\start-services-optimized.bat

# Run backend tests
dotnet test RepoLens.Tests/

# Run frontend tests
npm test --prefix repolens-ui

# Build for production
dotnet build -c Release
npm run build --prefix repolens-ui
```

### Key Remotes
- **Origin**: https://github.com/Renandgmail/autonomiccomputing.git
- **Current Branch**: master
- **Last Commit**: cfa8492 (comprehensive integration test system)

## Next Steps

1. **Set up branch protection** rules on GitHub
2. **Configure CI/CD pipeline** for automated testing
3. **Establish code review** requirements
4. **Create issue templates** for bug reports and features
5. **Set up automated deployments** for staging/production

## Emergency Contacts

- **Repository Owner**: Renandgmail
- **Primary Maintainer**: Development Team
- **Critical Issues**: Create GitHub issue with `urgent` label

---

**Remember**: Always pull before pushing, test before committing, and communicate changes with the team!
