/**
 * Repository Service for L2 Repository Dashboard
 * Implements business logic for repository-specific insights
 * Focuses on 60-second decision time target for Engineering Managers
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;

namespace RepoLens.Api.Services
{
    public interface IRepositoryService
    {
        Task<RepositorySummary> GetRepositorySummaryAsync(int repositoryId);
        Task<QualityHotspotsResponse> GetQualityHotspotsAsync(GetQualityHotspotsRequest request);
        Task<RepositoryActivityResponse> GetRepositoryActivityAsync(GetRepositoryActivityRequest request);
        Task<RepositoryQuickActions> GetQuickActionsAsync(int repositoryId);
        Task<Models.RepositoryContext> GetRepositoryContextAsync(int repositoryId);
        Task<bool> RefreshRepositoryDataAsync(int repositoryId);
    }

    public class RepositoryService : IRepositoryService
    {
        private readonly RepoLensDbContext _context;
        private readonly ILogger<RepositoryService> _logger;

        public RepositoryService(RepoLensDbContext context, ILogger<RepositoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Zone 1: Get repository summary with 4 exact metrics
        /// Health Score | Active Contributors | Critical Issues | Technical Debt Hours
        /// </summary>
        public async Task<RepositorySummary> GetRepositorySummaryAsync(int repositoryId)
        {
            try
            {
                _logger.LogInformation("Getting repository summary for repository {RepositoryId}", repositoryId);

                var repository = await _context.Repositories
                    .Include(r => r.Metrics)
                    .Include(r => r.ContributorMetrics)
                    .FirstOrDefaultAsync(r => r.Id == repositoryId);

                if (repository == null)
                {
                    throw new ArgumentException($"Repository with ID {repositoryId} not found");
                }

                // Calculate health score (if not already calculated)
                var healthScore = await CalculateHealthScoreAsync(repositoryId);
                var healthTrend = await CalculateHealthTrendAsync(repositoryId);
                
                // Active contributors in last 30 days
                var activeContributors = await GetActiveContributorsCountAsync(repositoryId);
                
                // Critical issues count
                var criticalIssues = await GetCriticalIssuesCountAsync(repositoryId);
                
                // Technical debt in hours
                var technicalDebtHours = await CalculateTechnicalDebtAsync(repositoryId);

                var summary = new RepositorySummary
                {
                    HealthScore = healthScore,
                    HealthTrend = healthTrend,
                    ActiveContributors = activeContributors,
                    CriticalIssues = criticalIssues,
                    TechnicalDebtHours = technicalDebtHours,
                    LastCalculated = DateTime.UtcNow
                };

                _logger.LogInformation("Repository summary calculated: Health={Health}%, Contributors={Contributors}, Issues={Issues}, Debt={Debt}h", 
                    healthScore, activeContributors, criticalIssues, technicalDebtHours);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository summary for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        /// <summary>
        /// Zone 2: Get quality hotspots ranked by composite score (complexity × churn × quality deficit)
        /// Most important panel - shows worst files first
        /// </summary>
        public async Task<QualityHotspotsResponse> GetQualityHotspotsAsync(GetQualityHotspotsRequest request)
        {
            try
            {
                _logger.LogInformation("Getting quality hotspots for repository {RepositoryId}", request.RepositoryId);

                var query = _context.RepositoryFiles
                    .Where(f => f.RepositoryId == request.RepositoryId)
                    .Where(f => !string.IsNullOrEmpty(f.FilePath))
                    .AsQueryable();

                // Calculate hotspots based on complexity, churn, and quality
                var files = await query.ToListAsync();
                var hotspots = new List<QualityHotspot>();

                foreach (var file in files)
                {
                    var hotspot = await CalculateFileHotspotAsync(file);
                    if (hotspot.HotspotScore > 0.3) // Only include files above threshold
                    {
                        hotspots.Add(hotspot);
                    }
                }

                // Sort by hotspot score (highest first - worst files first)
                hotspots = hotspots.OrderByDescending(h => h.HotspotScore).ToList();

                // Apply filters
                if (request.MinSeverity.HasValue)
                {
                    hotspots = hotspots.Where(h => h.Severity >= request.MinSeverity.Value).ToList();
                }

                if (request.IssueTypeFilter.HasValue)
                {
                    hotspots = hotspots.Where(h => h.IssueType == request.IssueTypeFilter.Value).ToList();
                }

                var totalCount = hotspots.Count;
                var limitedHotspots = hotspots.Take(request.Limit).ToList();

                var response = new QualityHotspotsResponse
                {
                    Hotspots = limitedHotspots,
                    TotalCount = totalCount
                };

                _logger.LogInformation("Found {TotalCount} hotspots, returning top {ShownCount} for repository {RepositoryId}", 
                    totalCount, limitedHotspots.Count, request.RepositoryId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality hotspots for repository {RepositoryId}", request.RepositoryId);
                throw;
            }
        }

        /// <summary>
        /// Zone 3: Get repository activity feed (quality events only, NOT commits)
        /// Shows quality gate failures, new issues, security flags, etc.
        /// </summary>
        public async Task<RepositoryActivityResponse> GetRepositoryActivityAsync(GetRepositoryActivityRequest request)
        {
            try
            {
                _logger.LogInformation("Getting repository activity for repository {RepositoryId}", request.RepositoryId);

                var activities = new List<RepositoryActivity>();
                var since = request.Since ?? DateTime.UtcNow.AddDays(-RepositoryConstants.ACTIVITY_DAYS_BACK);

                // Generate quality events (mock data for now - replace with real events)
                activities.AddRange(await GenerateQualityEventsAsync(request.RepositoryId, since));

                // Filter by type if specified
                if (request.TypeFilters?.Any() == true)
                {
                    activities = activities.Where(a => request.TypeFilters.Contains(a.Type)).ToList();
                }

                // Sort by timestamp descending (newest first)
                activities = activities.OrderByDescending(a => a.Timestamp).ToList();

                // Apply limit
                var limitedActivities = activities.Take(request.Limit).ToList();

                var response = new RepositoryActivityResponse
                {
                    Activities = limitedActivities,
                    TotalCount = activities.Count,
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogInformation("Found {TotalCount} activity events for repository {RepositoryId}", 
                    activities.Count, request.RepositoryId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository activity for repository {RepositoryId}", request.RepositoryId);
                throw;
            }
        }

        /// <summary>
        /// Zone 4: Get quick actions for repository navigation
        /// </summary>
        public async Task<RepositoryQuickActions> GetQuickActionsAsync(int repositoryId)
        {
            try
            {
                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    throw new ArgumentException($"Repository with ID {repositoryId} not found");
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

                return quickActions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick actions for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        /// <summary>
        /// Get repository context for breadcrumb and navigation
        /// </summary>
        public async Task<Models.RepositoryContext> GetRepositoryContextAsync(int repositoryId)
        {
            try
            {
                var repository = await _context.Repositories
                    .FirstOrDefaultAsync(r => r.Id == repositoryId);

                if (repository == null)
                {
                    throw new ArgumentException($"Repository with ID {repositoryId} not found");
                }

                var healthScore = await CalculateHealthScoreAsync(repositoryId);
                var healthTrend = await CalculateHealthTrendAsync(repositoryId);

                var context = new Models.RepositoryContext
                {
                    RepositoryId = repositoryId,
                    Name = repository.Name,
                    Url = repository.Url,
                    HealthScore = healthScore,
                    HealthTrend = healthTrend,
                    LastSync = repository.LastSyncAt ?? DateTime.UtcNow,
                    SyncStatus = GetSyncStatus(repository)
                };

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository context for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        /// <summary>
        /// Manual refresh of repository data
        /// </summary>
        public async Task<bool> RefreshRepositoryDataAsync(int repositoryId)
        {
            try
            {
                _logger.LogInformation("Refreshing repository data for repository {RepositoryId}", repositoryId);

                var repository = await _context.Repositories.FindAsync(repositoryId);
                if (repository == null)
                {
                    return false;
                }

                // Trigger background refresh (this would typically queue a job)
                // For now, just update the LastSyncAt timestamp
                repository.LastSyncAt = DateTime.UtcNow;
                repository.Status = RepositoryStatus.Syncing;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Repository refresh initiated for repository {RepositoryId}", repositoryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing repository data for repository {RepositoryId}", repositoryId);
                return false;
            }
        }

        #region Private Helper Methods

        private async Task<double> CalculateHealthScoreAsync(int repositoryId)
        {
            // Use existing repository metrics or calculate on demand
            var metrics = await _context.RepositoryMetrics
                .Where(m => m.RepositoryId == repositoryId)
                .OrderByDescending(m => m.CalculatedAt)
                .FirstOrDefaultAsync();

            if (metrics != null && metrics.CalculatedAt > DateTime.UtcNow.AddHours(-24))
            {
                return metrics.OverallScore;
            }

            // Calculate health score based on multiple factors
            var complexityScore = await CalculateComplexityScoreAsync(repositoryId);
            var testCoverageScore = await CalculateTestCoverageScoreAsync(repositoryId);
            var securityScore = await CalculateSecurityScoreAsync(repositoryId);
            var maintenanceScore = await CalculateMaintenanceScoreAsync(repositoryId);

            // Weighted average
            var healthScore = (complexityScore * 0.25) + (testCoverageScore * 0.25) + 
                             (securityScore * 0.3) + (maintenanceScore * 0.2);

            return Math.Round(healthScore, 1);
        }

        private async Task<TrendIndicator> CalculateHealthTrendAsync(int repositoryId)
        {
            var recentMetrics = await _context.RepositoryMetrics
                .Where(m => m.RepositoryId == repositoryId)
                .OrderByDescending(m => m.CalculatedAt)
                .Take(2)
                .ToListAsync();

            if (recentMetrics.Count < 2)
            {
            return new TrendIndicator { Direction = TrendDirection.Flat, Delta = "0%", Context = "insufficient data" };
            }

            var current = recentMetrics[0].OverallScore;
            var previous = recentMetrics[1].OverallScore;
            var change = current - previous;

            return new TrendIndicator
            {
                Direction = change > 1 ? TrendDirection.Up : change < -1 ? TrendDirection.Down : TrendDirection.Flat,
                Delta = $"{Math.Abs(change):F1}%",
                Context = "vs last week",
                PositiveDirection = TrendDirection.Up
            };
        }

        private async Task<int> GetActiveContributorsCountAsync(int repositoryId)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-RepositoryConstants.CONTRIBUTOR_DAYS_BACK);
            
            var activeContributors = await _context.ContributorMetrics
                .Where(c => c.RepositoryId == repositoryId)
                .Where(c => c.LastCommitAt >= cutoffDate)
                .CountAsync();

            return activeContributors;
        }

        private async Task<int> GetCriticalIssuesCountAsync(int repositoryId)
        {
            // Count files with critical complexity, security issues, or test coverage problems
            var criticalFiles = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .Where(f => f.CyclomaticComplexity > 20 || // High complexity
                           f.TestCoveragePercentage < 30) // Low test coverage
                .CountAsync();

            return criticalFiles;
        }

        private async Task<double> CalculateTechnicalDebtAsync(int repositoryId)
        {
            // Calculate technical debt based on file complexity and quality issues
            var files = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .Where(f => f.CyclomaticComplexity.HasValue)
                .ToListAsync();

            double totalDebtHours = 0;

            foreach (var file in files)
            {
                // Estimate time to fix based on complexity and issues
                var complexity = file.CyclomaticComplexity ?? 0;
                var testCoverage = file.TestCoveragePercentage ?? 100;

                if (complexity > 10 || testCoverage < 50)
                {
                    // SQALE method estimation
                    totalDebtHours += (complexity * 0.1) + ((100 - testCoverage) * 0.05);
                }
            }

            return Math.Round(totalDebtHours, 1);
        }

        private async Task<QualityHotspot> CalculateFileHotspotAsync(RepositoryFile file)
        {
            // Calculate hotspot score: complexity × churn × (1 - quality)
            var complexity = Math.Min((file.CyclomaticComplexity ?? 1) / 20.0, 1.0);
            var churn = await CalculateFileChurnAsync(file);
            var quality = Math.Max(0, (file.TestCoveragePercentage ?? 0) / 100.0);
            
            var hotspotScore = complexity * churn * (1.0 - quality);

            var severity = hotspotScore switch
            {
                >= RepositoryConstants.CRITICAL_SEVERITY_THRESHOLD => HotspotSeverity.Critical,
                >= RepositoryConstants.HIGH_SEVERITY_THRESHOLD => HotspotSeverity.High,
                >= RepositoryConstants.MEDIUM_SEVERITY_THRESHOLD => HotspotSeverity.Medium,
                _ => HotspotSeverity.Low
            };

            var issueType = DetermineIssueType(file);
            var estimatedHours = CalculateEstimatedFixTime(file, severity);

            return new QualityHotspot
            {
                FileId = file.Id,
                FilePath = file.FilePath,
                RepositoryId = file.RepositoryId,
                Severity = severity,
                IssueType = issueType,
                EstimatedFixHours = estimatedHours,
                HotspotScore = hotspotScore,
                DetectedAt = DateTime.UtcNow
            };
        }

        private async Task<double> CalculateFileChurnAsync(RepositoryFile file)
        {
            // Calculate churn rate based on how frequently the file changes
            // This would typically look at git history
            // For now, return a mock value
            return 0.5; // Placeholder
        }

        private HotspotIssueType DetermineIssueType(RepositoryFile file)
        {
            var complexity = file.CyclomaticComplexity ?? 0;
            var testCoverage = file.TestCoveragePercentage ?? 100;

            if (complexity > 15) return HotspotIssueType.Complexity;
            if (testCoverage < 30) return HotspotIssueType.TestCoverage;
            if (file.FilePath.Contains("security", StringComparison.OrdinalIgnoreCase)) return HotspotIssueType.Security;
            
            return HotspotIssueType.TechnicalDebt;
        }

        private double CalculateEstimatedFixTime(RepositoryFile file, HotspotSeverity severity)
        {
            var baseTime = severity switch
            {
                HotspotSeverity.Critical => 4.0,
                HotspotSeverity.High => 2.0,
                HotspotSeverity.Medium => 1.0,
                _ => 0.5
            };

            var complexity = file.CyclomaticComplexity ?? 1;
            return baseTime + (complexity * 0.1);
        }

        private async Task<List<RepositoryActivity>> GenerateQualityEventsAsync(int repositoryId, DateTime since)
        {
            var activities = new List<RepositoryActivity>();

            // Mock quality events - replace with real event sourcing
            var events = new[]
            {
                new { Type = RepositoryActivityType.BuildSuccess, Desc = "Build #234 passed", Hours = 2 },
                new { Type = RepositoryActivityType.QualityGatePass, Desc = "Quality gate passed", Hours = 4 },
                new { Type = RepositoryActivityType.SyncComplete, Desc = "Auto-sync: 23 new commits analyzed", Hours = 6 },
                new { Type = RepositoryActivityType.NewCriticalIssue, Desc = "New complexity hotspot detected in PaymentService.cs", Hours = 8 },
                new { Type = RepositoryActivityType.SecurityVulnerabilityDetected, Desc = "Security vulnerability flagged in auth module", Hours = 12 }
            };

            foreach (var evt in events)
            {
                activities.Add(new RepositoryActivity
                {
                    Type = evt.Type,
                    Description = evt.Desc,
                    Timestamp = DateTime.UtcNow.AddHours(-evt.Hours),
                    Severity = evt.Type == RepositoryActivityType.NewCriticalIssue ? ActivitySeverity.Critical : ActivitySeverity.Info,
                    NavigationRoute = $"/repositories/{repositoryId}/analytics"
                });
            }

            return activities;
        }

        private string GetSyncStatus(Repository repository)
        {
            if (repository.Status == RepositoryStatus.Syncing) return "syncing";
            if (repository.Status == RepositoryStatus.Error) return "failed";
            return "synced";
        }

        #endregion

        #region Score Calculation Methods

        private async Task<double> CalculateComplexityScoreAsync(int repositoryId)
        {
            var avgComplexity = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId && f.CyclomaticComplexity.HasValue)
                .AverageAsync(f => f.CyclomaticComplexity.Value);

            // Convert complexity to 0-100 scale (lower complexity = higher score)
            return Math.Max(0, 100 - (avgComplexity * 5));
        }

        private async Task<double> CalculateTestCoverageScoreAsync(int repositoryId)
        {
            var avgCoverage = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId && f.TestCoveragePercentage.HasValue)
                .AverageAsync(f => f.TestCoveragePercentage.Value);

            return avgCoverage;
        }

        private async Task<double> CalculateSecurityScoreAsync(int repositoryId)
        {
            // Count security issues and convert to score
            var securityIssues = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .Where(f => f.FilePath.Contains("security", StringComparison.OrdinalIgnoreCase))
                .CountAsync();

            return Math.Max(0, 100 - (securityIssues * 10));
        }

        private async Task<double> CalculateMaintenanceScoreAsync(int repositoryId)
        {
            var repository = await _context.Repositories.FindAsync(repositoryId);
            if (repository?.LastSyncAt == null) return 50;

            var daysSinceSync = (DateTime.UtcNow - repository.LastSyncAt.Value).TotalDays;
            return Math.Max(0, 100 - (daysSinceSync * 2));
        }

        #endregion
    }
}
