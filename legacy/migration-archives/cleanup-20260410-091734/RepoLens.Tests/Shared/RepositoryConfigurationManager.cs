using System.Text.Json;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;

namespace RepoLens.Tests.Shared;

/// <summary>
/// Manages repository configurations for comprehensive integration testing
/// Handles special cases like autonomiccomputing repo vs standard repos
/// </summary>
public class RepositoryConfigurationManager
{
    private readonly ILogger<RepositoryConfigurationManager> _logger;
    private readonly string _configurationFilePath;
    private TestRepositoryConfiguration? _configuration;

    public RepositoryConfigurationManager(ILogger<RepositoryConfigurationManager> logger, string? configFilePath = null)
    {
        _logger = logger;
        _configurationFilePath = configFilePath ?? "testRepositories.json";
    }

    /// <summary>
    /// Loads repository configurations from JSON file
    /// </summary>
    public async Task<TestRepositoryConfiguration> LoadConfigurationAsync()
    {
        if (_configuration != null)
            return _configuration;

        try
        {
            _logger.LogInformation("📋 Loading repository configuration from {FilePath}", _configurationFilePath);

            if (!File.Exists(_configurationFilePath))
            {
                _logger.LogWarning("⚠️ Configuration file not found: {FilePath}", _configurationFilePath);
                return CreateDefaultConfiguration();
            }

            var jsonContent = await File.ReadAllTextAsync(_configurationFilePath);
            _configuration = JsonSerializer.Deserialize<TestRepositoryConfiguration>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (_configuration == null)
            {
                _logger.LogError("❌ Failed to deserialize configuration file");
                return CreateDefaultConfiguration();
            }

            _logger.LogInformation("✅ Loaded configuration with {Count} repository sets", 
                _configuration.RepositoryConfigurations.Count);

            // Add the special autonomiccomputing repository
            AddAutonomicComputingRepository();

            return _configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading repository configuration");
            return CreateDefaultConfiguration();
        }
    }

    /// <summary>
    /// Gets repositories for comprehensive integration testing
    /// </summary>
    public async Task<List<TestRepository>> GetIntegrationTestRepositoriesAsync()
    {
        var config = await LoadConfigurationAsync();
        var repositories = new List<TestRepository>();

        // Add autonomiccomputing repository first (special handling)
        repositories.Add(GetAutonomicComputingRepository());

        // Add standard repositories for metrics testing
        var standardRepos = GetStandardRepositoriesForMetrics(config);
        repositories.AddRange(standardRepos);

        _logger.LogInformation("📊 Prepared {Count} repositories for integration testing", repositories.Count);
        _logger.LogInformation("   - 1 special repo (autonomiccomputing): Full analysis + code search");
        _logger.LogInformation("   - {StandardCount} standard repos: Metrics collection only", standardRepos.Count);

        return repositories;
    }

    /// <summary>
    /// Gets the autonomiccomputing repository with special configuration
    /// </summary>
    public TestRepository GetAutonomicComputingRepository()
    {
        return new TestRepository
        {
            Owner = "Renandgmail",
            Name = "autonomiccomputing",
            Description = "Autonomic Computing Research Project - Special Test Repository",
            Url = "https://github.com/Renandgmail/autonomiccomputing",
            TestType = RepositoryTestType.FullAnalysisWithCodeSearch,
            ProviderType = ProviderType.GitHub,
            Priority = TestPriority.High,
            ExpectedFeatures = new List<string>
            {
                "Code Search",
                "Natural Language Processing",
                "Vocabulary Extraction",
                "Complete Metrics Collection",
                "Advanced Analytics",
                "File Analysis",
                "Contributor Analytics",
                "Repository Processing",
                "Git Provider Integration",
                "Real-time Metrics",
                "Orchestrated Metrics"
            },
            TestConfiguration = new RepositoryTestConfiguration
            {
                EnableCodeSearch = true,
                EnableVocabularyExtraction = true,
                EnableAdvancedAnalytics = true,
                EnableRealTimeMetrics = true,
                EnableOrchestration = true,
                MaxAnalysisTimeMinutes = 30,
                ExpectLargeDataset = true,
                ValidateSearchIndices = true,
                ValidateVocabulary = true
            }
        };
    }

