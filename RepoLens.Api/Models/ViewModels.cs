using RepoLens.Core.Entities;

namespace RepoLens.Api.Models;

// Repository Models
public class RepositoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultBranch { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public DateTime? LastAnalysisAt { get; set; }
    public RepositoryStatus Status { get; set; }
    public bool IsPrivate { get; set; }
    public bool AutoSync { get; set; }
    public int SyncIntervalMinutes { get; set; }
    public string? SyncErrorMessage { get; set; }
    public string? OwnerName { get; set; }
    public string? OrganizationName { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    
    // Analytics properties
    public int ProcessedCommits { get; set; }
    public int ProcessedFiles { get; set; }
    public int ContributorCount { get; set; }
    public long SizeInBytes { get; set; }
    
    // Metrics data properties
    public int TotalCommits { get; set; }
    public int TotalFiles { get; set; }
    public int TotalContributors { get; set; }
    public string[] Languages { get; set; } = Array.Empty<string>();
    
    // Calculated properties
    public string FormattedLastSync => LastSyncAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
    public string StatusBadgeClass => Status switch
    {
        RepositoryStatus.Active => "badge-success",
        RepositoryStatus.Syncing => "badge-warning",
        RepositoryStatus.Error => "badge-danger",
        RepositoryStatus.Archived => "badge-secondary",
        _ => "badge-secondary"
    };
}

public class RepositoryStatsViewModel
{
    public int TotalCommits { get; set; }
    public int TotalFiles { get; set; }
    public int TotalContributors { get; set; }
    public long SizeInBytes { get; set; }
    public DateTime? LastCommitDate { get; set; }
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public double CodeQualityScore { get; set; }
    public double ProjectHealthScore { get; set; }
    
    public string FormattedSize => FormatBytes(SizeInBytes);
    
    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}

public class CommitViewModel
{
    public string Sha { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ShortSha => Sha.Length > 7 ? Sha[..7] : Sha;
    public string FormattedTimestamp => Timestamp.ToString("MMM dd, yyyy HH:mm");
}

// Add missing enums
public enum ProcessingStatus
{
    Pending = 1,
    InProgress = 2,
    Processing = 2, // Alias for InProgress
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum HealthStatus
{
    Healthy = 1,
    Warning = 2,
    Critical = 3,
    Unknown = 4
}

public enum ActivityType
{
    RepositoryAdded = 1,
    RepositorySynced = 2,
    RepositoryAnalyzed = 3,
    RepositoryError = 4,
    UserActivity = 5,
    SystemEvent = 6,
    RepositoryProcessed = 7
}

// Search Models
public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int? RepositoryId { get; set; }
    public string[] FileTypes { get; set; } = Array.Empty<string>();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageSize { get; set; } = 50;
    public int Page { get; set; } = 1;
}

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<SearchResultItem> Results { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class SearchResultItem
{
    public string RepositoryName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int ArtifactId { get; set; }
    public SearchMatchType MatchType { get; set; }
    public string[] ContextLines { get; set; } = Array.Empty<string>();
}

public enum SearchMatchType
{
    FileName,
    CodeContent,
    Comment,
    ClassName,
    MethodName
}

public class FileContentViewModel
{
    public string RepositoryName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string? Metadata { get; set; }
}

// System Health Models
public class SystemHealthViewModel
{
    public DateTime CheckedAt { get; set; }
    public HealthStatus OverallStatus { get; set; }
    public HealthCheckResult DatabaseHealth { get; set; } = new();
    public HealthCheckResult StorageHealth { get; set; } = new();
    public SystemStatsViewModel RepositoryStats { get; set; } = new();
    public SystemStatsViewModel ProcessingStats { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public int ResponseTimeMs { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}

public class SystemStatsViewModel
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int ErrorCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class DashboardStatsViewModel
{
    public int TotalRepositories { get; set; }
    public int TotalArtifacts { get; set; }
    public long TotalStorageBytes { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public string FormattedStorageSize => FormatBytes(TotalStorageBytes);

    private static string FormatBytes(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}

public class ActivityItem
{
    public string Id { get; set; } = string.Empty;
    public ActivityType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ProcessingStatus Status { get; set; }
    public string? Details { get; set; }

    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");
    public string RelativeTime => GetRelativeTime(Timestamp);

    private static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        
        return timeSpan.TotalDays switch
        {
            >= 365 => $"{(int)(timeSpan.TotalDays / 365)} year(s) ago",
            >= 30 => $"{(int)(timeSpan.TotalDays / 30)} month(s) ago",
            >= 1 => $"{(int)timeSpan.TotalDays} day(s) ago",
            _ => timeSpan.TotalHours switch
            {
                >= 1 => $"{(int)timeSpan.TotalHours} hour(s) ago",
                _ => timeSpan.TotalMinutes switch
                {
                    >= 1 => $"{(int)timeSpan.TotalMinutes} minute(s) ago",
                    _ => "Just now"
                }
            }
        };
    }
}

// Repository Management Models
public class RepositoryListViewModel
{
    public List<RepositoryViewModel> Repositories { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SearchTerm { get; set; } = string.Empty;
    public ProcessingStatus? StatusFilter { get; set; }
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class RepositoryDetailsViewModel
{
    public RepositoryViewModel Repository { get; set; } = new();
    public RepositoryStatsViewModel Stats { get; set; } = new();
    public List<CommitViewModel> RecentCommits { get; set; } = new();
    public List<FileTreeItem> FileTree { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class FileTreeItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public List<FileTreeItem> Children { get; set; } = new();
    public int ArtifactId { get; set; }
}

// API Response Models
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string errorMessage)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }

    public static ApiResponse<T> ValidationErrorResult(List<string> validationErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ValidationErrors = validationErrors
        };
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

// Processing Models
public class ProcessingProgressViewModel
{
    public string RepositoryId { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
    public int TotalCommits { get; set; }
    public int ProcessedCommits { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public string? CurrentOperation { get; set; }
    public string? ErrorMessage { get; set; }

    public double ProgressPercentage => TotalCommits > 0 ? (double)ProcessedCommits / TotalCommits * 100 : 0;
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
}

// Authentication Models
public class RegisterRequestModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginRequestModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseModel
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public UserViewModel? User { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DebugInfo { get; set; }
}

public class UserViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? TimeZone { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
}
