using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Service for analyzing repository code structure and extracting code intelligence
/// </summary>
public interface IRepositoryAnalysisService
{
    /// <summary>
    /// Start full analysis of a repository including file scanning and code element extraction
    /// </summary>
    /// <param name="repositoryId">Repository to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job ID for tracking analysis progress</returns>
    Task<int> StartFullAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start incremental analysis for specific files or changes
    /// </summary>
    /// <param name="repositoryId">Repository to analyze</param>
    /// <param name="specificFiles">Optional list of specific files to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job ID for tracking analysis progress</returns>
    Task<int> StartIncrementalAnalysisAsync(int repositoryId, string[]? specificFiles = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get analysis progress for a specific job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis progress information</returns>
    Task<AnalysisProgress> GetAnalysisProgressAsync(int jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stop a running analysis job
    /// </summary>
    /// <param name="jobId">Job identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successfully stopped</returns>
    Task<bool> StopAnalysisAsync(int jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get analysis results for a repository
    /// </summary>
    /// <param name="repositoryId">Repository identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete analysis results</returns>
    Task<RepositoryAnalysisResult> GetAnalysisResultAsync(int repositoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for analyzing individual files and extracting code elements
/// </summary>
public interface IFileAnalysisService
{
    /// <summary>
    /// Analyze a single file and extract code elements
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="content">File content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis results for the file</returns>
    Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, string content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a file extension is supported for analysis
    /// </summary>
    /// <param name="fileExtension">File extension (e.g., ".cs", ".js")</param>
    /// <returns>True if supported</returns>
    bool IsSupported(string fileExtension);
    
    /// <summary>
    /// Get list of supported programming languages
    /// </summary>
    string[] SupportedLanguages { get; }
}

// Supporting models
public class AnalysisProgress
{
    public int JobId { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Processing, Completed, Failed
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public string? CurrentFile { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ProcessingErrors { get; set; } = new();
    
    public double ProgressPercentage => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;
    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;
}

public class RepositoryAnalysisResult
{
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string ScanStatus { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public int TotalCodeElements { get; set; }
    public DateTime? LastAnalysisAt { get; set; }
    public string? ScanErrorMessage { get; set; }
    
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public Dictionary<string, int> FileTypeDistribution { get; set; } = new();
    public Dictionary<CodeElementType, int> CodeElementDistribution { get; set; } = new();
    
    public List<RepositoryFile> Files { get; set; } = new();
    public List<string> SupportedLanguages { get; set; } = new();
    public List<string> ProcessingErrors { get; set; } = new();
}

public class FileAnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public List<CodeElement> CodeElements { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ProcessingTimeMs { get; set; }
}