    /// <summary>
    /// Gets standard repositories for metrics collection testing
    /// </summary>
    public List<TestRepository> GetStandardRepositoriesForMetrics(TestRepositoryConfiguration? config = null)
    {
        config ??= _configuration ?? CreateDefaultConfiguration();
        
        // Get medium-sized repository set for balanced testing
        var repositorySet = config.RepositoryConfigurations.ContainsKey("medium") 
            ? config.RepositoryConfigurations["medium"]
            : config.RepositoryConfigurations["small"];

        var standardRepos = repositorySet.Repositories.Select(repo => new TestRepository
        {
            Owner = repo.Owner,
            Name = repo.Name,
            Description = repo.Description,
            Url = $"https://github.com/{repo.Owner}/{repo.Name}",
            TestType = RepositoryTestType.MetricsCollectionOnly,
            ProviderType = ProviderType.GitHub,
            Priority = TestPriority.Medium,
            ExpectedFeatures = new List<string>
            {
                "Basic Metrics Collection",
                "File Metrics",
                "Contributor Metrics",
                "Repository Analysis",
                "Git Provider Integration"
            },
            TestConfiguration = new RepositoryTestConfiguration
            {
                EnableCodeSearch = false,
                EnableVocabularyExtraction = false,
                EnableAdvancedAnalytics = true,
                EnableRealTimeMetrics = true,
                EnableOrchestration = true,
                MaxAnalysisTimeMinutes = 15,
                ExpectLargeDataset = false,
                ValidateSearchIndices = false,
                ValidateVocabulary = false
            }
        }).ToList();

        return standardRepos;
    }

