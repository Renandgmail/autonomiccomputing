namespace RepoLens.Core.Entities;

public class Repository
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public string? LastSyncCommit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Provider information
    public ProviderType ProviderType { get; set; } = ProviderType.Unknown;
    public string? AuthTokenReference { get; set; }
    
    // Authentication and authorization properties
    public int? OwnerId { get; set; }
    public int? OrganizationId { get; set; }
    public RepositoryType Type { get; set; } = RepositoryType.Git;
    public RepositoryStatus Status { get; set; } = RepositoryStatus.Active;
    public bool IsPrivate { get; set; } = false;
    public string? Description { get; set; }
    public string? DefaultBranch { get; set; } = "main";
    
    // Sync and analysis settings
    public DateTime? LastSyncAt { get; set; }
    public DateTime? LastAnalysisAt { get; set; }
    public bool AutoSync { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 60;
    public string? SyncErrorMessage { get; set; }
    
    // Access credentials (encrypted)
    public string? AccessToken { get; set; }
    public string? Username { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    
    // Code Intelligence Extensions (Action Item #3)
    public string? ScanStatus { get; set; } = "Pending";
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public string? ScanErrorMessage { get; set; }
    
    // Navigation properties
    public virtual User? Owner { get; set; }
    public virtual Organization? Organization { get; set; }
    public virtual ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
    public virtual ICollection<RepositoryMetrics> Metrics { get; set; } = new List<RepositoryMetrics>();
    public virtual ICollection<ContributorMetrics> ContributorMetrics { get; set; } = new List<ContributorMetrics>();
    public virtual ICollection<FileMetrics> FileMetrics { get; set; } = new List<FileMetrics>();
    
    // Code Intelligence Navigation Properties
    public virtual ICollection<RepositoryFile> RepositoryFiles { get; set; } = new List<RepositoryFile>();
}

public enum RepositoryType
{
    Git = 1,
    TFS = 2,
    SVN = 3,
    Perforce = 4,
    Mercurial = 5
}

public enum RepositoryStatus
{
    Active = 1,
    Inactive = 2,
    Syncing = 3,
    Error = 4,
    Archived = 5
}
