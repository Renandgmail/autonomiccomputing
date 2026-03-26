using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepoLens.Core.Entities;

public class RepositoryMetrics
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public DateTime MeasurementDate { get; set; }
    
    // Code Quality Metrics
    public int TotalLinesOfCode { get; set; }
    public int EffectiveLinesOfCode { get; set; }
    public int CommentLines { get; set; }
    public int BlankLines { get; set; }
    public double CommentRatio { get; set; }
    public double DuplicationPercentage { get; set; }
    public int CodeSmells { get; set; }
    public double TechnicalDebtHours { get; set; }
    public double MaintainabilityIndex { get; set; }
    
    // Complexity Metrics
    public double AverageCyclomaticComplexity { get; set; }
    public int MaxCyclomaticComplexity { get; set; }
    public double AverageMethodLength { get; set; }
    public double AverageClassSize { get; set; }
    public int TotalMethods { get; set; }
    public int TotalClasses { get; set; }
    public double CognitiveComplexity { get; set; }
    public double HalsteadVolume { get; set; }
    public double HalsteadDifficulty { get; set; }
    
    // Development Activity Metrics
    public int CommitsLastWeek { get; set; }
    public int CommitsLastMonth { get; set; }
    public int CommitsLastQuarter { get; set; }
    public double AverageCommitSize { get; set; }
    public int FilesChangedLastWeek { get; set; }
    public int LinesAddedLastWeek { get; set; }
    public int LinesDeletedLastWeek { get; set; }
    public double DevelopmentVelocity { get; set; }
    
    // Repository Structure Metrics
    public int TotalFiles { get; set; }
    public int TotalDirectories { get; set; }
    public long RepositorySizeBytes { get; set; }
    public int MaxDirectoryDepth { get; set; }
    public double AverageFileSize { get; set; }
    public int BinaryFileCount { get; set; }
    public int TextFileCount { get; set; }
    
    // Language Distribution (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? LanguageDistribution { get; set; }
    
    // File Type Distribution (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? FileTypeDistribution { get; set; }
    
    // Hourly Activity Patterns (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? HourlyActivityPattern { get; set; }
    
    // Daily Activity Patterns (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? DailyActivityPattern { get; set; }
    
    // Test Coverage Metrics
    public double LineCoveragePercentage { get; set; }
    public double BranchCoveragePercentage { get; set; }
    public double FunctionCoveragePercentage { get; set; }
    public double TestToCodeRatio { get; set; }
    
    // Security & Dependencies
    public int TotalDependencies { get; set; }
    public int OutdatedDependencies { get; set; }
    public int VulnerableDependencies { get; set; }
    public int SecurityVulnerabilities { get; set; }
    public int CriticalVulnerabilities { get; set; }
    
    // Build & Quality Gates
    public double BuildSuccessRate { get; set; }
    public double TestPassRate { get; set; }
    public int QualityGateFailures { get; set; }
    
    // Documentation Metrics
    public double DocumentationCoverage { get; set; }
    public double ApiDocumentationCoverage { get; set; }
    public int ReadmeWordCount { get; set; }
    public int WikiPageCount { get; set; }
    
    // Collaboration Metrics
    public int ActiveContributors { get; set; }
    public int TotalContributors { get; set; }
    public double BusFactor { get; set; }
    public double CodeOwnershipConcentration { get; set; }
    
    // Navigation Property
    public Repository? Repository { get; set; }
    
    // Calculated Properties
    public string FormattedRepositorySize => FormatBytes(RepositorySizeBytes);
    public double CodeQualityScore => CalculateCodeQualityScore();
    public double ProjectHealthScore => CalculateProjectHealthScore();
    
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }
    
    private double CalculateCodeQualityScore()
    {
        // Calculate a weighted quality score from various metrics
        var score = 0.0;
        
        // Maintainability (30%)
        score += MaintainabilityIndex * 0.3;
        
        // Test Coverage (25%)
        score += LineCoveragePercentage * 0.25;
        
        // Documentation (20%)
        score += DocumentationCoverage * 0.2;
        
        // Code Complexity (15%)
        var complexityScore = Math.Max(0, 100 - (AverageCyclomaticComplexity * 10));
        score += complexityScore * 0.15;
        
        // Technical Debt (10%)
        var debtScore = Math.Max(0, 100 - TechnicalDebtHours);
        score += debtScore * 0.1;
        
        return Math.Min(100, Math.Max(0, score));
    }
    
    private double CalculateProjectHealthScore()
    {
        // Calculate overall project health
        var score = 0.0;
        
        // Code Quality (40%)
        score += CodeQualityScore * 0.4;
        
        // Build Health (20%)
        score += BuildSuccessRate * 0.2;
        
        // Security (20%)
        var securityScore = Math.Max(0, 100 - (SecurityVulnerabilities * 10));
        score += securityScore * 0.2;
        
        // Activity Level (10%)
        var activityScore = Math.Min(100, CommitsLastMonth * 2);
        score += activityScore * 0.1;
        
        // Team Health (10%)
        var teamScore = Math.Min(100, BusFactor * 20);
        score += teamScore * 0.1;
        
        return Math.Min(100, Math.Max(0, score));
    }
}
