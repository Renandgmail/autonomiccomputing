/**
 * Repository Controller for L2 Repository Dashboard
 * Provides API endpoints for repository-specific insights
 * Focuses on 60-second decision time for Engineering Managers
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers
{
    [ApiController]
    [Route("api/repositories")]
    public class RepositoryController : ControllerBase
    {
        private readonly RepoLensDbContext _context;
        private readonly ILogger<RepositoryController> _logger;

        public RepositoryController(RepoLensDbContext context, ILogger<RepositoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Zone 1: Get repository summary with 4 exact metrics
        /// GET /api/repositories/{id}/summary
        /// </summary>
        [HttpGet("{repositoryId}/summary")]
        public async Task<ActionResult<RepositorySummary>> GetRepositorySummary(
            [Required] int repositoryId)
        {
            try
            {
                _logger.LogInformation("Getting repository summary for repository {RepositoryId}", repositoryId);

                var repository = await _context.Repositories
                    .FirstOrDefaultAsync(r => r.Id == repositoryId);

                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                // Calculate or get cached metrics
                var summary = await CalculateRepositorySummaryAsync(repositoryId);

                _logger.LogInformation("Repository summary retrieved for repository {RepositoryId}", repositoryId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository summary for repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while getting repository summary");
            }
        }

        /// <summary>
        /// Zone 2: Get quality hotspots ranked by composite score
        /// GET /api/repositories/{id}/hotspots
        /// </summary>
        [HttpGet("{repositoryId}/hotspots")]
        public async Task<ActionResult<QualityHotspotsResponse>> GetQualityHotspots(
            [Required] int repositoryId,
            [FromQuery] int limit = 5,
            [FromQuery] HotspotSeverity? minSeverity = null,
            [FromQuery] HotspotIssueType? issueType = null)
        {
            try
            {
                _logger.LogInformation("Getting quality hotspots for repository {RepositoryId}", repositoryId);

                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                var request = new GetQualityHotspotsRequest
                {
                    RepositoryId = repositoryId,
                    Limit = Math.Min(limit, RepositoryConstants.MAX_HOTSPOTS_LIMIT),
                    MinSeverity = minSeverity,
                    IssueTypeFilter = issueType
                };

                var hotspots = await CalculateQualityHotspotsAsync(request);

                _logger.LogInformation("Quality hotspots retrieved for repository {RepositoryId}", repositoryId);
                return Ok(hotspots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality hotspots for repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while getting quality hotspots");
            }
        }

        /// <summary>
        /// Zone 3: Get repository activity feed (quality events only)
        /// GET /api/repositories/{id}/activity
        /// </summary>
        [HttpGet("{repositoryId}/activity")]
        public async Task<ActionResult<RepositoryActivityResponse>> GetRepositoryActivity(
            [Required] int repositoryId,
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? since = null)
        {
            try
            {
                _logger.LogInformation("Getting repository activity for repository {RepositoryId}", repositoryId);

                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                var request = new GetRepositoryActivityRequest
                {
                    RepositoryId = repositoryId,
                    Limit = Math.Min(limit, RepositoryConstants.MAX_ACTIVITY_LIMIT),
                    Since = since
                };

                var activity = await GenerateRepositoryActivityAsync(request);

                _logger.LogInformation("Repository activity retrieved for repository {RepositoryId}", repositoryId);
                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository activity for repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while getting repository activity");
            }
        }

        /// <summary>
        /// Zone 4: Get quick actions for repository navigation
        /// GET /api/repositories/{id}/actions
        /// </summary>
        [HttpGet("{repositoryId}/actions")]
        public async Task<ActionResult<RepositoryQuickActions>> GetQuickActions(
            [Required] int repositoryId)
        {
            try
            {
                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                var quickActions = new RepositoryQuickActions
                {
                    RepositoryId = repositoryId,
                    Actions = new List<QuickActionRoute>
                    {
                        new() { Name = "Search", Route = $"/repositories/{repositoryId}/search", Icon = "Search", Description = "Search code in this repository" },
                        new() { Name = "Analytics", Route = $"/repositories/{repositoryId}/analytics", Icon = "Analytics", Description = "View repository analytics" },
                        new() { Name = "Code Graph", Route = $"/repositories/{repositoryId}/codegraph", Icon = "AccountTree", Description = "Visualize code architecture" },
                        new() { Name = "Team", Route = $"/repositories/{repositoryId}/team", Icon = "Group", Description = "View team analytics" },
                        new() { Name = "Security", Route = $"/repositories/{repositoryId}/security", Icon = "Security", Description = "Security analysis" },
                        new() { Name = "Export", Route = $"/repositories/{repositoryId}/export", Icon = "Download", Description = "Export repository report" }
                    }
                };

                return Ok(quickActions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick actions for repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while getting quick actions");
            }
        }

        /// <summary>
        /// Get repository context for breadcrumb and navigation
        /// GET /api/repositories/{id}/context
        /// </summary>
        [HttpGet("{repositoryId}/context")]
        public async Task<ActionResult<Models.RepositoryContext>> GetRepositoryContext(
            [Required] int repositoryId)
        {
            try
            {
                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                var healthScore = await CalculateHealthScoreAsync(repositoryId);

                var context = new Models.RepositoryContext
                {
                    RepositoryId = repositoryId,
                    Name = repository.Name,
                    Url = repository.Url,
                    HealthScore = healthScore,
                    HealthTrend = new TrendIndicator { Direction = TrendDirection.Flat, Delta = "0%", Context = "vs last week" },
                    LastSync = repository.LastSyncAt ?? DateTime.UtcNow,
                    SyncStatus = GetSyncStatus(repository)
                };

                return Ok(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository context for repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while getting repository context");
            }
        }

        /// <summary>
        /// Manual refresh of repository data
        /// POST /api/repositories/{id}/refresh
        /// </summary>
        [HttpPost("{repositoryId}/refresh")]
        public async Task<ActionResult<bool>> RefreshRepository([Required] int repositoryId)
        {
            try
            {
                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return NotFound($"Repository with ID {repositoryId} not found");
                }

                // Update sync timestamp and status
                repository.LastSyncAt = DateTime.UtcNow;
                repository.Status = RepositoryStatus.Syncing;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Repository refresh initiated for repository {RepositoryId}", repositoryId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing repository {RepositoryId}", repositoryId);
                return StatusCode(500, "Internal server error while refreshing repository");
            }
        }

        #region Private Helper Methods

        private async Task<RepositorySummary> CalculateRepositorySummaryAsync(int repositoryId)
        {
            // Calculate basic metrics (mock data for now)
            var healthScore = await CalculateHealthScoreAsync(repositoryId);
            var activeContributors = await GetActiveContributorsCountAsync(repositoryId);
            var criticalIssues = await GetCriticalIssuesCountAsync(repositoryId);
            var technicalDebt = await CalculateTechnicalDebtAsync(repositoryId);

            return new RepositorySummary
            {
                HealthScore = healthScore,
                HealthTrend = new TrendIndicator
                {
                    Direction = TrendDirection.Up,
                    Delta = "2.1%",
                    Context = "vs last week",
                    PositiveDirection = TrendDirection.Up
                },
                ActiveContributors = activeContributors,
                CriticalIssues = criticalIssues,
                TechnicalDebtHours = technicalDebt,
                LastCalculated = DateTime.UtcNow
            };
        }

        private async Task<QualityHotspotsResponse> CalculateQualityHotspotsAsync(GetQualityHotspotsRequest request)
        {
            // Get repository files and calculate hotspots
            var files = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == request.RepositoryId)
                .Take(20) // Limit for demo
                .ToListAsync();

            var hotspots = new List<QualityHotspot>();

            // Generate mock hotspots based on file data
            for (int i = 0; i < Math.Min(files.Count, 5); i++)
            {
                var file = files[i];
                var severity = (HotspotSeverity)((i % 4) + 1); // Rotate through severities
                var issueType = (HotspotIssueType)(i % 6); // Rotate through issue types
                
                var hotspot = new QualityHotspot
                {
                    FileId = file.Id,
                    FilePath = file.FilePath,
                    RepositoryId = file.RepositoryId,
                    Severity = severity,
                    IssueType = issueType,
                    EstimatedFixHours = severity switch
                    {
                        HotspotSeverity.Critical => 4.0 + (i * 0.5),
                        HotspotSeverity.High => 2.0 + (i * 0.3),
                        HotspotSeverity.Medium => 1.0 + (i * 0.2),
                        _ => 0.5 + (i * 0.1)
                    },
                    HotspotScore = 1.0 - (i * 0.15), // Decreasing score
                    DetectedAt = DateTime.UtcNow.AddHours(-i)
                };

                hotspots.Add(hotspot);
            }

            // Apply filters
            if (request.MinSeverity.HasValue)
            {
                hotspots = hotspots.Where(h => h.Severity >= request.MinSeverity.Value).ToList();
            }

            if (request.IssueTypeFilter.HasValue)
            {
                hotspots = hotspots.Where(h => h.IssueType == request.IssueTypeFilter.Value).ToList();
            }

            var totalCount = hotspots.Count + 10; // Mock additional hotspots
            var limitedHotspots = hotspots.Take(request.Limit).ToList();

            return new QualityHotspotsResponse
            {
                Hotspots = limitedHotspots,
                TotalCount = totalCount
            };
        }

        private async Task<RepositoryActivityResponse> GenerateRepositoryActivityAsync(GetRepositoryActivityRequest request)
        {
            await Task.CompletedTask; // Async placeholder

            var activities = new List<RepositoryActivity>
            {
                new() { Type = RepositoryActivityType.BuildSuccess, Description = "Build #234 passed", Timestamp = DateTime.UtcNow.AddHours(-2), Severity = ActivitySeverity.Success },
                new() { Type = RepositoryActivityType.QualityGatePass, Description = "Quality gate passed", Timestamp = DateTime.UtcNow.AddHours(-4), Severity = ActivitySeverity.Success },
                new() { Type = RepositoryActivityType.SyncComplete, Description = "Auto-sync: 23 new commits analyzed", Timestamp = DateTime.UtcNow.AddHours(-6), Severity = ActivitySeverity.Info },
                new() { Type = RepositoryActivityType.NewCriticalIssue, Description = "New complexity hotspot detected in PaymentService.cs", Timestamp = DateTime.UtcNow.AddHours(-8), Severity = ActivitySeverity.Critical },
                new() { Type = RepositoryActivityType.SecurityVulnerabilityDetected, Description = "Security vulnerability flagged in auth module", Timestamp = DateTime.UtcNow.AddHours(-12), Severity = ActivitySeverity.Error }
            };

            // Apply time filter
            if (request.Since.HasValue)
            {
                activities = activities.Where(a => a.Timestamp >= request.Since.Value).ToList();
            }

            // Apply limit
            var limitedActivities = activities.Take(request.Limit).ToList();

            return new RepositoryActivityResponse
            {
                Activities = limitedActivities,
                TotalCount = activities.Count,
                LastUpdated = DateTime.UtcNow
            };
        }

        private async Task<double> CalculateHealthScoreAsync(int repositoryId)
        {
            await Task.CompletedTask; // Async placeholder
            
            // Mock calculation - replace with real algorithm
            var repository = await _context.Repositories.FindAsync(repositoryId);
            if (repository == null) return 0;

            // Mock health score based on repository age and activity
            var daysSinceCreation = (DateTime.UtcNow - repository.CreatedAt).TotalDays;
            var baseScore = Math.Max(50, 95 - (daysSinceCreation / 30)); // Decrease over time

            return Math.Round(baseScore, 1);
        }

        private async Task<int> GetActiveContributorsCountAsync(int repositoryId)
        {
            // Count contributors who committed in the last 30 days
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            
            var contributors = await _context.ContributorMetrics
                .Where(c => c.RepositoryId == repositoryId)
                .CountAsync();

            return Math.Max(1, contributors); // At least 1
        }

        private async Task<int> GetCriticalIssuesCountAsync(int repositoryId)
        {
            await Task.CompletedTask; // Async placeholder
            
            // Mock critical issues based on file count
            var fileCount = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .CountAsync();

            return Math.Max(0, fileCount / 20); // 1 critical issue per 20 files
        }

        private async Task<double> CalculateTechnicalDebtAsync(int repositoryId)
        {
            await Task.CompletedTask; // Async placeholder
            
            // Mock technical debt calculation
            var fileCount = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .CountAsync();

            return Math.Round(fileCount * 0.5, 1); // 0.5 hours per file on average
        }

        private string GetSyncStatus(Repository repository)
        {
            return repository.Status switch
            {
                RepositoryStatus.Syncing => "syncing",
                RepositoryStatus.Error => "failed",
                _ => "synced"
            };
        }

        #endregion
    }
}
