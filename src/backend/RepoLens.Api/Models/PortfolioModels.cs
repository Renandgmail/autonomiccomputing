using RepoLens.Core.Entities;

namespace RepoLens.Api.Models;

/// <summary>
/// Zone 1: Summary metrics for Engineering Manager portfolio overview
/// Exactly 4 metrics as specified in L1_PORTFOLIO_DASHBOARD.md
/// </summary>
public class PortfolioSummary
{
    /// <summary>
    /// Total number of connected repositories
    /// Links to: Settings > Repositories
    /// </summary>
    public int TotalRepositories { get; set; }
    
    /// <summary>
    /// Average health score across all repositories (0-100%)
    /// Includes trend arrow (up/down/flat)
    /// Links to: Filtered repo list (no navigation)
    /// </summary>
    public double AverageHealthScore { get; set; }
    public TrendIndicator HealthScoreTrend { get; set; }
    
    /// <summary>
    /// Count of repositories with critical issues
    /// Links to: Filtered repo list showing critical only
    /// </summary>
    public int CriticalIssuesCount { get; set; }
    
    /// <summary>
    /// Number of active teams working on repositories
    /// Display only (no link)
    /// </summary>
    public int ActiveTeamsCount { get; set; }
    
    /// <summary>
    /// Last calculation timestamp
    /// </summary>
    public DateTime LastCalculated { get; set; }
}

/// <summary>
/// Zone 2: Repository item for Engineering Manager decision making
/// Default sorted by health score ascending (worst first)
/// Starred repositories appear above sorted results
/// </summary>
public class PortfolioRepository
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Health score (0-100%) determines sort order and color band
    /// </summary>
    public double HealthScore { get; set; }
    public RepositoryHealthBand HealthBand { get; set; }
    public TrendIndicator HealthTrend { get; set; }
    
    /// <summary>
    /// Primary programming language for badge display
    /// </summary>
    public string PrimaryLanguage { get; set; } = "Unknown";
    
    /// <summary>
    /// Issue counts for Engineering Manager attention
    /// </summary>
    public RepositoryIssues Issues { get; set; } = new();
    
    /// <summary>
    /// Last synchronization for staleness indication
    /// </summary>
    public DateTime LastSync { get; set; }
    
    /// <summary>
    /// Whether this repository is starred by the current user
    /// Starred repositories appear at the top of the list
    /// </summary>
    public bool IsStarred { get; set; }
    
    /// <summary>
    /// Team responsible for this repository
    /// Used for filtering and team analytics
    /// </summary>
    public string TeamName { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if repository has critical issues requiring immediate attention
    /// </summary>
    public bool HasCriticalIssues { get; set; }
}

/// <summary>
/// Zone 3: Critical issue requiring Engineering Manager attention
/// Only displayed when >= 1 repository has critical-severity issues
/// Maximum 5 items shown before "See all X critical issues" link
/// </summary>
public class CriticalIssue
{
    public string Id { get; set; } = string.Empty;
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// One sentence description of the critical issue
    /// Examples:
    /// - "3 critical security vulnerabilities"
    /// - "Technical debt exceeds 40 hours"
    /// - "Test coverage below 80%"
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Critical issue type for routing and prioritization
    /// </summary>
    public CriticalIssueType Type { get; set; }
    
    /// <summary>
    /// Severity level for display styling
    /// </summary>
    public IssueSeverity Severity { get; set; }
    
    /// <summary>
    /// When this issue was first detected
    /// </summary>
    public DateTime DetectedAt { get; set; }
    
    /// <summary>
    /// Direct route to the relevant L2 repository dashboard section
    /// </summary>
    public string NavigationRoute { get; set; } = string.Empty;
}

/// <summary>
/// Health score color bands as specified in L1_PORTFOLIO_DASHBOARD.md
/// Used for consistent color coding across the portfolio
/// </summary>
public enum RepositoryHealthBand
{
    /// <summary>
    /// 90-100%: No action required (Green #16A34A)
    /// </summary>
    Excellent = 5,
    
    /// <summary>
    /// 70-89%: Monitor, low priority (Teal #0D9488)
    /// </summary>
    Good = 4,
    
    /// <summary>
    /// 50-69%: Plan improvement sprint (Amber #D97706)
    /// </summary>
    Fair = 3,
    
    /// <summary>
    /// 30-49%: Immediate attention needed (Orange #EA580C)
    /// </summary>
    Poor = 2,
    
    /// <summary>
    /// 0-29%: Escalate to leadership (Red #DC2626)
    /// </summary>
    Critical = 1
}

