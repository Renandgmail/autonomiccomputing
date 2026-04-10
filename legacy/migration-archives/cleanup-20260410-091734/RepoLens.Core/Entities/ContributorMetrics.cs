using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepoLens.Core.Entities;

public class ContributorMetrics
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string ContributorName { get; set; } = string.Empty;
    public string ContributorEmail { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Basic Contribution Stats
    public int CommitCount { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }
    public int LinesModified { get; set; }
    public int FilesModified { get; set; }
    public int FilesAdded { get; set; }
    public int FilesDeleted { get; set; }
    
    // Advanced Contribution Analysis
    public double ContributionPercentage { get; set; }
    public int WorkingDays { get; set; }
    public double AverageCommitSize { get; set; }
    public double CommitFrequency { get; set; }
    public int LongestCommitStreak { get; set; }
    public int CurrentCommitStreak { get; set; }
    
    // Code Ownership
    public int OwnedFiles { get; set; }
    public double CodeOwnershipPercentage { get; set; }
    public int UniqueFilesTouched { get; set; }
    
    // Collaboration Metrics
    public int PullRequestsCreated { get; set; }
    public int PullRequestsReviewed { get; set; }
    public int CodeReviewComments { get; set; }
    public int IssuesCreated { get; set; }
    public int IssuesResolved { get; set; }
    public int MentionedInCommits { get; set; }
    
    // Temporal Patterns (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? HourlyActivityPattern { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? WeeklyActivityPattern { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? MonthlyActivityPattern { get; set; }
    
    // Language Expertise (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? LanguageContributions { get; set; }
    
    // File Type Contributions (JSON stored)
    [Column(TypeName = "jsonb")]
    public string? FileTypeContributions { get; set; }
    
    // Quality Metrics
    public double AvgCommitMessageLength { get; set; }
    public double CommitMessageQualityScore { get; set; }
    public int BugFixCommits { get; set; }
    public int FeatureCommits { get; set; }
    public int RefactoringCommits { get; set; }
    public int DocumentationCommits { get; set; }
    
    // Team Dynamics
    public bool IsCoreContributor { get; set; }
    public bool IsNewContributor { get; set; }
    public DateTime FirstContribution { get; set; }
    public DateTime LastContribution { get; set; }
    public int DaysActive { get; set; }
    public double RetentionScore { get; set; }
    
    // Additional properties for backward compatibility
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int FilesChanged { get; set; }
    public DateTime FirstCommit { get; set; }
    public DateTime LastCommit { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsPrimaryContributor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties needed for Repository Service
    public DateTime LastCommitAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public Repository? Repository { get; set; }
    
    // Calculated Properties
    public double ProductivityScore => CalculateProductivityScore();
    public string ContributorLevel => GetContributorLevel();
    public double ImpactScore => CalculateImpactScore();
    
    private double CalculateProductivityScore()
    {
        if (WorkingDays == 0) return 0;
        
        var commitsPerDay = (double)CommitCount / WorkingDays;
        var linesPerDay = (double)(LinesAdded + LinesModified) / WorkingDays;
        var filesPerDay = (double)FilesModified / WorkingDays;
        
        // Normalize and weight the metrics
        var score = 0.0;
        score += Math.Min(commitsPerDay * 10, 50) * 0.3; // Max 50 points for commits
        score += Math.Min(linesPerDay / 10, 30) * 0.4;   // Max 30 points for lines
        score += Math.Min(filesPerDay * 5, 20) * 0.3;    // Max 20 points for files
        
        return Math.Min(100, score);
    }
    
    private string GetContributorLevel()
    {
        var score = ProductivityScore;
        var ownership = CodeOwnershipPercentage;
        var tenure = (DateTime.UtcNow - FirstContribution).TotalDays;
        
        if (IsCoreContributor && ownership > 20 && tenure > 365)
            return "Core Maintainer";
        if (ownership > 10 && tenure > 180)
            return "Senior Contributor";
        if (CommitCount > 50 && tenure > 90)
            return "Regular Contributor";
        if (CommitCount > 10)
            return "Active Contributor";
        if (IsNewContributor)
            return "New Contributor";
        
        return "Occasional Contributor";
    }
    
    private double CalculateImpactScore()
    {
        var score = 0.0;
        
        // Code contribution impact (40%)
        score += ContributionPercentage * 0.4;
        
        // Code ownership impact (25%)
        score += CodeOwnershipPercentage * 0.25;
        
        // Collaboration impact (20%)
        var collaborationScore = (PullRequestsReviewed + CodeReviewComments) * 0.5;
        score += Math.Min(collaborationScore, 20) * 0.2;
        
        // Quality impact (15%)
        var qualityScore = CommitMessageQualityScore;
        score += qualityScore * 0.15;
        
        return Math.Min(100, score);
    }
}