    /// <summary>
    /// Validates repository configuration for testing
    /// </summary>
    public async Task<RepositoryValidationResult> ValidateRepositoryForTestingAsync(TestRepository repository)
    {
        _logger.LogDebug("🔍 Validating repository: {Owner}/{Name}", repository.Owner, repository.Name);

        var result = new RepositoryValidationResult
        {
            Repository = repository,
            IsValid = true,
            ValidationTimestamp = DateTime.UtcNow
        };

        try
        {
            // Basic URL validation
            if (string.IsNullOrEmpty(repository.Url) || !Uri.IsWellFormedUriString(repository.Url, UriKind.Absolute))
            {
                result.IsValid = false;
                result.Issues.Add("Invalid repository URL");
            }

            // Owner and name validation
            if (string.IsNullOrEmpty(repository.Owner) || string.IsNullOrEmpty(repository.Name))
            {
                result.IsValid = false;
                result.Issues.Add("Repository owner and name are required");
            }

            // Test configuration validation
            if (repository.TestConfiguration == null)
            {
                result.IsValid = false;
                result.Issues.Add("Test configuration is required");
            }
            else
            {
                // Validate special requirements for autonomiccomputing
                if (repository.TestType == RepositoryTestType.FullAnalysisWithCodeSearch)
                {
                    if (!repository.TestConfiguration.EnableCodeSearch)
                    {
                        result.IsValid = false;
                        result.Issues.Add("Code search must be enabled for full analysis");
                    }

                    if (repository.TestConfiguration.MaxAnalysisTimeMinutes < 20)
                    {
                        result.Warnings.Add("Analysis time may be insufficient for full analysis");
                    }
                }
            }

            // Check if repository is accessible (basic check)
            if (result.IsValid && repository.ProviderType == ProviderType.GitHub)
            {
                await ValidateGitHubRepositoryAccessAsync(repository, result);
            }

            var status = result.IsValid ? "✅ VALID" : "❌ INVALID";
            _logger.LogDebug("{Status} - {Owner}/{Name}: {IssueCount} issues, {WarningCount} warnings", 
                status, repository.Owner, repository.Name, result.Issues.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating repository {Owner}/{Name}", repository.Owner, repository.Name);
            result.IsValid = false;
            result.Issues.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    private async Task ValidateGitHubRepositoryAccessAsync(TestRepository repository, RepositoryValidationResult result)
    {
        try
        {
            // Simple HTTP check to see if repository is accessible
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "RepoLens-Integration-Test");
            
            var response = await httpClient.GetAsync($"https://api.github.com/repos/{repository.Owner}/{repository.Name}");
            
            if (response.IsSuccessStatusCode)
            {
                result.AccessibilityChecks.Add("✅ GitHub API accessible");
                _logger.LogDebug("✅ GitHub repository {Owner}/{Name} is accessible", repository.Owner, repository.Name);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result.Warnings.Add("Repository may be private or not found");
                _logger.LogWarning("⚠️ GitHub repository {Owner}/{Name} returned 404", repository.Owner, repository.Name);
            }
            else
            {
                result.Warnings.Add($"GitHub API returned: {response.StatusCode}");
                _logger.LogWarning("⚠️ GitHub API returned {StatusCode} for {Owner}/{Name}", 
                    response.StatusCode, repository.Owner, repository.Name);
            }
        }
        catch (Exception ex)
        {
            result.Warnings.Add($"Could not verify repository access: {ex.Message}");
            _logger.LogDebug("ℹ️ Could not verify access to {Owner}/{Name}: {Message}", 
                repository.Owner, repository.Name, ex.Message);
        }
    }

    /// <summary>
    /// Gets test execution plan based on repository configurations
    /// </summary>
    public async Task<TestExecutionPlan> CreateExecutionPlanAsync()
    {
        _logger.LogInformation("📋 Creating test execution plan...");

        var repositories = await GetIntegrationTestRepositoriesAsync();
        var plan = new TestExecutionPlan
        {
            CreatedTimestamp = DateTime.UtcNow,
            TotalRepositories = repositories.Count
        };

        // Validate all repositories
        foreach (var repo in repositories)
        {
            var validation = await ValidateRepositoryForTestingAsync(repo);
            plan.RepositoryValidations.Add(validation);

            if (validation.IsValid)
            {
                plan.ValidRepositories.Add(repo);
            }
            else
            {
                plan.InvalidRepositories.Add(repo);
            }
        }

        // Create execution phases
        plan.ExecutionPhases = CreateExecutionPhases(plan.ValidRepositories);

        // Estimate total execution time
        plan.EstimatedExecutionTimeMinutes = plan.ValidRepositories
            .Sum(r => r.TestConfiguration?.MaxAnalysisTimeMinutes ?? 10) + 15; // +15 for setup/cleanup

        _logger.LogInformation("✅ Execution plan created:");
        _logger.LogInformation("   - Total repositories: {Total}", plan.TotalRepositories);
        _logger.LogInformation("   - Valid repositories: {Valid}", plan.ValidRepositories.Count);
        _logger.LogInformation("   - Invalid repositories: {Invalid}", plan.InvalidRepositories.Count);
        _logger.LogInformation("   - Execution phases: {Phases}", plan.ExecutionPhases.Count);
        _logger.LogInformation("   - Estimated time: {Minutes} minutes", plan.EstimatedExecutionTimeMinutes);

        return plan;
    }

    private List<TestExecutionPhase> CreateExecutionPhases(List<TestRepository> validRepositories)
    {
        var phases = new List<TestExecutionPhase>();

        // Phase 1: Database Setup
        phases.Add(new TestExecutionPhase
        {
            Name = "Database Setup",
            Description = "Clean database and prepare for testing",
            Order = 1,
            Repositories = new List<TestRepository>(),
            EstimatedTimeMinutes = 5
        });

        // Phase 2: Autonomic Computing (Special)
        var autonomicRepo = validRepositories.FirstOrDefault(r => 
            r.TestType == RepositoryTestType.FullAnalysisWithCodeSearch);
        
        if (autonomicRepo != null)
        {
            phases.Add(new TestExecutionPhase
            {
                Name = "Autonomic Computing Analysis",
                Description = "Full analysis with code search for autonomiccomputing repository",
                Order = 2,
                Repositories = new List<TestRepository> { autonomicRepo },
                EstimatedTimeMinutes = autonomicRepo.TestConfiguration?.MaxAnalysisTimeMinutes ?? 30
            });
        }

        // Phase 3: Standard Repositories (Metrics Only)
        var standardRepos = validRepositories.Where(r => 
            r.TestType == RepositoryTestType.MetricsCollectionOnly).ToList();

        if (standardRepos.Any())
        {
            phases.Add(new TestExecutionPhase
            {
                Name = "Standard Repository Metrics",
                Description = "Metrics collection for standard repositories",
                Order = 3,
                Repositories = standardRepos,
                EstimatedTimeMinutes = standardRepos.Sum(r => r.TestConfiguration?.MaxAnalysisTimeMinutes ?? 15)
            });
        }

        // Phase 4: Database Validation
        phases.Add(new TestExecutionPhase
        {
            Name = "Database Validation",
            Description = "Verify all data was correctly stored in database",
            Order = 4,
            Repositories = validRepositories,
            EstimatedTimeMinutes = 10
        });

        return phases;
    }

    private void AddAutonomicComputingRepository()
    {
        if (_configuration == null) return;

        // Add autonomiccomputing to a special configuration
        if (!_configuration.RepositoryConfigurations.ContainsKey("special"))
        {
            _configuration.RepositoryConfigurations["special"] = new RepositorySet
            {
                Description = "Special repositories requiring full analysis",
                Repositories = new List<RepositoryInfo>()
            };
        }

        var autonomicRepo = new RepositoryInfo
        {
            Owner = "Renandgmail",
            Name = "autonomiccomputing",
            Description = "Autonomic Computing Research Project - Full Analysis Target"
        };

        var specialRepos = _configuration.RepositoryConfigurations["special"].Repositories;
        if (!specialRepos.Any(r => r.Owner == autonomicRepo.Owner && r.Name == autonomicRepo.Name))
        {
            specialRepos.Add(autonomicRepo);
            _logger.LogDebug("✅ Added autonomiccomputing repository to special configuration");
        }
    }

    private TestRepositoryConfiguration CreateDefaultConfiguration()
    {
        _logger.LogInformation("📝 Creating default repository configuration");

        return new TestRepositoryConfiguration
        {
            RepositoryConfigurations = new Dictionary<string, RepositorySet>
            {
                ["small"] = new RepositorySet
                {
                    Description = "Small test set for quick validation",
                    Repositories = new List<RepositoryInfo>
                    {
                        new() { Owner = "microsoft", Name = "vscode", Description = "Visual Studio Code" },
                        new() { Owner = "facebook", Name = "react", Description = "React JavaScript Library" }
                    }
                },
                ["special"] = new RepositorySet
                {
                    Description = "Special repositories for full analysis",
                    Repositories = new List<RepositoryInfo>
                    {
                        new() { Owner = "Renandgmail", Name = "autonomiccomputing", Description = "Autonomic Computing Project" }
                    }
                }
            },
            DefaultConfiguration = "small",
            TestSettings = new TestSettings
            {
                DelayBetweenRequests = 2000,
                EnableMetricsCollection = true,
                EnableAnalyticsApiTest = true,
                EnableComprehensiveReport = true,
                VerifyAllTables = true
            }
        };
    }
}

#region Data Models

/// <summary>
/// Test repository configuration loaded from JSON
/// </summary>
public class TestRepositoryConfiguration
{
    public Dictionary<string, RepositorySet> RepositoryConfigurations { get; set; } = new();
    public string DefaultConfiguration { get; set; } = "small";
    public TestSettings TestSettings { get; set; } = new();
}

/// <summary>
/// Set of repositories for testing
/// </summary>
public class RepositorySet
{
    public string Description { get; set; } = string.Empty;
    public List<RepositoryInfo> Repositories { get; set; } = new();
}

/// <summary>
/// Basic repository information from configuration
/// </summary>
public class RepositoryInfo
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Test settings from configuration
/// </summary>
public class TestSettings
{
    public int DelayBetweenRequests { get; set; } = 1000;
    public bool EnableMetricsCollection { get; set; } = true;
    public bool EnableAnalyticsApiTest { get; set; } = true;
    public bool EnableComprehensiveReport { get; set; } = true;
    public bool VerifyAllTables { get; set; } = true;
}

/// <summary>
/// Enhanced test repository with testing configuration
/// </summary>
public class TestRepository
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public RepositoryTestType TestType { get; set; }
    public ProviderType ProviderType { get; set; }
    public TestPriority Priority { get; set; }
    public List<string> ExpectedFeatures { get; set; } = new();
    public RepositoryTestConfiguration? TestConfiguration { get; set; }

    public override string ToString()
    {
        return $"{Owner}/{Name} ({TestType})";
    }
}

/// <summary>
/// Configuration for testing a specific repository
/// </summary>
public class RepositoryTestConfiguration
{
    public bool EnableCodeSearch { get; set; }
    public bool EnableVocabularyExtraction { get; set; }
    public bool EnableAdvancedAnalytics { get; set; }
    public bool EnableRealTimeMetrics { get; set; }
    public bool EnableOrchestration { get; set; }
    public int MaxAnalysisTimeMinutes { get; set; } = 15;
    public bool ExpectLargeDataset { get; set; }
    public bool ValidateSearchIndices { get; set; }
    public bool ValidateVocabulary { get; set; }
}

/// <summary>
/// Types of repository testing
/// </summary>
public enum RepositoryTestType
{
    MetricsCollectionOnly,
    FullAnalysisWithCodeSearch
}

/// <summary>
/// Test priority levels
/// </summary>
public enum TestPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Repository validation result
/// </summary>
public class RepositoryValidationResult
{
    public TestRepository Repository { get; set; } = new();
    public bool IsValid { get; set; }
    public DateTime ValidationTimestamp { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> AccessibilityChecks { get; set; } = new();

    public override string ToString()
    {
        var status = IsValid ? "✅ VALID" : "❌ INVALID";
        var summary = $"{status} - {Repository.Owner}/{Repository.Name}";
        
        if (Issues.Any())
            summary += $" ({Issues.Count} issues)";
        if (Warnings.Any())
            summary += $" ({Warnings.Count} warnings)";
            
        return summary;
    }
}

/// <summary>
/// Test execution plan
/// </summary>
public class TestExecutionPlan
{
    public DateTime CreatedTimestamp { get; set; }
    public int TotalRepositories { get; set; }
    public List<TestRepository> ValidRepositories { get; set; } = new();
    public List<TestRepository> InvalidRepositories { get; set; } = new();
    public List<RepositoryValidationResult> RepositoryValidations { get; set; } = new();
    public List<TestExecutionPhase> ExecutionPhases { get; set; } = new();
    public int EstimatedExecutionTimeMinutes { get; set; }

    public override string ToString()
    {
        return $"Test Plan: {ValidRepositories.Count}/{TotalRepositories} valid repos, " +
               $"{ExecutionPhases.Count} phases, ~{EstimatedExecutionTimeMinutes}min";
    }
}

/// <summary>
/// Phase in test execution
/// </summary>
public class TestExecutionPhase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<TestRepository> Repositories { get; set; } = new();
    public int EstimatedTimeMinutes { get; set; }

    public override string ToString()
    {
        return $"Phase {Order}: {Name} ({Repositories.Count} repos, ~{EstimatedTimeMinutes}min)";
    }
}

#endregion
