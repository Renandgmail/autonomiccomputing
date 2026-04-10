using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Api.Models;

namespace RepoLens.Api.Services;

/// <summary>
/// Portfolio Service for Engineering Manager focused analytics
/// Implements business logic for L1 Portfolio Dashboard
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Get Zone 1 summary metrics for portfolio overview
    /// </summary>
    Task<PortfolioSummary> GetPortfolioSummaryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get Zone 2 repository list with filtering and sorting
    /// Default sort: starred first, then health score ascending (worst first)
    /// </summary>
    Task<PortfolioRepositoryListResponse> GetRepositoryListAsync(
        PortfolioFilters filters, 
        string userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get Zone 3 critical issues requiring immediate attention
    /// Maximum 5 items returned, conditional display when >= 1 exists
    /// </summary>
    Task<List<CriticalIssue>> GetCriticalIssuesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Toggle repository star status for current user
    /// Starred repositories appear at top of repository list
    /// </summary>
    Task<bool> ToggleRepositoryStarAsync(int repositoryId, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get available filter options for repository list
    /// </summary>
    Task<PortfolioFilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken = default);
}

public class PortfolioService : IPortfolioService
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<PortfolioService> _logger;
    
    // TODO: Add when user favorites are implemented
    // private readonly IUserRepository _userRepository;
    // private readonly IRepositoryStarRepository _starRepository;

    public PortfolioService(
        IRepositoryRepository repositoryRepository,
        ILogger<PortfolioService> logger)
    {
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<PortfolioSummary> GetPortfolioSummaryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating portfolio summary metrics");
        
        try
        {
            var repositories = await _repositoryRepository.GetAllAsync();
            
            if (!repositories.Any())
            {
                return new PortfolioSummary
                {
                    TotalRepositories = 0,
                    AverageHealthScore = 0,
                    CriticalIssuesCount = 0,
                    ActiveTeamsCount = 0,
                    HealthScoreTrend = new TrendIndicator 
                    { 
                        Direction = TrendDirection.Flat, 
                        Delta = "No data", 
                        Context = "no repositories"
                    },
                    LastCalculated = DateTime.UtcNow
                };
            }

            // Calculate health scores for all repositories
            var healthScores = new List<double>();
            var criticalIssuesCount = 0;
            var teams = new HashSet<string>();

            foreach (var repo in repositories)
            {
                // Calculate repository health score
                var healthScore = await CalculateRepositoryHealthScore(repo);
                healthScores.Add(healthScore);
                
                // Check for critical issues
                if (await HasCriticalIssues(repo, healthScore))
                {
                    criticalIssuesCount++;
                }
                
                // Track teams (extract from repo name or metadata)
                var team = ExtractTeamFromRepository(repo);
                if (!string.IsNullOrEmpty(team))
                {
                    teams.Add(team);
                }
            }

            // Calculate average health score
            var averageHealthScore = healthScores.Average();
            
            // Calculate health score trend (placeholder - would need historical data)
            var healthTrend = CalculateHealthScoreTrend(averageHealthScore);

            return new PortfolioSummary
            {
                TotalRepositories = repositories.Count(),
                AverageHealthScore = Math.Round(averageHealthScore, 1),
                CriticalIssuesCount = criticalIssuesCount,
                ActiveTeamsCount = teams.Count,
                HealthScoreTrend = healthTrend,
                LastCalculated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating portfolio summary");
            throw;
        }
    }

    public async Task<PortfolioRepositoryListResponse> GetRepositoryListAsync(
        PortfolioFilters filters, 
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting repository list with filters: {@Filters}", filters);
        
        try
        {
            var repositories = await _repositoryRepository.GetAllAsync();
            var portfolioRepos = new List<PortfolioRepository>();

            // Convert to portfolio repository models
            foreach (var repo in repositories)
            {
                var portfolioRepo = await ConvertToPortfolioRepository(repo, userId);
                portfolioRepos.Add(portfolioRepo);
            }

            var totalCount = portfolioRepos.Count;

            // Apply filters
            var filteredRepos = ApplyFilters(portfolioRepos, filters);
            
            // Apply sorting: starred first, then health score ascending (worst first)
            var sortedRepos = filteredRepos
                .OrderByDescending(r => r.IsStarred)  // Starred first
                .ThenBy(r => r.HealthScore)           // Then worst health first
                .ThenBy(r => r.Name)                  // Then alphabetical
                .ToList();

            // Get filter options for UI
            var filterOptions = await CalculateFilterOptions(portfolioRepos);

            return new PortfolioRepositoryListResponse
            {
                Repositories = sortedRepos,
                TotalCount = totalCount,
                FilteredCount = sortedRepos.Count,
                FilterOptions = filterOptions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository list");
            throw;
        }
    }

    public async Task<List<CriticalIssue>> GetCriticalIssuesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Identifying critical issues across portfolio");
        
        try
        {
            var repositories = await _repositoryRepository.GetAllAsync();
            var criticalIssues = new List<CriticalIssue>();

            foreach (var repo in repositories)
            {
                var issues = await IdentifyRepositoryCriticalIssues(repo);
                criticalIssues.AddRange(issues);
            }

            // Sort by severity and detection date, limit to 5
            var topCriticalIssues = criticalIssues
                .OrderByDescending(i => i.Severity)
                .ThenByDescending(i => i.DetectedAt)
                .Take(5)
                .ToList();

            return topCriticalIssues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying critical issues");
            throw;
        }
    }

    public async Task<bool> ToggleRepositoryStarAsync(int repositoryId, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Toggling star for repository {RepositoryId} by user {UserId}", repositoryId, userId);
        
        try
        {
            // TODO: Implement user favorites when user system is ready
            // For now, return a placeholder
            await Task.Delay(100, cancellationToken); // Simulate async operation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling repository star");
            throw;
        }
    }

    public async Task<PortfolioFilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting portfolio filter options");
        
        try
        {
            var repositories = await _repositoryRepository.GetAllAsync();
            var languages = new HashSet<string>();
            var teams = new HashSet<string>();
            var healthBandCounts = new Dictionary<RepositoryHealthBand, int>();

            // Initialize health band counts
            foreach (RepositoryHealthBand band in Enum.GetValues<RepositoryHealthBand>())
            {
                healthBandCounts[band] = 0;
            }

            foreach (var repo in repositories)
            {
                // Extract language
                var language = ExtractPrimaryLanguage(repo);
                if (!string.IsNullOrEmpty(language))
                {
                    languages.Add(language);
                }

                // Extract team
                var team = ExtractTeamFromRepository(repo);
                if (!string.IsNullOrEmpty(team))
                {
                    teams.Add(team);
                }

                // Calculate health band
                var healthScore = await CalculateRepositoryHealthScore(repo);
                var healthBand = CalculateHealthBand(healthScore);
                healthBandCounts[healthBand]++;
            }

            return new PortfolioFilterOptions
            {
                Languages = languages.OrderBy(l => l).ToList(),
                Teams = teams.OrderBy(t => t).ToList(),
                HealthBandCounts = healthBandCounts
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter options");
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<double> CalculateRepositoryHealthScore(Repository repository)
    {
        // TODO: Implement comprehensive health score calculation
        // For now, return a placeholder based on repository properties
        
        double score = 85.0; // Base score
        
        // Adjust based on last sync (staleness penalty)
        var daysSinceSync = (DateTime.UtcNow - repository.CreatedAt).TotalDays;
        if (daysSinceSync > 30)
        {
            score -= Math.Min(30, daysSinceSync - 30); // Max 30 point penalty
        }
        
        // Adjust based on repository name (temporary heuristic)
        if (repository.Name.ToLower().Contains("legacy") || repository.Name.ToLower().Contains("old"))
        {
            score -= 20;
        }
        
        if (repository.Name.ToLower().Contains("test") || repository.Name.ToLower().Contains("demo"))
        {
            score += 10;
        }
        
        // Ensure score is within bounds
        return Math.Max(0, Math.Min(100, score));
    }

    private async Task<bool> HasCriticalIssues(Repository repository, double healthScore)
    {
        // Repository has critical issues if:
        // 1. Health score is in critical band (0-29%)
        // 2. Repository is stale (no recent activity)
        // TODO: Add security vulnerabilities, test coverage, technical debt checks
        
        if (healthScore < 30)
        {
            return true;
        }
        
        var daysSinceSync = (DateTime.UtcNow - repository.CreatedAt).TotalDays;
        if (daysSinceSync > 30)
        {
            return true;
        }
        
        return false;
    }

    private async Task<PortfolioRepository> ConvertToPortfolioRepository(Repository repository, string userId)
    {
        var healthScore = await CalculateRepositoryHealthScore(repository);
        var healthBand = CalculateHealthBand(healthScore);
        var hasCriticalIssues = await HasCriticalIssues(repository, healthScore);
        
        // TODO: Get actual star status from user preferences
        var isStarred = false; // Placeholder
        
        // TODO: Get actual issue counts from repository analysis
        var issues = new RepositoryIssues
        {
            Critical = hasCriticalIssues ? 1 : 0,
            High = Random.Shared.Next(0, 5),
            Medium = Random.Shared.Next(0, 10),
            Low = Random.Shared.Next(0, 15)
        };

        return new PortfolioRepository
        {
            Id = repository.Id,
            Name = repository.Name,
            Url = repository.Url,
            HealthScore = healthScore,
            HealthBand = healthBand,
            HealthTrend = CalculateHealthTrend(healthScore),
            PrimaryLanguage = ExtractPrimaryLanguage(repository),
            Issues = issues,
            LastSync = repository.CreatedAt, // TODO: Use actual last sync time
            IsStarred = isStarred,
            TeamName = ExtractTeamFromRepository(repository),
            HasCriticalIssues = hasCriticalIssues
        };
    }

    private List<PortfolioRepository> ApplyFilters(List<PortfolioRepository> repositories, PortfolioFilters filters)
    {
        var filtered = repositories.AsEnumerable();

        // Filter by health bands
        if (filters.HealthBands.Any())
        {
            filtered = filtered.Where(r => filters.HealthBands.Contains(r.HealthBand));
        }

        // Filter by languages
        if (filters.Languages.Any())
        {
            filtered = filtered.Where(r => filters.Languages.Contains(r.PrimaryLanguage));
        }

        // Filter by teams
        if (filters.Teams.Any())
        {
            filtered = filtered.Where(r => filters.Teams.Contains(r.TeamName));
        }

        // Filter by critical issues
        if (filters.HasCriticalIssuesOnly)
        {
            filtered = filtered.Where(r => r.HasCriticalIssues);
        }

        // Filter by starred
        if (filters.StarredOnly)
        {
            filtered = filtered.Where(r => r.IsStarred);
        }

        return filtered.ToList();
    }

    private async Task<List<CriticalIssue>> IdentifyRepositoryCriticalIssues(Repository repository)
    {
        var issues = new List<CriticalIssue>();
        var healthScore = await CalculateRepositoryHealthScore(repository);

        // Critical health score
        if (healthScore < 30)
        {
            issues.Add(new CriticalIssue
            {
                Id = $"health_{repository.Id}",
                RepositoryId = repository.Id,
                RepositoryName = repository.Name,
                Description = $"Health score critical at {healthScore:F1}% - requires immediate attention",
                Type = CriticalIssueType.HealthCritical,
                Severity = IssueSeverity.Critical,
                DetectedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 7)), // Simulated detection
                NavigationRoute = $"/repositories/{repository.Id}/health"
            });
        }

        // Stale repository
        var daysSinceSync = (DateTime.UtcNow - repository.CreatedAt).TotalDays;
        if (daysSinceSync > 30)
        {
            issues.Add(new CriticalIssue
            {
                Id = $"stale_{repository.Id}",
                RepositoryId = repository.Id,
                RepositoryName = repository.Name,
                Description = $"No activity for {daysSinceSync:F0} days - potential abandonment",
                Type = CriticalIssueType.Stale,
                Severity = IssueSeverity.High,
                DetectedAt = repository.CreatedAt.AddDays(30),
                NavigationRoute = $"/repositories/{repository.Id}/activity"
            });
        }

        // TODO: Add more critical issue detection logic:
        // - Security vulnerabilities
        // - Technical debt > 40 hours
        // - Test coverage < 80%
        // - Performance issues
        // - Compliance violations

        return issues;
    }

    private async Task<PortfolioFilterOptions> CalculateFilterOptions(List<PortfolioRepository> repositories)
    {
        var languages = repositories.Select(r => r.PrimaryLanguage).Distinct().OrderBy(l => l).ToList();
        var teams = repositories.Select(r => r.TeamName).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t).ToList();
        
        var healthBandCounts = repositories
            .GroupBy(r => r.HealthBand)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Ensure all health bands are present
        foreach (RepositoryHealthBand band in Enum.GetValues<RepositoryHealthBand>())
        {
            if (!healthBandCounts.ContainsKey(band))
            {
                healthBandCounts[band] = 0;
            }
        }

        return new PortfolioFilterOptions
        {
            Languages = languages,
            Teams = teams,
            HealthBandCounts = healthBandCounts
        };
    }

    private RepositoryHealthBand CalculateHealthBand(double healthScore)
    {
        return healthScore switch
        {
            >= 90 => RepositoryHealthBand.Excellent,
            >= 70 => RepositoryHealthBand.Good,
            >= 50 => RepositoryHealthBand.Fair,
            >= 30 => RepositoryHealthBand.Poor,
            _ => RepositoryHealthBand.Critical
        };
    }

    private TrendIndicator CalculateHealthScoreTrend(double currentScore)
    {
        // TODO: Implement actual trend calculation with historical data
        // For now, return a simulated trend
        var random = Random.Shared;
        var directions = new[] { TrendDirection.Up, TrendDirection.Down, TrendDirection.Flat };
        var direction = directions[random.Next(directions.Length)];
        
        var delta = direction switch
        {
            TrendDirection.Up => $"+{random.Next(1, 8)}%",
            TrendDirection.Down => $"-{random.Next(1, 8)}%",
            _ => "stable"
        };

        return new TrendIndicator
        {
            Direction = direction,
            Delta = delta,
            Context = "vs last week",
            PositiveDirection = TrendDirection.Up
        };
    }

    private TrendIndicator CalculateHealthTrend(double healthScore)
    {
        // Individual repository health trend
        // TODO: Implement with historical data
        return CalculateHealthScoreTrend(healthScore);
    }

    private string ExtractPrimaryLanguage(Repository repository)
    {
        // TODO: Implement actual language detection from repository analysis
        // For now, extract from repository name or use heuristics
        
        var name = repository.Name.ToLower();
        if (name.Contains("react") || name.Contains("js") || name.Contains("frontend"))
            return "JavaScript";
        if (name.Contains("api") || name.Contains("backend") || name.Contains(".net"))
            return "C#";
        if (name.Contains("python") || name.Contains("py"))
            return "Python";
        if (name.Contains("java"))
            return "Java";
        if (name.Contains("go") || name.Contains("golang"))
            return "Go";
        
        return "Unknown";
    }

    private string ExtractTeamFromRepository(Repository repository)
    {
        // TODO: Implement actual team extraction from repository metadata
        // For now, extract from repository name patterns
        
        var name = repository.Name.ToLower();
        if (name.Contains("frontend") || name.Contains("ui") || name.Contains("web"))
            return "Frontend Team";
        if (name.Contains("backend") || name.Contains("api") || name.Contains("service"))
            return "Backend Team";
        if (name.Contains("mobile") || name.Contains("ios") || name.Contains("android"))
            return "Mobile Team";
        if (name.Contains("data") || name.Contains("analytics"))
            return "Data Team";
        if (name.Contains("devops") || name.Contains("infra"))
            return "DevOps Team";
        
        return "Platform Team"; // Default
    }

    #endregion
}
