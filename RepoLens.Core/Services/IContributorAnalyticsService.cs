using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Service for analyzing contributor patterns and team collaboration metrics
/// </summary>
public interface IContributorAnalyticsService
{
    /// <summary>
    /// Calculates comprehensive contributor metrics for a repository
    /// </summary>
    /// <param name="repositoryId">The repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of contributor metrics for all contributors</returns>
    Task<List<ContributorMetrics>> CalculateContributorMetricsAsync(int repositoryId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes contributor patterns for a specific contributor
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="contributorEmail">Contributor email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Contributor pattern analysis</returns>
    Task<ContributorPatternAnalysis> AnalyzeContributorPatternsAsync(int repositoryId, string contributorEmail, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates team collaboration metrics for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team collaboration analysis</returns>
    Task<TeamCollaborationMetrics> AnalyzeTeamCollaborationAsync(int repositoryId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assesses productivity metrics for contributors
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="timeFrame">Time frame for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Productivity assessment results</returns>
    Task<ProductivityAssessment> AssessProductivityAsync(int repositoryId, TimeSpan timeFrame, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes team risks including bus factor and knowledge concentration
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team risk analysis</returns>
    Task<TeamRiskAnalysis> AnalyzeTeamRisksAsync(int repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recognizes activity patterns for contributors
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activity pattern recognition results</returns>
    Task<ActivityPatternRecognition> RecognizeActivityPatternsAsync(int repositoryId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracks growth and skill development for contributors
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="contributorEmail">Contributor email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Growth tracking analysis</returns>
    Task<GrowthTrackingAnalysis> TrackGrowthAsync(int repositoryId, string contributorEmail, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Contributor pattern analysis results
/// </summary>
public class ContributorPatternAnalysis
{
    public string ContributorEmail { get; set; } = string.Empty;
    public double CommitFrequency { get; set; }
    public Dictionary<string, double> CodeOwnership { get; set; } = new();
    public List<string> ExpertiseAreas { get; set; } = new();
    public Dictionary<DayOfWeek, int> WeeklyPattern { get; set; } = new();
    public Dictionary<int, int> HourlyPattern { get; set; } = new();
    public double ConsistencyScore { get; set; }
    public List<string> PreferredFileTypes { get; set; } = new();
}

/// <summary>
/// Team collaboration metrics
/// </summary>
public class TeamCollaborationMetrics
{
    public int TotalContributors { get; set; }
    public double CollaborationIndex { get; set; }
    public Dictionary<string, List<string>> CodeReviewNetwork { get; set; } = new();
    public Dictionary<string, double> KnowledgeSharingScore { get; set; } = new();
    public double TeamCohesion { get; set; }
    public List<string> IsolatedContributors { get; set; } = new();
    public Dictionary<string, string> MentoringRelationships { get; set; } = new();
    public double CommunicationEffectiveness { get; set; }
}

/// <summary>
/// Productivity assessment results
/// </summary>
public class ProductivityAssessment
{
    public Dictionary<string, double> VelocityTrends { get; set; } = new();
    public Dictionary<string, double> QualityScores { get; set; } = new();
    public Dictionary<string, double> DeliveryConsistency { get; set; } = new();
    public double TeamProductivity { get; set; }
    public List<string> HighPerformers { get; set; } = new();
    public List<string> ImprovementCandidates { get; set; } = new();
    public Dictionary<string, double> EfficiencyRatios { get; set; } = new();
}

/// <summary>
/// Team risk analysis results
/// </summary>
public class TeamRiskAnalysis
{
    public double BusFactor { get; set; }
    public Dictionary<string, double> SinglePointsOfFailure { get; set; } = new();
    public Dictionary<string, double> KnowledgeConcentration { get; set; } = new();
    public List<string> CriticalContributors { get; set; } = new();
    public Dictionary<string, List<string>> KnowledgeGaps { get; set; } = new();
    public double TeamResilience { get; set; }
    public List<string> RiskMitigationRecommendations { get; set; } = new();
}

/// <summary>
/// Activity pattern recognition results
/// </summary>
public class ActivityPatternRecognition
{
    public Dictionary<string, Dictionary<DayOfWeek, double>> WorkingHours { get; set; } = new();
    public Dictionary<string, List<string>> SeasonalPatterns { get; set; } = new();
    public Dictionary<string, string> CollaborationStyles { get; set; } = new();
    public Dictionary<string, double> FocusTimeRatios { get; set; } = new();
    public Dictionary<string, List<string>> PreferredTaskTypes { get; set; } = new();
    public Dictionary<string, double> MultitaskingTendency { get; set; } = new();
}

/// <summary>
/// Growth tracking analysis results
/// </summary>
public class GrowthTrackingAnalysis
{
    public string ContributorEmail { get; set; } = string.Empty;
    public Dictionary<string, double> SkillDevelopment { get; set; } = new();
    public List<string> DomainExpansion { get; set; } = new();
    public Dictionary<DateTime, double> ImpactEvolution { get; set; } = new();
    public double LearningVelocity { get; set; }
    public List<string> EmergingSkills { get; set; } = new();
    public double MentorshipCapability { get; set; } = new();
    public List<string> GrowthRecommendations { get; set; } = new();
}
