using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RepoLens.Api.Controllers;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using Xunit;

namespace RepoLens.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IRepositoryMetricsRepository> _mockMetricsRepository;
    private readonly Mock<ILogger<AnalyticsController>> _mockLogger;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _mockMetricsRepository = new Mock<IRepositoryMetricsRepository>();
        _mockLogger = new Mock<ILogger<AnalyticsController>>();
        _controller = new AnalyticsController(_mockMetricsRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRepositoryHistory_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var repositoryId = 1;
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        
        var mockMetrics = new List<RepositoryMetrics>
        {
            new RepositoryMetrics
            {
                Id = 1,
                RepositoryId = repositoryId,
                MeasurementDate = DateTime.UtcNow.AddDays(-15),
                CommitsLastMonth = 25,
                TotalFiles = 150,
                RepositorySizeBytes = 1024000,
                ActiveContributors = 3,
                MaintainabilityIndex = 85.5,
                LineCoveragePercentage = 75.0,
                DocumentationCoverage = 80.0,
                AverageCyclomaticComplexity = 2.5,
                TechnicalDebtHours = 12.5,
                BuildSuccessRate = 95.0,
                SecurityVulnerabilities = 1,
                BusFactor = 3.0,
                TotalLinesOfCode = 5000,
                DevelopmentVelocity = 8.2
            },
            new RepositoryMetrics
            {
                Id = 2,
                RepositoryId = repositoryId,
                MeasurementDate = DateTime.UtcNow.AddDays(-10),
                CommitsLastMonth = 30,
                TotalFiles = 155,
                RepositorySizeBytes = 1100000,
                ActiveContributors = 4,
                MaintainabilityIndex = 87.2,
                LineCoveragePercentage = 78.0,
                DocumentationCoverage = 85.0,
                AverageCyclomaticComplexity = 2.0,
                TechnicalDebtHours = 11.0,
                BuildSuccessRate = 98.0,
                SecurityVulnerabilities = 0,
                BusFactor = 4.0,
                TotalLinesOfCode = 5200,
                DevelopmentVelocity = 9.1
            }
        };

        _mockMetricsRepository
            .Setup(x => x.GetMetricsHistoryAsync(repositoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockMetrics);

        // Act
        var result = await _controller.GetRepositoryHistory(repositoryId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        // Verify the repository was called with correct parameters
        _mockMetricsRepository.Verify(
            x => x.GetMetricsHistoryAsync(repositoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetRepositoryTrends_WithValidData_ReturnsChartData()
    {
        // Arrange
        var repositoryId = 1;
        var days = 30;
        
        var mockTrendData = new List<RepositoryMetrics>
        {
            new RepositoryMetrics
            {
                Id = 1,
                RepositoryId = repositoryId,
                MeasurementDate = DateTime.UtcNow.AddDays(-20),
                CommitsLastMonth = 20,
                TotalFiles = 100,
                MaintainabilityIndex = 80.0,
                LineCoveragePercentage = 75.0,
                DocumentationCoverage = 70.0,
                AverageCyclomaticComplexity = 3.0,
                TechnicalDebtHours = 15.0,
                BuildSuccessRate = 90.0,
                SecurityVulnerabilities = 2,
                BusFactor = 2.0,
                ActiveContributors = 2,
                RepositorySizeBytes = 900000
            },
            new RepositoryMetrics
            {
                Id = 2,
                RepositoryId = repositoryId,
                MeasurementDate = DateTime.UtcNow.AddDays(-10),
                CommitsLastMonth = 25,
                TotalFiles = 110,
                MaintainabilityIndex = 85.0,
                LineCoveragePercentage = 80.0,
                DocumentationCoverage = 75.0,
                AverageCyclomaticComplexity = 2.5,
                TechnicalDebtHours = 10.0,
                BuildSuccessRate = 95.0,
                SecurityVulnerabilities = 1,
                BusFactor = 3.0,
                ActiveContributors = 3,
                RepositorySizeBytes = 1000000
            }
        };

        var mockSummaryTrends = new Dictionary<string, object>
        {
            ["commits_trend"] = 5,
            ["files_trend"] = 10,
            ["quality_trend"] = 5.0,
            ["period_days"] = days
        };

        _mockMetricsRepository
            .Setup(x => x.GetTrendDataAsync(repositoryId, days))
            .ReturnsAsync(mockTrendData);

        _mockMetricsRepository
            .Setup(x => x.GetSummaryTrendsAsync(repositoryId, days))
            .ReturnsAsync(mockSummaryTrends);

        // Act
        var result = await _controller.GetRepositoryTrends(repositoryId, days);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        // Verify both methods were called
        _mockMetricsRepository.Verify(x => x.GetTrendDataAsync(repositoryId, days), Times.Once);
        _mockMetricsRepository.Verify(x => x.GetSummaryTrendsAsync(repositoryId, days), Times.Once);
    }

    [Fact]
    public async Task GetRepositoryTrends_WithNoData_ReturnsEmptyResult()
    {
        // Arrange
        var repositoryId = 1;
        var days = 30;
        
        _mockMetricsRepository
            .Setup(x => x.GetTrendDataAsync(repositoryId, days))
            .ReturnsAsync(new List<RepositoryMetrics>());

        _mockMetricsRepository
            .Setup(x => x.GetSummaryTrendsAsync(repositoryId, days))
            .ReturnsAsync(new Dictionary<string, object>());

        // Act
        var result = await _controller.GetRepositoryTrends(repositoryId, days);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetLanguageTrends_WithValidData_ReturnsLanguageDistribution()
    {
        // Arrange
        var repositoryId = 1;
        var days = 30;
        var languageJson = """{"C#": 60, "TypeScript": 25, "JavaScript": 15}""";
        
        var mockTrendData = new List<RepositoryMetrics>
        {
            new RepositoryMetrics
            {
                Id = 1,
                RepositoryId = repositoryId,
                MeasurementDate = DateTime.UtcNow.AddDays(-5),
                LanguageDistribution = languageJson
            }
        };

        _mockMetricsRepository
            .Setup(x => x.GetTrendDataAsync(repositoryId, days))
            .ReturnsAsync(mockTrendData);

        // Act
        var result = await _controller.GetLanguageTrends(repositoryId, days);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        _mockMetricsRepository.Verify(x => x.GetTrendDataAsync(repositoryId, days), Times.Once);
    }

    [Fact]
    public async Task GetAnalyticsSummary_WithMetrics_ReturnsAggregatedData()
    {
        // Arrange
        var mockLatestMetrics = new List<RepositoryMetrics>
        {
            new RepositoryMetrics
            {
                Id = 1,
                RepositoryId = 1,
                MaintainabilityIndex = 85.0,
                LineCoveragePercentage = 75.0,
                DocumentationCoverage = 80.0,
                AverageCyclomaticComplexity = 2.0,
                TechnicalDebtHours = 10.5,
                BuildSuccessRate = 95.0,
                SecurityVulnerabilities = 1,
                BusFactor = 3.0,
                TotalLinesOfCode = 5000,
                TotalFiles = 150,
                CommitsLastMonth = 25,
                ActiveContributors = 3,
                MeasurementDate = DateTime.UtcNow
            },
            new RepositoryMetrics
            {
                Id = 2,
                RepositoryId = 2,
                MaintainabilityIndex = 80.0,
                LineCoveragePercentage = 80.0,
                DocumentationCoverage = 75.0,
                AverageCyclomaticComplexity = 2.5,
                TechnicalDebtHours = 8.0,
                BuildSuccessRate = 90.0,
                SecurityVulnerabilities = 2,
                BusFactor = 2.0,
                TotalLinesOfCode = 3000,
                TotalFiles = 100,
                CommitsLastMonth = 15,
                ActiveContributors = 2,
                MeasurementDate = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockMetricsRepository
            .Setup(x => x.GetAllLatestMetricsAsync())
            .ReturnsAsync(mockLatestMetrics);

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        _mockMetricsRepository.Verify(x => x.GetAllLatestMetricsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAnalyticsSummary_WithNoMetrics_ReturnsEmptyMessage()
    {
        // Arrange
        _mockMetricsRepository
            .Setup(x => x.GetAllLatestMetricsAsync())
            .ReturnsAsync(new List<RepositoryMetrics>());

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetActivityPatterns_WithValidData_ReturnsActivityData()
    {
        // Arrange
        var repositoryId = 1;
        var hourlyJson = """{"09": 10, "10": 15, "11": 20, "14": 25}""";
        var dailyJson = """{"Monday": 30, "Tuesday": 25, "Wednesday": 35, "Thursday": 20, "Friday": 15}""";
        
        var mockMetrics = new RepositoryMetrics
        {
            Id = 1,
            RepositoryId = repositoryId,
            HourlyActivityPattern = hourlyJson,
            DailyActivityPattern = dailyJson,
            MeasurementDate = DateTime.UtcNow
        };

        _mockMetricsRepository
            .Setup(x => x.GetLatestMetricsAsync(repositoryId))
            .ReturnsAsync(mockMetrics);

        // Act
        var result = await _controller.GetActivityPatterns(repositoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        
        _mockMetricsRepository.Verify(x => x.GetLatestMetricsAsync(repositoryId), Times.Once);
    }

    [Fact]
    public async Task GetActivityPatterns_WithNoMetrics_ReturnsNotFound()
    {
        // Arrange
        var repositoryId = 1;
        
        _mockMetricsRepository
            .Setup(x => x.GetLatestMetricsAsync(repositoryId))
            .ReturnsAsync((RepositoryMetrics?)null);

        // Act
        var result = await _controller.GetActivityPatterns(repositoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
        Assert.False(response.Success);
        
        _mockMetricsRepository.Verify(x => x.GetLatestMetricsAsync(repositoryId), Times.Once);
    }
}
