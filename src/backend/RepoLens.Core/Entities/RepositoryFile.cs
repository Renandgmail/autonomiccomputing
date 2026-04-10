using System.ComponentModel.DataAnnotations;

namespace RepoLens.Core.Entities;

public class RepositoryFile
{
    public int Id { get; set; } // Use int to match existing pattern
    public int RepositoryId { get; set; } // Match existing Repository.Id type
    
    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string FileExtension { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Language { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    public int LineCount { get; set; }
    public DateTime LastModified { get; set; }
    
    [Required]
    [StringLength(64)]
    public string FileHash { get; set; } = string.Empty;
    
    public FileProcessingStatus ProcessingStatus { get; set; } = FileProcessingStatus.Pending;
    public int? ProcessingTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Additional property for tests
    public string? Content { get; set; }
    
    // Quality metrics properties for Repository Service calculations
    public int? CyclomaticComplexity { get; set; }
    public double? TestCoveragePercentage { get; set; }
    
    // Navigation properties
    public virtual Repository Repository { get; set; } = null!;
    public virtual ICollection<CodeElement> CodeElements { get; set; } = new List<CodeElement>();
}

// Follow existing enum pattern from RepositoryStatus
public enum FileProcessingStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
