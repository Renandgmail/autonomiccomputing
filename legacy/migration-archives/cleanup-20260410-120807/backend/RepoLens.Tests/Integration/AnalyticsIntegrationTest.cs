using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Infrastructure;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore;
using RepoLens.Api.Models;

namespace RepoLens.Tests.Integration;

public class AnalyticsIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnalyticsIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRepositoryHistory_ReturnsRealDataFromDatabase()
    {
        // Arrange
        var repository = await CreateTestRepositoryAsync();
        var metrics = await CreateTestMetricsAsync(repository.Id);

        // Act
        var response = await _client.GetAsync($"/api/analytics/repository/{repository.Id}/history");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains real data from database
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("dataPoints", responseJson);
        Assert.Contains("data", responseJson);
    }

    [Fact]
    public async Task GetRepositoryTrends_ReturnsRealTrendData()
    {
        // Arrange
        var repository = await CreateTestRepositoryAsync();
        await CreateTestMetricsAsync(repository.Id);

        // Act
        var response = await _client.GetAsync($"/api/analytics/repository/{repository.Id}/trends?days=30");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains chart data
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("chartData", responseJson);
        Assert.Contains("trends", responseJson);
    }

    [Fact]
    public async Task GetLanguageTrends_ReturnsLanguageDistribution()
    {
        // Arrange
        var repository = await CreateTestRepositoryAsync();
        await CreateTestMetricsWithLanguagesAsync(repository.Id);

        // Act
        var response = await _client.GetAsync($"/api/analytics/repository/{repository.Id}/language-trends?days=30");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains language data
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("languages", responseJson);
    }

    [Fact]
    public async Task GetAnalyticsSummary_ReturnsAggregatedMetrics()
    {
        // Arrange
        var repository1 = await CreateTestRepositoryAsync();
        var repository2 = await CreateTestRepositoryAsync();
        await CreateTestMetricsAsync(repository1.Id);
        await CreateTestMetricsAsync(repository2.Id);

        // Act
        var response = await _client.GetAsync("/api/analytics/summary");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains aggregated data
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("totalRepositories", responseJson);
        Assert.Contains("averageQualityScore", responseJson);
        Assert.Contains("totalLinesOfCode", responseJson);
    }

    [Fact]
    public async Task GetActivityPatterns_ReturnsActivityData()
    {
        // Arrange
        var repository = await CreateTestRepositoryAsync();
        await CreateTestMetricsWithActivityAsync(repository.Id);

        // Act
        var response = await _client.GetAsync($"/api/analytics/repository/{repository.Id}/activity-patterns");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains activity patterns
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("patterns", responseJson);
        Assert.Contains("hourly", responseJson);
        Assert.Contains("daily", responseJson);
    }

    private async Task<Repository> CreateTestRepositoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var repository = new Repository
        {
            Name = $"TestRepo{Guid.NewGuid():N}",
            Url = $"https://github.com/test/repo{Guid.NewGuid():N}",
            Description = "Test repository for analytics integration test",
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

    private async Task<RepositoryMetrics> CreateTestMetricsAsync(int repositoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var metrics = new RepositoryMetrics
        {
            RepositoryId = repositoryId,
            MeasurementDate = DateTime.UtcNow.AddDays(-1),
            TotalLinesOfCode = 5000,
            TotalFiles = 150,
            RepositorySizeBytes = 1024000,
            CommitsLastMonth = 25,
            ActiveContributors = 3,
            TotalContributors = 5,
            MaintainabilityIndex = 85.5,
            LineCoveragePercentage = 75.0,
            DocumentationCoverage = 80.0,
            AverageCyclomaticComplexity = 2.5,
            TechnicalDebtHours = 12.5,
            BuildSuccessRate = 95.0,
            SecurityVulnerabilities = 1,
            BusFactor = 3.0,
            DevelopmentVelocity = 8.2,
            CodeSmells = 5,
            DuplicationPercentage = 3.2,
            TestPassRate = 98.0,
            BranchCoveragePercentage = 72.0,
            FunctionCoveragePercentage = 85.0
        };

        context.RepositoryMetrics.Add(metrics);
        await context.SaveChangesAsync();

        return metrics;
    }

    private async Task<RepositoryMetrics> CreateTestMetricsWithLanguagesAsync(int repositoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var languageDistribution = JsonSerializer.Serialize(new Dictionary<string, int>
        {
            {"C#", 60},
            {"TypeScript", 25},
            {"JavaScript", 15}
        });

        var metrics = new RepositoryMetrics
        {
            RepositoryId = repositoryId,
            MeasurementDate = DateTime.UtcNow.AddDays(-1),
            TotalLinesOfCode = 5000,
            TotalFiles = 150,
            RepositorySizeBytes = 1024000,
            CommitsLastMonth = 25,
            ActiveContributors = 3,
            TotalContributors = 5,
            MaintainabilityIndex = 85.5,
            LineCoveragePercentage = 75.0,
            DocumentationCoverage = 80.0,
            AverageCyclomaticComplexity = 2.5,
            TechnicalDebtHours = 12.5,
            BuildSuccessRate = 95.0,
            SecurityVulnerabilities = 1,
            BusFactor = 3.0,
            DevelopmentVelocity = 8.2,
            LanguageDistribution = languageDistribution
        };

        context.RepositoryMetrics.Add(metrics);
        await context.SaveChangesAsync();

        return metrics;
    }

    private async Task<RepositoryMetrics> CreateTestMetricsWithActivityAsync(int repositoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var hourlyActivity = JsonSerializer.Serialize(new Dictionary<string, int>
        {
            {"09", 10},
            {"10", 15},
            {"11", 20},
            {"14", 25},
            {"15", 18}
        });

        var dailyActivity = JsonSerializer.Serialize(new Dictionary<string, int>
        {
            {"Monday", 30},
            {"Tuesday", 25},
            {"Wednesday", 35},
            {"Thursday", 20},
            {"Friday", 15}
        });

        var metrics = new RepositoryMetrics
        {
            RepositoryId = repositoryId,
            MeasurementDate = DateTime.UtcNow.AddDays(-1),
            TotalLinesOfCode = 5000,
            TotalFiles = 150,
            RepositorySizeBytes = 1024000,
            CommitsLastMonth = 25,
            ActiveContributors = 3,
            TotalContributors = 5,
            MaintainabilityIndex = 85.5,
            LineCoveragePercentage = 75.0,
            DocumentationCoverage = 80.0,
            AverageCyclomaticComplexity = 2.5,
            TechnicalDebtHours = 12.5,
            BuildSuccessRate = 95.0,
            SecurityVulnerabilities = 1,
            BusFactor = 3.0,
            DevelopmentVelocity = 8.2,
            HourlyActivityPattern = hourlyActivity,
            DailyActivityPattern = dailyActivity
        };

        context.RepositoryMetrics.Add(metrics);
        await context.SaveChangesAsync();

        return metrics;
    }
}
