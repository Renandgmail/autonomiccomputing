using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepoLens.Core.Entities;

public class FileMetrics
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = string.Empty;
    public DateTime LastAnalyzed { get; set; }
    
    // Basic File Properties
    public long FileSizeBytes { get; set; }
    public int LineCount { get; set; }
    public int EffectiveLineCount { get; set; }
    public int CommentLineCount { get; set; }
    public int BlankLineCount { get; set; }
    public double CommentDensity { get; set; }
    
    // Code Complexity
    public double CyclomaticComplexity { get; set; }
    public double CognitiveComplexity { get; set; }
    public int MethodCount { get; set; }
    public int ClassCount { get; set; }
    public int FunctionCount { get; set; }
    public double AverageMethodLength { get; set; }
    public double MaxMethodLength { get; set; }
    public double NestingDepth { get; set; }
    
    // Code Quality Indicators
    public int CodeSmellCount { get; set; }
    public int CriticalIssues { get; set; }
    public int MajorIssues { get; set; }
    public int MinorIssues { get; set; }
    public double MaintainabilityIndex { get; set; }
    public double TechnicalDebtMinutes { get; set; }
    public int DuplicationLines { get; set; }
    public double DuplicationPercentage { get; set; }
    
    // Change History Metrics
    public int TotalCommits { get; set; }
    public int UniqueContributors { get; set; }
    public DateTime FirstCommit { get; set; }
    public DateTime LastCommit { get; set; }
    public int CommitsLastMonth { get; set; }
    public int CommitsLastQuarter { get; set; }
    public double ChangeFrequency { get; set; }
    public double ChurnRate { get; set; }
    
    // File Evolution
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public int TimesRenamed { get; set; }
    public int TimesMoved { get; set; }
    public double StabilityScore { get; set; }
    public double MaturityScore { get; set; }
    
    // Security & Vulnerability
    public int SecurityHotspots { get; set; }
    public int VulnerabilityCount { get; set; }
    public bool ContainsSensitiveData { get; set; }
    public bool HasSecurityAnnotations { get; set; }
    
    // Dependencies & Coupling
    public int IncomingDependencies { get; set; }
    public int OutgoingDependencies { get; set; }
    public double CouplingFactor { get; set; }
    public double CohesionLevel { get; set; }
    public int ExternalLibraryReferences { get; set; }
    
    // Documentation & Testing
    public double DocumentationCoverage { get; set; }
    public bool HasUnitTests { get; set; }
    public double TestCoverage { get; set; }
    public int TestCount { get; set; }
    public bool HasDocumentationComments { get; set; }
    
    // File Classification
    public string FileCategory { get; set; } = string.Empty; // Source, Test, Config, Documentation, etc.
    public bool IsGeneratedCode { get; set; }
    public bool IsThirdParty { get; set; }
    public bool IsBinaryFile { get; set; }
    public bool IsConfigurationFile { get; set; }
    public bool IsTestFile { get; set; }
    
    // Performance Indicators
    public bool IsHotspot { get; set; } // Frequently changed
    public bool IsColdspot { get; set; } // Rarely changed
    public double MaintenanceEffort { get; set; }
    public double RefactoringPriority { get; set; }
    public double BugProneness { get; set; }
    
    // Contributor Analysis (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? ContributorBreakdown { get; set; }
    
    // Change Patterns (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? ChangePatterns { get; set; }
    
    // Dependency Graph (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? DependencyGraph { get; set; }
    
    // Issue History (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? IssueHistory { get; set; }
    
    // Navigation Properties
    public Repository? Repository { get; set; }
    
    // Calculated Properties
    public string FormattedFileSize => FormatBytes(FileSizeBytes);
    public string FileType => GetFileType();
    public double QualityScore => CalculateQualityScore();
    public double MaintenanceRisk => CalculateMaintenanceRisk();
    public string RiskLevel => GetRiskLevel();
    
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1 && counter < suffixes.Length - 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
    
    private string GetFileType()
    {
        if (IsTestFile) return "Test";
        if (IsConfigurationFile) return "Configuration";
        if (IsGeneratedCode) return "Generated";
        if (IsThirdParty) return "Third Party";
        if (IsBinaryFile) return "Binary";
        
        return FileCategory switch
        {
            "Source" => "Source Code",
            "Documentation" => "Documentation",
            "Build" => "Build Script",
            "Data" => "Data File",
            _ => "Unknown"
        };
    }
    
    private double CalculateQualityScore()
    {
        var score = 100.0;
        
        // Deduct points for complexity
        score -= Math.Min(CyclomaticComplexity * 2, 30);
        score -= Math.Min(CognitiveComplexity * 1.5, 20);
        
        // Deduct points for issues
        score -= CriticalIssues * 10;
        score -= MajorIssues * 5;
        score -= MinorIssues * 1;
        
        // Deduct points for technical debt
        score -= Math.Min(TechnicalDebtMinutes / 10, 15);
        
        // Deduct points for duplication
        score -= DuplicationPercentage / 2;
        
        // Add points for documentation
        score += DocumentationCoverage * 0.1;
        
        // Add points for test coverage
        score += TestCoverage * 0.1;
        
        return Math.Max(0, Math.Min(100, score));
    }
    
    private double CalculateMaintenanceRisk()
    {
        var risk = 0.0;
        
        // High complexity increases risk
        risk += Math.Min(CyclomaticComplexity * 3, 40);
        
        // High change frequency increases risk
        risk += Math.Min(ChangeFrequency * 10, 25);
        
        // Large files are riskier
        risk += Math.Min(LineCount / 100, 15);
        
        // Technical debt increases risk
        risk += Math.Min(TechnicalDebtMinutes / 20, 10);
        
        // Low test coverage increases risk
        risk += Math.Max(0, (100 - TestCoverage) * 0.1);
        
        return Math.Min(100, risk);
    }
    
    private string GetRiskLevel()
    {
        var risk = MaintenanceRisk;
        return risk switch
        {
            >= 80 => "Critical",
            >= 60 => "High",
            >= 40 => "Medium",
            >= 20 => "Low",
            _ => "Minimal"
        };
    }
}
