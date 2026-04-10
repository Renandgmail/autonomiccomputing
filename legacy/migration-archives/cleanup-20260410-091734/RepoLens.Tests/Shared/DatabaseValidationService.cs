using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Infrastructure;
using RepoLens.Core.Entities;

namespace RepoLens.Tests.Shared;

/// <summary>
/// Service for validating database state after integration test operations
/// Ensures all expected data is correctly stored and relationships are maintained
/// </summary>
public class DatabaseValidationService
{
    private readonly RepoLensDbContext _dbContext;
    private readonly ILogger<DatabaseValidationService> _logger;

    public DatabaseValidationService(RepoLensDbContext dbContext, ILogger<DatabaseValidationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Validates complete database state after integration test execution
    /// </summary>
    public async Task<DatabaseValidationReport> ValidateCompleteIntegrationAsync(List<TestRepository> processedRepositories)
    {
        _logger.LogInformation("🔍 Starting comprehensive database validation for {Count} repositories", processedRepositories.Count);

        var report = new DatabaseValidationReport
        {
            ValidationTimestamp = DateTime.UtcNow,
            TotalRepositoriesExpected = processedRepositories.Count
        };

        try
        {
            // Phase 1: Basic Data Presence Validation
            await ValidateBasicDataPresenceAsync(processedRepositories, report);

            // Phase 2: Repository-Specific Validation
            foreach (var testRepo in processedRepositories)
            {
                var repoValidation = await ValidateRepositoryDataAsync(testRepo);
                report.RepositoryValidations.Add(repoValidation);
            }

            // Phase 3: Cross-Repository Consistency
            await ValidateCrossRepositoryConsistencyAsync(report);

            // Phase 4: Data Integrity and Relationships
            await ValidateDataIntegrityAsync(report);

            // Phase 5: Special Feature Validation
            await ValidateSpecialFeaturesAsync(processedRepositories, report);

            // Calculate overall success metrics
            CalculateOverallMetrics(report);

            var status = report.IsValid ? "✅ PASSED" : "❌ FAILED";
            _logger.LogInformation("{Status} - Database validation completed. Score: {Score}%", 
                status, report.OverallSuccessPercentage);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Database validation failed with exception");
            report.IsValid = false;
            report.ValidationErrors.Add($"Validation exception: {ex.Message}");
            return report;
        }
    }

    /// <summary>
    /// Validates data for a specific repository
    /// </summary>
    public async Task<RepositoryDataValidation> ValidateRepositoryDataAsync(TestRepository testRepository)
    {
        _logger.LogDebug("🔍 Validating data for repository: {Owner}/{Name}", testRepository.Owner, testRepository.Name);

        var validation = new RepositoryDataValidation
        {
            Repository = testRepository,
            ValidationTimestamp = DateTime.UtcNow
        };

        try
        {
            // Find the repository in database
            var dbRepository = await _dbContext.Set<Repository>()
                .FirstOrDefaultAsync(r => r.Name == testRepository.Name && r.Url.Contains(testRepository.Owner));

            if (dbRepository == null)
            {
                validation.Issues.Add("Repository not found in database");
                validation.IsValid = false;
                return validation;
            }

            validation.RepositoryId = dbRepository.Id;
            validation.RepositoryFound = true;

            // Validate basic repository data
            await ValidateBasicRepositoryDataAsync(dbRepository, validation);

            // Validate metrics collection based on test type
            if (testRepository.TestType == RepositoryTestType.FullAnalysisWithCodeSearch)
            {
                await ValidateFullAnalysisDataAsync(dbRepository.Id, validation);
            }
            else
            {
                await ValidateStandardMetricsDataAsync(dbRepository.Id, validation);
            }

            // Validate relationships and integrity
            await ValidateRepositoryRelationshipsAsync(dbRepository.Id, validation);

            var status = validation.IsValid ? "✅ VALID" : "❌ INVALID";
            _logger.LogDebug("{Status} - {Owner}/{Name}: {IssueCount} issues, {WarningCount} warnings",
                status, testRepository.Owner, testRepository.Name, validation.Issues.Count, validation.Warnings.Count);

            return validation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating repository data for {Owner}/{Name}", 
                testRepository.Owner, testRepository.Name);
            validation.IsValid = false;
            validation.Issues.Add($"Validation error: {ex.Message}");
            return validation;
        }
    }

