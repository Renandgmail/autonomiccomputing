using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;
using System.Text.Json;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for repository analytics and metrics data
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // Temporarily disabled for testing
public class AnalyticsController : ControllerBase
{
    private readonly IRepositoryMetricsRepository _metricsRepository;
    private readonly IFileMetricsRepository _fileMetricsRepository;
    private readonly IContributorMetricsRepository _contributorMetricsRepository;
    private readonly IFileMetricsService _fileMetricsService;
    private readonly IContributorAnalyticsService _contributorAnalyticsService;
    private readonly IVocabularyExtractionService _vocabularyService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IRepositoryMetricsRepository metricsRepository,
        IFileMetricsRepository fileMetricsRepository,
        IContributorMetricsRepository contributorMetricsRepository,
        IFileMetricsService fileMetricsService,
        IContributorAnalyticsService contributorAnalyticsService,
        IVocabularyExtractionService vocabularyService,
        ILogger<AnalyticsController> logger)
    {
        _metricsRepository = metricsRepository;
        _fileMetricsRepository = fileMetricsRepository;
        _contributorMetricsRepository = contributorMetricsRepository;
        _fileMetricsService = fileMetricsService;
        _contributorAnalyticsService = contributorAnalyticsService;
        _vocabularyService = vocabularyService;
        _logger = logger;
    }

    /// <summary>
    /// Gets detailed metrics history for a repository over a time period
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="startDate">Start date for history (defaults to 30 days ago)</param>
    /// <param name="endDate">End date for history (defaults to now)</param>
    /// <returns>Time series data of repository metrics</returns>
    /// <response code="200">Returns the repository metrics history</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Gets repository trend analysis with chart-ready data
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <returns>Repository trends with chart data for commits, files, quality, and contributors</returns>
    /// <response code="200">Returns the repository trends</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Gets language distribution trends for a repository over time
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="days">Number of days to analyze (default: 30)</param>
    /// <returns>Language distribution data with percentages</returns>
    /// <response code="200">Returns the language trends</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Gets overall analytics summary across all repositories
    /// </summary>
    /// <returns>Aggregate metrics and statistics summary</returns>
    /// <response code="200">Returns the analytics summary</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Gets activity patterns for a repository showing hourly and daily commit distributions
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Activity patterns with hourly and daily breakdowns</returns>
    /// <response code="200">Returns the activity patterns</response>
    /// <response code="404">Repository metrics not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
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

    /// <summary>
    /// Gets file-level metrics and quality assessment for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Page size for pagination (default: 20)</param>
    /// <param name="sortBy">Sort criteria: complexity, quality, debt, health (default: health)</param>
    /// <param name="sortOrder">Sort order: asc, desc (default: desc)</param>
    /// <returns>File metrics with quality scores, complexity analysis, and technical debt</returns>
    /// <response code="200">Returns the file metrics</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("repository/{repositoryId}/files")]
    public async Task<IActionResult> GetFileMetrics(
        int repositoryId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "health",
        [FromQuery] string sortOrder = "desc")
    {
        try
        {
            _logger.LogInformation("Getting file metrics for {RepositoryId}, page {Page}, sortBy {SortBy}", 
                repositoryId, page, sortBy);

            var fileMetrics = await _fileMetricsRepository.GetRepositoryFileMetricsAsync(repositoryId);
            
            if (!fileMetrics.Any())
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    repositoryId,
                    message = "No file metrics available - analysis may be in progress",
                    files = new List<object>(),
                    pagination = new { page, pageSize, total = 0, totalPages = 0 }
                }));
            }

            // Apply sorting
            var sortedMetrics = sortBy.ToLower() switch
            {
                "complexity" => sortOrder.ToLower() == "asc" 
                    ? fileMetrics.OrderBy(f => f.CyclomaticComplexity) 
                    : fileMetrics.OrderByDescending(f => f.CyclomaticComplexity),
                "quality" => sortOrder.ToLower() == "asc" 
                    ? fileMetrics.OrderBy(f => f.MaintainabilityIndex) 
                    : fileMetrics.OrderByDescending(f => f.MaintainabilityIndex),
                "debt" => sortOrder.ToLower() == "asc" 
                    ? fileMetrics.OrderBy(f => f.TechnicalDebtMinutes) 
                    : fileMetrics.OrderByDescending(f => f.TechnicalDebtMinutes),
                _ => sortOrder.ToLower() == "asc" 
                    ? fileMetrics.OrderBy(f => f.QualityScore) 
                    : fileMetrics.OrderByDescending(f => f.QualityScore)
            };

            var totalFiles = sortedMetrics.Count();
            var totalPages = (int)Math.Ceiling(totalFiles / (double)pageSize);
            var pagedMetrics = sortedMetrics.Skip((page - 1) * pageSize).Take(pageSize);

            var fileData = pagedMetrics.Select(f => new
            {
                id = f.Id,
                filePath = f.FilePath,
                fileName = Path.GetFileName(f.FilePath),
                language = f.PrimaryLanguage,
                size = new
                {
                    lines = f.LineCount,
                    bytes = f.FileSizeBytes,
                    effectiveLines = f.EffectiveLineCount
                },
                complexity = new
                {
                    cyclomatic = Math.Round(f.CyclomaticComplexity, 2),
                    cognitive = Math.Round(f.CognitiveComplexity, 2),
                    maxNesting = f.NestingDepth,
                    averageMethodLength = f.AverageMethodLength
                },
                quality = new
                {
                    maintainabilityIndex = Math.Round(f.MaintainabilityIndex, 2),
                    codeSmells = f.CodeSmellCount,
                    technicalDebt = Math.Round(f.TechnicalDebtMinutes, 1),
                    commentDensity = Math.Round(f.CommentDensity, 2),
                    duplicationPercentage = Math.Round(f.DuplicationPercentage, 2)
                },
                security = new
                {
                    hotspots = f.SecurityHotspots,
                    vulnerabilityCount = f.VulnerabilityCount
                },
                health = new
                {
                    overallScore = Math.Round(f.QualityScore, 2),
                    bugProneness = Math.Round(f.BugProneness, 2),
                    stabilityScore = Math.Round(f.StabilityScore, 2),
                    maturityScore = Math.Round(f.MaturityScore, 2)
                },
                change = new
                {
                    churnRate = Math.Round(f.ChurnRate, 2),
                    changeFrequency = f.ChangeFrequency,
                    lastModified = f.LastCommit
                },
                analysis = new
                {
                    lastAnalyzed = f.LastAnalyzed,
                    version = f.Id.ToString()
                }
            }).ToList();

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                files = fileData,
                pagination = new { page, pageSize, total = totalFiles, totalPages },
                summary = new
                {
                    averageHealth = Math.Round(fileMetrics.Average(f => f.QualityScore), 2),
                    averageComplexity = Math.Round(fileMetrics.Average(f => f.CyclomaticComplexity), 2),
                    totalTechnicalDebt = Math.Round(fileMetrics.Sum(f => f.TechnicalDebtMinutes) / 60.0, 1), // Convert to hours
                    highRiskFiles = fileMetrics.Count(f => f.QualityScore < 30.0),
                    languageBreakdown = fileMetrics.GroupBy(f => f.PrimaryLanguage)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count())
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file metrics for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve file metrics"));
        }
    }

    /// <summary>
    /// Gets detailed analysis for a specific file including recommendations
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filePath">URL-encoded file path</param>
    /// <returns>Detailed file analysis with complexity, quality, security, and improvement recommendations</returns>
    /// <response code="200">Returns the detailed file analysis</response>
    /// <response code="404">File not found or not analyzed</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("repository/{repositoryId}/files/{*filePath}")]
    public async Task<IActionResult> GetFileDetails(int repositoryId, string filePath)
    {
        try
        {
            var decodedPath = Uri.UnescapeDataString(filePath);
            _logger.LogInformation("Getting file details for {RepositoryId}/{FilePath}", repositoryId, decodedPath);

            var fileMetric = await _fileMetricsRepository.GetFileMetricsAsync(repositoryId, decodedPath);
            
            if (fileMetric == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult($"File metrics not found for {decodedPath}"));
            }

            // Get additional analysis if available
            var recommendations = new List<string>();
            var codeSmellDetails = new List<string>();

            // Parse code smells if available
            if (!string.IsNullOrEmpty(fileMetric.ChangePatterns))
            {
                try
                {
                    codeSmellDetails = JsonSerializer.Deserialize<List<string>>(fileMetric.ChangePatterns) ?? new List<string>();
                }
                catch (JsonException)
                {
                    codeSmellDetails = new List<string> { fileMetric.ChangePatterns };
                }
            }

            // Generate recommendations based on metrics
            if (fileMetric.CyclomaticComplexity > 10)
                recommendations.Add("Consider breaking down complex methods (Cyclomatic Complexity > 10)");
            if (fileMetric.CognitiveComplexity > 15)
                recommendations.Add("Simplify complex logic to improve readability (Cognitive Complexity > 15)");
            if (fileMetric.NestingDepth > 4)
                recommendations.Add("Reduce nesting depth to improve maintainability (Max Nesting > 4)");
            if (fileMetric.CommentDensity < 10)
                recommendations.Add("Add more documentation comments (Comment Density < 10%)");
            if (fileMetric.DuplicationPercentage > 5)
                recommendations.Add("Remove duplicate code to improve maintainability");
            if (fileMetric.SecurityHotspots > 0)
                recommendations.Add($"Address {fileMetric.SecurityHotspots} security hotspot(s)");
            if (fileMetric.TechnicalDebtMinutes > 60)
                recommendations.Add($"Consider refactoring to reduce technical debt ({Math.Round(fileMetric.TechnicalDebtMinutes / 60.0, 1)}h estimated)");

            var fileDetails = new
            {
                file = new
                {
                    id = fileMetric.Id,
                    repositoryId = fileMetric.RepositoryId,
                    filePath = fileMetric.FilePath,
                    fileName = Path.GetFileName(fileMetric.FilePath),
                    directory = Path.GetDirectoryName(fileMetric.FilePath),
                    language = fileMetric.PrimaryLanguage,
                    extension = fileMetric.FileExtension
                },
                size = new
                {
                    totalLines = fileMetric.LineCount,
                    effectiveLines = fileMetric.EffectiveLineCount,
                    commentLines = fileMetric.CommentLineCount,
                    blankLines = fileMetric.BlankLineCount,
                    fileSizeBytes = fileMetric.FileSizeBytes
                },
                complexity = new
                {
                    cyclomatic = Math.Round(fileMetric.CyclomaticComplexity, 2),
                    cognitive = Math.Round(fileMetric.CognitiveComplexity, 2),
                    nesting = new
                    {
                        maxDepth = fileMetric.NestingDepth,
                        averageDepth = Math.Round(fileMetric.NestingDepth, 2)
                    },
                    methods = new
                    {
                        total = fileMetric.MethodCount,
                        averageLength = Math.Round(fileMetric.AverageMethodLength, 2),
                        maxLength = fileMetric.MaxMethodLength,
                        complexMethods = fileMetric.MethodCount // approximation
                    }
                },
                quality = new
                {
                    maintainabilityIndex = Math.Round(fileMetric.MaintainabilityIndex, 2),
                    codeSmells = new
                    {
                        total = fileMetric.CodeSmellCount,
                        details = codeSmellDetails
                    },
                    technicalDebt = new
                    {
                        minutes = Math.Round(fileMetric.TechnicalDebtMinutes, 1),
                        hours = Math.Round(fileMetric.TechnicalDebtMinutes / 60.0, 1),
                        ratio = Math.Round(fileMetric.TechnicalDebtMinutes / Math.Max(fileMetric.LineCount, 1), 3)
                    },
                    documentation = new
                    {
                        commentDensity = Math.Round(fileMetric.CommentDensity, 2),
                        documentationCoverage = Math.Round(fileMetric.DocumentationCoverage, 2)
                    },
                    duplication = new
                    {
                        percentage = Math.Round(fileMetric.DuplicationPercentage, 2),
                        lines = fileMetric.DuplicationLines
                    }
                },
                security = new
                {
                    hotspots = fileMetric.SecurityHotspots,
                    vulnerabilityCount = fileMetric.VulnerabilityCount,
                    hasSecurityIssues = fileMetric.SecurityHotspots > 0
                },
                health = new
                {
                    overallScore = Math.Round(fileMetric.QualityScore, 2),
                    scoreCategory = fileMetric.QualityScore switch
                    {
                        >= 80.0 => "Excellent",
                        >= 60.0 => "Good",
                        >= 40.0 => "Fair",
                        >= 20.0 => "Poor",
                        _ => "Critical"
                    },
                    bugProneness = Math.Round(fileMetric.BugProneness, 2),
                    stabilityScore = Math.Round(fileMetric.StabilityScore, 2),
                    maturityScore = Math.Round(fileMetric.MaturityScore, 2)
                },
                change = new
                {
                    churnRate = Math.Round(fileMetric.ChurnRate, 2),
                    changeFrequency = fileMetric.ChangeFrequency,
                    lastModified = fileMetric.LastCommit,
                    isVolatile = fileMetric.ChurnRate > 0.5
                },
                analysis = new
                {
                    analyzedAt = fileMetric.LastAnalyzed,
                    analysisVersion = "1.0",
                    isUpToDate = fileMetric.LastAnalyzed > fileMetric.LastCommit
                },
                recommendations = new
                {
                    total = recommendations.Count,
                    suggestions = recommendations,
                    priority = fileMetric.QualityScore < 20.0 ? "High" :
                              fileMetric.QualityScore < 40.0 ? "Medium" : "Low"
                }
            };

            return Ok(ApiResponse<object>.SuccessResult(fileDetails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file details for {RepositoryId}/{FilePath}", repositoryId, filePath);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve file details"));
        }
    }

    /// <summary>
    /// Gets code quality hotspots for a repository - files that need immediate attention
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="limit">Maximum number of hotspots to return (default: 10)</param>
    /// <returns>List of files with critical quality issues prioritized by impact</returns>
    /// <response code="200">Returns the quality hotspots</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("repository/{repositoryId}/quality/hotspots")]
    public async Task<IActionResult> GetQualityHotspots(int repositoryId, [FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("Getting quality hotspots for {RepositoryId}, limit {Limit}", repositoryId, limit);

            var fileMetrics = await _fileMetricsRepository.GetRepositoryFileMetricsAsync(repositoryId);
            
            if (!fileMetrics.Any())
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    repositoryId,
                    message = "No file metrics available for hotspot analysis",
                    hotspots = new List<object>()
                }));
            }

            // Calculate hotspot score based on multiple factors
            var hotspotsWithScore = fileMetrics.Select(f => new
            {
                fileMetric = f,
                hotspotScore = CalculateHotspotScore(f)
            })
            .Where(h => h.hotspotScore > 0) // Only include files with issues
            .OrderByDescending(h => h.hotspotScore)
            .Take(limit);

            var hotspots = hotspotsWithScore.Select(h => new
            {
                filePath = h.fileMetric.FilePath,
                fileName = Path.GetFileName(h.fileMetric.FilePath),
                language = h.fileMetric.PrimaryLanguage,
                hotspotScore = Math.Round(h.hotspotScore, 2),
                priority = h.hotspotScore switch
                {
                    >= 8.0 => "Critical",
                    >= 6.0 => "High",
                    >= 4.0 => "Medium",
                    _ => "Low"
                },
                issues = new
                {
                    healthScore = Math.Round(h.fileMetric.QualityScore, 2),
                    complexity = Math.Round(h.fileMetric.CyclomaticComplexity, 2),
                    technicalDebt = Math.Round(h.fileMetric.TechnicalDebtMinutes / 60.0, 1),
                    codeSmells = h.fileMetric.CodeSmellCount,
                    securityHotspots = h.fileMetric.SecurityHotspots,
                    churnRate = Math.Round(h.fileMetric.ChurnRate, 2),
                    bugProneness = Math.Round(h.fileMetric.BugProneness, 2)
                },
                impact = new
                {
                    linesOfCode = h.fileMetric.LineCount,
                    changeFrequency = h.fileMetric.ChangeFrequency,
                    stabilityScore = Math.Round(h.fileMetric.StabilityScore, 2)
                },
                recommendations = GenerateHotspotRecommendations(h.fileMetric)
            }).ToList();

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                hotspots,
                summary = new
                {
                    totalHotspots = hotspots.Count,
                    criticalIssues = hotspots.Count(h => h.priority == "Critical"),
                    highPriorityIssues = hotspots.Count(h => h.priority == "High"),
                    averageHotspotScore = hotspots.Any() ? Math.Round(hotspots.Average(h => h.hotspotScore), 2) : 0,
                    recommendedActions = GetTopRecommendations(hotspots)
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get quality hotspots for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve quality hotspots"));
        }
    }

    /// <summary>
    /// Gets code relationship graph for a repository showing complete structure
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <returns>Complete code graph with nodes and relationships</returns>
    /// <response code="200">Returns the code graph</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("repository/{repositoryId}/code-graph")]
    public async Task<IActionResult> GetCodeGraph(int repositoryId)
    {
        try
        {
            _logger.LogInformation("Getting code graph for {RepositoryId}", repositoryId);

            // This would require AST analysis and graph construction
            // For now, return a placeholder structure
            return Ok(ApiResponse<object>.SuccessResult(new
            {
                repositoryId,
                message = "Code graph analysis requires AST analysis and graph construction to be enabled",
                nodes = new List<object>(),
                edges = new List<object>(),
                metadata = new
                {
                    totalNodes = 0,
                    totalEdges = 0,
                    maxDepth = 0,
                    entryPoints = new List<string>(),
                    orphanNodes = new List<string>(),
                    circularDependencies = new List<List<string>>()
                }
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get code graph for {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve code graph"));
        }
    }

    private static double CalculateHotspotScore(Core.Entities.FileMetrics fileMetric)
    {
        var score = 0.0;

        // Quality score impact (inverted - lower quality = higher hotspot score)
        score += (100.0 - fileMetric.QualityScore) / 10.0;

        // Complexity impact
        if (fileMetric.CyclomaticComplexity > 15) score += 2.0;
        else if (fileMetric.CyclomaticComplexity > 10) score += 1.0;

        // Technical debt impact
        if (fileMetric.TechnicalDebtMinutes > 120) score += 2.0; // > 2 hours
        else if (fileMetric.TechnicalDebtMinutes > 60) score += 1.0; // > 1 hour

        // Code smells impact
        score += Math.Min(fileMetric.CodeSmellCount * 0.5, 2.0);

        // Security hotspots impact
        score += fileMetric.SecurityHotspots * 1.5;

        // Change frequency impact (volatile files are riskier)
        if (fileMetric.ChurnRate > 0.7) score += 1.5;
        else if (fileMetric.ChurnRate > 0.4) score += 1.0;

        // Bug proneness impact
        if (fileMetric.BugProneness > 0.7) score += 1.0;

        // Size impact (larger files with issues are more problematic)
        if (fileMetric.LineCount > 1000) score *= 1.2;
        else if (fileMetric.LineCount > 500) score *= 1.1;

        return Math.Min(score, 10.0); // Cap at 10.0
    }

    private static List<string> GenerateHotspotRecommendations(Core.Entities.FileMetrics fileMetric)
    {
        var recommendations = new List<string>();

        if (fileMetric.QualityScore < 20.0)
            recommendations.Add("Priority refactoring needed - file quality is critical");
        if (fileMetric.CyclomaticComplexity > 15)
            recommendations.Add("Break down complex methods to improve maintainability");
        if (fileMetric.TechnicalDebtMinutes > 120)
            recommendations.Add("Significant refactoring required to reduce technical debt");
        if (fileMetric.SecurityHotspots > 0)
            recommendations.Add($"Address {fileMetric.SecurityHotspots} security issue(s) immediately");
        if (fileMetric.CodeSmellCount > 5)
            recommendations.Add("Multiple code quality issues need attention");
        if (fileMetric.ChurnRate > 0.7)
            recommendations.Add("Stabilize frequently changing code through better design");
        if (fileMetric.BugProneness > 0.7)
            recommendations.Add("Add comprehensive testing - high bug risk detected");

        return recommendations.Take(3).ToList(); // Limit to top 3 recommendations
    }

    private static List<string> GetTopRecommendations(IEnumerable<dynamic> hotspots)
    {
        var allRecommendations = new List<string>();
        foreach (var hotspot in hotspots)
        {
            allRecommendations.AddRange(hotspot.recommendations);
        }

        return allRecommendations
            .GroupBy(r => r)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"{g.Key} ({g.Count()} files)")
            .ToList();
    }
}
