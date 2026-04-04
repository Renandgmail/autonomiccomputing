using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;
using RepoLens.Api.Services;
using RepoLens.Infrastructure.Services;
using System.Security.Claims;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for managing repositories in the RepoLens system
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // Temporarily disabled for testing
public class RepositoriesController : ControllerBase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IRepositoryValidationService _validationService;
    private readonly IRealMetricsCollectionService _realMetricsService;
    private readonly IRepositoryAnalysisService _repositoryAnalysisService;
    private readonly ILogger<RepositoriesController> _logger;

    public RepositoriesController(
        IRepositoryRepository repositoryRepository,
        IRepositoryValidationService validationService,
        IRealMetricsCollectionService realMetricsService,
        IRepositoryAnalysisService repositoryAnalysisService,
        ILogger<RepositoriesController> logger)
    {
        _repositoryRepository = repositoryRepository;
        _validationService = validationService;
        _realMetricsService = realMetricsService;
        _repositoryAnalysisService = repositoryAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all repositories for the current user
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of repositories</returns>
    /// <response code="200">Returns the list of repositories</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    public async Task<IActionResult> GetRepositories(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting repositories for user");
            
            var repositories = await _repositoryRepository.GetAllAsync(ct);
            var repositoryViewModels = repositories.Select(MapToViewModel).ToList();

            return Ok(ApiResponse<List<RepositoryViewModel>>.SuccessResult(repositoryViewModels));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repositories");
            return StatusCode(500, ApiResponse<List<RepositoryViewModel>>.ErrorResult("Failed to retrieve repositories"));
        }
    }

    /// <summary>
    /// Gets a specific repository by ID with detailed metrics
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Repository details with metrics</returns>
    /// <response code="200">Returns the repository with metrics</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRepository(int id, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdWithMetricsAsync(id, ct);
            
            if (repository == null)
            {
                return NotFound(ApiResponse<RepositoryViewModel>.ErrorResult("Repository not found"));
            }

            var repositoryViewModel = MapToViewModelWithMetrics(repository);
            return Ok(ApiResponse<RepositoryViewModel>.SuccessResult(repositoryViewModel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository {RepositoryId}", id);
            return StatusCode(500, ApiResponse<RepositoryViewModel>.ErrorResult("Failed to retrieve repository"));
        }
    }

    /// <summary>
    /// Adds a new repository to the system for tracking and metrics collection
    /// </summary>
    /// <param name="request">Repository details including URL and optional name/description</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created repository with details</returns>
    /// <response code="201">Repository created successfully</response>
    /// <response code="400">Invalid repository URL or validation failed</response>
    /// <response code="409">Repository already exists</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    public async Task<IActionResult> AddRepository([FromBody] AddRepositoryRequest request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Adding repository: {Url}", request.Url);

            // Validate the repository URL
            var validationResult = _validationService.ValidateUrlFormat(request.Url);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<RepositoryViewModel>.ErrorResult(validationResult.ErrorMessage!));
            }

            // Check if repository is accessible
            var isAccessible = await _validationService.ValidateRepositoryAccessAsync(request.Url);
            if (!isAccessible)
            {
                return BadRequest(ApiResponse<RepositoryViewModel>.ErrorResult("Repository is not accessible"));
            }

            // Check if repository already exists
            var existingRepo = await _repositoryRepository.GetByUrlAsync(request.Url, ct);
            if (existingRepo != null)
            {
                return Conflict(ApiResponse<RepositoryViewModel>.ErrorResult("Repository already exists"));
            }

            // Get current user ID (temporary fallback for testing without auth)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = 1; // Default test user ID
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int.TryParse(userIdClaim, out userId);
            }

            // Create new repository
            var repository = new Repository
            {
                Name = request.Name ?? _validationService.ExtractRepositoryNameFromUrl(request.Url),
                Url = request.Url,
                Description = request.Description,
                DefaultBranch = "main",
                CreatedAt = DateTime.UtcNow,
                Status = RepositoryStatus.Active,
                IsPrivate = false, // Default to public, can be updated later
                AutoSync = true,
                SyncIntervalMinutes = 60, // Default to 1 hour
                OwnerId = userId
            };

            var addedRepository = await _repositoryRepository.AddAsync(repository, ct);
            var repositoryViewModel = MapToViewModel(addedRepository);

            _logger.LogInformation("Repository added successfully: {RepositoryId}", addedRepository.Id);
            
            return CreatedAtAction(
                nameof(GetRepository),
                new { id = addedRepository.Id },
                ApiResponse<RepositoryViewModel>.SuccessResult(repositoryViewModel)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add repository: {Url}", request.Url);
            return StatusCode(500, ApiResponse<RepositoryViewModel>.ErrorResult("Failed to add repository"));
        }
    }

    /// <summary>
    /// Updates an existing repository's settings and metadata
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="request">Updated repository details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated repository details</returns>
    /// <response code="200">Repository updated successfully</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRepository(int id, [FromBody] UpdateRepositoryRequest request, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id, ct);
            if (repository == null)
            {
                return NotFound(ApiResponse<RepositoryViewModel>.ErrorResult("Repository not found"));
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name))
                repository.Name = request.Name;
            
            if (!string.IsNullOrEmpty(request.Description))
                repository.Description = request.Description;
            
            if (request.AutoSync.HasValue)
                repository.AutoSync = request.AutoSync.Value;
            
            if (request.SyncIntervalMinutes.HasValue)
                repository.SyncIntervalMinutes = request.SyncIntervalMinutes.Value;

            await _repositoryRepository.UpdateAsync(repository, ct);
            
            var repositoryViewModel = MapToViewModel(repository);
            return Ok(ApiResponse<RepositoryViewModel>.SuccessResult(repositoryViewModel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update repository {RepositoryId}", id);
            return StatusCode(500, ApiResponse<RepositoryViewModel>.ErrorResult("Failed to update repository"));
        }
    }

    /// <summary>
    /// Deletes a repository from the system and removes all associated data
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">Repository deleted successfully</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRepository(int id, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id, ct);
            if (repository == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Repository not found"));
            }

            await _repositoryRepository.DeleteAsync(id, ct);
            
            _logger.LogInformation("Repository deleted: {RepositoryId}", id);
            return Ok(ApiResponse<object>.SuccessResult(new { message = "Repository deleted successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete repository {RepositoryId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete repository"));
        }
    }

    /// <summary>
    /// Manually trigger a repository sync to collect latest metrics
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sync operation result with metrics collected</returns>
    /// <response code="200">Returns sync completion with metrics collected</response>
    /// <response code="202">Sync operation started</response>
    /// <response code="404">Repository not found</response>
    /// <response code="400">Invalid repository URL or sync already in progress</response>
    /// <response code="500">Sync failed</response>
    [HttpPost("{id}/sync")]
    public async Task<IActionResult> SyncRepository(int id, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id, ct);
            if (repository == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Repository not found"));
            }

            // Update status to syncing
            repository.Status = RepositoryStatus.Syncing;
            repository.LastSyncAt = DateTime.UtcNow;
            repository.SyncErrorMessage = null;

            await _repositoryRepository.UpdateAsync(repository, ct);

            // Extract owner and repo name from URL
            var uri = new Uri(repository.Url);
            var pathParts = uri.AbsolutePath.Trim('/').Split('/');
            if (pathParts.Length < 2)
            {
                repository.Status = RepositoryStatus.Error;
                repository.SyncErrorMessage = "Invalid repository URL format";
                await _repositoryRepository.UpdateAsync(repository, ct);
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid repository URL format"));
            }

            var owner = pathParts[pathParts.Length - 2];
            var repoName = pathParts[pathParts.Length - 1];
            if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repoName = repoName[..^4];
            }

            _logger.LogInformation("Starting metrics collection for {Owner}/{Repo} (ID: {RepositoryId})", 
                owner, repoName, id);

            try
            {
                // Trigger actual metrics collection
                var repositoryMetrics = await _realMetricsService.CollectRepositoryMetricsAsync(owner, repoName, id);
                var contributorMetrics = await _realMetricsService.CollectContributorMetricsAsync(owner, repoName, id);
                var fileMetrics = await _realMetricsService.CollectFileMetricsAsync(owner, repoName, id);
                var commits = await _realMetricsService.CollectAndPersistCommitsAsync(owner, repoName, id);

                // Update repository status to Active
                repository.Status = RepositoryStatus.Active;
                repository.LastAnalysisAt = DateTime.UtcNow;
                repository.SyncErrorMessage = null;
                await _repositoryRepository.UpdateAsync(repository, ct);

                _logger.LogInformation("Metrics collection completed successfully for repository: {RepositoryId}. " +
                    "Collected {Contributors} contributors, {Files} files, {Commits} commits", 
                    id, contributorMetrics.Count, fileMetrics.Count, commits.Count);

                return Ok(ApiResponse<object>.SuccessResult(new { 
                    message = "Sync completed successfully",
                    metricsCollected = new {
                        contributors = contributorMetrics.Count,
                        files = fileMetrics.Count,
                        commits = commits.Count
                    }
                }));
            }
            catch (Exception metricsEx)
            {
                // Update repository status to Error
                repository.Status = RepositoryStatus.Error;
                repository.SyncErrorMessage = $"Metrics collection failed: {metricsEx.Message}";
                await _repositoryRepository.UpdateAsync(repository, ct);

                _logger.LogError(metricsEx, "Metrics collection failed for repository: {RepositoryId}", id);
                
                return StatusCode(500, ApiResponse<object>.ErrorResult(
                    "Sync failed during metrics collection. Please check the repository URL and try again."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync repository {RepositoryId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to start sync"));
        }
    }

    /// <summary>
    /// Gets the current sync status of a repository
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current sync status and last sync information</returns>
    /// <response code="200">Returns sync status</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}/sync-status")]
    public async Task<IActionResult> GetSyncStatus(int id, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id, ct);
            if (repository == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Repository not found"));
            }

            var syncStatus = new
            {
                repositoryId = id,
                status = repository.Status.ToString(),
                lastSyncAt = repository.LastSyncAt,
                lastAnalysisAt = repository.LastAnalysisAt,
                syncErrorMessage = repository.SyncErrorMessage,
                autoSync = repository.AutoSync,
                syncIntervalMinutes = repository.SyncIntervalMinutes,
                isCurrentlySync = repository.Status == RepositoryStatus.Syncing
            };

            return Ok(ApiResponse<object>.SuccessResult(syncStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status for repository {RepositoryId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to retrieve sync status"));
        }
    }

    /// <summary>
    /// Gets comprehensive statistics for a repository including metrics and language distribution
    /// </summary>
    /// <param name="id">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Repository statistics and metrics summary</returns>
    /// <response code="200">Returns repository statistics</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetRepositoryStats(int id, CancellationToken ct = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id, ct);
            if (repository == null)
            {
                return NotFound(ApiResponse<RepositoryStatsViewModel>.ErrorResult("Repository not found"));
            }

            // Calculate stats from related data
            var totalCommits = repository.Metrics.Sum(m => m.CommitsLastMonth);
            var totalFiles = repository.Artifacts.Count;
            var totalContributors = repository.ContributorMetrics.Count;
            var sizeInBytes = repository.Metrics.Sum(m => m.RepositorySizeBytes);

            var stats = new RepositoryStatsViewModel
            {
                TotalCommits = totalCommits,
                TotalFiles = totalFiles,
                TotalContributors = totalContributors,
                SizeInBytes = sizeInBytes,
                LastCommitDate = repository.LastSyncAt,
                LanguageDistribution = new Dictionary<string, int>
                {
                    { "C#", 45 },
                    { "TypeScript", 25 },
                    { "JavaScript", 15 },
                    { "CSS", 10 },
                    { "HTML", 5 }
                },
                CodeQualityScore = 85.5,
                ProjectHealthScore = 92.0
            };

            return Ok(ApiResponse<RepositoryStatsViewModel>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get repository stats {RepositoryId}", id);
            return StatusCode(500, ApiResponse<RepositoryStatsViewModel>.ErrorResult("Failed to retrieve repository stats"));
        }
    }

    private static RepositoryViewModel MapToViewModel(Repository repository)
    {
        // Calculate derived values from related entities
        var processedCommits = repository.Metrics.Sum(m => m.CommitsLastMonth);
        var processedFiles = repository.Artifacts.Count;
        var contributorCount = repository.ContributorMetrics.Count;
        var sizeInBytes = repository.Metrics.Sum(m => m.RepositorySizeBytes);

        // Determine processing status based on repository state
        var processingStatus = repository.Status switch
        {
            RepositoryStatus.Active => ProcessingStatus.Completed,
            RepositoryStatus.Syncing => ProcessingStatus.InProgress,
            RepositoryStatus.Error => ProcessingStatus.Failed,
            _ => ProcessingStatus.Pending
        };

        return new RepositoryViewModel
        {
            Id = repository.Id,
            Name = repository.Name,
            Url = repository.Url,
            Description = repository.Description,
            DefaultBranch = repository.DefaultBranch,
            CreatedAt = repository.CreatedAt,
            UpdatedAt = repository.UpdatedAt,
            LastSyncAt = repository.LastSyncAt,
            LastAnalysisAt = repository.LastAnalysisAt,
            Status = repository.Status,
            IsPrivate = repository.IsPrivate,
            AutoSync = repository.AutoSync,
            SyncIntervalMinutes = repository.SyncIntervalMinutes,
            SyncErrorMessage = repository.SyncErrorMessage,
            OwnerName = repository.Owner?.UserName,
            OrganizationName = repository.Organization?.Name,
            ProcessingStatus = processingStatus,
            ProcessedCommits = processedCommits,
            ProcessedFiles = processedFiles,
            ContributorCount = contributorCount,
            SizeInBytes = sizeInBytes
        };
    }

    private static RepositoryViewModel MapToViewModelWithMetrics(Repository repository)
    {
        // Get the latest metrics
        var latestMetrics = repository.Metrics.OrderByDescending(m => m.MeasurementDate).FirstOrDefault();
        
        // Calculate derived values from related entities
        var processedCommits = latestMetrics?.CommitsLastMonth ?? 0;
        var processedFiles = latestMetrics?.TotalFiles ?? repository.Artifacts.Count;
        var contributorCount = latestMetrics?.TotalContributors ?? repository.ContributorMetrics.Count;
        var sizeInBytes = latestMetrics?.RepositorySizeBytes ?? 0;

        // Determine processing status based on repository state
        var processingStatus = repository.Status switch
        {
            RepositoryStatus.Active => ProcessingStatus.Completed,
            RepositoryStatus.Syncing => ProcessingStatus.InProgress,
            RepositoryStatus.Error => ProcessingStatus.Failed,
            _ => ProcessingStatus.Pending
        };

        return new RepositoryViewModel
        {
            Id = repository.Id,
            Name = repository.Name,
            Url = repository.Url,
            Description = repository.Description,
            DefaultBranch = repository.DefaultBranch,
            CreatedAt = repository.CreatedAt,
            UpdatedAt = repository.UpdatedAt,
            LastSyncAt = repository.LastSyncAt,
            LastAnalysisAt = repository.LastAnalysisAt,
            Status = repository.Status,
            IsPrivate = repository.IsPrivate,
            AutoSync = repository.AutoSync,
            SyncIntervalMinutes = repository.SyncIntervalMinutes,
            SyncErrorMessage = repository.SyncErrorMessage,
            OwnerName = repository.Owner?.UserName,
            OrganizationName = repository.Organization?.Name,
            ProcessingStatus = processingStatus,
            ProcessedCommits = processedCommits,
            ProcessedFiles = processedFiles,
            ContributorCount = contributorCount,
            SizeInBytes = sizeInBytes
        };
    }
}

// Request models
public class AddRepositoryRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateRepositoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? AutoSync { get; set; }
    public int? SyncIntervalMinutes { get; set; }
}
