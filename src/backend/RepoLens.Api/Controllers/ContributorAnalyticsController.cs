using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for contributor analytics and team intelligence operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContributorAnalyticsController : ControllerBase
{
    private readonly IContributorAnalyticsService _contributorAnalyticsService;
    private readonly ILogger<ContributorAnalyticsController> _logger;

    public ContributorAnalyticsController(
        IContributorAnalyticsService contributorAnalyticsService,
        ILogger<ContributorAnalyticsController> logger)
    {
        _contributorAnalyticsService = contributorAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate comprehensive contributor metrics for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of contributor metrics with detailed analysis</returns>
    /// <response code="200">Returns contributor metrics with comprehensive analytics</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error during analysis</response>
    [HttpGet("repositories/{repositoryId:int}/contributors/metrics")]
    public async Task<IActionResult> GetContributorMetrics(
        [FromRoute] int repositoryId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating contributor metrics for repository {RepositoryId}", repositoryId);

            var metrics = await _contributorAnalyticsService.CalculateContributorMetricsAsync(repositoryId, cancellationToken);

            if (!metrics.Any())
            {
                _logger.LogInformation("No contributor data found for repository {RepositoryId}", repositoryId);
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    repositoryId = repositoryId,
                    contributors = new List<object>(),
                    summary = new
                    {
                        totalContributors = 0,
                        totalCommits = 0,
                        message = "No contributor data available for this repository"
                    }
                }));
            }

            var response = new
            {
                repositoryId = repositoryId,
                contributors = metrics.Select(m => new
                {
                    email = m.ContributorEmail,
                    name = m.ContributorName,
                    periodStart = m.PeriodStart,
                    periodEnd = m.PeriodEnd,
                    commitCount = m.CommitCount,
                    linesAdded = m.LinesAdded,
                    linesDeleted = m.LinesDeleted,
                    linesModified = m.LinesModified,
                    filesModified = m.FilesModified,
                    filesAdded = m.FilesAdded,
                    filesDeleted = m.FilesDeleted,
                    contributionPercentage = Math.Round(m.ContributionPercentage, 2),
                    workingDays = m.WorkingDays,
                    averageCommitSize = Math.Round(m.AverageCommitSize, 1),
                    commitFrequency = Math.Round(m.CommitFrequency, 3),
                    longestCommitStreak = m.LongestCommitStreak,
                    currentCommitStreak = m.CurrentCommitStreak,
                    ownedFiles = m.OwnedFiles,
                    codeOwnershipPercentage = Math.Round(m.CodeOwnershipPercentage, 2),
                    uniqueFilesTouched = m.UniqueFilesTouched,
                    pullRequestsCreated = m.PullRequestsCreated,
                    pullRequestsReviewed = m.PullRequestsReviewed,
                    codeReviewComments = m.CodeReviewComments,
                    issuesCreated = m.IssuesCreated,
                    issuesResolved = m.IssuesResolved,
                    mentionedInCommits = m.MentionedInCommits,
                    hourlyActivityPattern = m.HourlyActivityPattern,
                    weeklyActivityPattern = m.WeeklyActivityPattern,
                    monthlyActivityPattern = m.MonthlyActivityPattern,
                    languageContributions = m.LanguageContributions,
                    fileTypeContributions = m.FileTypeContributions,
                    avgCommitMessageLength = Math.Round(m.AvgCommitMessageLength, 1),
                    commitMessageQualityScore = Math.Round(m.CommitMessageQualityScore, 1),
                    bugFixCommits = m.BugFixCommits,
                    featureCommits = m.FeatureCommits,
                    refactoringCommits = m.RefactoringCommits,
                    documentationCommits = m.DocumentationCommits,
                    isCoreContributor = m.IsCoreContributor,
                    isNewContributor = m.IsNewContributor,
                    firstContribution = m.FirstContribution,
                    lastContribution = m.LastContribution,
                    daysActive = m.DaysActive,
                    retentionScore = Math.Round(m.RetentionScore, 3)
                }),
                summary = new
                {
                    totalContributors = metrics.Count,
                    totalCommits = metrics.Sum(m => m.CommitCount),
                    totalLinesAdded = metrics.Sum(m => m.LinesAdded),
                    totalLinesDeleted = metrics.Sum(m => m.LinesDeleted),
                    averageCommitsPerContributor = Math.Round(metrics.Average(m => m.CommitCount), 1),
                    coreContributors = metrics.Count(m => m.IsCoreContributor),
                    newContributors = metrics.Count(m => m.IsNewContributor),
                    averageRetentionScore = Math.Round(metrics.Average(m => m.RetentionScore), 3)
                }
            };

            _logger.LogInformation("Successfully calculated metrics for {ContributorCount} contributors in repository {RepositoryId}", 
                metrics.Count, repositoryId);

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating contributor metrics for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to calculate contributor metrics"));
        }
    }

    /// <summary>
    /// Analyze patterns for a specific contributor
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="contributorEmail">Contributor email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed pattern analysis for the contributor</returns>
    /// <response code="200">Returns contributor pattern analysis</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository or contributor not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/contributors/{contributorEmail}/patterns")]
    public async Task<IActionResult> AnalyzeContributorPatterns(
        [FromRoute] int repositoryId,
        [FromRoute] string contributorEmail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contributorEmail))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Contributor email is required"));
            }

            _logger.LogInformation("Analyzing patterns for contributor {Email} in repository {RepositoryId}", 
                contributorEmail, repositoryId);

            var patterns = await _contributorAnalyticsService.AnalyzeContributorPatternsAsync(
                repositoryId, contributorEmail, cancellationToken);

            var response = new
            {
                contributorEmail = patterns.ContributorEmail,
                commitFrequency = Math.Round(patterns.CommitFrequency, 3),
                codeOwnership = patterns.CodeOwnership.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                expertiseAreas = patterns.ExpertiseAreas,
                weeklyPattern = patterns.WeeklyPattern,
                hourlyPattern = patterns.HourlyPattern,
                consistencyScore = Math.Round(patterns.ConsistencyScore, 3),
                preferredFileTypes = patterns.PreferredFileTypes
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing patterns for contributor {Email} in repository {RepositoryId}", 
                contributorEmail, repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to analyze contributor patterns"));
        }
    }

    /// <summary>
    /// Analyze team collaboration metrics for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Team collaboration analysis with network effects</returns>
    /// <response code="200">Returns team collaboration metrics</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/team/collaboration")]
    public async Task<IActionResult> AnalyzeTeamCollaboration(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing team collaboration for repository {RepositoryId}", repositoryId);

            var collaboration = await _contributorAnalyticsService.AnalyzeTeamCollaborationAsync(
                repositoryId, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                totalContributors = collaboration.TotalContributors,
                collaborationIndex = Math.Round(collaboration.CollaborationIndex, 3),
                codeReviewNetwork = collaboration.CodeReviewNetwork,
                knowledgeSharingScore = collaboration.KnowledgeSharingScore.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                teamCohesion = Math.Round(collaboration.TeamCohesion, 3),
                isolatedContributors = collaboration.IsolatedContributors,
                mentoringRelationships = collaboration.MentoringRelationships,
                communicationEffectiveness = Math.Round(collaboration.CommunicationEffectiveness, 3)
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing team collaboration for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to analyze team collaboration"));
        }
    }

    /// <summary>
    /// Assess team productivity over a specified time frame
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="timeFrameDays">Time frame in days (default: 30, max: 365)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive productivity assessment</returns>
    /// <response code="200">Returns productivity assessment</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/team/productivity")]
    public async Task<IActionResult> AssessProductivity(
        [FromRoute] int repositoryId,
        [FromQuery] int timeFrameDays = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (timeFrameDays <= 0 || timeFrameDays > 365)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Time frame must be between 1 and 365 days"));
            }

            _logger.LogInformation("Assessing productivity for repository {RepositoryId} over {Days} days", 
                repositoryId, timeFrameDays);

            var timeFrame = TimeSpan.FromDays(timeFrameDays);
            var productivity = await _contributorAnalyticsService.AssessProductivityAsync(
                repositoryId, timeFrame, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                timeFrameDays = timeFrameDays,
                velocityTrends = productivity.VelocityTrends.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 2)
                ),
                qualityScores = productivity.QualityScores.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                deliveryConsistency = productivity.DeliveryConsistency.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                teamProductivity = Math.Round(productivity.TeamProductivity, 3),
                highPerformers = productivity.HighPerformers,
                improvementCandidates = productivity.ImprovementCandidates,
                efficiencyRatios = productivity.EfficiencyRatios.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                )
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing productivity for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to assess productivity"));
        }
    }

    /// <summary>
    /// Analyze team risks including bus factor and knowledge concentration
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive team risk analysis</returns>
    /// <response code="200">Returns team risk analysis</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/team/risks")]
    public async Task<IActionResult> AnalyzeTeamRisks(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing team risks for repository {RepositoryId}", repositoryId);

            var risks = await _contributorAnalyticsService.AnalyzeTeamRisksAsync(repositoryId, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                busFactor = Math.Round(risks.BusFactor, 1),
                singlePointsOfFailure = risks.SinglePointsOfFailure.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                knowledgeConcentration = risks.KnowledgeConcentration.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                criticalContributors = risks.CriticalContributors,
                knowledgeGaps = risks.KnowledgeGaps,
                teamResilience = Math.Round(risks.TeamResilience, 3),
                riskMitigationRecommendations = risks.RiskMitigationRecommendations,
                riskLevel = risks.BusFactor <= 2 ? "High" : risks.BusFactor <= 4 ? "Medium" : "Low"
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing team risks for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to analyze team risks"));
        }
    }

    /// <summary>
    /// Recognize activity patterns for contributors in a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activity pattern recognition results</returns>
    /// <response code="200">Returns activity pattern analysis</response>
    /// <response code="400">Invalid repository ID</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/team/activity-patterns")]
    public async Task<IActionResult> RecognizeActivityPatterns(
        [FromRoute] int repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Recognizing activity patterns for repository {RepositoryId}", repositoryId);

            var patterns = await _contributorAnalyticsService.RecognizeActivityPatternsAsync(
                repositoryId, cancellationToken);

            var response = new
            {
                repositoryId = repositoryId,
                workingHours = patterns.WorkingHours,
                seasonalPatterns = patterns.SeasonalPatterns,
                collaborationStyles = patterns.CollaborationStyles,
                focusTimeRatios = patterns.FocusTimeRatios.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                preferredTaskTypes = patterns.PreferredTaskTypes,
                multitaskingTendency = patterns.MultitaskingTendency.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                )
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing activity patterns for repository {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to recognize activity patterns"));
        }
    }

    /// <summary>
    /// Track growth and skill development for a specific contributor
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="contributorEmail">Contributor email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Growth tracking analysis with skill development insights</returns>
    /// <response code="200">Returns growth tracking analysis</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Repository or contributor not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("repositories/{repositoryId:int}/contributors/{contributorEmail}/growth")]
    public async Task<IActionResult> TrackGrowth(
        [FromRoute] int repositoryId,
        [FromRoute] string contributorEmail,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contributorEmail))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Contributor email is required"));
            }

            _logger.LogInformation("Tracking growth for contributor {Email} in repository {RepositoryId}", 
                contributorEmail, repositoryId);

            var growth = await _contributorAnalyticsService.TrackGrowthAsync(
                repositoryId, contributorEmail, cancellationToken);

            var response = new
            {
                contributorEmail = growth.ContributorEmail,
                repositoryId = repositoryId,
                skillDevelopment = growth.SkillDevelopment.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => Math.Round(kvp.Value, 3)
                ),
                domainExpansion = growth.DomainExpansion,
                impactEvolution = growth.ImpactEvolution,
                learningVelocity = Math.Round(growth.LearningVelocity, 3),
                emergingSkills = growth.EmergingSkills,
                mentorshipCapability = Math.Round(growth.MentorshipCapability, 3),
                growthRecommendations = growth.GrowthRecommendations
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking growth for contributor {Email} in repository {RepositoryId}", 
                contributorEmail, repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to track growth"));
        }
    }
}