/// <summary>
/// Critical issue classification for Engineering Manager decision making
/// Based on business impact and urgency
/// </summary>
public enum CriticalIssueType
{
    /// <summary>
    /// Security vulnerabilities of any severity
    /// Immediate attention required
    /// </summary>
    Security,
    
    /// <summary>
    /// Technical debt exceeding 40 hours
    /// Plan remediation sprint
    /// </summary>
    TechnicalDebt,
    
    /// <summary>
    /// Test coverage below 80% threshold
    /// Quality risk accumulation
    /// </summary>
    TestCoverage,
    
    /// <summary>
    /// Repository health in critical band (0-29%)
    /// Comprehensive review needed
    /// </summary>
    HealthCritical,
    
    /// <summary>
    /// No commits in 30+ days
    /// Potential abandonment
    /// </summary>
    Stale,
    
    /// <summary>
    /// Performance degradation detected
    /// User experience impact
    /// </summary>
    Performance,
    
    /// <summary>
    /// Compliance violations detected
    /// Regulatory risk
    /// </summary>
    Compliance
}

/// <summary>
/// Issue severity levels for display styling and prioritization
/// </summary>
public enum IssueSeverity
{
    Critical = 4,
    High = 3,
    Medium = 2,
    Low = 1,
    Info = 0
}

/// <summary>
/// Trend direction indicator for health score and metrics
/// </summary>
public class TrendIndicator
{
    /// <summary>
    /// Trend direction: up, down, or flat
    /// </summary>
    public TrendDirection Direction { get; set; }
    
    /// <summary>
    /// Magnitude of change (e.g., "+5%", "-2 points", "stable")
    /// </summary>
    public string Delta { get; set; } = string.Empty;
    
    /// <summary>
    /// Time context for the trend (e.g., "vs last week", "this month")
    /// </summary>
    public string Context { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether up is positive or negative for this metric
    /// </summary>
    public TrendDirection PositiveDirection { get; set; } = TrendDirection.Up;
}

/// <summary>
/// Trend direction enumeration
/// </summary>
public enum TrendDirection
{
    Up,
    Down,
    Flat
}

/// <summary>
/// Repository issue counts for Engineering Manager overview
/// </summary>
public class RepositoryIssues
{
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
    
    /// <summary>
    /// Total count of all issues
    /// </summary>
    public int Total => Critical + High + Medium + Low;
    
    /// <summary>
    /// Whether this repository has any critical issues
    /// </summary>
    public bool HasCritical => Critical > 0;
}

/// <summary>
/// Request model for repository starring/unstarring
/// </summary>
public class StarRepositoryRequest
{
    public int RepositoryId { get; set; }
    public bool IsStarred { get; set; }
}

/// <summary>
/// Filter options for Zone 2 repository list
/// All filters can be combined
/// </summary>
public class PortfolioFilters
{
    /// <summary>
    /// Filter by health band (can select multiple)
    /// </summary>
    public List<RepositoryHealthBand> HealthBands { get; set; } = new();
    
    /// <summary>
    /// Filter by primary language (can select multiple)
    /// </summary>
    public List<string> Languages { get; set; } = new();
    
    /// <summary>
    /// Filter by team (can select multiple)
    /// </summary>
    public List<string> Teams { get; set; } = new();
    
    /// <summary>
    /// Show only repositories with critical issues
    /// </summary>
    public bool HasCriticalIssuesOnly { get; set; }
    
    /// <summary>
    /// Show only starred repositories
    /// </summary>
    public bool StarredOnly { get; set; }
}

/// <summary>
/// Response model for portfolio repository list with filtering and sorting
/// </summary>
public class PortfolioRepositoryListResponse
{
    /// <summary>
    /// Filtered and sorted repository list
    /// Default sort: starred first, then health score ascending (worst first)
    /// </summary>
    public List<PortfolioRepository> Repositories { get; set; } = new();
    
    /// <summary>
    /// Total count before filtering (for pagination)
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Count after filtering
    /// </summary>
    public int FilteredCount { get; set; }
    
    /// <summary>
    /// Available filter options based on current data
    /// </summary>
    public PortfolioFilterOptions FilterOptions { get; set; } = new();
}

/// <summary>
/// Available filter options for the frontend UI
/// </summary>
public class PortfolioFilterOptions
{
    /// <summary>
    /// Available languages in the portfolio
    /// </summary>
    public List<string> Languages { get; set; } = new();
    
    /// <summary>
    /// Available teams in the portfolio
    /// </summary>
    public List<string> Teams { get; set; } = new();
    
    /// <summary>
    /// Health band distribution counts
    /// </summary>
    public Dictionary<RepositoryHealthBand, int> HealthBandCounts { get; set; } = new();
}
