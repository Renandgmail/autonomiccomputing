using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Api.Services;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Portfolio Controller for Engineering Manager focused L1 Dashboard
/// Provides endpoints for the three dashboard zones as specified in L1_PORTFOLIO_DASHBOARD.md
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(
        IPortfolioService portfolioService,
        ILogger<PortfolioController> logger)
    {
        _portfolioService = portfolioService;
        _logger = logger;
    }

    /// <summary>
    /// Get Zone 1 summary metrics for Engineering Manager portfolio overview
    /// Returns exactly 4 metrics: repositories, avg health, critical issues, teams
    /// </summary>
    /// <returns>Portfolio summary with 4 key metrics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(PortfolioSummary), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PortfolioSummary>> GetPortfolioSummary(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting portfolio summary for Engineering Manager dashboard");
            
            var summary = await _portfolioService.GetPortfolioSummaryAsync(cancellationToken);
            
            _logger.LogInformation("Portfolio summary calculated successfully: {TotalRepos} repositories, {AvgHealth}% avg health, {CriticalIssues} critical issues", 
                summary.TotalRepositories, summary.AverageHealthScore, summary.CriticalIssuesCount);
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio summary");
            return StatusCode(500, new { error = "Failed to get portfolio summary", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Zone 2 repository list with filtering and sorting for Engineering Manager decision making
    /// Default sort: starred repositories first, then health score ascending (worst first)
    /// </summary>
    /// <param name="healthBands">Filter by health bands (can select multiple)</param>
    /// <param name="languages">Filter by programming languages (can select multiple)</param>
    /// <param name="teams">Filter by teams (can select multiple)</param>
    /// <param name="hasCriticalIssuesOnly">Show only repositories with critical issues</param>
    /// <param name="starredOnly">Show only starred repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered and sorted repository list for decision making</returns>
    [HttpGet("repositories")]
    [ProducesResponseType(typeof(PortfolioRepositoryListResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PortfolioRepositoryListResponse>> GetRepositoryList(
        [FromQuery] List<RepositoryHealthBand>? healthBands = null,
        [FromQuery] List<string>? languages = null,
        [FromQuery] List<string>? teams = null,
        [FromQuery] bool hasCriticalIssuesOnly = false,
        [FromQuery] bool starredOnly = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting repository list with filters - HealthBands: {HealthBands}, Languages: {Languages}, Teams: {Teams}, CriticalOnly: {CriticalOnly}, StarredOnly: {StarredOnly}",
                healthBands, languages, teams, hasCriticalIssuesOnly, starredOnly);

            var filters = new PortfolioFilters
            {
                HealthBands = healthBands ?? new List<RepositoryHealthBand>(),
                Languages = languages ?? new List<string>(),
                Teams = teams ?? new List<string>(),
                HasCriticalIssuesOnly = hasCriticalIssuesOnly,
                StarredOnly = starredOnly
            };

            // TODO: Get actual user ID from authentication context
            var userId = "current-user"; // Placeholder

            var repositoryList = await _portfolioService.GetRepositoryListAsync(filters, userId, cancellationToken);
            
            _logger.LogInformation("Repository list retrieved successfully: {TotalCount} total, {FilteredCount} after filtering", 
                repositoryList.TotalCount, repositoryList.FilteredCount);
            
            return Ok(repositoryList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository list");
            return StatusCode(500, new { error = "Failed to get repository list", details = ex.Message });
        }
    }

    /// <summary>
    /// Get Zone 3 critical issues requiring immediate Engineering Manager attention
    /// Conditional display: only shown when >= 1 repository has critical issues
    /// Maximum 5 items returned before "See all X critical issues" link
    /// </summary>
    /// <returns>Top 5 critical issues across portfolio</returns>
    [HttpGet("critical-issues")]
    [ProducesResponseType(typeof(List<CriticalIssue>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<CriticalIssue>>> GetCriticalIssues(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting critical issues for Engineering Manager attention");
            
            var criticalIssues = await _portfolioService.GetCriticalIssuesAsync(cancellationToken);
            
            _logger.LogInformation("Critical issues retrieved successfully: {Count} issues found", criticalIssues.Count);
            
            return Ok(criticalIssues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical issues");
            return StatusCode(500, new { error = "Failed to get critical issues", details = ex.Message });
        }
    }

    /// <summary>
    /// Toggle repository star status for current user
    /// Starred repositories appear at the top of the repository list
    /// </summary>
    /// <param name="repositoryId">Repository ID to star/unstar</param>
    /// <param name="request">Star request with new status</param>
    /// <returns>Success status of star toggle</returns>
    [HttpPost("repositories/{repositoryId}/star")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> ToggleRepositoryStar(
        int repositoryId, 
        [FromBody] StarRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (repositoryId != request.RepositoryId)
            {
                _logger.LogWarning("Repository ID mismatch: URL {UrlId} vs Body {BodyId}", repositoryId, request.RepositoryId);
                return BadRequest(new { error = "Repository ID mismatch" });
            }

            _logger.LogInformation("Toggling star for repository {RepositoryId} to {IsStarred}", repositoryId, request.IsStarred);

            // TODO: Get actual user ID from authentication context
            var userId = "current-user"; // Placeholder

            var success = await _portfolioService.ToggleRepositoryStarAsync(repositoryId, userId, cancellationToken);
            
            if (success)
            {
                _logger.LogInformation("Repository {RepositoryId} star status updated successfully", repositoryId);
                return Ok(true);
            }
            
            _logger.LogWarning("Failed to update star status for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "Failed to update star status" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid repository ID: {RepositoryId}", repositoryId);
            return NotFound(new { error = "Repository not found", repositoryId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling repository star for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "Failed to toggle repository star", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete repository star (alternative endpoint)
    /// </summary>
    [HttpDelete("repositories/{repositoryId}/star")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> UnstarRepository(int repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unstarring repository {RepositoryId}", repositoryId);

            // TODO: Get actual user ID from authentication context
            var userId = "current-user"; // Placeholder

            var success = await _portfolioService.ToggleRepositoryStarAsync(repositoryId, userId, cancellationToken);
            
            if (success)
            {
                _logger.LogInformation("Repository {RepositoryId} unstarred successfully", repositoryId);
                return Ok(true);
            }
            
            return StatusCode(500, new { error = "Failed to unstar repository" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid repository ID: {RepositoryId}", repositoryId);
            return NotFound(new { error = "Repository not found", repositoryId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unstarring repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "Failed to unstar repository", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available filter options for the repository list
    /// Used to populate filter dropdowns in the UI
    /// </summary>
    /// <returns>Available languages, teams, and health band counts</returns>
    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(PortfolioFilterOptions), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PortfolioFilterOptions>> GetFilterOptions(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting portfolio filter options");
            
            var filterOptions = await _portfolioService.GetFilterOptionsAsync(cancellationToken);
            
            _logger.LogInformation("Filter options retrieved successfully: {Languages} languages, {Teams} teams", 
                filterOptions.Languages.Count, filterOptions.Teams.Count);
            
            return Ok(filterOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter options");
            return StatusCode(500, new { error = "Failed to get filter options", details = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint for portfolio service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), 200)]
    public ActionResult GetHealth()
    {
        return Ok(new 
        { 
            service = "Portfolio",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