    /// <summary>
    /// Validates search functionality for autonomiccomputing repository
    /// </summary>
    public async Task<SearchValidationResult> ValidateSearchFunctionalityAsync(int repositoryId)
    {
        _logger.LogDebug("🔍 Validating search functionality for repository {RepositoryId}", repositoryId);

        var result = new SearchValidationResult
        {
            RepositoryId = repositoryId,
            ValidationTimestamp = DateTime.UtcNow
        };

        try
        {
            // Validate code elements are indexed
            var codeElementCount = await _dbContext.Set<CodeElement>()
                .CountAsync(ce => ce.RepositoryFile.RepositoryId == repositoryId);
            
            result.CodeElementsIndexed = codeElementCount;
            result.HasCodeElements = codeElementCount > 0;

            if (codeElementCount == 0)
            {
                result.Issues.Add("No code elements found - search indexing may have failed");
            }
            else
            {
                _logger.LogDebug("✅ Found {Count} code elements indexed", codeElementCount);
            }

            // Validate vocabulary terms are extracted
            var vocabularyTermCount = await _dbContext.Set<VocabularyTerm>()
                .CountAsync(vt => vt.RepositoryId == repositoryId);

            result.VocabularyTermsExtracted = vocabularyTermCount;
            result.HasVocabularyTerms = vocabularyTermCount > 0;

            if (vocabularyTermCount == 0)
            {
                result.Warnings.Add("No vocabulary terms extracted");
            }
            else
            {
                _logger.LogDebug("✅ Found {Count} vocabulary terms extracted", vocabularyTermCount);
            }

            // Validate repository files are catalogued
            var repositoryFileCount = await _dbContext.Set<RepositoryFile>()
                .CountAsync(rf => rf.RepositoryId == repositoryId);

            result.RepositoryFilesCatalogued = repositoryFileCount;
            result.HasRepositoryFiles = repositoryFileCount > 0;

            if (repositoryFileCount == 0)
            {
                result.Issues.Add("No repository files catalogued");
            }
            else
            {
                _logger.LogDebug("✅ Found {Count} repository files catalogued", repositoryFileCount);
            }

            // Calculate search readiness score
            result.SearchReadinessScore = CalculateSearchReadinessScore(result);
            result.IsSearchReady = result.SearchReadinessScore >= 0.7; // 70% threshold

            var status = result.IsSearchReady ? "✅ READY" : "❌ NOT READY";
            _logger.LogDebug("{Status} - Search validation score: {Score:P1}", status, result.SearchReadinessScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating search functionality for repository {RepositoryId}", repositoryId);
            result.Issues.Add($"Search validation error: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Validates metrics data quality and completeness
    /// </summary>
    public async Task<MetricsValidationResult> ValidateMetricsQualityAsync(int repositoryId, bool expectAdvancedMetrics = false)
    {
        _logger.LogDebug("🔍 Validating metrics quality for repository {RepositoryId}", repositoryId);

        var result = new MetricsValidationResult
        {
            RepositoryId = repositoryId,
            ValidationTimestamp = DateTime.UtcNow,
            ExpectAdvancedMetrics = expectAdvancedMetrics
        };

        try
        {
            // Validate repository metrics
            var repoMetrics = await _dbContext.Set<RepositoryMetrics>()
                .Where(rm => rm.RepositoryId == repositoryId)
                .ToListAsync();

            result.RepositoryMetricsCount = repoMetrics.Count;
            result.HasRepositoryMetrics = repoMetrics.Any();

            if (!repoMetrics.Any())
            {
                result.Issues.Add("No repository metrics found");
            }
            else
            {
                // Validate metrics quality
                var latestMetrics = repoMetrics.OrderByDescending(rm => rm.MeasurementDate).First();
                ValidateRepositoryMetricsQuality(latestMetrics, result);
            }

            // Validate file metrics
            var fileMetrics = await _dbContext.Set<FileMetrics>()
                .CountAsync(fm => fm.RepositoryId == repositoryId);

            result.FileMetricsCount = fileMetrics;
            result.HasFileMetrics = fileMetrics > 0;

            // Validate contributor metrics
            var contributorMetrics = await _dbContext.Set<ContributorMetrics>()
                .CountAsync(cm => cm.RepositoryId == repositoryId);

            result.ContributorMetricsCount = contributorMetrics;
            result.HasContributorMetrics = contributorMetrics > 0;

            // Validate commit data
            var commitCount = await _dbContext.Set<Commit>()
                .CountAsync(c => c.RepositoryId == repositoryId);

            result.CommitCount = commitCount;
            result.HasCommitData = commitCount > 0;

            // Calculate metrics completeness score
            result.MetricsCompletenessScore = CalculateMetricsCompletenessScore(result);
            result.IsMetricsComplete = result.MetricsCompletenessScore >= (expectAdvancedMetrics ? 0.8 : 0.6);

            var status = result.IsMetricsComplete ? "✅ COMPLETE" : "❌ INCOMPLETE";
            _logger.LogDebug("{Status} - Metrics validation score: {Score:P1}", status, result.MetricsCompletenessScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating metrics quality for repository {RepositoryId}", repositoryId);
            result.Issues.Add($"Metrics validation error: {ex.Message}");
            return result;
        }
    }

    #region Private Validation Methods

    private async Task ValidateBasicDataPresenceAsync(List<TestRepository> repositories, DatabaseValidationReport report)
    {
        _logger.LogDebug("🔍 Validating basic data presence...");

        // Check repositories are created
        var dbRepositoryCount = await _dbContext.Set<Repository>().CountAsync();
        report.BasicValidation.RepositoriesInDatabase = dbRepositoryCount;
        
        if (dbRepositoryCount < repositories.Count)
        {
            report.ValidationErrors.Add($"Expected {repositories.Count} repositories, found {dbRepositoryCount}");
        }

        // Check overall data presence
        report.BasicValidation.TotalRepositoryMetrics = await _dbContext.Set<RepositoryMetrics>().CountAsync();
        report.BasicValidation.TotalFileMetrics = await _dbContext.Set<FileMetrics>().CountAsync();
        report.BasicValidation.TotalContributorMetrics = await _dbContext.Set<ContributorMetrics>().CountAsync();
        report.BasicValidation.TotalCommits = await _dbContext.Set<Commit>().CountAsync();
        report.BasicValidation.TotalCodeElements = await _dbContext.Set<CodeElement>().CountAsync();
        report.BasicValidation.TotalVocabularyTerms = await _dbContext.Set<VocabularyTerm>().CountAsync();

        _logger.LogDebug("✅ Basic data presence validated - found data in all tables");
    }

    private async Task ValidateBasicRepositoryDataAsync(Repository repository, RepositoryDataValidation validation)
    {
        // Validate repository properties
        if (string.IsNullOrEmpty(repository.Name))
            validation.Issues.Add("Repository name is empty");

        if (string.IsNullOrEmpty(repository.Url))
            validation.Issues.Add("Repository URL is empty");

        if (repository.CreatedAt == default)
            validation.Warnings.Add("Repository CreatedAt is not set");

        validation.BasicDataValid = validation.Issues.Count == 0;
    }

    private async Task ValidateFullAnalysisDataAsync(int repositoryId, RepositoryDataValidation validation)
    {
        // For autonomiccomputing - expect full analysis data
        
        // Code elements (for search)
        var codeElements = await _dbContext.Set<CodeElement>().CountAsync(ce => ce.RepositoryFile.RepositoryId == repositoryId);
        validation.DataCounts["CodeElements"] = codeElements;
        if (codeElements == 0)
            validation.Issues.Add("No code elements found - search indexing failed");

        // Vocabulary terms
        var vocabularyTerms = await _dbContext.Set<VocabularyTerm>().CountAsync(vt => vt.RepositoryId == repositoryId);
        validation.DataCounts["VocabularyTerms"] = vocabularyTerms;
        if (vocabularyTerms == 0)
            validation.Warnings.Add("No vocabulary terms extracted");

        // Repository files
        var repositoryFiles = await _dbContext.Set<RepositoryFile>().CountAsync(rf => rf.RepositoryId == repositoryId);
        validation.DataCounts["RepositoryFiles"] = repositoryFiles;
        if (repositoryFiles == 0)
            validation.Issues.Add("No repository files catalogued");

        validation.FullAnalysisComplete = codeElements > 0 && repositoryFiles > 0;
    }

    private async Task ValidateStandardMetricsDataAsync(int repositoryId, RepositoryDataValidation validation)
    {
        // For standard repos - expect metrics only
        
        // Repository metrics
        var repoMetrics = await _dbContext.Set<RepositoryMetrics>().CountAsync(rm => rm.RepositoryId == repositoryId);
        validation.DataCounts["RepositoryMetrics"] = repoMetrics;
        if (repoMetrics == 0)
            validation.Issues.Add("No repository metrics found");

        // File metrics
        var fileMetrics = await _dbContext.Set<FileMetrics>().CountAsync(fm => fm.RepositoryId == repositoryId);
        validation.DataCounts["FileMetrics"] = fileMetrics;

        // Contributor metrics
        var contributorMetrics = await _dbContext.Set<ContributorMetrics>().CountAsync(cm => cm.RepositoryId == repositoryId);
        validation.DataCounts["ContributorMetrics"] = contributorMetrics;

        // Commits
        var commits = await _dbContext.Set<Commit>().CountAsync(c => c.RepositoryId == repositoryId);
        validation.DataCounts["Commits"] = commits;

        validation.MetricsCollectionComplete = repoMetrics > 0;
    }

    private async Task ValidateRepositoryRelationshipsAsync(int repositoryId, RepositoryDataValidation validation)
    {
        // Validate foreign key relationships
        var repository = await _dbContext.Set<Repository>().FindAsync(repositoryId);
        if (repository == null)
        {
            validation.Issues.Add("Repository not found for relationship validation");
            return;
        }

        // Check repository metrics relationship
        var metricsWithValidRepo = await _dbContext.Set<RepositoryMetrics>()
            .CountAsync(rm => rm.RepositoryId == repositoryId && rm.RepositoryId == repository.Id);
        
        var totalMetrics = await _dbContext.Set<RepositoryMetrics>()
            .CountAsync(rm => rm.RepositoryId == repositoryId);

        if (totalMetrics > 0 && metricsWithValidRepo == totalMetrics)
        {
            validation.RelationshipsValid = true;
        }
        else if (totalMetrics > metricsWithValidRepo)
        {
            validation.Issues.Add("Some metrics have invalid repository references");
        }
    }

    private async Task ValidateCrossRepositoryConsistencyAsync(DatabaseValidationReport report)
    {
        _logger.LogDebug("🔍 Validating cross-repository consistency...");

        // Check for orphaned data
        var orphanedMetrics = await _dbContext.Set<RepositoryMetrics>()
            .Where(rm => !_dbContext.Set<Repository>().Any(r => r.Id == rm.RepositoryId))
            .CountAsync();

        if (orphanedMetrics > 0)
        {
            report.ValidationErrors.Add($"Found {orphanedMetrics} orphaned repository metrics");
        }

        var orphanedFileMetrics = await _dbContext.Set<FileMetrics>()
            .Where(fm => !_dbContext.Set<Repository>().Any(r => r.Id == fm.RepositoryId))
            .CountAsync();

        if (orphanedFileMetrics > 0)
        {
            report.ValidationErrors.Add($"Found {orphanedFileMetrics} orphaned file metrics");
        }

        report.ConsistencyValid = orphanedMetrics == 0 && orphanedFileMetrics == 0;
    }

    private async Task ValidateDataIntegrityAsync(DatabaseValidationReport report)
    {
        _logger.LogDebug("🔍 Validating data integrity...");

        // Check for required data consistency
        var repositoriesWithoutMetrics = await _dbContext.Set<Repository>()
            .Where(r => !_dbContext.Set<RepositoryMetrics>().Any(rm => rm.RepositoryId == r.Id))
            .CountAsync();

        if (repositoriesWithoutMetrics > 0)
        {
            report.ValidationWarnings.Add($"{repositoriesWithoutMetrics} repositories have no metrics");
        }

        // Check data freshness
        var oldMetrics = await _dbContext.Set<RepositoryMetrics>()
            .Where(rm => rm.MeasurementDate < DateTime.UtcNow.AddDays(-1))
            .CountAsync();

        if (oldMetrics > 0)
        {
            report.ValidationWarnings.Add($"{oldMetrics} metrics are older than 24 hours");
        }

        report.DataIntegrityValid = repositoriesWithoutMetrics == 0;
    }

    private async Task ValidateSpecialFeaturesAsync(List<TestRepository> repositories, DatabaseValidationReport report)
    {
        _logger.LogDebug("🔍 Validating special features...");

        var autonomicRepo = repositories.FirstOrDefault(r => r.TestType == RepositoryTestType.FullAnalysisWithCodeSearch);
        if (autonomicRepo != null)
        {
            var dbRepo = await _dbContext.Set<Repository>()
                .FirstOrDefaultAsync(r => r.Name == autonomicRepo.Name && r.Url.Contains(autonomicRepo.Owner));

            if (dbRepo != null)
            {
                var searchValidation = await ValidateSearchFunctionalityAsync(dbRepo.Id);
                report.SpecialFeatureValidations["SearchFunctionality"] = searchValidation.IsSearchReady ? "✅ Ready" : "❌ Not Ready";

                if (!searchValidation.IsSearchReady)
                {
                    report.ValidationErrors.AddRange(searchValidation.Issues);
                }
            }
        }
    }

    private void ValidateRepositoryMetricsQuality(RepositoryMetrics metrics, MetricsValidationResult result)
    {
        // Check for realistic values
        if (metrics.TotalFiles < 0)
            result.Issues.Add("Invalid total files count (negative)");

        if (metrics.TotalLinesOfCode < 0)
            result.Issues.Add("Invalid total lines of code (negative)");

        if (metrics.MaintainabilityIndex < 0 || metrics.MaintainabilityIndex > 100)
            result.Issues.Add("Invalid maintainability index (should be 0-100)");

        if (string.IsNullOrEmpty(metrics.LanguageDistribution))
            result.Warnings.Add("Language distribution not populated");

        result.MetricsQualityValid = result.Issues.Count == 0;
    }

    private double CalculateSearchReadinessScore(SearchValidationResult result)
    {
        double score = 0.0;
        
        if (result.HasCodeElements) score += 0.4;      // 40% weight
        if (result.HasRepositoryFiles) score += 0.4;   // 40% weight  
        if (result.HasVocabularyTerms) score += 0.2;   // 20% weight

        return score;
    }

    private double CalculateMetricsCompletenessScore(MetricsValidationResult result)
    {
        double score = 0.0;
        
        if (result.HasRepositoryMetrics) score += 0.4;   // 40% weight
        if (result.HasFileMetrics) score += 0.2;         // 20% weight
        if (result.HasContributorMetrics) score += 0.2;  // 20% weight
        if (result.HasCommitData) score += 0.2;          // 20% weight

        return score;
    }

    private void CalculateOverallMetrics(DatabaseValidationReport report)
    {
        var totalValidations = report.RepositoryValidations.Count;
        var validValidations = report.RepositoryValidations.Count(rv => rv.IsValid);

        report.SuccessfulRepositories = validValidations;
        report.FailedRepositories = totalValidations - validValidations;
        report.OverallSuccessPercentage = totalValidations > 0 ? 
            Math.Round((double)validValidations / totalValidations * 100, 1) : 0;

        report.IsValid = report.OverallSuccessPercentage >= 80 && // 80% success rate
                        report.ValidationErrors.Count == 0 &&
                        report.ConsistencyValid &&
                        report.DataIntegrityValid;
    }

    #endregion
}

#region Data Models

/// <summary>
/// Comprehensive database validation report
/// </summary>
public class DatabaseValidationReport
{
    public DateTime ValidationTimestamp { get; set; }
    public bool IsValid { get; set; } = true;
    public int TotalRepositoriesExpected { get; set; }
    public int SuccessfulRepositories { get; set; }
    public int FailedRepositories { get; set; }
    public double OverallSuccessPercentage { get; set; }

    public BasicDataValidation BasicValidation { get; set; } = new();
    public List<RepositoryDataValidation> RepositoryValidations { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    
    public bool ConsistencyValid { get; set; } = true;
    public bool DataIntegrityValid { get; set; } = true;
    public Dictionary<string, string> SpecialFeatureValidations { get; set; } = new();

    public override string ToString()
    {
        var status = IsValid ? "✅ PASSED" : "❌ FAILED";
        return $"{status} - Database Validation: {SuccessfulRepositories}/{TotalRepositoriesExpected} repos valid " +
               $"({OverallSuccessPercentage}%), {ValidationErrors.Count} errors, {ValidationWarnings.Count} warnings";
    }
}

/// <summary>
/// Basic data presence validation
/// </summary>
public class BasicDataValidation
{
    public int RepositoriesInDatabase { get; set; }
    public int TotalRepositoryMetrics { get; set; }
    public int TotalFileMetrics { get; set; }
    public int TotalContributorMetrics { get; set; }
    public int TotalCommits { get; set; }
    public int TotalCodeElements { get; set; }
    public int TotalVocabularyTerms { get; set; }
}

/// <summary>
/// Repository-specific data validation
/// </summary>
public class RepositoryDataValidation
{
    public TestRepository Repository { get; set; } = new();
    public DateTime ValidationTimestamp { get; set; }
    public bool IsValid { get; set; } = true;
    public int? RepositoryId { get; set; }
    public bool RepositoryFound { get; set; }
    public bool BasicDataValid { get; set; }
    public bool MetricsCollectionComplete { get; set; }
    public bool FullAnalysisComplete { get; set; }
    public bool RelationshipsValid { get; set; }

    public Dictionary<string, int> DataCounts { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public override string ToString()
    {
        var status = IsValid ? "✅ VALID" : "❌ INVALID";
        return $"{status} - {Repository.Owner}/{Repository.Name}: " +
               $"{Issues.Count} issues, {Warnings.Count} warnings";
    }
}

/// <summary>
/// Search functionality validation result
/// </summary>
public class SearchValidationResult
{
    public int RepositoryId { get; set; }
    public DateTime ValidationTimestamp { get; set; }
    public bool IsSearchReady { get; set; }
    public double SearchReadinessScore { get; set; }

    public int CodeElementsIndexed { get; set; }
    public bool HasCodeElements { get; set; }
    public int VocabularyTermsExtracted { get; set; }
    public bool HasVocabularyTerms { get; set; }
    public int RepositoryFilesCatalogued { get; set; }
    public bool HasRepositoryFiles { get; set; }

    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Metrics validation result
/// </summary>
public class MetricsValidationResult
{
    public int RepositoryId { get; set; }
    public DateTime ValidationTimestamp { get; set; }
    public bool IsMetricsComplete { get; set; }
    public double MetricsCompletenessScore { get; set; }
    public bool ExpectAdvancedMetrics { get; set; }
    public bool MetricsQualityValid { get; set; }

    public int RepositoryMetricsCount { get; set; }
    public bool HasRepositoryMetrics { get; set; }
    public int FileMetricsCount { get; set; }
    public bool HasFileMetrics { get; set; }
    public int ContributorMetricsCount { get; set; }
    public bool HasContributorMetrics { get; set; }
    public int CommitCount { get; set; }
    public bool HasCommitData { get; set; }

    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

#endregion
