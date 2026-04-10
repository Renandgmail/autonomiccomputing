using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;
using RepoLens.Infrastructure.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace RepoLens.Tests.Services;

public class AnalyticsCalculationTests
{
    private readonly Mock<ILogger<FileMetricsService>> _mockLogger;
    private readonly Mock<ICommitRepository> _mockCommitRepository;
    private readonly FileMetricsService _fileMetricsService;

    public AnalyticsCalculationTests()
    {
        _mockLogger = new Mock<ILogger<FileMetricsService>>();
        _mockCommitRepository = new Mock<ICommitRepository>();
        _fileMetricsService = new FileMetricsService(_mockLogger.Object, _mockCommitRepository.Object);
    }

    [Theory]
    [InlineData(1, 10, 50, 0.95)] // Low complexity, good maintainability
    [InlineData(5, 45, 150, 0.78)] // Medium complexity
    [InlineData(15, 90, 300, 0.45)] // High complexity, poor maintainability
    public void CalculateFileHealthScore_WithVariousMetrics_ReturnsExpectedScore(
        int cyclomaticComplexity, 
        int technicalDebtMinutes, 
        int linesOfCode, 
        double expectedScoreRange)
    {
        // Arrange
        var complexity = new ComplexityMetricsResult
        {
            CyclomaticComplexity = cyclomaticComplexity,
            CognitiveComplexity = cyclomaticComplexity + 2,
            LinesOfCode = linesOfCode,
            EffectiveLines = (int)(linesOfCode * 0.8)
        };

        var quality = new QualityMetricsResult
        {
            MaintainabilityIndex = 100 - (technicalDebtMinutes / 2),
            CodeSmellCount = cyclomaticComplexity / 3,
            DocumentationCoverage = 0.7,
            TechnicalDebtMinutes = technicalDebtMinutes
        };

        var changes = new ChangePatternMetrics
        {
            ChurnRate = 1.5,
            ChangeFrequency = 0.8,
            TotalCommits = 10
        };

        // Act
        var healthScore = _fileMetricsService.CalculateFileHealthScore(complexity, quality, changes);

        // Assert
        healthScore.Should().BeInRange(0.0, 1.0);
        healthScore.Should().BeApproximately(expectedScoreRange, 0.2);
    }

