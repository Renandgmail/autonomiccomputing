using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for Git provider operations and metrics collection
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GitProviderController : ControllerBase
{
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly ILogger<GitProviderController> _logger;

    public GitProviderController(
        IGitProviderFactory gitProviderFactory,
        ILogger<GitProviderController> logger)
    {
        _gitProviderFactory = gitProviderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Validate repository access and provider capabilities
    /// </summary>
    /// <param name="request">Repository validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation results including provider capabilities and access status</returns>
    /// <response code="200">Returns validation results</response>
    /// <response code="400">Invalid repository URL or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error during validation</response>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateRepositoryAccess(
        [FromBody] RepositoryValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository URL is required"));
            }

            _logger.LogInformation("Validating repository access for URL: {Url}", request.RepositoryUrl);

            var provider = _gitProviderFactory.GetProvider(request.RepositoryUrl);
            if (provider == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    $"No provider found for repository URL: {request.RepositoryUrl}. Supported providers: GitHub, GitLab, Bitbucket, Azure DevOps"));
            }

            var validationResult = await provider.ValidateAccessAsync(request.RepositoryUrl, cancellationToken);

            var response = new
            {
                repositoryUrl = request.RepositoryUrl,
                providerType = provider.ProviderType.ToString(),
                canHandle = provider.CanHandle(request.RepositoryUrl),
                validation = new
                {
                    isValid = validationResult.IsValid,
                    details = validationResult.Details,
                    errorMessage = validationResult.ErrorMessage
                },
                capabilities = new
                {
                    canCollectMetrics = validationResult.IsValid,
                    canCollectContributors = validationResult.IsValid,
                    canCollectFileMetrics = validationResult.IsValid,
                    supportsRealTimeData = provider.ProviderType != ProviderType.Local,
                    supportsAuthentication = provider.ProviderType != ProviderType.Local
                },
                recommendations = GenerateProviderRecommendations(provider.ProviderType, validationResult)
            };

            _logger.LogInformation("Validation completed for {Url}: {Valid}, Provider: {Provider}", 
                request.RepositoryUrl, validationResult.IsValid, provider.ProviderType);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating repository access for URL: {Url}", request.RepositoryUrl);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to validate repository access"));
        }
    }

    /// <summary>
    /// Collect comprehensive repository metrics from the appropriate Git provider
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="request">Metrics collection request with provider configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive repository metrics from the Git provider</returns>
    /// <response code="200">Returns repository metrics</response>
    /// <response code="400">Invalid repository ID or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during metrics collection</response>
    [HttpPost("repositories/{repositoryId:int}/metrics")]
    public async Task<IActionResult> CollectRepositoryMetrics(
        [FromRoute] int repositoryId,
        [FromBody] MetricsCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository URL is required"));
            }

            _logger.LogInformation("Collecting repository metrics for ID: {RepositoryId}, URL: {Url}", 
                repositoryId, request.RepositoryUrl);

            var provider = _gitProviderFactory.GetProvider(request.RepositoryUrl);
            if (provider == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    $"No provider found for repository URL: {request.RepositoryUrl}"));
            }

            var context = new RepoLens.Core.Entities.RepositoryContext(
                RepositoryId: repositoryId,
                Url: request.RepositoryUrl,
                ProviderType: provider.ProviderType,
                AuthToken: request.AccessToken,
                LocalClonePath: null,
                Owner: null,
                RepoName: null
            );

            var metrics = await provider.CollectMetricsAsync(context, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                providerType = provider.ProviderType.ToString(),
                collectionTimestamp = metrics.MeasurementDate,
                repositoryUrl = request.RepositoryUrl,
                
                // Core repository statistics
                repository = new
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
                
                // Language and file type analysis
                codeAnalysis = new
                {
                    languageDistribution = metrics.LanguageDistribution,
                    fileTypeDistribution = metrics.FileTypeDistribution,
                    averageCyclomaticComplexity = Math.Round(metrics.AverageCyclomaticComplexity, 2),
                    totalMethods = metrics.TotalMethods,
                    totalClasses = metrics.TotalClasses,
                    maintainabilityIndex = Math.Round(metrics.MaintainabilityIndex, 1)
                },
                
                // Activity and development metrics
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
                
                // Team and collaboration metrics
                collaboration = new
                {
                    totalContributors = metrics.TotalContributors,
                    activeContributors = metrics.ActiveContributors,
                    busFactor = Math.Round(metrics.BusFactor, 1),
                    contributorDiversity = metrics.TotalContributors > 0 ? 
                        Math.Round((double)metrics.ActiveContributors / metrics.TotalContributors * 100, 1) : 0
                },
                
                // Quality and health metrics
                quality = new
                {
                    lineCoveragePercentage = Math.Round(metrics.LineCoveragePercentage, 1),
                    documentationCoverage = Math.Round(metrics.DocumentationCoverage, 1),
                    buildSuccessRate = Math.Round(metrics.BuildSuccessRate, 1),
                    securityVulnerabilities = metrics.SecurityVulnerabilities
                },
                
                // Provider-specific insights
                providerInsights = new
                {
                    dataFreshness = "Real-time from " + provider.ProviderType,
                    collectionMethod = GetCollectionMethodDescription(provider.ProviderType),
                    limitations = GetProviderLimitations(provider.ProviderType),
                    nextUpdateRecommended = DateTime.UtcNow.AddHours(GetUpdateInterval(provider.ProviderType))
                }
            };

            _logger.LogInformation("Successfully collected metrics for repository {RepositoryId} using {Provider}. " +
                "Files: {Files}, Contributors: {Contributors}, Lines: {Lines}", 
                repositoryId, provider.ProviderType, metrics.TotalFiles, metrics.TotalContributors, metrics.TotalLinesOfCode);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access for repository {RepositoryId}: {Message}", repositoryId, ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResult("Authentication failed. Please check your access token."));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation for repository {RepositoryId}: {Message}", repositoryId, ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting repository metrics for ID: {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect repository metrics"));
        }
    }

    /// <summary>
    /// Collect contributor metrics from the appropriate Git provider
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="request">Contributor metrics collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed contributor metrics from the Git provider</returns>
    /// <response code="200">Returns contributor metrics</response>
    /// <response code="400">Invalid repository ID or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during contributor analysis</response>
    [HttpPost("repositories/{repositoryId:int}/contributors")]
    public async Task<IActionResult> CollectContributorMetrics(
        [FromRoute] int repositoryId,
        [FromBody] MetricsCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository URL is required"));
            }

            _logger.LogInformation("Collecting contributor metrics for repository {RepositoryId}", repositoryId);

            var provider = _gitProviderFactory.GetProvider(request.RepositoryUrl);
            if (provider == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    $"No provider found for repository URL: {request.RepositoryUrl}"));
            }

            var context = new RepoLens.Core.Entities.RepositoryContext(
                RepositoryId: repositoryId,
                Url: request.RepositoryUrl,
                ProviderType: provider.ProviderType,
                AuthToken: request.AccessToken,
                LocalClonePath: null,
                Owner: null,
                RepoName: null
            );

            var contributorMetrics = await provider.CollectContributorMetricsAsync(context, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                providerType = provider.ProviderType.ToString(),
                collectionTimestamp = DateTime.UtcNow,
                repositoryUrl = request.RepositoryUrl,
                
                summary = new
                {
                    totalContributors = contributorMetrics.Count,
                    coreContributors = contributorMetrics.Count(c => c.IsCoreContributor),
                    newContributors = contributorMetrics.Count(c => c.IsNewContributor),
                    averageRetentionScore = contributorMetrics.Any() ? 
                        Math.Round(contributorMetrics.Average(c => c.RetentionScore), 2) : 0,
                    totalCommits = contributorMetrics.Sum(c => c.CommitCount),
                    totalLinesAdded = contributorMetrics.Sum(c => c.LinesAdded),
                    averageCommitFrequency = contributorMetrics.Any() ? 
                        Math.Round(contributorMetrics.Average(c => c.CommitFrequency), 3) : 0
                },
                
                contributors = contributorMetrics.Select(c => new
                {
                    name = c.ContributorName,
                    email = c.ContributorEmail,
                    period = new
                    {
                        start = c.PeriodStart,
                        end = c.PeriodEnd
                    },
                    
                    // Contribution statistics
                    contributions = new
                    {
                        commitCount = c.CommitCount,
                        linesAdded = c.LinesAdded,
                        linesDeleted = c.LinesDeleted,
                        filesModified = c.FilesModified,
                        contributionPercentage = Math.Round(c.ContributionPercentage, 2)
                    },
                    
                    // Activity patterns
                    activity = new
                    {
                        workingDays = c.WorkingDays,
                        commitFrequency = Math.Round(c.CommitFrequency, 3),
                        averageCommitSize = Math.Round(c.AverageCommitSize, 1),
                        hourlyActivityPattern = c.HourlyActivityPattern
                    },
                    
                    // Contributor profile
                    profile = new
                    {
                        isCoreContributor = c.IsCoreContributor,
                        isNewContributor = c.IsNewContributor,
                        retentionScore = Math.Round(c.RetentionScore, 3),
                        firstContribution = c.FirstContribution,
                        lastContribution = c.LastContribution
                    }
                }).ToList(),
                
                insights = new
                {
                    topContributors = contributorMetrics
                        .OrderByDescending(c => c.CommitCount)
                        .Take(5)
                        .Select(c => new { name = c.ContributorName, commits = c.CommitCount })
                        .ToList(),
                    
                    riskFactors = new
                    {
                        highDependencyContributors = contributorMetrics
                            .Where(c => c.ContributionPercentage > 30)
                            .Select(c => c.ContributorName)
                            .ToList(),
                        newContributorGrowth = contributorMetrics.Count(c => c.IsNewContributor)
                    },
                    
                    recommendations = GenerateContributorRecommendations(contributorMetrics)
                }
            };

            _logger.LogInformation("Successfully collected contributor metrics for repository {RepositoryId}. " +
                "Contributors: {Count}, Core: {Core}, New: {New}", 
                repositoryId, contributorMetrics.Count, 
                contributorMetrics.Count(c => c.IsCoreContributor), 
                contributorMetrics.Count(c => c.IsNewContributor));

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting contributor metrics for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect contributor metrics"));
        }
    }

    /// <summary>
    /// Collect file-level metrics from the appropriate Git provider
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="request">File metrics collection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed file metrics from the Git provider</returns>
    /// <response code="200">Returns file metrics</response>
    /// <response code="400">Invalid repository ID or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during file analysis</response>
    [HttpPost("repositories/{repositoryId:int}/files")]
    public async Task<IActionResult> CollectFileMetrics(
        [FromRoute] int repositoryId,
        [FromBody] FileMetricsCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RepositoryUrl))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository URL is required"));
            }

            _logger.LogInformation("Collecting file metrics for repository {RepositoryId}", repositoryId);

            var provider = _gitProviderFactory.GetProvider(request.RepositoryUrl);
            if (provider == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    $"No provider found for repository URL: {request.RepositoryUrl}"));
            }

            var context = new RepoLens.Core.Entities.RepositoryContext(
                RepositoryId: repositoryId,
                Url: request.RepositoryUrl,
                ProviderType: provider.ProviderType,
                AuthToken: request.AccessToken,
                LocalClonePath: null,
                Owner: null,
                RepoName: null
            );

            var fileMetrics = await provider.CollectFileMetricsAsync(context, cancellationToken);

            // Apply filtering if specified
            var filteredMetrics = fileMetrics.AsEnumerable();
            if (request.FileExtensions?.Any() == true)
            {
                filteredMetrics = filteredMetrics.Where(f => 
                    request.FileExtensions.Contains(f.FileExtension, StringComparer.OrdinalIgnoreCase));
            }
            if (request.IncludeTestFiles == false)
            {
                filteredMetrics = filteredMetrics.Where(f => !f.IsTestFile);
            }
            if (request.IncludeConfigFiles == false)
            {
                filteredMetrics = filteredMetrics.Where(f => !f.IsConfigurationFile);
            }

            var finalMetrics = filteredMetrics.ToList();

            var response = new
            {
                repositoryId = repositoryId,
                providerType = provider.ProviderType.ToString(),
                collectionTimestamp = DateTime.UtcNow,
                repositoryUrl = request.RepositoryUrl,
                
                summary = new
                {
                    totalFiles = finalMetrics.Count,
                    totalSizeBytes = finalMetrics.Sum(f => f.FileSizeBytes),
                    totalLines = finalMetrics.Sum(f => f.LineCount),
                    averageComplexity = finalMetrics.Any() ? 
                        Math.Round(finalMetrics.Average(f => f.CyclomaticComplexity), 2) : 0,
                    averageMaintainability = finalMetrics.Any() ? 
                        Math.Round(finalMetrics.Average(f => f.MaintainabilityIndex), 1) : 0,
                    hotspotCount = finalMetrics.Count(f => f.IsHotspot),
                    testFileCount = finalMetrics.Count(f => f.IsTestFile),
                    configFileCount = finalMetrics.Count(f => f.IsConfigurationFile)
                },
                
                languageBreakdown = finalMetrics
                    .GroupBy(f => f.PrimaryLanguage)
                    .OrderByDescending(g => g.Sum(f => f.FileSizeBytes))
                    .Select(g => new
                    {
                        language = g.Key,
                        fileCount = g.Count(),
                        totalSizeBytes = g.Sum(f => f.FileSizeBytes),
                        totalLines = g.Sum(f => f.LineCount),
                        averageComplexity = Math.Round(g.Average(f => f.CyclomaticComplexity), 2)
                    })
                    .ToList(),
                
                files = finalMetrics.Select(f => new
                {
                    path = f.FilePath,
                    name = f.FileName,
                    extension = f.FileExtension,
                    language = f.PrimaryLanguage,
                    category = f.FileCategory,
                    
                    // Size and complexity
                    sizeBytes = f.FileSizeBytes,
                    lineCount = f.LineCount,
                    effectiveLineCount = f.EffectiveLineCount,
                    commentLineCount = f.CommentLineCount,
                    blankLineCount = f.BlankLineCount,
                    
                    // Quality metrics
                    cyclomaticComplexity = Math.Round(f.CyclomaticComplexity, 2),
                    maintainabilityIndex = Math.Round(f.MaintainabilityIndex, 1),
                    testCoverage = Math.Round(f.TestCoverage, 1),
                    
                    // Classification flags
                    isTestFile = f.IsTestFile,
                    isConfigurationFile = f.IsConfigurationFile,
                    isHotspot = f.IsHotspot,
                    
                    // Activity information
                    lastAnalyzed = f.LastAnalyzed,
                    firstCommit = f.FirstCommit,
                    lastCommit = f.LastCommit
                }).ToList(),
                
                insights = new
                {
                    hotspots = finalMetrics
                        .Where(f => f.IsHotspot)
                        .OrderByDescending(f => f.CyclomaticComplexity)
                        .Take(10)
                        .Select(f => new { 
                            file = f.FilePath, 
                            complexity = Math.Round(f.CyclomaticComplexity, 2),
                            maintainability = Math.Round(f.MaintainabilityIndex, 1)
                        })
                        .ToList(),
                    
                    qualityConcerns = finalMetrics
                        .Where(f => f.MaintainabilityIndex < 60)
                        .OrderBy(f => f.MaintainabilityIndex)
                        .Take(10)
                        .Select(f => new { 
                            file = f.FilePath, 
                            maintainability = Math.Round(f.MaintainabilityIndex, 1),
                            complexity = Math.Round(f.CyclomaticComplexity, 2)
                        })
                        .ToList(),
                    
                    recommendations = GenerateFileRecommendations(finalMetrics)
                },
                
                filtering = new
                {
                    applied = request.FileExtensions?.Any() == true || 
                             request.IncludeTestFiles == false || 
                             request.IncludeConfigFiles == false,
                    fileExtensions = request.FileExtensions,
                    includeTestFiles = request.IncludeTestFiles,
                    includeConfigFiles = request.IncludeConfigFiles,
                    resultCount = finalMetrics.Count,
                    totalAvailable = fileMetrics.Count
                }
            };

            _logger.LogInformation("Successfully collected file metrics for repository {RepositoryId}. " +
                "Files analyzed: {Count}, Hotspots: {Hotspots}, Average complexity: {Complexity}", 
                repositoryId, finalMetrics.Count, finalMetrics.Count(f => f.IsHotspot),
                finalMetrics.Any() ? finalMetrics.Average(f => f.CyclomaticComplexity) : 0);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting file metrics for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to collect file metrics"));
        }
    }

    /// <summary>
    /// Get supported provider types and their capabilities
    /// </summary>
    /// <returns>List of supported Git providers and their capabilities</returns>
    /// <response code="200">Returns supported providers</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("providers")]
    public IActionResult GetSupportedProviders()
    {
        try
        {
            var providers = new[]
            {
                new
                {
                    type = ProviderType.GitHub.ToString(),
                    name = "GitHub",
                    description = "GitHub repositories with full API integration",
                    capabilities = new[]
                    {
                        "Real-time repository metrics",
                        "Contributor analysis", 
                        "File-level metrics",
                        "Activity patterns",
                        "Language analysis",
                        "Collaboration insights"
                    },
                    urlPatterns = new[] { "github.com", "github.io" },
                    authenticationRequired = true,
                    rateLimitInfo = "5,000 requests/hour with authentication"
                },
                new
                {
                    type = ProviderType.GitLab.ToString(),
                    name = "GitLab",
                    description = "GitLab repositories with API integration",
                    capabilities = new[]
                    {
                        "Repository metrics",
                        "Contributor analysis",
                        "File analysis",
                        "Project statistics"
                    },
                    urlPatterns = new[] { "gitlab.com" },
                    authenticationRequired = true,
                    rateLimitInfo = "2,000 requests/minute with authentication"
                },
                new
                {
                    type = ProviderType.Bitbucket.ToString(),
                    name = "Bitbucket",
                    description = "Bitbucket repositories with API integration",
                    capabilities = new[]
                    {
                        "Repository metrics",
                        "Basic contributor analysis",
                        "File structure analysis"
                    },
                    urlPatterns = new[] { "bitbucket.org" },
                    authenticationRequired = true,
                    rateLimitInfo = "1,000 requests/hour with authentication"
                },
                new
                {
                    type = ProviderType.AzureDevOps.ToString(),
                    name = "Azure DevOps",
                    description = "Azure DevOps repositories with REST API integration",
                    capabilities = new[]
                    {
                        "Repository metrics",
                        "Work item integration",
                        "Build pipeline metrics",
                        "Team collaboration analysis"
                    },
                    urlPatterns = new[] { "dev.azure.com", "visualstudio.com" },
                    authenticationRequired = true,
                    rateLimitInfo = "200 requests/minute with PAT"
                },
                new
                {
                    type = ProviderType.Local.ToString(),
                    name = "Local Git",
                    description = "Local Git repositories",
                    capabilities = new[]
                    {
                        "Basic repository analysis",
                        "Local file metrics",
                        "Git history analysis"
                    },
                    urlPatterns = new[] { "file://", "local path" },
                    authenticationRequired = false,
                    rateLimitInfo = "No rate limits"
                }
            };

            var response = new
            {
                supportedProviders = providers,
                totalProviders = providers.Length,
                defaultProvider = "GitHub",
                recommendedSetup = new
                {
                    github = "Configure GitHub Personal Access Token for full API access",
                    gitlab = "Configure GitLab Access Token for project-level access",
                    bitbucket = "Configure Bitbucket App Password for repository access",
                    azureDevOps = "Configure Personal Access Token with Code Read permissions",
                    local = "No additional setup required for local repositories"
                }
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported providers");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get provider information"));
        }
    }

    #region Private Helper Methods

    private List<string> GenerateProviderRecommendations(ProviderType providerType, ProviderValidationResult validationResult)
    {
        var recommendations = new List<string>();

        if (!validationResult.IsValid)
        {
            recommendations.Add("Verify repository URL format and accessibility");
            if (providerType != ProviderType.Local)
            {
                recommendations.Add("Check authentication token permissions");
            }
        }
        else
        {
            recommendations.Add($"Provider {providerType} is properly configured");
            if (providerType != ProviderType.Local)
            {
                recommendations.Add("Consider setting up webhooks for real-time updates");
            }
        }

        return recommendations;
    }

    private string GetCollectionMethodDescription(ProviderType providerType) => providerType switch
    {
        ProviderType.GitHub => "GitHub REST API v3 with full repository access",
        ProviderType.GitLab => "GitLab REST API v4 with project-level access",
        ProviderType.Bitbucket => "Bitbucket REST API v2 with repository access",
        ProviderType.AzureDevOps => "Azure DevOps REST API with Git repository integration",
        ProviderType.Local => "Local Git repository analysis using LibGit2",
        _ => "Standard Git provider integration"
    };

    private List<string> GetProviderLimitations(ProviderType providerType) => providerType switch
    {
        ProviderType.GitHub => new List<string> { "Rate limited to 5,000 requests/hour", "Public repositories preferred" },
        ProviderType.GitLab => new List<string> { "Rate limited to 2,000 requests/minute", "Project access token required" },
        ProviderType.Bitbucket => new List<string> { "Rate limited to 1,000 requests/hour", "Limited historical data" },
        ProviderType.AzureDevOps => new List<string> { "Rate limited to 200 requests/minute", "Organization access required" },
        ProviderType.Local => new List<string> { "No remote data access", "Limited to local Git history" },
        _ => new List<string>()
    };

    private int GetUpdateInterval(ProviderType providerType) => providerType switch
    {
        ProviderType.GitHub => 6, // 6 hours
        ProviderType.GitLab => 8, // 8 hours
        ProviderType.Bitbucket => 12, // 12 hours
        ProviderType.AzureDevOps => 6, // 6 hours
        ProviderType.Local => 24, // 24 hours
        _ => 12
    };

    private List<string> GenerateContributorRecommendations(IReadOnlyList<ContributorMetrics> contributors)
    {
        var recommendations = new List<string>();

        var coreContributors = contributors.Count(c => c.IsCoreContributor);
        var newContributors = contributors.Count(c => c.IsNewContributor);
        var totalContributions = contributors.Sum(c => c.CommitCount);

        if (coreContributors < 3)
            recommendations.Add("Consider developing more core contributors to reduce bus factor risk");
        
        if (newContributors == 0)
            recommendations.Add("Focus on attracting new contributors to maintain project vitality");
        
        if (contributors.Any() && contributors.Max(c => c.ContributionPercentage) > 50)
            recommendations.Add("High contribution concentration detected - consider knowledge sharing initiatives");

        if (contributors.Any() && contributors.Average(c => c.RetentionScore) < 50)
            recommendations.Add("Low contributor retention - review onboarding and engagement processes");

        return recommendations;
    }

    private List<string> GenerateFileRecommendations(List<FileMetrics> files)
    {
        var recommendations = new List<string>();

        var hotspotCount = files.Count(f => f.IsHotspot);
        var lowMaintainability = files.Count(f => f.MaintainabilityIndex < 60);
        var highComplexity = files.Count(f => f.CyclomaticComplexity > 20);

        if (hotspotCount > files.Count * 0.1)
            recommendations.Add($"{hotspotCount} hotspot files detected - consider refactoring high-activity files");
        
        if (lowMaintainability > 0)
            recommendations.Add($"{lowMaintainability} files have low maintainability scores - prioritize code cleanup");
        
        if (highComplexity > 0)
            recommendations.Add($"{highComplexity} files have high complexity - consider breaking down into smaller components");

        if (files.Any() && files.Average(f => f.TestCoverage) < 70)
            recommendations.Add("Average test coverage is below 70% - increase test coverage for better quality");

        return recommendations;
    }

    #endregion
}

// Request models
public class RepositoryValidationRequest
{
    /// <summary>
    /// Repository URL to validate
    /// </summary>
    [Required]
    public string RepositoryUrl { get; set; } = string.Empty;
}

public class MetricsCollectionRequest
{
    /// <summary>
    /// Repository URL to collect metrics from
    /// </summary>
    [Required]
    public string RepositoryUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token for private repository access
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Branch name to analyze (default: main)
    /// </summary>
    public string? BranchName { get; set; }
}

public class FileMetricsCollectionRequest : MetricsCollectionRequest
{
    /// <summary>
    /// Filter files by extensions (e.g., [".cs", ".js"])
    /// </summary>
    public string[]? FileExtensions { get; set; }
    
    /// <summary>
    /// Include test files in analysis (default: true)
    /// </summary>
    public bool? IncludeTestFiles { get; set; } = true;
    
    /// <summary>
    /// Include configuration files in analysis (default: true)
    /// </summary>
    public bool? IncludeConfigFiles { get; set; } = true;
}
