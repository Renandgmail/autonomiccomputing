# Contributing to RepoLens

Thank you for your interest in contributing to RepoLens! This document provides guidelines and information for contributors.

## 🤝 How to Contribute

### Reporting Issues
- **Search existing issues** first to avoid duplicates
- **Use the issue templates** when available
- **Provide detailed information** including steps to reproduce, expected behavior, and environment details
- **Include screenshots** for UI-related issues
- **Add appropriate labels** to help categorize the issue

### Suggesting Features
- **Check the roadmap** to see if the feature is already planned
- **Open a feature request** with detailed description and use cases
- **Discuss the proposal** with maintainers before starting work
- **Consider the scope** - start with small, focused features

## 🛠️ Development Setup

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm
- PostgreSQL 15+
- Git
- Visual Studio Code or Visual Studio 2022

### Local Development
1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/yourusername/repolens.git
   cd repolens
   ```

2. **Set up the database**
   ```bash
   createdb repolens_dev
   ```

3. **Configure settings**
   ```bash
   # Copy and update configuration
   cp RepoLens.Api/appsettings.json RepoLens.Api/appsettings.Development.json
   # Add your GitHub token and database connection
   ```

4. **Install dependencies**
   ```bash
   # Backend dependencies are restored automatically
   # Install frontend dependencies
   cd repolens-ui
   npm install
   cd ..
   ```

5. **Run the application**
   ```bash
   .\start-services-optimized.bat
   ```

## 📝 Coding Standards

### .NET (C#) Guidelines
- **Follow Microsoft C# Coding Conventions**
- **Use meaningful names** for variables, methods, and classes
- **Add XML documentation** for public APIs
- **Use async/await** for asynchronous operations
- **Handle exceptions** appropriately with logging
- **Write unit tests** for new functionality

#### Example:
```csharp
/// <summary>
/// Validates repository URL format and accessibility
/// </summary>
/// <param name="url">Repository URL to validate</param>
/// <returns>Validation result with success status and error message</returns>
public async Task<ValidationResult> ValidateRepositoryAsync(string url)
{
    if (string.IsNullOrWhiteSpace(url))
        return ValidationResult.Failure("Repository URL cannot be empty");
    
    try
    {
        // Validation logic here
        return ValidationResult.Success();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to validate repository {Url}", url);
        throw;
    }
}
```

### TypeScript/React Guidelines
- **Use TypeScript** for all new code
- **Define proper interfaces** for props and state
- **Use functional components** with hooks
- **Follow Material-UI patterns** for styling
- **Add error handling** and loading states
- **Write component tests** with React Testing Library

#### Example:
```typescript
interface RepositoryCardProps {
  repository: Repository;
  onMetricsClick: (id: number) => void;
  loading?: boolean;
}

export const RepositoryCard: React.FC<RepositoryCardProps> = ({
  repository,
  onMetricsClick,
  loading = false
}) => {
  const handleMetricsClick = useCallback(() => {
    onMetricsClick(repository.id);
  }, [repository.id, onMetricsClick]);

  return (
    <Card>
      <CardContent>
        <Typography variant="h6">{repository.name}</Typography>
        <Button 
          onClick={handleMetricsClick}
          disabled={loading}
          startIcon={<Analytics />}
        >
          View Metrics
        </Button>
      </CardContent>
    </Card>
  );
};
```

## 🧪 Testing Guidelines

### Backend Testing
- **Write unit tests** for business logic
- **Use Moq** for mocking dependencies
- **Test both success and failure scenarios**
- **Include integration tests** for database operations
- **Maintain >80% code coverage**

#### Example:
```csharp
[Fact]
public async Task ValidateUrlFormat_WithValidGitHubUrl_ShouldReturnSuccess()
{
    // Arrange
    var url = "https://github.com/user/repo.git";
    
    // Act
    var result = _service.ValidateUrlFormat(url);
    
    // Assert
    result.IsValid.Should().BeTrue();
    result.ErrorMessage.Should().BeNull();
}
```

### Frontend Testing
- **Test component behavior** not implementation
- **Use React Testing Library**
- **Test user interactions**
- **Mock API calls** appropriately

#### Example:
```typescript
test('renders repository name and metrics button', async () => {
  const repository: Repository = {
    id: 1,
    name: 'test-repo',
    url: 'https://github.com/user/test-repo'
  };
  
  render(<RepositoryCard repository={repository} onMetricsClick={jest.fn()} />);
  
  expect(screen.getByText('test-repo')).toBeInTheDocument();
  expect(screen.getByRole('button', { name: /view metrics/i })).toBeInTheDocument();
});
```

## 🔄 Pull Request Process

### Before Submitting
1. **Create a feature branch** from main
2. **Write clear commit messages**
3. **Run all tests** and ensure they pass
4. **Update documentation** as needed
5. **Test your changes** thoroughly

### PR Requirements
- **Clear title and description**
- **Reference related issues** (fixes #123)
- **Include screenshots** for UI changes
- **Add tests** for new functionality
- **Ensure CI passes** before requesting review

### Commit Message Format
```
type(scope): description