    [Fact]
    public void CalculateRepositoryHealthScore_WithAllMetrics_ReturnsValidScore()
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            MaintainabilityIndex = 85.0,
            LineCoveragePercentage = 78.0,
            BuildSuccessRate = 95.0,
            SecurityVulnerabilities = 2,
            TechnicalDebtHours = 12.5,
            AverageCyclomaticComplexity = 2.8,
            DocumentationCoverage = 82.0,
            DuplicationPercentage = 3.2,
            BusFactor = 4.0,
            TestPassRate = 97.0
        };

        // Act
        var healthScore = CalculateHealthScore(metrics);

        // Assert
        healthScore.Should().BeInRange(0.0, 100.0);
        healthScore.Should().BeApproximately(85.0, 10.0); // Should be around 85% based on good metrics
    }

    [Fact]
    public void CalculateCodeQualityScore_WithQualityMetrics_ReturnsAccurateScore()
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            MaintainabilityIndex = 75.0,
            AverageCyclomaticComplexity = 3.5,
            DuplicationPercentage = 4.8,
            DocumentationCoverage = 70.0,
            TechnicalDebtHours = 18.5,
            CodeSmells = 8
        };

        // Act
        var qualityScore = CalculateCodeQualityScore(metrics);

        // Assert
        qualityScore.Should().BeInRange(0.0, 100.0);
        qualityScore.Should().BeApproximately(65.0, 15.0); // Medium quality
    }

    [Theory]
    [InlineData(1, 95, 98, 85.0)] // Excellent metrics
    [InlineData(5, 75, 85, 65.0)] // Good metrics
    [InlineData(12, 45, 60, 35.0)] // Poor metrics
    public void CalculateSecurityScore_WithVaryingVulnerabilities_ReturnsExpectedRange(
        int vulnerabilities, 
        double buildSuccessRate, 
        double testPassRate, 
        double expectedScore)
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            SecurityVulnerabilities = vulnerabilities,
            BuildSuccessRate = buildSuccessRate,
            TestPassRate = testPassRate,
            LineCoveragePercentage = testPassRate - 10,
            DocumentationCoverage = 75.0
        };

        // Act
        var securityScore = CalculateSecurityScore(metrics);

        // Assert
        securityScore.Should().BeInRange(0.0, 100.0);
        securityScore.Should().BeApproximately(expectedScore, 20.0);
    }

    [Fact]
    public void CalculateActivityLevelScore_WithCommitAndContributorData_ReturnsValidScore()
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            CommitsLastMonth = 45,
            ActiveContributors = 6,
            TotalContributors = 8,
            DevelopmentVelocity = 7.5
        };

        // Act
        var activityScore = CalculateActivityLevelScore(metrics);

        // Assert
        activityScore.Should().BeInRange(0.0, 100.0);
        activityScore.Should().BeGreaterThan(50.0); // Should indicate good activity
    }

    [Fact]
    public void CalculateMaintenanceScore_WithMaintenanceMetrics_ReturnsAccurateScore()
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            TechnicalDebtHours = 15.0,
            OutdatedDependencies = 3,
            CodeSmells = 6,
            DuplicationPercentage = 2.8,
            DocumentationCoverage = 85.0
        };

        // Act
        var maintenanceScore = CalculateMaintenanceScore(metrics);

        // Assert
        maintenanceScore.Should().BeInRange(0.0, 100.0);
        maintenanceScore.Should().BeApproximately(75.0, 15.0);
    }

    [Theory]
    [InlineData("C#", 60, "TypeScript", 25, "JavaScript", 15)]
    [InlineData("Python", 80, "JavaScript", 15, "CSS", 5)]
    public void CalculateLanguageDistribution_WithMultipleLanguages_ReturnsNormalizedPercentages(
        string lang1, int percentage1, string lang2, int percentage2, string lang3, int percentage3)
    {
        // Arrange
        var languageStats = new Dictionary<string, int>
        {
            [lang1] = percentage1,
            [lang2] = percentage2,
            [lang3] = percentage3
        };

        // Act
        var distribution = CalculateLanguageDistribution(languageStats);

        // Assert
        distribution.Should().NotBeNull();
        distribution.Values.Sum().Should().BeApproximately(100, 1); // Should sum to ~100%
        distribution[lang1].Should().BeApproximately(percentage1, 5);
        distribution[lang2].Should().BeApproximately(percentage2, 5);
        distribution[lang3].Should().BeApproximately(percentage3, 5);
    }

    [Fact]
    public void CalculateBusFactor_WithContributorDistribution_ReturnsRealisticFactor()
    {
        // Arrange
        var contributors = new List<(string Name, int Commits, double Percentage)>
        {
            ("Alice", 150, 50.0),
            ("Bob", 75, 25.0),
            ("Charlie", 45, 15.0),
            ("David", 30, 10.0)
        };

        // Act
        var busFactor = CalculateBusFactor(contributors);

        // Assert
        busFactor.Should().BeInRange(1.0, contributors.Count);
        busFactor.Should().BeLessThan(3.0); // Should indicate concentration risk
    }

    [Theory]
    [InlineData(2.0, 15.0, 8, 65)] // Low complexity, reasonable debt
    [InlineData(5.5, 45.0, 18, 45)] // High complexity, high debt
    [InlineData(1.8, 8.0, 3, 85)] // Excellent metrics
    public void CalculateOverallRepositoryScore_IntegratesAllMetrics_ReturnsWeightedScore(
        double avgComplexity, double techDebt, int vulnerabilities, int expectedScore)
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            MaintainabilityIndex = 80.0,
            AverageCyclomaticComplexity = avgComplexity,
            TechnicalDebtHours = techDebt,
            SecurityVulnerabilities = vulnerabilities,
            LineCoveragePercentage = 75.0,
            BuildSuccessRate = 90.0,
            DocumentationCoverage = 70.0,
            CommitsLastMonth = 25,
            ActiveContributors = 4
        };

        // Act
        var overallScore = CalculateOverallScore(metrics);

        // Assert
        overallScore.Should().BeInRange(0.0, 100.0);
        overallScore.Should().BeApproximately(expectedScore, 15.0);
    }

    [Fact]
    public void CalculateRecommendations_WithVariousMetrics_ReturnsActionableInsights()
    {
        // Arrange
        var metrics = new RepositoryMetrics
        {
            MaintainabilityIndex = 65.0, // Below average
            LineCoveragePercentage = 45.0, // Low coverage
            SecurityVulnerabilities = 8, // High vulnerabilities
            TechnicalDebtHours = 35.0, // High debt
            DocumentationCoverage = 30.0, // Poor documentation
            BusFactor = 1.5 // High risk
        };

        // Act
        var recommendations = CalculateRecommendations(metrics);

        // Assert
        recommendations.Should().NotBeNull();
        recommendations.Improvements.Should().Contain(item => item.Contains("test coverage"));
        recommendations.Improvements.Should().Contain(item => item.Contains("security"));
        recommendations.Improvements.Should().Contain(item => item.Contains("documentation"));
        recommendations.Improvements.Should().Contain(item => item.Contains("technical debt"));
        
        recommendations.Strengths.Should().NotBeEmpty();
        recommendations.Priority.Should().Be("High");
    }

    // Helper methods that would be in the actual service
    private double CalculateHealthScore(RepositoryMetrics metrics)
    {
        var maintainabilityWeight = 0.25;
        var coverageWeight = 0.20;
        var buildWeight = 0.15;
        var securityWeight = 0.20;
        var documentationWeight = 0.10;
        var complexityWeight = 0.10;

        var score = (metrics.MaintainabilityIndex * maintainabilityWeight) +
                   (metrics.LineCoveragePercentage * coverageWeight) +
                   (metrics.BuildSuccessRate * buildWeight) +
                   ((10 - Math.Min(metrics.SecurityVulnerabilities, 10)) * 10 * securityWeight) +
                   (metrics.DocumentationCoverage * documentationWeight) +
                   (Math.Max(0, 10 - metrics.AverageCyclomaticComplexity) * 10 * complexityWeight);

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateCodeQualityScore(RepositoryMetrics metrics)
    {
        var maintainabilityWeight = 0.30;
        var complexityWeight = 0.25;
        var duplicationWeight = 0.20;
        var documentationWeight = 0.15;
        var debtWeight = 0.10;

        var score = (metrics.MaintainabilityIndex * maintainabilityWeight) +
                   (Math.Max(0, 10 - metrics.AverageCyclomaticComplexity) * 10 * complexityWeight) +
                   (Math.Max(0, 10 - metrics.DuplicationPercentage) * 10 * duplicationWeight) +
                   (metrics.DocumentationCoverage * documentationWeight) +
                   (Math.Max(0, 100 - metrics.TechnicalDebtHours) * debtWeight);

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateSecurityScore(RepositoryMetrics metrics)
    {
        var vulnerabilityWeight = 0.40;
        var buildWeight = 0.25;
        var testWeight = 0.25;
        var coverageWeight = 0.10;

        var vulnScore = Math.Max(0, 10 - metrics.SecurityVulnerabilities) * 10;
        var score = (vulnScore * vulnerabilityWeight) +
                   (metrics.BuildSuccessRate * buildWeight) +
                   (metrics.TestPassRate * testWeight) +
                   (metrics.LineCoveragePercentage * coverageWeight);

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateActivityLevelScore(RepositoryMetrics metrics)
    {
        var commitScore = Math.Min(100, metrics.CommitsLastMonth * 2);
        var contributorScore = Math.Min(100, metrics.ActiveContributors * 15);
        var velocityScore = Math.Min(100, metrics.DevelopmentVelocity * 10);

        return (commitScore * 0.4) + (contributorScore * 0.3) + (velocityScore * 0.3);
    }

    private double CalculateMaintenanceScore(RepositoryMetrics metrics)
    {
        var debtScore = Math.Max(0, 100 - metrics.TechnicalDebtHours);
        var dependencyScore = Math.Max(0, 100 - (metrics.OutdatedDependencies * 10));
        var smellScore = Math.Max(0, 100 - (metrics.CodeSmells * 5));
        var duplicationScore = Math.Max(0, 100 - (metrics.DuplicationPercentage * 10));
        var docScore = metrics.DocumentationCoverage;

        return (debtScore * 0.25) + (dependencyScore * 0.2) + (smellScore * 0.2) + 
               (duplicationScore * 0.15) + (docScore * 0.2);
    }

    private Dictionary<string, double> CalculateLanguageDistribution(Dictionary<string, int> languageStats)
    {
        var total = languageStats.Values.Sum();
        if (total == 0) return new Dictionary<string, double>();

        return languageStats.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round((kvp.Value / (double)total) * 100, 1)
        );
    }

    private double CalculateBusFactor(List<(string Name, int Commits, double Percentage)> contributors)
    {
        if (!contributors.Any()) return 0;

        var sortedContributors = contributors.OrderByDescending(c => c.Commits).ToList();
        var cumulativePercentage = 0.0;
        var busFactor = 0;

        foreach (var contributor in sortedContributors)
        {
            busFactor++;
            cumulativePercentage += contributor.Percentage;
            
            if (cumulativePercentage >= 50.0) // 50% of commits
                break;
        }

        return Math.Max(1.0, busFactor);
    }

    private double CalculateOverallScore(RepositoryMetrics metrics)
    {
        var healthScore = CalculateHealthScore(metrics);
        var qualityScore = CalculateCodeQualityScore(metrics);
        var securityScore = CalculateSecurityScore(metrics);
        var activityScore = CalculateActivityLevelScore(metrics);
        var maintenanceScore = CalculateMaintenanceScore(metrics);

        return (healthScore * 0.25) + (qualityScore * 0.25) + (securityScore * 0.2) + 
               (activityScore * 0.15) + (maintenanceScore * 0.15);
    }

    private RecommendationsResult CalculateRecommendations(RepositoryMetrics metrics)
    {
        var improvements = new List<string>();
        var strengths = new List<string>();
        var priority = "Medium";

        if (metrics.LineCoveragePercentage < 60)
        {
            improvements.Add($"Increase test coverage from {metrics.LineCoveragePercentage:F1}% to at least 70%");
            priority = "High";
        }

        if (metrics.SecurityVulnerabilities > 5)
        {
            improvements.Add($"Address {metrics.SecurityVulnerabilities} security vulnerabilities");
            priority = "High";
        }

        if (metrics.DocumentationCoverage < 50)
        {
            improvements.Add($"Improve documentation coverage from {metrics.DocumentationCoverage:F1}%");
        }

        if (metrics.TechnicalDebtHours > 20)
        {
            improvements.Add($"Reduce technical debt from {metrics.TechnicalDebtHours:F1} hours");
        }

        if (metrics.MaintainabilityIndex > 80)
            strengths.Add("Excellent code maintainability");

        if (metrics.BusFactor > 3)
            strengths.Add("Good knowledge distribution among team members");

        return new RecommendationsResult
        {
            Improvements = improvements,
            Strengths = strengths,
            Priority = priority
        };
    }
}

public class RecommendationsResult
{
    public List<string> Improvements { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public string Priority { get; set; } = "Medium";
}
