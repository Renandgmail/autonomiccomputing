using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RepoLens.Api.Controllers;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using System.Security.Claims;
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

        // Setup controller context for authorization
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim(ClaimTypes.Email, "test@example.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task GetRepositoryHistory_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var repositoryId = 1;
        var mockMetrics = CreateMockMetricsList();
        
        _mockMetricsRepository.Setup(x => x.GetMetricsHistoryAsync(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockMetrics);

        // Act
        var result = await _controller.GetRepositoryHistory(repositoryId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
        
        // Verify repository was called with correct parameters
        _mockMetricsRepository.Verify(x => x.GetMetricsHistoryAsync(
            repositoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task GetRepositoryHistory_WithNoData_ReturnsEmptyResult()
    {
        // Arrange
        var repositoryId = 1;
        _mockMetricsRepository.Setup(x => x.GetMetricsHistoryAsync(
            It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<RepositoryMetrics>());

        // Act
        var result = await _controller.GetRepositoryHistory(repositoryId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
    }

    [Fact]
    public async Task GetRepositoryTrends_WithValidRepositoryId_ReturnsSuccessResult()
    {
        // Arrange
        var repositoryId = 1;
        var mockMetrics = CreateMockMetricsList();
        var mockTrends = new Dictionary<string, object>
        {
            ["commits_trend"] = 5,
            ["files_trend"] = 10,
            ["quality_trend"] = 2.5
        };

        _mockMetricsRepository.Setup(x => x.GetTrendDataAsync(repositoryId, It.IsAny<int>()))
            .ReturnsAsync(mockMetrics);
        _mockMetricsRepository.Setup(x => x.GetSummaryTrendsAsync(repositoryId, It.IsAny<int>()))
            .ReturnsAsync(mockTrends);

        // Act
        var result = await _controller.GetRepositoryTrends(repositoryId, 30);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);

        _mockMetricsRepository.Verify(x => x.GetTrendDataAsync(repositoryId, 30), Times.Once);
        _mockMetricsRepository.Verify(x => x.GetSummaryTrendsAsync(repositoryId, 30), Times.Once);
    }

    [Fact]
    public async Task GetRepositoryTrends_WithNoData_ReturnsNoDataMessage()
    {
        // Arrange
        var repositoryId = 1;
        _mockMetricsRepository.Setup(x => x.GetTrendDataAsync(repositoryId, It.IsAny<int>()))
            .ReturnsAsync(new List<RepositoryMetrics>());
        _mockMetricsRepository.Setup(x => x.GetSummaryTrendsAsync(repositoryId, It.IsAny<int>()))
            .ReturnsAsync(new Dictionary<string, object>());

        // Act
        var result = await _controller.GetRepositoryTrends(repositoryId, 30);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
    }

    [Fact]
    public async Task GetAnalyticsSummary_WithData_ReturnsAggregatedMetrics()
    {
        // Arrange
        var mockMetrics = CreateMockMetricsList();
        _mockMetricsRepository.Setup(x => x.GetAllLatestMetricsAsync())
            .ReturnsAsync(mockMetrics);

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);

        _mockMetricsRepository.Verify(x => x.GetAllLatestMetricsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAnalyticsSummary_WithNoData_ReturnsNoMetricsMessage()
    {
        // Arrange
        _mockMetricsRepository.Setup(x => x.GetAllLatestMetricsAsync())
            .ReturnsAsync(new List<RepositoryMetrics>());

        // Act
        var result = await _controller.GetAnalyticsSummary();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);
    }

    [Fact]
    public async Task GetActivityPatterns_WithValidData_ReturnsPatterns()
    {
        // Arrange
        var repositoryId = 1;
        var mockMetrics = CreateMockMetrics();
        _mockMetricsRepository.Setup(x => x.GetLatestMetricsAsync(repositoryId))
            .ReturnsAsync(mockMetrics);

        // Act
        var result = await _controller.GetActivityPatterns(repositoryId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult?.Value);

        _mockMetricsRepository.Verify(x => x.GetLatestMetricsAsync(repositoryId), Times.Once);
    }

    [Fact]
    public async Task GetActivityPatterns_WithNoData_ReturnsNotFound()
    {
        // Arrange
        var repositoryId = 1;
        _mockMetricsRepository.Setup(x => x.GetLatestMetricsAsync(repositoryId))
            .ReturnsAsync((RepositoryMetrics?)null);

        // Act
        var result = await _controller.GetActivityPatterns(repositoryId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    private List<RepositoryMetrics> CreateMockMetricsList()
    {
        return new List<RepositoryMetrics>
        {
            CreateMockMetrics(1, DateTime.UtcNow.AddDays(-2)),
            CreateMockMetrics(2, DateTime.UtcNow.AddDays(-1)),
            CreateMockMetrics(3, DateTime.UtcNow)
        };
    }

    private RepositoryMetrics CreateMockMetrics(int id = 1, DateTime? measurementDate = null)
    {
        return new RepositoryMetrics
        {
            Id = id,
            RepositoryId = 1,
            MeasurementDate = measurementDate ?? DateTime.UtcNow,
            TotalFiles = 100 + id * 5,
            CommitsLastMonth = 50 + id * 2,
            ActiveContributors = 3 + id,
            TotalLinesOfCode = 5000 + id * 100,
            LineCoveragePercentage = 75.5 + id,
            TechnicalDebtHours = 8.5 - id * 0.5,
            DevelopmentVelocity = 15.5 + id,
            RepositorySizeBytes = 1024000 + id * 50000,
            HourlyActivityPattern = "{\"0\":0,\"1\":0,\"2\":0,\"3\":0,\"4\":0,\"5\":0,\"6\":0,\"7\":1,\"8\":5,\"9\":10,\"10\":15,\"11\":12,\"12\":8,\"13\":10,\"14\":15,\"15\":20,\"16\":18,\"17\":12,\"18\":5,\"19\":3,\"20\":1,\"21\":0,\"22\":0,\"23\":0}",
            DailyActivityPattern = "{\"Sunday\":5,\"Monday\":25,\"Tuesday\":30,\"Wednesday\":28,\"Thursday\":26,\"Friday\":22,\"Saturday\":8}",
            LanguageDistribution = "{\"C#\":45,\"TypeScript\":25,\"JavaScript\":15,\"CSS\":10,\"HTML\":5}"
        };
    }
}
