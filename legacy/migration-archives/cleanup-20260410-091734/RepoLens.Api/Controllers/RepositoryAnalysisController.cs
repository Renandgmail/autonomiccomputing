using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for repository analysis and processing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepositoryAnalysisController : ControllerBase
{
    private readonly IRepositoryAnalysisService _repositoryAnalysisService;
    private readonly ILogger<RepositoryAnalysisController> _logger;

    public RepositoryAnalysisController(
        IRepositoryAnalysisService repositoryAnalysisService,
        ILogger<RepositoryAnalysisController> logger)
    {
        _repositoryAnalysisService = repositoryAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Start full analysis of a repository including file scanning and code element extraction
    /// </summary>
    /// <param name="repositoryId">Repository ID to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis job information with job ID for tracking</returns>
    /// <response code="200">Returns analysis job ID and initial status</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during analysis startup</response>
    [HttpPost("repositories/{repositoryId:int}/analyze/full")]
    public async Task<IActionResult> StartFullAnalysis(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting full analysis for repository {RepositoryId}", repositoryId);

            var jobId = await _repositoryAnalysisService.StartFullAnalysisAsync(repositoryId, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                jobId = jobId,
                message = "Full repository analysis started successfully",
                statusUrl = $"/api/repositoryanalysis/jobs/{jobId}/progress",
                estimatedDuration = "5-15 minutes depending on repository size"
            };

            _logger.LogInformation("Started full analysis job {JobId} for repository {RepositoryId}", jobId, repositoryId);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting full analysis for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to start repository analysis"));
        }
    }

    /// <summary>
    /// Start incremental analysis for specific files or recent changes
    /// </summary>
    /// <param name="repositoryId">Repository ID to analyze</param>
    /// <param name="request">Incremental analysis request with optional file list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis job information with job ID for tracking</returns>
    /// <response code="200">Returns analysis job ID and initial status</response>
    /// <response code="400">Invalid repository ID or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during analysis startup</response>
    [HttpPost("repositories/{repositoryId:int}/analyze/incremental")]
    public async Task<IActionResult> StartIncrementalAnalysis(
        [FromRoute] int repositoryId,
        [FromBody] IncrementalAnalysisRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting incremental analysis for repository {RepositoryId} with {FileCount} specific files", 
                repositoryId, request?.SpecificFiles?.Length ?? 0);

            var jobId = await _repositoryAnalysisService.StartIncrementalAnalysisAsync(
                repositoryId, request?.SpecificFiles, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                jobId = jobId,
                specificFiles = request?.SpecificFiles ?? Array.Empty<string>(),
                message = request?.SpecificFiles?.Any() == true 
                    ? $"Incremental analysis started for {request.SpecificFiles.Length} specific files"
                    : "Incremental analysis started for recent changes",
                statusUrl = $"/api/repositoryanalysis/jobs/{jobId}/progress",
                estimatedDuration = "1-5 minutes for incremental updates"
            };

            _logger.LogInformation("Started incremental analysis job {JobId} for repository {RepositoryId}", jobId, repositoryId);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting incremental analysis for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to start incremental analysis"));
        }
    }

    /// <summary>
    /// Get analysis progress and status for a specific job
    /// </summary>
    /// <param name="jobId">Analysis job ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed progress information including current status and estimates</returns>
    /// <response code="200">Returns analysis progress details</response>
    /// <response code="400">Invalid job ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Analysis job not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("jobs/{jobId:int}/progress")]
    public async Task<IActionResult> GetAnalysisProgress(
        [FromRoute] int jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var progress = await _repositoryAnalysisService.GetAnalysisProgressAsync(jobId, cancellationToken);

            var response = new
            {
                jobId = progress.JobId,
                repositoryId = progress.RepositoryId,
                repositoryName = progress.RepositoryName,
                status = progress.Status,
                totalFiles = progress.TotalFiles,
                processedFiles = progress.ProcessedFiles,
                currentFile = progress.CurrentFile,
                progressPercentage = Math.Round(progress.ProgressPercentage, 1),
                startTime = progress.StartTime,
                elapsedTime = new
                {
                    totalSeconds = Math.Round(progress.ElapsedTime.TotalSeconds, 1),
                    formatted = FormatElapsedTime(progress.ElapsedTime)
                },
                estimatedCompletion = progress.EstimatedCompletion,
                errorMessage = progress.ErrorMessage,
                processingErrors = progress.ProcessingErrors?.Take(10).ToList(), // Limit errors shown
                isCompleted = progress.Status == "Completed",
                isFailed = progress.Status == "Failed",
                isCancelled = progress.Status == "Cancelled",
                isRunning = progress.Status == "Processing"
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Analysis job {JobId} not found: {Message}", jobId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for analysis job {JobId}", jobId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get analysis progress"));
        }
    }

    /// <summary>
    /// Stop a running analysis job
    /// </summary>
    /// <param name="jobId">Analysis job ID to stop</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation of job cancellation</returns>
    /// <response code="200">Returns cancellation confirmation</response>
    /// <response code="400">Invalid job ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Analysis job not found or not running</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("jobs/{jobId:int}/stop")]
    public async Task<IActionResult> StopAnalysis(
        [FromRoute] int jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Stopping analysis job {JobId}", jobId);

            var stopped = await _repositoryAnalysisService.StopAnalysisAsync(jobId, cancellationToken);

            if (stopped)
            {
                var response = new
                {
                    jobId = jobId,
                    message = "Analysis job cancelled successfully",
                    stoppedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Successfully stopped analysis job {JobId}", jobId);
                return Ok(ApiResponse<object>.SuccessResult(response));
            }
            else
            {
                return NotFound(ApiResponse<object>.ErrorResult("Analysis job not found or not running"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping analysis job {JobId}", jobId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to stop analysis job"));
        }
    }

    /// <summary>
    /// Get comprehensive analysis results for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="includeFiles">Whether to include detailed file information (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete analysis results including statistics and file details</returns>
    /// <response code="200">Returns comprehensive analysis results</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/results")]
    public async Task<IActionResult> GetAnalysisResults(
        [FromRoute] int repositoryId,
        [FromQuery] bool includeFiles = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting analysis results for repository {RepositoryId}", repositoryId);

            var result = await _repositoryAnalysisService.GetAnalysisResultAsync(repositoryId, cancellationToken);

            var response = new
            {
                repositoryId = result.RepositoryId,
                repositoryName = result.RepositoryName,
                scanStatus = result.ScanStatus,
                lastAnalysisAt = result.LastAnalysisAt,
                scanErrorMessage = result.ScanErrorMessage,
                
                // Summary statistics
                summary = new
                {
                    totalFiles = result.TotalFiles,
                    totalLines = result.TotalLines,
                    totalCodeElements = result.TotalCodeElements,
                    languageCount = result.LanguageDistribution.Count,
                    fileTypeCount = result.FileTypeDistribution.Count
                },
                
                // Distribution analysis
                distributions = new
                {
                    languages = result.LanguageDistribution.OrderByDescending(kvp => kvp.Value)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    fileTypes = result.FileTypeDistribution.OrderByDescending(kvp => kvp.Value)
                        .Take(10) // Top 10 file types
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    codeElements = result.CodeElementDistribution.OrderByDescending(kvp => kvp.Value)
                        .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
                },
                
                // Capabilities and configuration
                capabilities = new
                {
                    supportedLanguages = result.SupportedLanguages,
                    analysisFeatures = new[]
                    {
                        "Code element extraction",
                        "Language detection",
                        "File metrics calculation", 
                        "Complexity analysis",
                        "Dependency analysis"
                    }
                },
                
                // Processing information
                processing = new
                {
                    processingErrors = result.ProcessingErrors?.Take(5).ToList(),
                    hasErrors = result.ProcessingErrors?.Any() == true,
                    isAnalysisComplete = result.ScanStatus == "Completed"
                },
                
                // Conditional file details
                files = includeFiles ? result.Files.Select(f => new
                {
                    filePath = f.FilePath,
                    fileName = f.FileName,
                    fileExtension = f.FileExtension,
                    language = f.Language,
                    fileSize = f.FileSize,
                    lineCount = f.LineCount,
                    lastModified = f.LastModified,
                    codeElementCount = f.CodeElements.Count,
                    processingStatus = f.ProcessingStatus.ToString(),
                    processingTimeMs = f.ProcessingTime
                }).ToList() : null
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis results for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get analysis results"));
        }
    }

    /// <summary>
    /// Get analysis summary statistics for multiple repositories
    /// </summary>
    /// <param name="repositoryIds">List of repository IDs to get summaries for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary statistics for all specified repositories</returns>
    /// <response code="200">Returns analysis summaries</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("repositories/summaries")]
    public async Task<IActionResult> GetAnalysisSummaries(
        [FromBody] RepositorySummaryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.RepositoryIds?.Any() != true)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Repository IDs are required"));
            }

            if (request.RepositoryIds.Count > 50)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Maximum 50 repositories per request"));
            }

            _logger.LogInformation("Getting analysis summaries for {RepositoryCount} repositories", 
                request.RepositoryIds.Count);

            var summaries = new List<object>();
            var errors = new List<string>();

            foreach (var repositoryId in request.RepositoryIds)
            {
                try
                {
                    var result = await _repositoryAnalysisService.GetAnalysisResultAsync(repositoryId, cancellationToken);
                    
                    summaries.Add(new
                    {
                        repositoryId = result.RepositoryId,
                        repositoryName = result.RepositoryName,
                        scanStatus = result.ScanStatus,
                        lastAnalysisAt = result.LastAnalysisAt,
                        totalFiles = result.TotalFiles,
                        totalLines = result.TotalLines,
                        totalCodeElements = result.TotalCodeElements,
                        languageCount = result.LanguageDistribution.Count,
                        primaryLanguage = result.LanguageDistribution.OrderByDescending(kvp => kvp.Value)
                            .FirstOrDefault().Key,
                        hasErrors = result.ProcessingErrors?.Any() == true
                    });
                }
                catch (ArgumentException)
                {
                    errors.Add($"Repository {repositoryId} not found");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting summary for repository {RepositoryId}", repositoryId);
                    errors.Add($"Repository {repositoryId}: {ex.Message}");
                }
            }

            var response = new
            {
                summaries = summaries,
                totalCount = summaries.Count,
                errors = errors,
                requestedCount = request.RepositoryIds.Count
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analysis summaries");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get analysis summaries"));
        }
    }

    private static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
            return $"{elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s";
        else if (elapsed.TotalMinutes >= 1)
            return $"{elapsed.Minutes}m {elapsed.Seconds}s";
        else
            return $"{elapsed.Seconds}s";
    }
}

// Request models
public class IncrementalAnalysisRequest
{
    /// <summary>
    /// Optional list of specific files to analyze. If null or empty, will analyze recent changes.
    /// </summary>
    public string[]? SpecificFiles { get; set; }
}

public class RepositorySummaryRequest
{
    /// <summary>
    /// List of repository IDs to get summaries for (maximum 50)
    /// </summary>
    [Required]
    public List<int> RepositoryIds { get; set; } = new();
}
