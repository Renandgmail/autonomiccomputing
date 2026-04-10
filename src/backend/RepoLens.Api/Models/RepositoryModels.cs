/**
 * Repository Models for L2 Repository Dashboard
 * Focused on repository-specific insights for Engineering Managers and Team Leads
 * Supports 60-second decision time target
 */

using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Models
{
    // Zone 1: Repository Summary Models
    public class RepositorySummary
    {
        public double HealthScore { get; set; }
        public TrendIndicator HealthTrend { get; set; } = new();
        public int ActiveContributors { get; set; }
        public int CriticalIssues { get; set; }
        public double TechnicalDebtHours { get; set; }
        public DateTime LastCalculated { get; set; }
    }

    // Zone 2: Quality Hotspots Models
    public class QualityHotspot
    {
        public int FileId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public HotspotSeverity Severity { get; set; }
        public HotspotIssueType IssueType { get; set; }
        public double EstimatedFixHours { get; set; }
        public double HotspotScore { get; set; } // complexity × churn × (1 - quality)
        public DateTime DetectedAt { get; set; }
        
        // Navigation support
        public string NavigationRoute => $"/repositories/{RepositoryId}/files/{FileId}";
        public int RepositoryId { get; set; }
    }

    public enum HotspotSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum HotspotIssueType
    {
        Complexity,
        Security,
        TechnicalDebt,
        TestCoverage,
        Performance,
        Maintainability
    }

    public class QualityHotspotsResponse
    {
        public List<QualityHotspot> Hotspots { get; set; } = new();
        public int TotalCount { get; set; }
        public int ShownCount => Math.Min(Hotspots.Count, 5);
        public bool HasMore => TotalCount > 5;
    }

    // Zone 3: Activity Feed Models - Renamed to avoid conflicts
    public class RepositoryActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public RepositoryActivityType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ActivitySeverity Severity { get; set; } = ActivitySeverity.Info;
        public string? NavigationRoute { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public enum RepositoryActivityType
    {
        QualityGatePass,
        QualityGateFail,
        NewCriticalIssue,
        SecurityVulnerabilityDetected,
        ComplexitySpike,
        BuildSuccess,
        BuildFailure,
        SyncComplete,
        AnalysisComplete
    }

    public enum ActivitySeverity
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    public class RepositoryActivityResponse
    {
        public List<RepositoryActivity> Activities { get; set; } = new();
        public int TotalCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // Zone 4: Quick Actions Models (Navigation support)
    public class QuickActionRoute
    {
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }

    public class RepositoryQuickActions
    {
        public int RepositoryId { get; set; }
        public List<QuickActionRoute> Actions { get; set; } = new();
    }

    // Repository Context for breadcrumb and navigation
    public class RepositoryContext
    {
        public int RepositoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public double HealthScore { get; set; }
        public TrendIndicator HealthTrend { get; set; } = new();
        public DateTime LastSync { get; set; }
        public string SyncStatus { get; set; } = "synced"; // synced, syncing, failed
        public string BreadcrumbPath => $"Portfolio > {Name}";
    }

    // Request/Response models for API endpoints
    public class GetRepositorySummaryRequest
    {
        [Required]
        public int RepositoryId { get; set; }
    }

    public class GetQualityHotspotsRequest
    {
        [Required]
        public int RepositoryId { get; set; }
        public int Limit { get; set; } = 5;
        public HotspotSeverity? MinSeverity { get; set; }
        public HotspotIssueType? IssueTypeFilter { get; set; }
    }

    public class GetRepositoryActivityRequest
    {
        [Required]
        public int RepositoryId { get; set; }
        public int Limit { get; set; } = 10;
        public DateTime? Since { get; set; }
        public List<RepositoryActivityType>? TypeFilters { get; set; }
    }

    // Error handling models
    public class RepositoryError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // Constants and utilities
    public static class RepositoryConstants
    {
        public const int DEFAULT_HOTSPOTS_LIMIT = 5;
        public const int DEFAULT_ACTIVITY_LIMIT = 10;
        public const int MAX_HOTSPOTS_LIMIT = 50;
        public const int MAX_ACTIVITY_LIMIT = 100;
        
        // Hotspot score calculation weights
        public const double COMPLEXITY_WEIGHT = 0.4;
        public const double CHURN_WEIGHT = 0.3;
        public const double QUALITY_WEIGHT = 0.3;
        
        // Severity thresholds
        public const double CRITICAL_SEVERITY_THRESHOLD = 0.8;
        public const double HIGH_SEVERITY_THRESHOLD = 0.6;
        public const double MEDIUM_SEVERITY_THRESHOLD = 0.4;
        
        // Activity feed time ranges
        public const int ACTIVITY_DAYS_BACK = 30;
        public const int CONTRIBUTOR_DAYS_BACK = 30;
    }

    // Utility extension methods
    public static class RepositoryModelExtensions
    {
        public static string GetSeverityColor(this HotspotSeverity severity)
        {
            return severity switch
            {
                HotspotSeverity.Critical => "#DC2626", // Red
                HotspotSeverity.High => "#EA580C",     // Orange  
                HotspotSeverity.Medium => "#D97706",   // Amber
                HotspotSeverity.Low => "#16A34A",      // Green
                _ => "#6B7280" // Gray
            };
        }

        public static string GetIssueTypeLabel(this HotspotIssueType issueType)
        {
            return issueType switch
            {
                HotspotIssueType.Complexity => "Complexity",
                HotspotIssueType.Security => "Security",
                HotspotIssueType.TechnicalDebt => "Technical Debt",
                HotspotIssueType.TestCoverage => "Test Coverage",
                HotspotIssueType.Performance => "Performance",
                HotspotIssueType.Maintainability => "Maintainability",
                _ => "Unknown"
            };
        }

        public static string GetActivityIcon(this RepositoryActivityType activityType)
        {
            return activityType switch
            {
                RepositoryActivityType.QualityGatePass => "✅",
                RepositoryActivityType.QualityGateFail => "❌",
                RepositoryActivityType.NewCriticalIssue => "🔴",
                RepositoryActivityType.SecurityVulnerabilityDetected => "🛡️",
                RepositoryActivityType.ComplexitySpike => "📈",
                RepositoryActivityType.BuildSuccess => "✅",
                RepositoryActivityType.BuildFailure => "❌",
                RepositoryActivityType.SyncComplete => "🔄",
                RepositoryActivityType.AnalysisComplete => "🔍",
                _ => "ℹ️"
            };
        }

        public static string FormatEstimatedTime(this double hours)
        {
            if (hours < 1)
            {
                var minutes = (int)(hours * 60);
                return $"~{minutes}m";
            }
            else if (hours < 8)
            {
                return $"~{hours:F1}h";
            }
            else
            {
                var days = Math.Ceiling(hours / 8);
                return $"~{days}d";
            }
        }
    }
}
