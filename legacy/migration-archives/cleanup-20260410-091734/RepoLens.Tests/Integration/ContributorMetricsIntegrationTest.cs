using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using RepoLens.Infrastructure;
using Xunit;

namespace RepoLens.Tests.Integration;

public class ContributorMetricsIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ContributorMetricsIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ContributorAnalyticsService_CalculateContributorMetricsAsync_WithMultipleContributors_ReturnsComprehensiveMetrics()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);

        // Act
        var metrics = await contributorAnalyticsService.CalculateContributorMetricsAsync(repository.Id);

        // Assert
        Assert.NotNull(metrics);
        Assert.True(metrics.Count > 0);

        // Verify first contributor
        var firstContributor = metrics.First();
        Assert.NotNull(firstContributor.ContributorEmail);
        Assert.NotNull(firstContributor.ContributorName);
        Assert.Equal(repository.Id, firstContributor.RepositoryId);
        
        // Basic metrics
        Assert.True(firstContributor.CommitCount > 0);
        Assert.True(firstContributor.LinesAdded > 0);
        Assert.True(firstContributor.FilesModified > 0);
        
        // Advanced metrics
        Assert.True(firstContributor.ContributionPercentage >= 0);
        Assert.True(firstContributor.WorkingDays > 0);
        Assert.True(firstContributor.AverageCommitSize >= 0);
        
        // Code ownership metrics
        Assert.True(firstContributor.CodeOwnershipPercentage >= 0);
        Assert.True(firstContributor.UniqueFilesTouched > 0);
        
        // Quality metrics
        Assert.True(firstContributor.AvgCommitMessageLength > 0);
        Assert.True(firstContributor.CommitMessageQualityScore >= 0);
        
        // Team dynamics
        Assert.True(firstContributor.DaysActive > 0);
        Assert.True(firstContributor.RetentionScore >= 0 && firstContributor.RetentionScore <= 1);
        
        // Activity patterns should be valid JSON
        Assert.NotNull(firstContributor.HourlyActivityPattern);
        Assert.NotNull(firstContributor.WeeklyActivityPattern);
        Assert.NotNull(firstContributor.MonthlyActivityPattern);
    }

    [Fact]
    public async Task ContributorAnalyticsService_AnalyzeContributorPatternsAsync_ReturnsDetailedPatterns()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);
        
        var contributorEmail = "john.doe@example.com";

        // Act
        var patterns = await contributorAnalyticsService.AnalyzeContributorPatternsAsync(repository.Id, contributorEmail);

        // Assert
        Assert.NotNull(patterns);
        Assert.Equal(contributorEmail, patterns.ContributorEmail);
        Assert.True(patterns.CommitFrequency >= 0);
        Assert.NotNull(patterns.CodeOwnership);
        Assert.NotNull(patterns.ExpertiseAreas);
        Assert.NotNull(patterns.WeeklyPattern);
        Assert.NotNull(patterns.HourlyPattern);
        Assert.True(patterns.ConsistencyScore >= 0 && patterns.ConsistencyScore <= 1);
        Assert.NotNull(patterns.PreferredFileTypes);
    }

    [Fact]
    public async Task ContributorAnalyticsService_AnalyzeTeamCollaborationAsync_ReturnsTeamMetrics()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);

        // Act
        var collaboration = await contributorAnalyticsService.AnalyzeTeamCollaborationAsync(repository.Id);

        // Assert
        Assert.NotNull(collaboration);
        Assert.True(collaboration.TotalContributors > 0);
        Assert.True(collaboration.CollaborationIndex >= 0 && collaboration.CollaborationIndex <= 1);
        Assert.NotNull(collaboration.CodeReviewNetwork);
        Assert.NotNull(collaboration.KnowledgeSharingScore);
        Assert.True(collaboration.TeamCohesion >= 0);
        Assert.NotNull(collaboration.IsolatedContributors);
        Assert.NotNull(collaboration.MentoringRelationships);
        Assert.True(collaboration.CommunicationEffectiveness >= 0);
    }

    [Fact]
    public async Task ContributorAnalyticsService_AssessProductivityAsync_ReturnsProductivityMetrics()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);

        // Act
        var productivity = await contributorAnalyticsService.AssessProductivityAsync(repository.Id, TimeSpan.FromDays(30));

        // Assert
        Assert.NotNull(productivity);
        Assert.NotNull(productivity.VelocityTrends);
        Assert.NotNull(productivity.QualityScores);
        Assert.NotNull(productivity.DeliveryConsistency);
        Assert.True(productivity.TeamProductivity >= 0);
        Assert.NotNull(productivity.HighPerformers);
        Assert.NotNull(productivity.ImprovementCandidates);
        Assert.NotNull(productivity.EfficiencyRatios);
    }

    [Fact]
    public async Task ContributorAnalyticsService_AnalyzeTeamRisksAsync_ReturnsRiskAnalysis()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);

        // Act
        var risks = await contributorAnalyticsService.AnalyzeTeamRisksAsync(repository.Id);

        // Assert
        Assert.NotNull(risks);
        Assert.True(risks.BusFactor >= 1);
        Assert.NotNull(risks.SinglePointsOfFailure);
        Assert.NotNull(risks.KnowledgeConcentration);
        Assert.NotNull(risks.CriticalContributors);
        Assert.NotNull(risks.KnowledgeGaps);
        Assert.True(risks.TeamResilience >= 0 && risks.TeamResilience <= 1);
        Assert.NotNull(risks.RiskMitigationRecommendations);
    }

    [Fact]
    public async Task ContributorAnalyticsService_RecognizeActivityPatternsAsync_ReturnsActivityPatterns()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);

        // Act
        var patterns = await contributorAnalyticsService.RecognizeActivityPatternsAsync(repository.Id);

        // Assert
        Assert.NotNull(patterns);
        Assert.NotNull(patterns.WorkingHours);
        Assert.NotNull(patterns.SeasonalPatterns);
        Assert.NotNull(patterns.CollaborationStyles);
        Assert.NotNull(patterns.FocusTimeRatios);
        Assert.NotNull(patterns.PreferredTaskTypes);
        Assert.NotNull(patterns.MultitaskingTendency);
    }

    [Fact]
    public async Task ContributorAnalyticsService_TrackGrowthAsync_ReturnsGrowthTracking()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var contributorAnalyticsService = scope.ServiceProvider.GetRequiredService<IContributorAnalyticsService>();
        
        var repository = await CreateTestRepositoryAsync();
        await CreateTestCommitsAsync(repository.Id);
        
        var contributorEmail = "john.doe@example.com";

        // Act
        var growth = await contributorAnalyticsService.TrackGrowthAsync(repository.Id, contributorEmail);

        // Assert
        Assert.NotNull(growth);
        Assert.Equal(contributorEmail, growth.ContributorEmail);
        Assert.NotNull(growth.SkillDevelopment);
        Assert.NotNull(growth.DomainExpansion);
        Assert.NotNull(growth.ImpactEvolution);
        Assert.True(growth.LearningVelocity >= 0);
        Assert.NotNull(growth.EmergingSkills);
        Assert.True(growth.MentorshipCapability >= 0);
        Assert.NotNull(growth.GrowthRecommendations);
    }

    private async Task<Repository> CreateTestRepositoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var repository = new Repository
        {
            Name = $"TestRepo{Guid.NewGuid():N}",
            Url = $"https://github.com/test/repo{Guid.NewGuid():N}",
            Description = "Test repository for contributor metrics integration test",
            DefaultBranch = "main",
            CreatedAt = DateTime.UtcNow,
            Status = RepositoryStatus.Active,
            IsPrivate = false,
            AutoSync = true,
            SyncIntervalMinutes = 60,
            ProviderType = ProviderType.GitHub,
            OwnerId = 1 // Assuming test user exists
        };

        context.Repositories.Add(repository);
        await context.SaveChangesAsync();

        return repository;
    }

    private async Task CreateTestCommitsAsync(int repositoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var contributors = new[]
        {
            "john.doe@example.com",
            "jane.smith@example.com",
            "bob.wilson@example.com"
        };

        var commits = new List<Commit>();
        var baseDate = DateTime.UtcNow.AddDays(-30);

        for (int i = 0; i < 50; i++)
        {
            var contributor = contributors[i % contributors.Length];
            var commitDate = baseDate.AddDays(i % 30).AddHours(Random.Shared.Next(8, 18));
            
            var commit = new Commit
            {
                RepositoryId = repositoryId,
                Sha = Guid.NewGuid().ToString(),
                Author = contributor,
                Message = GenerateCommitMessage(i),
                Timestamp = commitDate
            };

            commits.Add(commit);
        }

        context.Commits.AddRange(commits);
        await context.SaveChangesAsync();
    }

    private static string GenerateCommitMessage(int index)
    {
        var messages = new[]
        {
            "Add new feature for user authentication",
            "Fix bug in data validation logic",
            "Refactor database connection handling",
            "Update documentation for API endpoints",
            "Implement unit tests for service layer",
            "Optimize performance in query processing",
            "Add error handling for edge cases",
            "Update UI components for better UX",
            "Configure CI/CD pipeline",
            "Add security improvements"
        };

        return messages[index % messages.Length];
    }

    private static string GenerateFilesChanged(int index)
    {
        var files = new[]
        {
            "src/Services/UserService.cs",
            "src/Controllers/AuthController.cs",
            "src/Models/User.cs",
            "tests/UserServiceTests.cs",
            "docs/API.md",
            "src/Data/DatabaseContext.cs",
            "src/Utils/ValidationHelper.cs",
            "frontend/components/Login.tsx",
            "scripts/deploy.sh",
            "src/Security/TokenValidator.cs"
        };

        var selectedFiles = new List<string>();
        var fileCount = Random.Shared.Next(1, 4); // 1-3 files per commit

        for (int i = 0; i < fileCount; i++)
        {
            var file = files[(index + i) % files.Length];
            if (!selectedFiles.Contains(file))
            {
                selectedFiles.Add(file);
            }
        }

        return string.Join(",", selectedFiles);
    }
}