- feat: new feature
- fix: bug fix
- docs: documentation changes
- style: code style changes
- refactor: code refactoring
- test: adding or modifying tests
- chore: maintenance tasks

Examples:
feat(api): add repository metrics collection
fix(ui): resolve navigation menu overflow
docs(readme): update installation instructions
```

## 🏗️ Project Structure

### Backend Structure
```
RepoLens.Api/
├── Controllers/        # REST API controllers
├── Commands/          # Command pattern implementations
├── Models/            # View models and DTOs
├── Services/          # Application services
└── Hubs/             # SignalR hubs

RepoLens.Core/
├── Entities/         # Business entities
├── Repositories/     # Repository interfaces
├── Services/         # Service interfaces
└── Exceptions/       # Custom exceptions

RepoLens.Infrastructure/
├── Repositories/     # Repository implementations
├── Services/         # Service implementations
├── Git/             # Git integration
└── Migrations/      # Database migrations
```

### Frontend Structure
```
repolens-ui/src/
├── components/       # React components
│   ├── auth/        # Authentication components
│   ├── dashboard/   # Dashboard components
│   ├── repositories/ # Repository management
│   └── layout/      # Layout components
├── services/        # API service layer
├── types/          # TypeScript type definitions
└── config/         # Configuration files
```

## 🐛 Debugging Tips

### Backend Debugging
- **Use Visual Studio debugger** or VS Code
- **Enable detailed logging** in development
- **Use Postman** for API testing
- **Check database state** with pgAdmin

### Frontend Debugging
- **Use React Developer Tools**
- **Check console** for errors and warnings
- **Use network tab** for API call debugging
- **Test responsive design** with device simulation

## 📦 Release Process

### Version Management
- **Follow Semantic Versioning** (MAJOR.MINOR.PATCH)
- **Update CHANGELOG.md** with notable changes
- **Tag releases** in Git
- **Create GitHub releases** with release notes

### Deployment
- **Test on staging environment** before production
- **Run database migrations** safely
- **Monitor application health** after deployment
- **Have rollback plan** ready

## 🤔 Getting Help

### Resources
- **GitHub Discussions** for questions and ideas
- **Issue tracker** for bugs and feature requests
- **Documentation** in the docs/ directory
- **Code comments** for inline explanations

### Contact
- Open an issue for bugs or feature requests
- Start a discussion for questions or proposals
- Review existing issues before creating new ones

## 📋 Checklist for Contributors

### Before Starting Work
- [ ] Issue exists and is assigned to you
- [ ] You understand the requirements
- [ ] Development environment is set up
- [ ] You've read relevant documentation

### Before Submitting PR
- [ ] Code follows project conventions
- [ ] All tests pass locally
- [ ] New tests are added for new functionality
- [ ] Documentation is updated
- [ ] Commit messages are clear
- [ ] PR description explains the changes

### Code Quality
- [ ] No hardcoded values or secrets
- [ ] Error handling is implemented
- [ ] Logging is appropriate
- [ ] Performance considerations addressed
- [ ] Security implications considered

## 🎯 Areas for Contribution

### High Priority
- **Bug fixes** in existing functionality
- **Test coverage** improvements
- **Documentation** enhancements
- **Performance** optimizations

### Medium Priority
- **New metrics** and analytics features
- **UI/UX** improvements
- **API enhancements**
- **Integration tests**

### Future Features
- **Advanced search** functionality
- **Additional Git providers** (GitLab, Bitbucket)
- **Export capabilities**
- **Mobile responsiveness**

Thank you for contributing to RepoLens! Your efforts help make this project better for everyone. 🚀
