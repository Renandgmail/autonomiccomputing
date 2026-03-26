using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Repositories;
using System.Text.Json;

namespace RepoLens.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IRepositoryMetricsRepository _metricsRepository;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IRepositoryMetricsRepository metricsRepository,
        ILogger<AnalyticsController> logger)
    {
        _metricsRepository = metricsRepository;
        _logger = logger;
    }

    [HttpGet("repository/{repositoryId}/history")]
    public async Task<IActionResult> GetRepositoryHistory(
        int repositoryId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            _logger.LogInformation("Getting repository history for {RepositoryId} from {StartDate} to {EndDate}", 
                repositoryId, start, end);

            var history = await _metricsRepository.GetMetricsHistoryAsync(repositoryId, start, end);
            
            var historyData = history.Select(h => new
            {
                date = h.MeasurementDate.ToString("yyyy-MM-dd"),
                timestamp = h.MeasurementDate,
                commits = h.CommitsLastMonth,
                files = h.TotalFiles,
                size = h.RepositorySizeBytes,
                contributors = h.ActiveContributors,
                qualityScore = Math.Round(h.CodeQualityScore, 2),
                healthScore = Math.Round(h.ProjectHealthScore, 2),
                linesOfCode = h.TotalLinesOfCode,
                testCoverage = h.LineCoveragePercentage,
                technicalDebt = h.TechnicalDebtHours,
                velocity = h.DevelopmentVelocity
            }).ToList();

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                period = new { start, end },
                dataPoints = historyData.Count,
                data = historyData
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository history for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve repository history"));
        }
    }

    [HttpGet("repository/{repositoryId}/trends")]
    public async Task<IActionResult> GetRepositoryTrends(int repositoryId, [FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Getting repository trends for {RepositoryId} over {Days} days", repositoryId, days);

            var trendData = await _metricsRepository.GetTrendDataAsync(repositoryId, days);
            var summaryTrends = await _metricsRepository.GetSummaryTrendsAsync(repositoryId, days);

            if (!trendData.Any())
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    repositoryId,
                    message = "No historical data available",
                    trends = new { },
                    chartData = new { }
                }));
            }

            // Prepare chart data
            var chartData = new
            {
                commits = trendData.Select(t => new { 
                    date = t.MeasurementDate.ToString("yyyy-MM-dd"), 
                    value = t.CommitsLastMonth 
                }).ToList(),
                files = trendData.Select(t => new { 
                    date = t.MeasurementDate.ToString("yyyy-MM-dd"), 
                    value = t.TotalFiles 
                }).ToList(),
                quality = trendData.Select(t => new { 
                    date = t.MeasurementDate.ToString("yyyy-MM-dd"), 
                    value = Math.Round(t.CodeQualityScore, 2) 
                }).ToList(),
                contributors = trendData.Select(t => new { 
                    date = t.MeasurementDate.ToString("yyyy-MM-dd"), 
                    value = t.ActiveContributors 
                }).ToList(),
                size = trendData.Select(t => new { 
                    date = t.MeasurementDate.ToString("yyyy-MM-dd"), 
                    value = t.RepositorySizeBytes 
                }).ToList()
            };

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                period = new { days, dataPoints = trendData.Count },
                trends = summaryTrends,
                chartData
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository trends for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve repository trends"));
        }
    }

    [HttpGet("repository/{repositoryId}/language-trends")]
    public async Task<IActionResult> GetLanguageTrends(int repositoryId, [FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Getting language trends for {RepositoryId} over {Days} days", repositoryId, days);

            var trendData = await _metricsRepository.GetTrendDataAsync(repositoryId, days);
            
            if (!trendData.Any())
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    repositoryId,
                    message = "No language data available",
                    languages = new { }
                }));
            }

            // Parse language distribution from the latest measurement
            var latest = trendData.Last();
            var languageData = new Dictionary<string, int>();
            
            if (!string.IsNullOrEmpty(latest.LanguageDistribution))
            {
                try
                {
                    languageData = JsonSerializer.Deserialize<Dictionary<string, int>>(latest.LanguageDistribution) 
                                   ?? new Dictionary<string, int>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse language distribution for repository {RepositoryId}", repositoryId);
                }
            }

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                period = new { days, dataPoints = trendData.Count },
                languages = languageData,
                lastUpdated = latest.MeasurementDate
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get language trends for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve language trends"));
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetAnalyticsSummary()
    {
        try
        {
            _logger.LogInformation("Getting analytics summary");

            var latestMetrics = await _metricsRepository.GetAllLatestMetricsAsync();
            
            if (!latestMetrics.Any())
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    message = "No metrics available",
                    summary = new { }
                }));
            }

            var summary = new
            {
                totalRepositories = latestMetrics.Count,
                averageQualityScore = Math.Round(latestMetrics.Average(m => m.CodeQualityScore), 2),
                averageHealthScore = Math.Round(latestMetrics.Average(m => m.ProjectHealthScore), 2),
                totalLinesOfCode = latestMetrics.Sum(m => m.TotalLinesOfCode),
                totalFiles = latestMetrics.Sum(m => m.TotalFiles),
                totalCommitsLastMonth = latestMetrics.Sum(m => m.CommitsLastMonth),
                activeContributors = latestMetrics.Sum(m => m.ActiveContributors),
                averageTestCoverage = Math.Round(latestMetrics.Average(m => m.LineCoveragePercentage), 2),
                totalTechnicalDebt = Math.Round(latestMetrics.Sum(m => m.TechnicalDebtHours), 2),
                lastUpdated = latestMetrics.Max(m => m.MeasurementDate)
            };

            return Ok(ApiResponse<object>.SuccessResult(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analytics summary");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve analytics summary"));
        }
    }

    [HttpGet("repository/{repositoryId}/activity-patterns")]
    public async Task<IActionResult> GetActivityPatterns(int repositoryId)
    {
        try
        {
            _logger.LogInformation("Getting activity patterns for {RepositoryId}", repositoryId);

            var latest = await _metricsRepository.GetLatestMetricsAsync(repositoryId);
            
            if (latest == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("No metrics found for repository"));
            }

            var hourlyActivity = new Dictionary<string, int>();
            var dailyActivity = new Dictionary<string, int>();

            // Parse activity patterns
            if (!string.IsNullOrEmpty(latest.HourlyActivityPattern))
            {
                try
                {
                    hourlyActivity = JsonSerializer.Deserialize<Dictionary<string, int>>(latest.HourlyActivityPattern) 
                                     ?? new Dictionary<string, int>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse hourly activity pattern for repository {RepositoryId}", repositoryId);
                }
            }

            if (!string.IsNullOrEmpty(latest.DailyActivityPattern))
            {
                try
                {
                    dailyActivity = JsonSerializer.Deserialize<Dictionary<string, int>>(latest.DailyActivityPattern) 
                                    ?? new Dictionary<string, int>();
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse daily activity pattern for repository {RepositoryId}", repositoryId);
                }
            }

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                patterns = new
                {
                    hourly = hourlyActivity,
                    daily = dailyActivity
                },
                lastUpdated = latest.MeasurementDate
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity patterns for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve activity patterns"));
        }
    }
}
