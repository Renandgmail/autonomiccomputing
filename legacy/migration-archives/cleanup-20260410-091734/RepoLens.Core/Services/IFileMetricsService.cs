using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Service for calculating comprehensive file-level metrics and quality assessments
/// </summary>
public interface IFileMetricsService
{
    /// <summary>
    /// Calculates comprehensive file metrics for a single file
    /// </summary>
    /// <param name="repositoryId">The repository ID</param>
    /// <param name="filePath">The file path within the repository</param>
    /// <param name="fileContent">The content of the file</param>
    /// <param name="codeElements">Parsed code elements from the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive file metrics</returns>
    Task<FileMetrics> CalculateFileMetricsAsync(int repositoryId, string filePath, string fileContent, 
        IEnumerable<CodeElement> codeElements, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates complexity metrics for a file
    /// </summary>
    /// <param name="fileContent">The file content</param>
    /// <param name="language">The programming language</param>
    /// <param name="codeElements">Parsed code elements</param>
    /// <returns>Complexity metrics result</returns>
    Task<ComplexityMetricsResult> CalculateComplexityAsync(string fileContent, string language, 
        IEnumerable<CodeElement> codeElements);

    /// <summary>
    /// Analyzes code quality metrics for a file
    /// </summary>
    /// <param name="fileContent">The file content</param>
    /// <param name="language">The programming language</param>
    /// <param name="codeElements">Parsed code elements</param>
    /// <returns>Code quality metrics</returns>
    Task<QualityMetricsResult> AnalyzeQualityMetricsAsync(string fileContent, string language, 
        IEnumerable<CodeElement> codeElements);

    /// <summary>
    /// Calculates file health score based on various metrics
    /// </summary>
    /// <param name="complexityMetrics">Complexity metrics</param>
    /// <param name="qualityMetrics">Quality metrics</param>
    /// <param name="changeMetrics">Change pattern metrics</param>
    /// <returns>File health score (0.0 to 1.0)</returns>
    double CalculateFileHealthScore(ComplexityMetricsResult complexityMetrics, 
        QualityMetricsResult qualityMetrics, ChangePatternMetrics? changeMetrics = null);

    /// <summary>
    /// Analyzes change patterns for a file based on git history
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filePath">File path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Change pattern metrics</returns>
    Task<ChangePatternMetrics> AnalyzeChangePatterns(int repositoryId, string filePath, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs security analysis on file content
    /// </summary>
    /// <param name="fileContent">File content</param>
    /// <param name="filePath">File path</param>
    /// <param name="language">Programming language</param>
    /// <returns>Security analysis results</returns>
    Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string fileContent, string filePath, string language);

    /// <summary>
    /// Analyzes performance characteristics of the file
    /// </summary>
    /// <param name="fileContent">File content</param>
    /// <param name="filePath">File path</param>
    /// <param name="language">Programming language</param>
    /// <param name="codeElements">Parsed code elements</param>
    /// <returns>Performance analysis results</returns>
    Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string fileContent, string filePath, 
        string language, IEnumerable<CodeElement> codeElements);
}

/// <summary>
/// Result of complexity analysis
/// </summary>
public class ComplexityMetricsResult
{
    public double CyclomaticComplexity { get; set; }
    public double CognitiveComplexity { get; set; }
    public double NestingDepth { get; set; }
    public int LinesOfCode { get; set; }
    public int EffectiveLines { get; set; }
    public double AverageMethodLength { get; set; }
    public double MaxMethodLength { get; set; }
}

/// <summary>
/// Result of quality analysis
/// </summary>
public class QualityMetricsResult
{
    public double MaintainabilityIndex { get; set; }
    public double TechnicalDebtMinutes { get; set; }
    public int CodeSmellCount { get; set; }
    public List<string> CodeSmells { get; set; } = new();
    public double CommentDensity { get; set; }
    public double DocumentationCoverage { get; set; }
    public double DuplicationPercentage { get; set; }
    public List<string> DuplicatedBlocks { get; set; } = new();
}

/// <summary>
/// Change pattern analysis results
/// </summary>
public class ChangePatternMetrics
{
    public double ChurnRate { get; set; }
    public double ChangeFrequency { get; set; }
    public int TotalCommits { get; set; }
    public int UniqueContributors { get; set; }
    public Dictionary<string, int> ContributorBreakdown { get; set; } = new();
    public DateTime FirstCommit { get; set; }
    public DateTime LastCommit { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
}

/// <summary>
/// Security analysis results
/// </summary>
public class SecurityAnalysisResult
{
    public int VulnerabilityCount { get; set; }
    public int SecurityHotspots { get; set; }
    public List<string> SecurityIssues { get; set; } = new();
    public bool ContainsSensitiveData { get; set; }
    public List<string> SensitiveDataPatterns { get; set; } = new();
}

/// <summary>
/// Performance analysis results
/// </summary>
public class PerformanceAnalysisResult
{
    public double CompilationImpact { get; set; }
    public long BundleSizeContribution { get; set; }
    public List<string> OptimizationOpportunities { get; set; } = new();
    public double MemoryFootprint { get; set; }
    public int CpuIntensiveOperations { get; set; }
}
