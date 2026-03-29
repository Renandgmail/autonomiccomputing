using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Infrastructure.Services;
using RepoLens.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for real-time metrics collection and analysis operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IRealMetricsCollectionService _metricsCollectionService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        IRealMetricsCollectionService metricsCollectionService,
        ILogger<MetricsController> logger)
    {
        _metricsCollectionService = metricsCollectionService;
        _logger = logger;
    }

    /// <summary>
    /// Collect comprehensive real-time repository metrics from GitHub
    /// </summary>
    /// <param name="request">Repository metrics collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete repository metrics with real-time GitHub data</returns>
    /// <response code="200">Returns comprehensive repository metrics</response>
    /// <response code="400">Invalid repository parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during metrics collection</response>
    [HttpPost("repositories/collect")]
    public async Task<IActionResult> CollectRepositoryMetrics(
        [FromBody] RepositoryMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Owner) || string.IsNullOrWhiteSpace(request.Repository))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Owner and Repository are required"));
            }

            _logger.LogInformation("Starting real-time metrics collection for {Owner}/{Repository}", 
                request.Owner, request.Repository);

            var metrics = await _metricsCollectionService.CollectRepositoryMetricsAsync(
                request.Owner, request.Repository, request.RepositoryId);

            var response = new
            {
                repositoryId = request.RepositoryId,
                owner = request.Owner,
                repository = request.Repository,
                collectionTimestamp = metrics.MeasurementDate,
                dataSource = "Real-time GitHub API",
                
                // Core repository statistics
                repositoryStats = new
                {
                    totalFiles = metrics.TotalFiles,
                    repositorySizeBytes = metrics.RepositorySizeBytes,
                    totalLinesOfCode = metrics.TotalLinesOfCode,
                    effectiveLinesOfCode = metrics.EffectiveLinesOfCode,
                    commentLines = metrics.CommentLines,
                    blankLines = metrics.BlankLines,
                    commentRatio = Math.Round(metrics.CommentRatio, 2),
                    binaryFileCount = metrics.BinaryFileCount,
                    textFileCount = metrics.TextFileCount
                },
                
                // Language and code analysis
                codeAnalysis = new
                {
                    languageDistribution = metrics.LanguageDistribution,
                    fileTypeDistribution = metrics.FileTypeDistribution,
                    averageCyclomaticComplexity = Math.Round(metrics.AverageCyclomaticComplexity, 2),
                    totalMethods = metrics.TotalMethods,
                    totalClasses = metrics.TotalClasses,
                    maintainabilityIndex = Math.Round(metrics.MaintainabilityIndex, 1)
                },
                
                // Real-time activity metrics
                activity = new
                {
                    commitsLastWeek = metrics.CommitsLastWeek,
                    commitsLastMonth = metrics.CommitsLastMonth,
                    commitsLastQuarter = metrics.CommitsLastQuarter,
                    developmentVelocity = Math.Round(metrics.DevelopmentVelocity, 2),
                    averageCommitSize = Math.Round(metrics.AverageCommitSize, 1),
                    linesAddedLastWeek = metrics.LinesAddedLastWeek,
                    filesChangedLastWeek = metrics.FilesChangedLastWeek,
                    hourlyActivityPattern = metrics.HourlyActivityPattern,
                    dailyActivityPattern = metrics.DailyActivityPattern
                },
                
                // Team collaboration insights
                collaboration = new
                {
                    totalContributors = metrics.TotalContributors,
                    activeContributors = metrics.ActiveContributors,
                    busFactor = Math.Round(metrics.BusFactor, 1),
                    contributorDiversity = metrics.TotalContributors > 0 ? 
                        Math.Round((double)metrics.ActiveContributors / metrics.TotalContributors * 100, 1) : 0
                },
                
                // Quality and health assessment
                quality = new
                {
                    codeQualityScore = Math.Round(metrics.CodeQualityScore, 1),
                    projectHealthScore = Math.Round(metrics.ProjectHealthScore, 1),
                    lineCoveragePercentage = Math.Round(metrics.LineCoveragePercentage, 1),
                    documentationCoverage = Math.Round(metrics.DocumentationCoverage, 1),
                    buildSuccessRate = Math.Round(metrics.BuildSuccessRate, 1),
                    securityVulnerabilities = metrics.SecurityVulnerabilities
                },
                
                // Data collection insights
                collectionInfo = new
                {
                    dataFreshness = "Real-time from GitHub API",
                    analysisDepth = "Comprehensive with 90-day history",
                    metricsCalculated = 25,
                    estimationMethods = new[]
                    {
                        "GitHub API data integration",
                        "Commit pattern analysis", 
                        "Language-based complexity estimation",
                        "Activity pattern recognition",
                        "Quality scoring algorithms"
                    },
                    nextRecommendedUpdate = DateTime.UtcNow.AddHours(6)
                }
            };

            _logger.LogInformation("Successfully collected repository metrics for {Owner}/{Repository}. " +
                "Quality: {Quality}, Health: {Health}, Contributors: {Contributors}", 
                request.Owner, request.Repository, metrics.CodeQualityScore, 
                metrics.ProjectHealthScore, metrics.TotalContributors);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            _logger.LogWarning("Repository {Owner}/{Repository} not found: {Message}", 
                request.Owner, request.Repository, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult($"Repository {request.Owner}/{request.Repository} not found or not accessible"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access for repository {Owner}/{Repository}: {Message}", 
                request.Owner, request.Repository, ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResult("GitHub API authentication failed. Please check your access token."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting repository metrics for {Owner}/{Repository}", 
                request.Owner, request.Repository);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect repository metrics"));
        }
    }

    /// <summary>
    /// Collect comprehensive contributor metrics with activity analysis
    /// </summary>
    /// <param name="request">Contributor metrics collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed contributor metrics with activity patterns and insights</returns>
    /// <response code="200">Returns contributor metrics and analysis</response>
    /// <response code="400">Invalid repository parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during contributor analysis</response>
    [HttpPost("contributors/collect")]
    public async Task<IActionResult> CollectContributorMetrics(
        [FromBody] ContributorMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Owner) || string.IsNullOrWhiteSpace(request.Repository))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Owner and Repository are required"));
            }

            _logger.LogInformation("Collecting contributor metrics for {Owner}/{Repository}", 
                request.Owner, request.Repository);

            var contributorMetrics = await _metricsCollectionService.CollectContributorMetricsAsync(
                request.Owner, request.Repository, request.RepositoryId);

            var response = new
            {
                repositoryId = request.RepositoryId,
                owner = request.Owner,
                repository = request.Repository,
                collectionTimestamp = DateTime.UtcNow,
                dataSource = "Real-time GitHub API with 90-day analysis",
                
                summary = new
                {
                    totalContributors = contributorMetrics.Count,
                    coreContributors = contributorMetrics.Count(c => c.IsCoreContributor),
                    newContributors = contributorMetrics.Count(c => c.IsNewContributor),
                    averageRetentionScore = contributorMetrics.Any() ? 
                        Math.Round(contributorMetrics.Average(c => c.RetentionScore), 2) : 0,
                    totalCommits = contributorMetrics.Sum(c => c.CommitCount),
                    totalLinesAdded = contributorMetrics.Sum(c => c.LinesAdded),
                    totalLinesDeleted = contributorMetrics.Sum(c => c.LinesDeleted),
                    totalFilesModified = contributorMetrics.Sum(c => c.FilesModified),
                    averageWorkingDays = contributorMetrics.Any() ? 
                        Math.Round(contributorMetrics.Average(c => c.WorkingDays), 1) : 0
                },
                
                contributors = contributorMetrics.Select(c => new
                {
                    name = c.ContributorName,
                    email = c.ContributorEmail,
                    period = new
                    {
                        start = c.PeriodStart,
                        end = c.PeriodEnd,
                        durationDays = (c.PeriodEnd - c.PeriodStart).TotalDays
                    },
                    
                    // Contribution statistics
                    contributions = new
                    {
                        commitCount = c.CommitCount,
                        linesAdded = c.LinesAdded,
                        linesDeleted = c.LinesDeleted,
                        filesModified = c.FilesModified,
                        contributionPercentage = Math.Round(c.ContributionPercentage, 2),
                        netLinesChanged = c.LinesAdded - c.LinesDeleted,
                        averageCommitSize = c.CommitCount > 0 ? 
                            Math.Round((double)(c.LinesAdded + c.LinesDeleted) / c.CommitCount, 1) : 0
                    },
                    
                    // Activity and engagement patterns
                    activity = new
                    {
                        workingDays = c.WorkingDays,
                        commitFrequency = Math.Round(c.WorkingDays > 0 ? (double)c.CommitCount / c.WorkingDays : 0, 3),
                        firstContribution = c.FirstContribution,
                        lastContribution = c.LastContribution,
                        daysSinceLastContribution = (DateTime.UtcNow - c.LastContribution).TotalDays,
                        hourlyActivityPattern = c.HourlyActivityPattern,
                        retentionScore = Math.Round(c.RetentionScore, 2)
                    },
                    
                    // Contributor classification
                    profile = new
                    {
                        isCoreContributor = c.IsCoreContributor,
                        isNewContributor = c.IsNewContributor,
                        contributorType = ClassifyContributor(c),
                        engagementLevel = CalculateEngagementLevel(c),
                        riskLevel = AssessContributorRisk(c)
                    }
                }).ToList(),
                
                // Advanced insights and analytics
                insights = new
                {
                    topContributors = contributorMetrics
                        .OrderByDescending(c => c.CommitCount)
                        .Take(5)
                        .Select(c => new { 
                            name = c.ContributorName, 
                            commits = c.CommitCount,
                            percentage = Math.Round(c.ContributionPercentage, 1)
                        })
                        .ToList(),
                    
                    activityDistribution = new
                    {
                        veryActive = contributorMetrics.Count(c => c.CommitCount >= 50),
                        active = contributorMetrics.Count(c => c.CommitCount >= 10 && c.CommitCount < 50),
                        moderate = contributorMetrics.Count(c => c.CommitCount >= 3 && c.CommitCount < 10),
                        occasional = contributorMetrics.Count(c => c.CommitCount > 0 && c.CommitCount < 3)
                    },
                    
                    riskFactors = new
                    {
                        highDependencyContributors = contributorMetrics
                            .Where(c => c.ContributionPercentage > 30)
                            .Select(c => new { name = c.ContributorName, percentage = Math.Round(c.ContributionPercentage, 1) })
                            .ToList(),
                        inactiveContributors = contributorMetrics
                            .Where(c => (DateTime.UtcNow - c.LastContribution).TotalDays > 30)
                            .Count(),
                        newContributorRetention = contributorMetrics
                            .Where(c => c.IsNewContributor)
                            .Average(c => c.RetentionScore),
                        busFactor = CalculateBusFactorFromContributors(contributorMetrics)
                    },
                    
                    recommendations = GenerateContributorRecommendations(contributorMetrics),
                    
                    trendsAndPatterns = new
                    {
                        mostActiveHours = GetMostActiveHours(contributorMetrics),
                        weekendContributors = contributorMetrics.Count(c => HasWeekendActivity(c)),
                        averageContributionSpan = contributorMetrics.Any() ? 
                            Math.Round(contributorMetrics.Average(c => (c.LastContribution - c.FirstContribution).TotalDays), 1) : 0
                    }
                }
            };

            _logger.LogInformation("Successfully collected contributor metrics for {Owner}/{Repository}. " +
                "Contributors: {Count}, Core: {Core}, New: {New}", 
                request.Owner, request.Repository, contributorMetrics.Count, 
                contributorMetrics.Count(c => c.IsCoreContributor), 
                contributorMetrics.Count(c => c.IsNewContributor));

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting contributor metrics for {Owner}/{Repository}", 
                request.Owner, request.Repository);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect contributor metrics"));
        }
    }

    /// <summary>
    /// Collect comprehensive file-level metrics with complexity analysis
    /// </summary>
    /// <param name="request">File metrics collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed file metrics with complexity and quality insights</returns>
    /// <response code="200">Returns file metrics and analysis</response>
    /// <response code="400">Invalid repository parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during file analysis</response>
    [HttpPost("files/collect")]
    public async Task<IActionResult> CollectFileMetrics(
        [FromBody] FileMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Owner) || string.IsNullOrWhiteSpace(request.Repository))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Owner and Repository are required"));
            }

            _logger.LogInformation("Collecting file metrics for {Owner}/{Repository}", 
                request.Owner, request.Repository);

            var fileMetrics = await _metricsCollectionService.CollectFileMetricsAsync(
                request.Owner, request.Repository, request.RepositoryId);

            var response = new
            {
                repositoryId = request.RepositoryId,
                owner = request.Owner,
                repository = request.Repository,
                collectionTimestamp = DateTime.UtcNow,
                dataSource = "Real-time GitHub Contents API",
                
                summary = new
                {
                    totalFiles = fileMetrics.Count,
                    totalSizeBytes = fileMetrics.Sum(f => f.FileSizeBytes),
                    totalLines = fileMetrics.Sum(f => f.LineCount),
                    totalEffectiveLines = fileMetrics.Sum(f => f.EffectiveLineCount),
                    totalCommentLines = fileMetrics.Sum(f => f.CommentLineCount),
                    averageFileSize = fileMetrics.Any() ? Math.Round(fileMetrics.Average(f => f.FileSizeBytes), 1) : 0,
                    averageComplexity = fileMetrics.Any() ? 
                        Math.Round(fileMetrics.Average(f => f.CyclomaticComplexity), 2) : 0,
                    averageMaintainability = fileMetrics.Any() ? 
                        Math.Round(fileMetrics.Average(f => f.MaintainabilityIndex), 1) : 0,
                    hotspotCount = fileMetrics.Count(f => f.IsHotspot),
                    testFileCount = fileMetrics.Count(f => f.IsTestFile),
                    configFileCount = fileMetrics.Count(f => f.IsConfigurationFile)
                },
                
                languageBreakdown = fileMetrics
                    .GroupBy(f => f.PrimaryLanguage)
                    .OrderByDescending(g => g.Sum(f => f.FileSizeBytes))
                    .Select(g => new
                    {
                        language = g.Key,
                        fileCount = g.Count(),
                        totalSizeBytes = g.Sum(f => f.FileSizeBytes),
                        totalLines = g.Sum(f => f.LineCount),
                        averageComplexity = Math.Round(g.Average(f => f.CyclomaticComplexity), 2),
                        averageMaintainability = Math.Round(g.Average(f => f.MaintainabilityIndex), 1),
                        hotspotCount = g.Count(f => f.IsHotspot)
                    })
                    .ToList(),
                
                categoryBreakdown = fileMetrics
                    .GroupBy(f => f.FileCategory)
                    .Select(g => new
                    {
                        category = g.Key,
                        fileCount = g.Count(),
                        totalSizeBytes = g.Sum(f => f.FileSizeBytes),
                        averageComplexity = Math.Round(g.Average(f => f.CyclomaticComplexity), 2)
                    })
                    .ToList(),
                
                files = fileMetrics.Select(f => new
                {
                    path = f.FilePath,
                    name = f.FileName,
                    extension = f.FileExtension,
                    language = f.PrimaryLanguage,
                    category = f.FileCategory,
                    
                    // Size and structure metrics
                    sizeBytes = f.FileSizeBytes,
                    lineCount = f.LineCount,
                    effectiveLineCount = f.EffectiveLineCount,
                    commentLineCount = f.CommentLineCount,
                    blankLineCount = f.BlankLineCount,
                    commentRatio = f.LineCount > 0 ? 
                        Math.Round((double)f.CommentLineCount / f.LineCount * 100, 2) : 0,
                    
                    // Quality and complexity metrics
                    cyclomaticComplexity = Math.Round(f.CyclomaticComplexity, 2),
                    maintainabilityIndex = Math.Round(f.MaintainabilityIndex, 1),
                    testCoverage = Math.Round(f.TestCoverage, 1),
                    
                    // Classification and flags
                    isTestFile = f.IsTestFile,
                    isConfigurationFile = f.IsConfigurationFile,
                    isHotspot = f.IsHotspot,
                    
                    // Analysis metadata
                    lastAnalyzed = f.LastAnalyzed,
                    firstCommit = f.FirstCommit,
                    lastCommit = f.LastCommit,
                    
                    // Derived insights
                    qualityGrade = CalculateQualityGrade(f),
                    complexityLevel = CalculateComplexityLevel(f.CyclomaticComplexity),
                    maintenanceRisk = CalculateMaintenanceRisk(f)
                }).ToList(),
                
                insights = new
                {
                    qualityDistribution = new
                    {
                        excellent = fileMetrics.Count(f => f.MaintainabilityIndex >= 85),
                        good = fileMetrics.Count(f => f.MaintainabilityIndex >= 70 && f.MaintainabilityIndex < 85),
                        fair = fileMetrics.Count(f => f.MaintainabilityIndex >= 60 && f.MaintainabilityIndex < 70),
                        poor = fileMetrics.Count(f => f.MaintainabilityIndex < 60)
                    },
                    
                    complexityDistribution = new
                    {
                        low = fileMetrics.Count(f => f.CyclomaticComplexity <= 10),
                        moderate = fileMetrics.Count(f => f.CyclomaticComplexity > 10 && f.CyclomaticComplexity <= 20),
                        high = fileMetrics.Count(f => f.CyclomaticComplexity > 20 && f.CyclomaticComplexity <= 50),
                        veryHigh = fileMetrics.Count(f => f.CyclomaticComplexity > 50)
                    },
                    
                    hotspots = fileMetrics
                        .Where(f => f.IsHotspot)
                        .OrderByDescending(f => f.CyclomaticComplexity)
                        .Take(10)
                        .Select(f => new { 
                            file = f.FilePath, 
                            complexity = Math.Round(f.CyclomaticComplexity, 2),
                            maintainability = Math.Round(f.MaintainabilityIndex, 1),
                            sizeBytes = f.FileSizeBytes
                        })
                        .ToList(),
                    
                    qualityConcerns = fileMetrics
                        .Where(f => f.MaintainabilityIndex < 60)
                        .OrderBy(f => f.MaintainabilityIndex)
                        .Take(10)
                        .Select(f => new { 
                            file = f.FilePath, 
                            maintainability = Math.Round(f.MaintainabilityIndex, 1),
                            complexity = Math.Round(f.CyclomaticComplexity, 2),
                            issues = IdentifyQualityIssues(f)
                        })
                        .ToList(),
                    
                    recommendations = GenerateFileRecommendations(fileMetrics),
                    
                    testingInsights = new
                    {
                        testCoverageAverage = fileMetrics.Where(f => !f.IsTestFile).Any() ? 
                            Math.Round(fileMetrics.Where(f => !f.IsTestFile).Average(f => f.TestCoverage), 1) : 0,
                        testFileRatio = fileMetrics.Any() ? 
                            Math.Round((double)fileMetrics.Count(f => f.IsTestFile) / fileMetrics.Count * 100, 1) : 0,
                        untestedFiles = fileMetrics.Count(f => !f.IsTestFile && f.TestCoverage < 50)
                    }
                }
            };

            _logger.LogInformation("Successfully collected file metrics for {Owner}/{Repository}. " +
                "Files: {Count}, Hotspots: {Hotspots}, Avg Complexity: {Complexity}", 
                request.Owner, request.Repository, fileMetrics.Count, 
                fileMetrics.Count(f => f.IsHotspot),
                fileMetrics.Any() ? fileMetrics.Average(f => f.CyclomaticComplexity) : 0);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting file metrics for {Owner}/{Repository}", 
                request.Owner, request.Repository);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect file metrics"));
        }
    }

    /// <summary>
    /// Collect and persist commit history with comprehensive analysis
    /// </summary>
    /// <param name="request">Commit collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commit collection results with analysis insights</returns>
    /// <response code="200">Returns commit collection results</response>
    /// <response code="400">Invalid repository parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during commit collection</response>
    [HttpPost("commits/collect")]
    public async Task<IActionResult> CollectCommitHistory(
        [FromBody] CommitCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Owner) || string.IsNullOrWhiteSpace(request.Repository))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Owner and Repository are required"));
            }

            _logger.LogInformation("Collecting commit history for {Owner}/{Repository}", 
                request.Owner, request.Repository);

            var commits = await _metricsCollectionService.CollectAndPersistCommitsAsync(
                request.Owner, request.Repository, request.RepositoryId);

            var response = new
            {
                repositoryId = request.RepositoryId,
                owner = request.Owner,
                repository = request.Repository,
                collectionTimestamp = DateTime.UtcNow,
                dataSource = "GitHub Commits API with 6-month history",
                
                summary = new
                {
                    totalCommitsCollected = commits.Count,
                    dateRange = new
                    {
                        earliest = commits.Any() ? commits.Min(c => c.Timestamp) : DateTime.UtcNow,
                        latest = commits.Any() ? commits.Max(c => c.Timestamp) : DateTime.UtcNow,
                        spanDays = commits.Any() ? 
                            (commits.Max(c => c.Timestamp) - commits.Min(c => c.Timestamp)).TotalDays : 0
                    },
                    uniqueAuthors = commits.Select(c => c.Author).Distinct().Count(),
                    averageCommitsPerDay = CalculateAverageCommitsPerDay(commits),
                    mostActiveAuthor = GetMostActiveAuthor(commits)
                },
                
                activityAnalysis = new
                {
                    commitsLastWeek = commits.Count(c => c.Timestamp > DateTime.UtcNow.AddDays(-7)),
                    commitsLastMonth = commits.Count(c => c.Timestamp > DateTime.UtcNow.AddDays(-30)),
                    commitsLastQuarter = commits.Count(c => c.Timestamp > DateTime.UtcNow.AddDays(-90)),
                    
                    dailyPattern = commits
                        .GroupBy(c => c.Timestamp.DayOfWeek)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    
                    hourlyPattern = commits
                        .GroupBy(c => c.Timestamp.Hour)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    
                    monthlyTrend = commits
                        .Where(c => c.Timestamp > DateTime.UtcNow.AddMonths(-12))
                        .GroupBy(c => new { c.Timestamp.Year, c.Timestamp.Month })
                        .Select(g => new 
                        { 
                            month = $"{g.Key.Year}-{g.Key.Month:00}", 
                            commits = g.Count() 
                        })
                        .OrderBy(x => x.month)
                        .ToList()
                },
                
                authorAnalysis = commits
                    .GroupBy(c => c.Author)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new
                    {
                        author = g.Key,
                        commitCount = g.Count(),
                        firstCommit = g.Min(c => c.Timestamp),
                        lastCommit = g.Max(c => c.Timestamp),
                        averageMessageLength = Math.Round(g.Average(c => c.Message.Length), 1),
                        percentage = Math.Round((double)g.Count() / commits.Count * 100, 2)
                    })
                    .ToList(),
                
                commitMessages = new
                {
                    averageLength = commits.Any() ? Math.Round(commits.Average(c => c.Message.Length), 1) : 0,
                    shortMessages = commits.Count(c => c.Message.Length < 30),
                    longMessages = commits.Count(c => c.Message.Length > 100),
                    commonKeywords = GetCommonCommitKeywords(commits),
                    conventionalCommits = AnalyzeConventionalCommits(commits)
                },
                
                insights = new
                {
                    developmentVelocity = CalculateDevelopmentVelocityFromCommits(commits),
                    collaborationLevel = CalculateCollaborationLevel(commits),
                    codeStability = CalculateCodeStability(commits),
                    recommendations = GenerateCommitRecommendations(commits)
                },
                
                persistenceInfo = new
                {
                    commitsPersisted = commits.Count,
                    persistedAt = DateTime.UtcNow,
                    databaseTable = "Commits",
                    nextRecommendedCollection = DateTime.UtcNow.AddHours(12)
                }
            };

            _logger.LogInformation("Successfully collected and persisted {Count} commits for {Owner}/{Repository}. " +
                "Authors: {Authors}, Date range: {Days} days", 
                commits.Count, request.Owner, request.Repository, 
                commits.Select(c => c.Author).Distinct().Count(),
                commits.Any() ? (commits.Max(c => c.Timestamp) - commits.Min(c => c.Timestamp)).TotalDays : 0);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting commit history for {Owner}/{Repository}", 
                request.Owner, request.Repository);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect commit history"));
        }
    }

    #region Private Helper Methods

    private string ClassifyContributor(Core.Entities.ContributorMetrics contributor)
    {
        if (contributor.IsCoreContributor) return "Core";
        if (contributor.IsNewContributor) return "New";
        if (contributor.CommitCount >= 10) return "Active";
        if (contributor.CommitCount >= 3) return "Regular";
        return "Occasional";
    }

    private string CalculateEngagementLevel(Core.Entities.ContributorMetrics contributor)
    {
        var score = 0;
        if (contributor.CommitCount >= 20) score += 3;
        else if (contributor.CommitCount >= 10) score += 2;
        else if (contributor.CommitCount >= 3) score += 1;
        
        if (contributor.WorkingDays >= 20) score += 2;
        else if (contributor.WorkingDays >= 10) score += 1;
        
        if (contributor.RetentionScore >= 70) score += 2;
        else if (contributor.RetentionScore >= 40) score += 1;

        return score switch
        {
            >= 6 => "High",
            >= 4 => "Medium",
            >= 2 => "Low",
            _ => "Minimal"
        };
    }

    private string AssessContributorRisk(Core.Entities.ContributorMetrics contributor)
    {
        var daysSinceLastContribution = (DateTime.UtcNow - contributor.LastContribution).TotalDays;
        
        if (contributor.IsCoreContributor && daysSinceLastContribution > 30) return "High";
        if (contributor.ContributionPercentage > 40) return "High";
        if (daysSinceLastContribution > 60) return "Medium";
        if (contributor.RetentionScore < 30) return "Medium";
        return "Low";
    }

    private double CalculateBusFactorFromContributors(List<Core.Entities.ContributorMetrics> contributors)
    {
        if (!contributors.Any()) return 0;
        
        var totalContributions = contributors.Sum(c => c.CommitCount);
        var cumulativeContributions = 0;
        var factor = 0;
        
        foreach (var contributor in contributors.OrderByDescending(c => c.CommitCount))
        {
            cumulativeContributions += contributor.CommitCount;
            factor++;
            if (cumulativeContributions >= totalContributions * 0.5) break;
        }
        
        return Math.Max(1, factor);
    }

    private List<string> GenerateContributorRecommendations(List<Core.Entities.ContributorMetrics> contributors)
    {
        var recommendations = new List<string>();
        
        var coreContributors = contributors.Count(c => c.IsCoreContributor);
        var newContributors = contributors.Count(c => c.IsNewContributor);
        var inactiveContributors = contributors.Count(c => (DateTime.UtcNow - c.LastContribution).TotalDays > 30);
        
        if (coreContributors < 3)
            recommendations.Add("Consider developing more core contributors to reduce bus factor risk");
        
        if (newContributors == 0)
            recommendations.Add("Focus on attracting new contributors to maintain project vitality");
        
        if (inactiveContributors > contributors.Count * 0.3)
            recommendations.Add("High contributor inactivity detected - review engagement strategies");
        
        var maxContribution = contributors.Any() ? contributors.Max(c => c.ContributionPercentage) : 0;
        if (maxContribution > 50)
            recommendations.Add("High contribution concentration - implement knowledge sharing initiatives");
        
        return recommendations;
    }

    private List<int> GetMostActiveHours(List<Core.Entities.ContributorMetrics> contributors)
    {
        var hourCounts = new Dictionary<int, int>();
        
        foreach (var contributor in contributors)
        {
            if (!string.IsNullOrEmpty(contributor.HourlyActivityPattern))
            {
                try
                {
                    var pattern = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(contributor.HourlyActivityPattern);
                    if (pattern != null)
                    {
                        foreach (var kvp in pattern)
                        {
                            if (int.TryParse(kvp.Key, out var hour))
                            {
                                hourCounts[hour] = hourCounts.GetValueOrDefault(hour, 0) + kvp.Value;
                            }
                        }
                    }
                }
                catch { /* Ignore parsing errors */ }
            }
        }
        
        return hourCounts.OrderByDescending(kvp => kvp.Value).Take(3).Select(kvp => kvp.Key).ToList();
    }

    private bool HasWeekendActivity(Core.Entities.ContributorMetrics contributor)
    {
        // This would need commit timestamp data to determine accurately
        // For now, estimate based on working days vs commit count
        return contributor.WorkingDays < contributor.CommitCount * 0.8;
    }

    private string CalculateQualityGrade(Core.Entities.FileMetrics file)
    {
        if (file.MaintainabilityIndex >= 85) return "A";
        if (file.MaintainabilityIndex >= 70) return "B";
        if (file.MaintainabilityIndex >= 60) return "C";
        if (file.MaintainabilityIndex >= 40) return "D";
        return "F";
    }

    private string CalculateComplexityLevel(double complexity)
    {
        if (complexity <= 10) return "Low";
        if (complexity <= 20) return "Moderate";
        if (complexity <= 50) return "High";
        return "Very High";
    }

    private string CalculateMaintenanceRisk(Core.Entities.FileMetrics file)
    {
        var score = 0;
        if (file.CyclomaticComplexity > 30) score += 3;
        else if (file.CyclomaticComplexity > 20) score += 2;
        else if (file.CyclomaticComplexity > 10) score += 1;
        
        if (file.MaintainabilityIndex < 50) score += 3;
        else if (file.MaintainabilityIndex < 70) score += 2;
        else if (file.MaintainabilityIndex < 85) score += 1;
        
        if (file.FileSizeBytes > 50000) score += 2;
        if (file.TestCoverage < 50) score += 1;

        return score switch
        {
            >= 7 => "Very High",
            >= 5 => "High",
            >= 3 => "Medium",
            >= 1 => "Low",
            _ => "Very Low"
        };
    }

    private List<string> IdentifyQualityIssues(Core.Entities.FileMetrics file)
    {
        var issues = new List<string>();
        
        if (file.CyclomaticComplexity > 30)
            issues.Add("Very high complexity");
        else if (file.CyclomaticComplexity > 20)
            issues.Add("High complexity");
        
        if (file.MaintainabilityIndex < 40)
            issues.Add("Very low maintainability");
        else if (file.MaintainabilityIndex < 60)
            issues.Add("Low maintainability");
        
        if (file.FileSizeBytes > 50000)
            issues.Add("Large file size");
        
        if (file.TestCoverage < 30)
            issues.Add("Very low test coverage");
        else if (file.TestCoverage < 60)
            issues.Add("Low test coverage");
        
        return issues;
    }

    private List<string> GenerateFileRecommendations(List<Core.Entities.FileMetrics> files)
    {
        var recommendations = new List<string>();
        
        var hotspots = files.Count(f => f.IsHotspot);
        var lowMaintainability = files.Count(f => f.MaintainabilityIndex < 60);
        var highComplexity = files.Count(f => f.CyclomaticComplexity > 30);
        var lowCoverage = files.Count(f => f.TestCoverage < 60);
        
        if (hotspots > 0)
            recommendations.Add($"Refactor {hotspots} hotspot files to improve maintainability");
        
        if (lowMaintainability > 0)
            recommendations.Add($"Address maintainability issues in {lowMaintainability} files");
        
        if (highComplexity > 0)
            recommendations.Add($"Reduce complexity in {highComplexity} files through decomposition");
        
        if (lowCoverage > 0)
            recommendations.Add($"Improve test coverage for {lowCoverage} files");
        
        return recommendations;
    }

    private double CalculateAverageCommitsPerDay(List<Core.Entities.Commit> commits)
    {
        if (!commits.Any()) return 0;
        
        var totalDays = (commits.Max(c => c.Timestamp) - commits.Min(c => c.Timestamp)).TotalDays;
        return totalDays > 0 ? Math.Round(commits.Count / totalDays, 2) : commits.Count;
    }

    private string GetMostActiveAuthor(List<Core.Entities.Commit> commits)
    {
        return commits.GroupBy(c => c.Author)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "Unknown";
    }

    private List<string> GetCommonCommitKeywords(List<Core.Entities.Commit> commits)
    {
        var keywords = new Dictionary<string, int>();
        var commonWords = new[] { "fix", "add", "update", "remove", "refactor", "feat", "docs", "style", "test" };
        
        foreach (var commit in commits)
        {
            var message = commit.Message.ToLower();
            foreach (var word in commonWords)
            {
                if (message.Contains(word))
                {
                    keywords[word] = keywords.GetValueOrDefault(word, 0) + 1;
                }
            }
        }
        
        return keywords.OrderByDescending(kvp => kvp.Value).Take(5).Select(kvp => kvp.Key).ToList();
    }

    private object AnalyzeConventionalCommits(List<Core.Entities.Commit> commits)
    {
        var conventional = commits.Count(c => IsConventionalCommit(c.Message));
        var total = commits.Count;
        
        return new
        {
            conventionalCount = conventional,
            conventionalPercentage = total > 0 ? Math.Round((double)conventional / total * 100, 1) : 0,
            followsStandard = conventional > total * 0.7
        };
    }

    private bool IsConventionalCommit(string message)
    {
        var conventionalPrefixes = new[] { "feat:", "fix:", "docs:", "style:", "refactor:", "test:", "chore:" };
        return conventionalPrefixes.Any(prefix => message.ToLower().StartsWith(prefix));
    }

    private double CalculateDevelopmentVelocityFromCommits(List<Core.Entities.Commit> commits)
    {
        if (!commits.Any()) return 0;
        
        var last30Days = commits.Count(c => c.Timestamp > DateTime.UtcNow.AddDays(-30));
        var previous30Days = commits.Count(c => c.Timestamp > DateTime.UtcNow.AddDays(-60) && c.Timestamp <= DateTime.UtcNow.AddDays(-30));
        
        if (previous30Days == 0) return last30Days;
        return Math.Round(((double)last30Days / previous30Days - 1) * 100, 2);
    }

    private double CalculateCollaborationLevel(List<Core.Entities.Commit> commits)
    {
        var uniqueAuthors = commits.Select(c => c.Author).Distinct().Count();
        var totalCommits = commits.Count;
        
        if (totalCommits == 0) return 0;
        return Math.Round((double)uniqueAuthors / totalCommits * 100, 2);
    }

    private double CalculateCodeStability(List<Core.Entities.Commit> commits)
    {
        if (!commits.Any()) return 100;
        
        var fixCommits = commits.Count(c => c.Message.ToLower().Contains("fix"));
        var totalCommits = commits.Count;
        
        return Math.Round((1 - (double)fixCommits / totalCommits) * 100, 2);
    }

    private List<string> GenerateCommitRecommendations(List<Core.Entities.Commit> commits)
    {
        var recommendations = new List<string>();
        
        var conventional = commits.Count(c => IsConventionalCommit(c.Message));
        var conventionalPercentage = commits.Count > 0 ? (double)conventional / commits.Count : 0;
        
        if (conventionalPercentage < 0.5)
            recommendations.Add("Adopt conventional commit message format for better tracking");
        
        var avgMessageLength = commits.Any() ? commits.Average(c => c.Message.Length) : 0;
        if (avgMessageLength < 20)
            recommendations.Add("Write more descriptive commit messages");
        
        var fixCommits = commits.Count(c => c.Message.ToLower().Contains("fix"));
        var fixPercentage = commits.Count > 0 ? (double)fixCommits / commits.Count : 0;
        if (fixPercentage > 0.3)
            recommendations.Add("High number of fix commits detected - consider improving code review process");
        
        return recommendations;
    }

    #endregion
}

// Request models
public class RepositoryMetricsRequest
{
    /// <summary>
    /// Repository owner/organization
    /// </summary>
    [Required]
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Repository name
    /// </summary>
    [Required]
    public string Repository { get; set; } = string.Empty;
    
    /// <summary>
    /// Repository ID in database
    /// </summary>
    [Required]
    public int RepositoryId { get; set; }
}

public class ContributorMetricsRequest : RepositoryMetricsRequest
{
}

public class FileMetricsRequest : RepositoryMetricsRequest
{
}

public class CommitCollectionRequest : RepositoryMetricsRequest
{
}
