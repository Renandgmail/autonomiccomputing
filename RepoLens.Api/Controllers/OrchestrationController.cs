using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for orchestrated metrics collection and comprehensive repository analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrchestrationController : ControllerBase
{
    private readonly IMetricsCollectionService _metricsCollectionService;
    private readonly ILogger<OrchestrationController> _logger;

    public OrchestrationController(
        IMetricsCollectionService metricsCollectionService,
        ILogger<OrchestrationController> logger)
    {
        _metricsCollectionService = metricsCollectionService;
        _logger = logger;
    }

    /// <summary>
    /// Execute comprehensive orchestrated repository analysis
    /// </summary>
    /// <param name="repositoryId">Repository ID for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete repository analysis with structure, git history, and quality metrics</returns>
    /// <response code="200">Returns comprehensive repository analysis results</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during orchestrated analysis</response>
    [HttpPost("repositories/{repositoryId:int}/analyze")]
    public async Task<IActionResult> ExecuteComprehensiveAnalysis(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository ID must be greater than 0"));
            }

            _logger.LogInformation("Starting comprehensive orchestrated analysis for repository {RepositoryId}", repositoryId);

            var repositoryMetrics = await _metricsCollectionService.CollectRepositoryMetricsAsync(repositoryId);

            var response = new
            {
                repositoryId = repositoryId,
                analysisTimestamp = repositoryMetrics.MeasurementDate,
                orchestrationEngine = "RepoLens Comprehensive Analysis Platform",
                analysisType = "Full Repository Orchestration",
                
                // Repository structure and composition
                structure = new
                {
                    totalFiles = repositoryMetrics.TotalFiles,
                    totalDirectories = repositoryMetrics.TotalDirectories,
                    repositorySizeBytes = repositoryMetrics.RepositorySizeBytes,
                    averageFileSize = repositoryMetrics.AverageFileSize,
                    maxDirectoryDepth = repositoryMetrics.MaxDirectoryDepth,
                    textFileCount = repositoryMetrics.TextFileCount,
                    binaryFileCount = repositoryMetrics.BinaryFileCount,
                    formattedSize = FormatBytes(repositoryMetrics.RepositorySizeBytes)
                },
                
                // Language and technology analysis
                composition = new
                {
                    languageDistribution = repositoryMetrics.LanguageDistribution,
                    fileTypeDistribution = repositoryMetrics.FileTypeDistribution,
                    primaryLanguage = GetPrimaryLanguage(repositoryMetrics.LanguageDistribution),
                    languageDiversity = CalculateLanguageDiversity(repositoryMetrics.LanguageDistribution)
                },
                
                // Code quality and maintainability
                quality = new
                {
                    totalLinesOfCode = repositoryMetrics.TotalLinesOfCode,
                    effectiveLinesOfCode = repositoryMetrics.EffectiveLinesOfCode,
                    commentLines = repositoryMetrics.CommentLines,
                    blankLines = repositoryMetrics.BlankLines,
                    commentRatio = Math.Round(repositoryMetrics.CommentRatio, 2),
                    maintainabilityIndex = Math.Round(repositoryMetrics.MaintainabilityIndex, 1),
                    technicalDebtHours = Math.Round(repositoryMetrics.TechnicalDebtHours, 1),
                    codeSmells = repositoryMetrics.CodeSmells,
                    duplicationPercentage = Math.Round(repositoryMetrics.DuplicationPercentage, 2),
                    qualityGateFailures = repositoryMetrics.QualityGateFailures,
                    overallQualityGrade = CalculateQualityGrade(repositoryMetrics.MaintainabilityIndex)
                },
                
                // Complexity analysis
                complexity = new
                {
                    averageCyclomaticComplexity = Math.Round(repositoryMetrics.AverageCyclomaticComplexity, 2),
                    maxCyclomaticComplexity = repositoryMetrics.MaxCyclomaticComplexity,
                    cognitiveComplexity = Math.Round(repositoryMetrics.CognitiveComplexity, 1),
                    averageMethodLength = Math.Round(repositoryMetrics.AverageMethodLength, 1),
                    averageClassSize = Math.Round(repositoryMetrics.AverageClassSize, 1),
                    totalMethods = repositoryMetrics.TotalMethods,
                    totalClasses = repositoryMetrics.TotalClasses,
                    complexityRating = CalculateComplexityRating(repositoryMetrics.AverageCyclomaticComplexity)
                },
                
                // Git activity and collaboration
                activity = new
                {
                    commitsLastWeek = repositoryMetrics.CommitsLastWeek,
                    commitsLastMonth = repositoryMetrics.CommitsLastMonth,
                    commitsLastQuarter = repositoryMetrics.CommitsLastQuarter,
                    developmentVelocity = Math.Round(repositoryMetrics.DevelopmentVelocity, 2),
                    averageCommitSize = Math.Round(repositoryMetrics.AverageCommitSize, 1),
                    filesChangedLastWeek = repositoryMetrics.FilesChangedLastWeek,
                    linesAddedLastWeek = repositoryMetrics.LinesAddedLastWeek,
                    linesDeletedLastWeek = repositoryMetrics.LinesDeletedLastWeek,
                    netLinesChanged = repositoryMetrics.LinesAddedLastWeek - repositoryMetrics.LinesDeletedLastWeek,
                    activityLevel = CalculateActivityLevel(repositoryMetrics.CommitsLastWeek, repositoryMetrics.CommitsLastMonth)
                },
                
                // Team collaboration insights
                collaboration = new
                {
                    totalContributors = repositoryMetrics.TotalContributors,
                    activeContributors = repositoryMetrics.ActiveContributors,
                    busFactor = Math.Round(repositoryMetrics.BusFactor, 1),
                    codeOwnershipConcentration = Math.Round(repositoryMetrics.CodeOwnershipConcentration, 3),
                    collaborationScore = CalculateCollaborationScore(repositoryMetrics.TotalContributors, repositoryMetrics.ActiveContributors),
                    teamRisk = AssessTeamRisk(repositoryMetrics.BusFactor, repositoryMetrics.CodeOwnershipConcentration)
                },
                
                // Activity patterns
                patterns = new
                {
                    hourlyActivityPattern = repositoryMetrics.HourlyActivityPattern,
                    dailyActivityPattern = repositoryMetrics.DailyActivityPattern,
                    peakActivityHours = ExtractPeakHours(repositoryMetrics.HourlyActivityPattern),
                    mostActiveDays = ExtractActiveDays(repositoryMetrics.DailyActivityPattern)
                },
                
                // Testing and documentation
                testing = new
                {
                    lineCoverage = Math.Round(repositoryMetrics.LineCoveragePercentage, 1),
                    branchCoverage = Math.Round(repositoryMetrics.BranchCoveragePercentage, 1),
                    functionCoverage = Math.Round(repositoryMetrics.FunctionCoveragePercentage, 1),
                    testToCodeRatio = Math.Round(repositoryMetrics.TestToCodeRatio, 2),
                    testPassRate = Math.Round(repositoryMetrics.TestPassRate, 1),
                    testingMaturity = CalculateTestingMaturity(repositoryMetrics.LineCoveragePercentage, repositoryMetrics.TestToCodeRatio)
                },
                
                // Documentation analysis
                documentation = new
                {
                    overallCoverage = Math.Round(repositoryMetrics.DocumentationCoverage, 1),
                    apiDocumentation = Math.Round(repositoryMetrics.ApiDocumentationCoverage, 1),
                    readmeWordCount = repositoryMetrics.ReadmeWordCount,
                    documentationScore = CalculateDocumentationScore(
                        repositoryMetrics.DocumentationCoverage, 
                        repositoryMetrics.ApiDocumentationCoverage, 
                        repositoryMetrics.ReadmeWordCount)
                },
                
                // Build and deployment health
                build = new
                {
                    successRate = Math.Round(repositoryMetrics.BuildSuccessRate, 1),
                    testPassRate = Math.Round(repositoryMetrics.TestPassRate, 1),
                    qualityGateFailures = repositoryMetrics.QualityGateFailures,
                    buildHealthStatus = CalculateBuildHealth(repositoryMetrics.BuildSuccessRate, repositoryMetrics.TestPassRate)
                },
                
                // Security and dependencies
                security = new
                {
                    totalDependencies = repositoryMetrics.TotalDependencies,
                    outdatedDependencies = repositoryMetrics.OutdatedDependencies,
                    vulnerableDependencies = repositoryMetrics.VulnerableDependencies,
                    securityVulnerabilities = repositoryMetrics.SecurityVulnerabilities,
                    criticalVulnerabilities = repositoryMetrics.CriticalVulnerabilities,
                    dependencyHealthPercentage = CalculateDependencyHealth(
                        repositoryMetrics.TotalDependencies, 
                        repositoryMetrics.OutdatedDependencies, 
                        repositoryMetrics.VulnerableDependencies),
                    securityRisk = AssessSecurityRisk(
                        repositoryMetrics.SecurityVulnerabilities, 
                        repositoryMetrics.CriticalVulnerabilities)
                },
                
                // Overall health scoring
                healthScores = new
                {
                    overallHealthScore = CalculateOverallHealth(repositoryMetrics),
                    qualityScore = CalculateQualityScore(repositoryMetrics),
                    maintainabilityScore = repositoryMetrics.MaintainabilityIndex,
                    securityScore = CalculateSecurityScore(repositoryMetrics),
                    teamHealthScore = CalculateTeamHealthScore(repositoryMetrics),
                    projectMaturity = CalculateProjectMaturity(repositoryMetrics)
                },
                
                // Actionable insights and recommendations
                insights = new
                {
                    strengths = GenerateStrengths(repositoryMetrics),
                    concerns = GenerateConcerns(repositoryMetrics),
                    recommendations = GenerateRecommendations(repositoryMetrics),
                    priorityActions = GeneratePriorityActions(repositoryMetrics)
                },
                
                // Analysis metadata
                analysis = new
                {
                    orchestrationMode = "Comprehensive Multi-Dimensional Analysis",
                    analysisComponents = new[]
                    {
                        "Repository Structure Analysis",
                        "Git History Mining",
                        "Language Distribution Assessment",
                        "Code Quality Evaluation",
                        "Complexity Analysis",
                        "Team Collaboration Assessment",
                        "Security Vulnerability Scanning",
                        "Documentation Analysis"
                    },
                    dataFreshness = "Real-time with historical context",
                    nextRecommendedAnalysis = DateTime.UtcNow.AddHours(24)
                }
            };

            _logger.LogInformation("Successfully completed comprehensive analysis for repository {RepositoryId}. " +
                "Overall health: {Health}, Quality: {Quality}, Security: {Security}", 
                repositoryId, 
                CalculateOverallHealth(repositoryMetrics),
                repositoryMetrics.MaintainabilityIndex,
                CalculateSecurityScore(repositoryMetrics));

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult($"Repository {repositoryId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing comprehensive analysis for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to execute comprehensive repository analysis"));
        }
    }

    /// <summary>
    /// Execute orchestrated contributor analysis for a specific time period
    /// </summary>
    /// <param name="repositoryId">Repository ID for analysis</param>
    /// <param name="request">Contributor analysis period request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive contributor analysis results</returns>
    /// <response code="200">Returns contributor analysis results</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during contributor analysis</response>
    [HttpPost("repositories/{repositoryId:int}/contributors")]
    public async Task<IActionResult> ExecuteContributorAnalysis(
        [FromRoute] int repositoryId,
        [FromBody] ContributorAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository ID must be greater than 0"));
            }

            if (request.PeriodStart >= request.PeriodEnd)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Period start must be before period end"));
            }

            _logger.LogInformation("Executing contributor analysis for repository {RepositoryId} from {Start} to {End}", 
                repositoryId, request.PeriodStart, request.PeriodEnd);

            var contributorMetrics = await _metricsCollectionService.CollectContributorMetricsAsync(
                repositoryId, request.PeriodStart, request.PeriodEnd);

            var response = new
            {
                repositoryId = repositoryId,
                analysisTimestamp = DateTime.UtcNow,
                analysisType = "Orchestrated Contributor Analysis",
                
                period = new
                {
                    start = request.PeriodStart,
                    end = request.PeriodEnd,
                    durationDays = (request.PeriodEnd - request.PeriodStart).TotalDays
                },
                
                summary = new
                {
                    totalContributors = contributorMetrics.Count,
                    analysisStatus = contributorMetrics.Count > 0 ? "Complete" : "No contributor data available",
                    dataSource = "Orchestrated Git Analysis Engine"
                },
                
                contributors = contributorMetrics.Select(c => new
                {
                    name = c.ContributorName,
                    email = c.ContributorEmail,
                    
                    contributions = new
                    {
                        commitCount = c.CommitCount,
                        linesAdded = c.LinesAdded,
                        linesDeleted = c.LinesDeleted,
                        filesModified = c.FilesModified,
                        contributionPercentage = Math.Round(c.ContributionPercentage, 2)
                    },
                    
                    engagement = new
                    {
                        workingDays = c.WorkingDays,
                        commitFrequency = Math.Round(c.CommitFrequency, 3),
                        isCoreContributor = c.IsCoreContributor,
                        isNewContributor = c.IsNewContributor
                    },
                    
                    timeline = new
                    {
                        firstContribution = c.FirstContribution,
                        lastContribution = c.LastContribution,
                        retentionScore = Math.Round(c.RetentionScore, 2)
                    },
                    
                    patterns = new
                    {
                        hourlyActivityPattern = c.HourlyActivityPattern
                    }
                }).ToList(),
                
                insights = new
                {
                    message = contributorMetrics.Count > 0 
                        ? "Contributor analysis completed successfully"
                        : "No contributor data available for the specified period. This may indicate a new repository or limited git history access.",
                    nextSteps = contributorMetrics.Count == 0 
                        ? new[]
                        {
                            "Verify repository has git history",
                            "Check git provider connectivity",
                            "Ensure proper authentication for private repositories"
                        }
                        : new[]
                        {
                            "Review contributor engagement patterns",
                            "Identify opportunities for team growth",
                            "Monitor contributor retention trends"
                        }
                },
                
                orchestration = new
                {
                    analysisEngine = "RepoLens Contributor Intelligence",
                    dataCollection = "Git history mining with contributor profiling",
                    nextRecommendedAnalysis = DateTime.UtcNow.AddDays(7)
                }
            };

            _logger.LogInformation("Successfully completed contributor analysis for repository {RepositoryId}. Contributors: {Count}", 
                repositoryId, contributorMetrics.Count);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult($"Repository {repositoryId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing contributor analysis for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to execute contributor analysis"));
        }
    }

    /// <summary>
    /// Execute orchestrated file-level analysis
    /// </summary>
    /// <param name="repositoryId">Repository ID for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive file analysis results</returns>
    /// <response code="200">Returns file analysis results</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during file analysis</response>
    [HttpPost("repositories/{repositoryId:int}/files")]
    public async Task<IActionResult> ExecuteFileAnalysis(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository ID must be greater than 0"));
            }

            _logger.LogInformation("Executing file analysis for repository {RepositoryId}", repositoryId);

            var fileMetrics = await _metricsCollectionService.CollectFileMetricsAsync(repositoryId);

            var response = new
            {
                repositoryId = repositoryId,
                analysisTimestamp = DateTime.UtcNow,
                analysisType = "Orchestrated File Analysis",
                
                summary = new
                {
                    totalFiles = fileMetrics.Count,
                    analysisStatus = fileMetrics.Count > 0 ? "Complete" : "No file data available",
                    dataSource = "Orchestrated File Intelligence Engine"
                },
                
                files = fileMetrics.Select(f => new
                {
                    path = f.FilePath,
                    name = f.FileName,
                    extension = f.FileExtension,
                    language = f.PrimaryLanguage,
                    category = f.FileCategory,
                    
                    metrics = new
                    {
                        sizeBytes = f.FileSizeBytes,
                        lineCount = f.LineCount,
                        effectiveLineCount = f.EffectiveLineCount,
                        commentLineCount = f.CommentLineCount,
                        blankLineCount = f.BlankLineCount
                    },
                    
                    quality = new
                    {
                        cyclomaticComplexity = Math.Round(f.CyclomaticComplexity, 2),
                        maintainabilityIndex = Math.Round(f.MaintainabilityIndex, 1),
                        testCoverage = Math.Round(f.TestCoverage, 1)
                    },
                    
                    classification = new
                    {
                        isTestFile = f.IsTestFile,
                        isConfigurationFile = f.IsConfigurationFile,
                        isHotspot = f.IsHotspot
                    },
                    
                    timestamps = new
                    {
                        lastAnalyzed = f.LastAnalyzed,
                        firstCommit = f.FirstCommit,
                        lastCommit = f.LastCommit
                    }
                }).ToList(),
                
                insights = new
                {
                    message = fileMetrics.Count > 0 
                        ? "File analysis completed successfully"
                        : "No file data available. This may indicate repository access limitations or analysis constraints.",
                    nextSteps = fileMetrics.Count == 0 
                        ? new[]
                        {
                            "Verify repository access permissions",
                            "Check file system connectivity",
                            "Review analysis scope configuration"
                        }
                        : new[]
                        {
                            "Review file complexity patterns",
                            "Identify refactoring opportunities", 
                            "Monitor code quality trends"
                        }
                },
                
                orchestration = new
                {
                    analysisEngine = "RepoLens File Intelligence",
                    dataCollection = "File system analysis with quality assessment",
                    nextRecommendedAnalysis = DateTime.UtcNow.AddDays(1)
                }
            };

            _logger.LogInformation("Successfully completed file analysis for repository {RepositoryId}. Files: {Count}", 
                repositoryId, fileMetrics.Count);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult($"Repository {repositoryId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing file analysis for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to execute file analysis"));
        }
    }

    /// <summary>
    /// Execute complete orchestrated analysis of all metrics
    /// </summary>
    /// <param name="repositoryId">Repository ID for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete orchestrated analysis results</returns>
    /// <response code="200">Returns complete analysis results</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during complete analysis</response>
    [HttpPost("repositories/{repositoryId:int}/complete")]
    public async Task<IActionResult> ExecuteCompleteOrchestration(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository ID must be greater than 0"));
            }

            _logger.LogInformation("Starting complete orchestrated analysis for repository {RepositoryId}", repositoryId);

            // Execute the complete orchestrated analysis
            await _metricsCollectionService.CollectAllMetricsAsync(repositoryId);

            // Get the updated repository metrics after complete collection
            var repositoryMetrics = await _metricsCollectionService.CollectRepositoryMetricsAsync(repositoryId);

            var response = new
            {
                repositoryId = repositoryId,
                analysisTimestamp = DateTime.UtcNow,
                analysisType = "Complete Orchestrated Analysis",
                orchestrationEngine = "RepoLens Enterprise Analytics Platform",
                
                executionSummary = new
                {
                    status = "Complete",
                    duration = "Orchestrated multi-phase analysis",
                    componentsExecuted = new[]
                    {
                        "Repository Structure Analysis",
                        "Contributor Metrics Collection", 
                        "File-Level Analysis",
                        "Comprehensive Quality Assessment"
                    }
                },
                
                // Core metrics from the orchestrated analysis
                metrics = new
                {
                    measurementDate = repositoryMetrics.MeasurementDate,
                    totalFiles = repositoryMetrics.TotalFiles,
                    totalDirectories = repositoryMetrics.TotalDirectories,
                    repositorySizeBytes = repositoryMetrics.RepositorySizeBytes,
                    totalLinesOfCode = repositoryMetrics.TotalLinesOfCode,
                    maintainabilityIndex = Math.Round(repositoryMetrics.MaintainabilityIndex, 1),
                    commitsLastMonth = repositoryMetrics.CommitsLastMonth,
                    totalContributors = repositoryMetrics.TotalContributors,
                    buildSuccessRate = Math.Round(repositoryMetrics.BuildSuccessRate, 1)
                },
                
                // Overall health assessment
                healthAssessment = new
                {
                    overallScore = CalculateOverallHealth(repositoryMetrics),
                    qualityRating = CalculateQualityGrade(repositoryMetrics.MaintainabilityIndex),
                    securityRisk = AssessSecurityRisk(repositoryMetrics.SecurityVulnerabilities, repositoryMetrics.CriticalVulnerabilities),
                    teamHealth = CalculateTeamHealthScore(repositoryMetrics),
                    projectMaturity = CalculateProjectMaturity(repositoryMetrics)
                },
                
                // Key insights from orchestrated analysis
                insights = new
                {
                    topStrengths = GenerateStrengths(repositoryMetrics).Take(3).ToList(),
                    primaryConcerns = GenerateConcerns(repositoryMetrics).Take(3).ToList(),
                    actionableRecommendations = GenerateRecommendations(repositoryMetrics).Take(5).ToList()
                },
                
                orchestration = new
                {
                    platformVersion = "RepoLens Enterprise v2.0",
                    analysisDepth = "Comprehensive Multi-Dimensional",
                    dataIntegration = "Repository + Git + Provider APIs",
                    nextOrchestration = DateTime.UtcNow.AddHours(24),
                    reportGenerated = DateTime.UtcNow
                }
            };

            _logger.LogInformation("Successfully completed orchestrated analysis for repository {RepositoryId}. " +
                "Health Score: {Health}, Files: {Files}, Contributors: {Contributors}", 
                repositoryId, CalculateOverallHealth(repositoryMetrics), 
                repositoryMetrics.TotalFiles, repositoryMetrics.TotalContributors);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult($"Repository {repositoryId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing complete orchestration for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to execute complete orchestrated analysis"));
        }
    }

    /// <summary>
    /// Check if metrics collection is needed for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis recommendation based on last collection date</returns>
    /// <response code="200">Returns collection recommendation</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error during check</response>
    [HttpGet("repositories/{repositoryId:int}/should-analyze")]
    public async Task<IActionResult> ShouldExecuteAnalysis(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId <= 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository ID must be greater than 0"));
            }

            _logger.LogInformation("Checking analysis recommendation for repository {RepositoryId}", repositoryId);

            var shouldCollect = await _metricsCollectionService.ShouldCollectMetricsAsync(repositoryId);

            var response = new
            {
                repositoryId = repositoryId,
                checkTimestamp = DateTime.UtcNow,
                
                recommendation = new
                {
                    shouldAnalyze = shouldCollect,
                    reason = shouldCollect 
                        ? "No analysis performed today - fresh analysis recommended"
                        : "Analysis already performed today - consider tomorrow",
                    priority = shouldCollect ? "High" : "Low",
                    estimatedDuration = "5-10 minutes for complete orchestrated analysis"
                },
                
                orchestration = new
                {
                    analysisFrequency = "Daily recommended for active repositories",
                    lastCheck = DateTime.UtcNow,
                    analysisTypes = new[]
                    {
                        "Complete Orchestrated Analysis",
                        "Repository Structure Analysis", 
                        "Contributor Analysis",
                        "File-Level Analysis"
                    }
                },
                
                nextSteps = shouldCollect 
                    ? new[]
                    {
                        "Execute complete orchestrated analysis",
                        "Review generated insights and recommendations",
                        "Schedule regular analysis cadence"
                    }
                    : new[]
                    {
                        "Review existing analysis results",
                        "Monitor repository changes",
                        "Schedule next analysis for tomorrow"
                    }
            };

            _logger.LogInformation("Analysis recommendation for repository {RepositoryId}: {ShouldAnalyze}", 
                repositoryId, shouldCollect);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking analysis recommendation for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to check analysis recommendation"));
        }
    }

    #region Private Helper Methods

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private static string GetPrimaryLanguage(string? languageDistribution)
    {
        if (string.IsNullOrEmpty(languageDistribution))
            return "Unknown";
            
        try
        {
            var languages = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(languageDistribution);
            return languages?.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private static double CalculateLanguageDiversity(string? languageDistribution)
    {
        if (string.IsNullOrEmpty(languageDistribution))
            return 0;
            
        try
        {
            var languages = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(languageDistribution);
            return languages?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string CalculateQualityGrade(double maintainabilityIndex)
    {
        return maintainabilityIndex switch
        {
            >= 85 => "A - Excellent",
            >= 70 => "B - Good", 
            >= 60 => "C - Fair",
            >= 40 => "D - Poor",
            _ => "F - Critical"
        };
    }

    private static string CalculateComplexityRating(double avgComplexity)
    {
        return avgComplexity switch
        {
            <= 5 => "Low",
            <= 10 => "Moderate",
            <= 15 => "High",
            _ => "Very High"
        };
    }

    private static string CalculateActivityLevel(int commitsLastWeek, int commitsLastMonth)
    {
        var weeklyAverage = commitsLastMonth / 4.0;
        var currentRatio = commitsLastWeek / weeklyAverage;
        
        return currentRatio switch
        {
            >= 1.5 => "Very Active",
            >= 1.0 => "Active",
            >= 0.5 => "Moderate",
            > 0 => "Low",
            _ => "Inactive"
        };
    }

    private static double CalculateCollaborationScore(int totalContributors, int activeContributors)
    {
        if (totalContributors == 0) return 0;
        return Math.Round((double)activeContributors / totalContributors * 100, 1);
    }

    private static string AssessTeamRisk(double busFactor, double ownershipConcentration)
    {
        if (busFactor < 2 || ownershipConcentration > 0.8)
            return "High";
        if (busFactor < 3 || ownershipConcentration > 0.6)
            return "Medium";
        return "Low";
    }

    private static List<string> ExtractPeakHours(string? hourlyPattern)
    {
        if (string.IsNullOrEmpty(hourlyPattern))
            return new List<string>();
            
        try
        {
            var hours = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(hourlyPattern);
            return hours?.OrderByDescending(kvp => kvp.Value).Take(3).Select(kvp => $"{kvp.Key}:00").ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static List<string> ExtractActiveDays(string? dailyPattern)
    {
        if (string.IsNullOrEmpty(dailyPattern))
            return new List<string>();
            
        try
        {
            var days = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(dailyPattern);
            return days?.OrderByDescending(kvp => kvp.Value).Take(3).Select(kvp => kvp.Key).ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string CalculateTestingMaturity(double lineCoverage, double testToCodeRatio)
    {
        var score = (lineCoverage / 100 + Math.Min(testToCodeRatio, 1.0)) / 2 * 100;
        return score switch
        {
            >= 80 => "Mature",
            >= 60 => "Developing",
            >= 40 => "Basic",
            _ => "Minimal"
        };
    }

    private static double CalculateDocumentationScore(double docCoverage, double apiDocCoverage, int readmeWords)
    {
        var readmeScore = Math.Min(readmeWords / 500.0, 1.0) * 100;
        return (docCoverage + apiDocCoverage + readmeScore) / 3;
    }

    private static string CalculateBuildHealth(double buildSuccess, double testPass)
    {
        var avgHealth = (buildSuccess + testPass) / 2;
        return avgHealth switch
        {
            >= 95 => "Excellent",
            >= 85 => "Good",
            >= 70 => "Fair",
            _ => "Poor"
        };
    }

    private static double CalculateDependencyHealth(int total, int outdated, int vulnerable)
    {
        if (total == 0) return 100;
        var healthyDeps = total - outdated - vulnerable;
        return Math.Round((double)healthyDeps / total * 100, 1);
    }

    private static string AssessSecurityRisk(int vulnerabilities, int criticalVulns)
    {
        if (criticalVulns > 0) return "Critical";
        if (vulnerabilities > 5) return "High";
        if (vulnerabilities > 0) return "Medium";
        return "Low";
    }

    private static double CalculateOverallHealth(RepositoryMetrics metrics)
    {
        var scores = new[]
        {
            metrics.MaintainabilityIndex,
            metrics.BuildSuccessRate,
            metrics.LineCoveragePercentage,
            Math.Max(0, 100 - metrics.SecurityVulnerabilities * 10),
            Math.Min(100, metrics.ActiveContributors * 20)
        };
        
        return Math.Round(scores.Average(), 1);
    }

    private static double CalculateQualityScore(RepositoryMetrics metrics)
    {
        return Math.Round((metrics.MaintainabilityIndex + 
                          (100 - Math.Min(metrics.TechnicalDebtHours * 10, 100)) +
                          (100 - Math.Min(metrics.CodeSmells * 5, 100))) / 3, 1);
    }

    private static double CalculateSecurityScore(RepositoryMetrics metrics)
    {
        var vulnerabilityPenalty = metrics.SecurityVulnerabilities * 15 + metrics.CriticalVulnerabilities * 30;
        var dependencyPenalty = metrics.VulnerableDependencies * 10;
        return Math.Max(0, 100 - vulnerabilityPenalty - dependencyPenalty);
    }

    private static double CalculateTeamHealthScore(RepositoryMetrics metrics)
    {
        var busFactorScore = Math.Min(metrics.BusFactor * 20, 100);
        var contributorScore = Math.Min(metrics.ActiveContributors * 15, 100);
        var ownershipScore = (1 - metrics.CodeOwnershipConcentration) * 100;
        
        return Math.Round((busFactorScore + contributorScore + ownershipScore) / 3, 1);
    }

    private static string CalculateProjectMaturity(RepositoryMetrics metrics)
    {
        var maturityFactors = new[]
        {
            metrics.DocumentationCoverage > 70,
            metrics.LineCoveragePercentage > 70,
            metrics.TotalContributors > 3,
            metrics.BuildSuccessRate > 90,
            metrics.SecurityVulnerabilities == 0
        };
        
        var maturityScore = maturityFactors.Count(f => f);
        return maturityScore switch
        {
            >= 4 => "Mature",
            >= 3 => "Established", 
            >= 2 => "Developing",
            _ => "Early Stage"
        };
    }

    private static List<string> GenerateStrengths(RepositoryMetrics metrics)
    {
        var strengths = new List<string>();
        
        if (metrics.MaintainabilityIndex >= 80)
            strengths.Add("High code maintainability and quality");
        if (metrics.BuildSuccessRate >= 95)
            strengths.Add("Excellent build stability");
        if (metrics.LineCoveragePercentage >= 80)
            strengths.Add("Strong test coverage");
        if (metrics.DocumentationCoverage >= 70)
            strengths.Add("Good documentation coverage");
        if (metrics.SecurityVulnerabilities == 0)
            strengths.Add("No known security vulnerabilities");
        if (metrics.ActiveContributors >= 5)
            strengths.Add("Active contributor community");
        
        return strengths.Take(5).ToList();
    }

    private static List<string> GenerateConcerns(RepositoryMetrics metrics)
    {
        var concerns = new List<string>();
        
        if (metrics.MaintainabilityIndex < 60)
            concerns.Add("Low maintainability index indicates quality issues");
        if (metrics.SecurityVulnerabilities > 0)
            concerns.Add($"{metrics.SecurityVulnerabilities} security vulnerabilities detected");
        if (metrics.LineCoveragePercentage < 50)
            concerns.Add("Low test coverage increases risk");
        if (metrics.BusFactor < 2)
            concerns.Add("High dependency on few contributors");
        if (metrics.TechnicalDebtHours > 20)
            concerns.Add("Significant technical debt accumulation");
        if (metrics.OutdatedDependencies > metrics.TotalDependencies * 0.3)
            concerns.Add("Many outdated dependencies");
        
        return concerns.Take(5).ToList();
    }

    private static List<string> GenerateRecommendations(RepositoryMetrics metrics)
    {
        var recommendations = new List<string>();
        
        if (metrics.MaintainabilityIndex < 70)
            recommendations.Add("Focus on code refactoring to improve maintainability");
        if (metrics.LineCoveragePercentage < 70)
            recommendations.Add("Increase test coverage to reduce quality risks");
        if (metrics.SecurityVulnerabilities > 0)
            recommendations.Add("Address security vulnerabilities immediately");
        if (metrics.BusFactor < 3)
            recommendations.Add("Encourage more contributors to reduce project risk");
        if (metrics.DocumentationCoverage < 60)
            recommendations.Add("Improve code documentation and API docs");
        if (metrics.OutdatedDependencies > 5)
            recommendations.Add("Update outdated dependencies regularly");
        if (metrics.TechnicalDebtHours > 15)
            recommendations.Add("Allocate time for technical debt reduction");
        
        return recommendations.Take(7).ToList();
    }

    private static List<string> GeneratePriorityActions(RepositoryMetrics metrics)
    {
        var actions = new List<(string action, int priority)>();
        
        if (metrics.CriticalVulnerabilities > 0)
            actions.Add(("Fix critical security vulnerabilities", 1));
        if (metrics.BuildSuccessRate < 80)
            actions.Add(("Stabilize build pipeline", 2));
        if (metrics.SecurityVulnerabilities > 0)
            actions.Add(("Address security vulnerabilities", 3));
        if (metrics.MaintainabilityIndex < 50)
            actions.Add(("Urgent code quality improvement", 4));
        if (metrics.BusFactor < 2)
            actions.Add(("Expand contributor base", 5));
        
        return actions.OrderBy(a => a.priority).Select(a => a.action).ToList();
    }

    #endregion
}

// Request models
public class ContributorAnalysisRequest
{
    /// <summary>
    /// Start date for contributor analysis period
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// End date for contributor analysis period
    /// </summary>
    [Required]
    public DateTime PeriodEnd { get; set; }
}
