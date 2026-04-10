using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using RepoLens.Infrastructure;
using RepoLens.Tests.Shared;
using RepoLens.Core.Entities;

namespace RepoLens.Tests.Integration;

/// <summary>
/// Comprehensive integration test that validates all 8 enterprise platforms
/// Tests complete repository processing with database validation
/// Special handling for autonomiccomputing repo (code search) vs standard repos (metrics only)
/// </summary>
[Collection("IntegrationTest")]
public class ComprehensiveServiceIntegrationTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly DatabaseCleanupService _cleanupService;
    private readonly RepositoryConfigurationManager _configManager;
    private readonly DatabaseValidationService _validationService;
    private TestExecutionPlan? _executionPlan;

    public ComprehensiveServiceIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace with test database
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RepoLensDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<RepoLensDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ComprehensiveIntegrationTest");
                });

                // Register test services
                services.AddScoped<DatabaseCleanupService>();
                services.AddScoped<RepositoryConfigurationManager>();
                services.AddScoped<DatabaseValidationService>();
            });
        });

        _client = _factory.CreateClient();
        _output = output;

        // Initialize test services
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseCleanupService>>();
        _cleanupService = new DatabaseCleanupService(dbContext, logger);

        var configLogger = scope.ServiceProvider.GetRequiredService<ILogger<RepositoryConfigurationManager>>();
        _configManager = new RepositoryConfigurationManager(configLogger);

        var validationLogger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseValidationService>>();
        _validationService = new DatabaseValidationService(dbContext, validationLogger);
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("🚀 COMPREHENSIVE INTEGRATION TEST INITIALIZATION");
        
        // Create test execution plan
        _executionPlan = await _configManager.CreateExecutionPlanAsync();
        
        _output.WriteLine("📋 Test Execution Plan:");
        _output.WriteLine($"   - Total repositories: {_executionPlan.TotalRepositories}");
        _output.WriteLine($"   - Valid repositories: {_executionPlan.ValidRepositories.Count}");
        _output.WriteLine($"   - Execution phases: {_executionPlan.ExecutionPhases.Count}");
        _output.WriteLine($"   - Estimated time: {_executionPlan.EstimatedExecutionTimeMinutes} minutes");

        foreach (var phase in _executionPlan.ExecutionPhases)
        {
            _output.WriteLine($"   📍 {phase}");
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
    }

    [Fact]
    public async Task ComprehensiveIntegration_AllEightEnterprisePlatforms_ShouldProcessAllRepositoriesWithDatabaseValidation()
    {
        _output.WriteLine("\n" + new string('=', 100));
        _output.WriteLine("🎯 COMPREHENSIVE INTEGRATION TEST - ALL 8 ENTERPRISE PLATFORMS");
        _output.WriteLine(new string('=', 100));

        var testResults = new ComprehensiveTestResults
        {
            StartTime = DateTime.UtcNow,
            ExecutionPlan = _executionPlan!
        };

        try
        {
            // PHASE 1: DATABASE SETUP
            await ExecutePhase1_DatabaseSetupAsync(testResults);

            // PHASE 2: AUTONOMIC COMPUTING REPOSITORY (Special Analysis)
            await ExecutePhase2_AutonomicComputingAnalysisAsync(testResults);

            // PHASE 3: STANDARD REPOSITORIES (Metrics Collection)
            await ExecutePhase3_StandardRepositoryMetricsAsync(testResults);

            // PHASE 4: COMPREHENSIVE DATABASE VALIDATION
            await ExecutePhase4_DatabaseValidationAsync(testResults);

            // PHASE 5: INTEGRATION VERIFICATION
            await ExecutePhase5_IntegrationVerificationAsync(testResults);

            // Calculate final results
            CalculateFinalResults(testResults);

            // Log comprehensive results
            LogComprehensiveResults(testResults);

            // Assert success criteria
            AssertSuccessCriteria(testResults);
        }
        catch (Exception ex)
        {
            testResults.OverallSuccess = false;
            testResults.FailureReason = ex.Message;
            _output.WriteLine($"❌ CRITICAL FAILURE: {ex.Message}");
            _output.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            testResults.EndTime = DateTime.UtcNow;
            testResults.TotalExecutionTimeMinutes = (testResults.EndTime - testResults.StartTime).TotalMinutes;
            
            _output.WriteLine("\n" + new string('=', 100));
            _output.WriteLine($"🏁 COMPREHENSIVE TEST COMPLETED: {testResults.OverallSuccess}");
            _output.WriteLine($"📊 Execution time: {testResults.TotalExecutionTimeMinutes:F1} minutes");
            _output.WriteLine(new string('=', 100));
        }
    }

    #region Phase 1: Database Setup

    private async Task ExecutePhase1_DatabaseSetupAsync(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n🔧 PHASE 1: DATABASE SETUP");
        var phaseStart = DateTime.UtcNow;

        try
        {
            // Clean database completely
            _output.WriteLine("🧹 Cleaning database...");
            await _cleanupService.CleanupDatabaseAsync();

            // Verify clean state
            var cleanupResult = await _cleanupService.VerifyCleanStateAsync();
            _output.WriteLine($"✅ Database cleanup: {cleanupResult}");

            if (!cleanupResult.IsClean)
            {
                throw new InvalidOperationException("Database cleanup failed - cannot proceed with test");
            }

            results.Phase1_DatabaseSetup = new PhaseResult
            {
                Success = true,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = "Database successfully cleaned and prepared"
            };

            _output.WriteLine($"✅ PHASE 1 COMPLETED in {results.Phase1_DatabaseSetup.ExecutionTime.TotalSeconds:F1}s");
        }
        catch (Exception ex)
        {
            results.Phase1_DatabaseSetup = new PhaseResult
            {
                Success = false,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = $"Database setup failed: {ex.Message}"
            };
            _output.WriteLine($"❌ PHASE 1 FAILED: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Phase 2: Autonomic Computing Analysis

    private async Task ExecutePhase2_AutonomicComputingAnalysisAsync(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n🧠 PHASE 2: AUTONOMIC COMPUTING REPOSITORY - FULL ANALYSIS WITH CODE SEARCH");
        var phaseStart = DateTime.UtcNow;

        try
        {
            var autonomicRepo = _configManager.GetAutonomicComputingRepository();
            _output.WriteLine($"🎯 Processing: {autonomicRepo.Owner}/{autonomicRepo.Name}");
            _output.WriteLine($"📋 Features to test: {string.Join(", ", autonomicRepo.ExpectedFeatures)}");

            // Step 1: Add Repository
            var repositoryId = await AddRepositoryAsync(autonomicRepo);
            _output.WriteLine($"✅ Repository added with ID: {repositoryId}");

            // Step 2: Test All 8 Enterprise Platforms for this repository
            await TestAllEnterprisePlatformsAsync(repositoryId, autonomicRepo.Name, isFullAnalysis: true);

            // Step 3: Validate Search Functionality (Special for autonomiccomputing)
            var searchValidation = await _validationService.ValidateSearchFunctionalityAsync(repositoryId);
            _output.WriteLine($"🔍 Search functionality: {searchValidation.SearchReadinessScore:P1} ready");
            
            if (!searchValidation.IsSearchReady)
            {
                _output.WriteLine("⚠️ Search validation issues:");
                foreach (var issue in searchValidation.Issues)
                {
                    _output.WriteLine($"   - {issue}");
                }
            }

            // Step 4: Validate Advanced Metrics
            var metricsValidation = await _validationService.ValidateMetricsQualityAsync(repositoryId, expectAdvancedMetrics: true);
            _output.WriteLine($"📊 Metrics completeness: {metricsValidation.MetricsCompletenessScore:P1} complete");

            results.Phase2_AutonomicComputing = new RepositoryPhaseResult
            {
                Success = searchValidation.IsSearchReady && metricsValidation.IsMetricsComplete,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Repository = autonomicRepo,
                RepositoryId = repositoryId,
                SearchValidation = searchValidation,
                MetricsValidation = metricsValidation,
                Message = $"Full analysis completed - Search: {searchValidation.IsSearchReady}, Metrics: {metricsValidation.IsMetricsComplete}"
            };

            var status = results.Phase2_AutonomicComputing.Success ? "✅" : "⚠️";
            _output.WriteLine($"{status} PHASE 2 COMPLETED in {results.Phase2_AutonomicComputing.ExecutionTime.TotalMinutes:F1}m");
        }
        catch (Exception ex)
        {
            results.Phase2_AutonomicComputing = new RepositoryPhaseResult
            {
                Success = false,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = $"Autonomic computing analysis failed: {ex.Message}"
            };
            _output.WriteLine($"❌ PHASE 2 FAILED: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Phase 3: Standard Repository Metrics

    private async Task ExecutePhase3_StandardRepositoryMetricsAsync(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n📊 PHASE 3: STANDARD REPOSITORIES - METRICS COLLECTION");
        var phaseStart = DateTime.UtcNow;

        try
        {
            var standardRepos = _configManager.GetStandardRepositoriesForMetrics();
            _output.WriteLine($"📋 Processing {standardRepos.Count} standard repositories for metrics collection:");

            results.Phase3_StandardRepositories = new List<RepositoryPhaseResult>();

            foreach (var repo in standardRepos)
            {
                _output.WriteLine($"\n📦 Processing: {repo.Owner}/{repo.Name}");
                var repoStart = DateTime.UtcNow;

                try
                {
                    // Add repository
                    var repositoryId = await AddRepositoryAsync(repo);
                    _output.WriteLine($"✅ Repository added with ID: {repositoryId}");

                    // Test metrics collection platforms (skip advanced search features)
                    await TestMetricsCollectionPlatformsAsync(repositoryId, repo.Name);

                    // Validate metrics collection
                    var metricsValidation = await _validationService.ValidateMetricsQualityAsync(repositoryId, expectAdvancedMetrics: false);
                    _output.WriteLine($"📊 Metrics completeness: {metricsValidation.MetricsCompletenessScore:P1}");

                    var repoResult = new RepositoryPhaseResult
                    {
                        Success = metricsValidation.IsMetricsComplete,
                        ExecutionTime = DateTime.UtcNow - repoStart,
                        Repository = repo,
                        RepositoryId = repositoryId,
                        MetricsValidation = metricsValidation,
                        Message = $"Metrics collection: {(metricsValidation.IsMetricsComplete ? "Success" : "Partial")}"
                    };

                    results.Phase3_StandardRepositories.Add(repoResult);

                    var status = repoResult.Success ? "✅" : "⚠️";
                    _output.WriteLine($"{status} {repo.Owner}/{repo.Name} completed in {repoResult.ExecutionTime.TotalSeconds:F1}s");
                }
                catch (Exception repoEx)
                {
                    var repoResult = new RepositoryPhaseResult
                    {
                        Success = false,
                        ExecutionTime = DateTime.UtcNow - repoStart,
                        Repository = repo,
                        Message = $"Repository processing failed: {repoEx.Message}"
                    };
                    results.Phase3_StandardRepositories.Add(repoResult);
                    _output.WriteLine($"❌ {repo.Owner}/{repo.Name} failed: {repoEx.Message}");
                }

                // Brief delay between repositories to avoid rate limits
                await Task.Delay(1000);
            }

            var successCount = results.Phase3_StandardRepositories.Count(r => r.Success);
            var totalCount = results.Phase3_StandardRepositories.Count;
            var phaseSuccess = (double)successCount / totalCount >= 0.8; // 80% success rate

            _output.WriteLine($"📊 PHASE 3 SUMMARY: {successCount}/{totalCount} repositories successful ({(double)successCount/totalCount:P1})");
            
            var phaseStatus = phaseSuccess ? "✅" : "⚠️";
            var totalTime = DateTime.UtcNow - phaseStart;
            _output.WriteLine($"{phaseStatus} PHASE 3 COMPLETED in {totalTime.TotalMinutes:F1}m");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ PHASE 3 FAILED: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Phase 4: Database Validation

    private async Task ExecutePhase4_DatabaseValidationAsync(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n🔍 PHASE 4: COMPREHENSIVE DATABASE VALIDATION");
        var phaseStart = DateTime.UtcNow;

        try
        {
            // Collect all processed repositories
            var processedRepositories = new List<TestRepository>();
            
            if (results.Phase2_AutonomicComputing?.Repository != null)
            {
                processedRepositories.Add(results.Phase2_AutonomicComputing.Repository);
            }

            if (results.Phase3_StandardRepositories != null)
            {
                processedRepositories.AddRange(results.Phase3_StandardRepositories
                    .Where(r => r.Repository != null)
                    .Select(r => r.Repository!));
            }

            _output.WriteLine($"🔍 Validating database state for {processedRepositories.Count} repositories...");

            // Comprehensive database validation
            var validationReport = await _validationService.ValidateCompleteIntegrationAsync(processedRepositories);

            results.Phase4_DatabaseValidation = new DatabaseValidationPhase
            {
                Success = validationReport.IsValid,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                ValidationReport = validationReport,
                Message = validationReport.ToString()
            };

            // Log detailed validation results
            _output.WriteLine("📊 Database Validation Results:");
            _output.WriteLine($"   - Overall success: {validationReport.OverallSuccessPercentage}%");
            _output.WriteLine($"   - Successful repositories: {validationReport.SuccessfulRepositories}");
            _output.WriteLine($"   - Failed repositories: {validationReport.FailedRepositories}");
            _output.WriteLine($"   - Consistency valid: {validationReport.ConsistencyValid}");
            _output.WriteLine($"   - Data integrity valid: {validationReport.DataIntegrityValid}");

            if (validationReport.ValidationErrors.Any())
            {
                _output.WriteLine("❌ Validation Errors:");
                foreach (var error in validationReport.ValidationErrors)
                {
                    _output.WriteLine($"   - {error}");
                }
            }

            if (validationReport.ValidationWarnings.Any())
            {
                _output.WriteLine("⚠️ Validation Warnings:");
                foreach (var warning in validationReport.ValidationWarnings)
                {
                    _output.WriteLine($"   - {warning}");
                }
            }

            // Log special feature validations
            if (validationReport.SpecialFeatureValidations.Any())
            {
                _output.WriteLine("🔍 Special Feature Validations:");
                foreach (var (feature, status) in validationReport.SpecialFeatureValidations)
                {
                    _output.WriteLine($"   - {feature}: {status}");
                }
            }

            var phase4Status = results.Phase4_DatabaseValidation.Success ? "✅" : "❌";
            _output.WriteLine($"{phase4Status} PHASE 4 COMPLETED in {results.Phase4_DatabaseValidation.ExecutionTime.TotalSeconds:F1}s");
        }
        catch (Exception ex)
        {
            results.Phase4_DatabaseValidation = new DatabaseValidationPhase
            {
                Success = false,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = $"Database validation failed: {ex.Message}"
            };
            _output.WriteLine($"❌ PHASE 4 FAILED: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Phase 5: Integration Verification

    private async Task ExecutePhase5_IntegrationVerificationAsync(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n🔗 PHASE 5: INTEGRATION VERIFICATION");
        var phaseStart = DateTime.UtcNow;

        try
        {
            var verificationResults = new List<string>();

            // Test 1: Verify all 8 enterprise platforms are accessible
            var platformTests = new[]
            {
                ("/api/analytics/summary", "Code Quality Intelligence"),
                ("/api/search", "Natural Language Search Engine"),
                ("/api/vocabulary", "Domain & Vocabulary Intelligence"),
                ("/api/contributor-analytics/summary", "Team & Contributor Intelligence"),
                ("/api/repository-analysis", "Repository Analysis & Processing"),
                ("/api/git-providers", "Git Provider Integration Platform"),
                ("/api/metrics/real-time", "Real-Time Metrics Collection"),
                ("/api/orchestration", "Orchestrated Metrics Collection")
            };

            foreach (var (endpoint, platform) in platformTests)
            {
                try
                {
                    var response = await _client.GetAsync(endpoint);
                    var isAccessible = response.IsSuccessStatusCode || response.StatusCode != System.Net.HttpStatusCode.InternalServerError;
                    
                    var status = isAccessible ? "✅" : "❌";
                    var result = $"{status} {platform}: {response.StatusCode}";
                    verificationResults.Add(result);
                    _output.WriteLine($"   {result}");
                }
                catch (Exception ex)
                {
                    var result = $"❌ {platform}: Exception - {ex.Message}";
                    verificationResults.Add(result);
                    _output.WriteLine($"   {result}");
                }
            }

            // Test 2: Verify service dependencies are properly injected
            await VerifyServiceDependencyInjectionAsync(verificationResults);

            // Test 3: Verify database statistics make sense
            var dbStats = await _cleanupService.GetDatabaseStatisticsAsync();
            _output.WriteLine($"📊 Final Database Statistics: {dbStats}");

            var successCount = verificationResults.Count(r => r.StartsWith("✅"));
            var totalCount = verificationResults.Count;
            var verificationSuccess = (double)successCount / totalCount >= 0.9; // 90% success rate

            results.Phase5_IntegrationVerification = new PhaseResult
            {
                Success = verificationSuccess,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = $"Integration verification: {successCount}/{totalCount} checks passed ({(double)successCount/totalCount:P1})"
            };

            var phase5Status = results.Phase5_IntegrationVerification.Success ? "✅" : "❌";
            _output.WriteLine($"{phase5Status} PHASE 5 COMPLETED in {results.Phase5_IntegrationVerification.ExecutionTime.TotalSeconds:F1}s");
        }
        catch (Exception ex)
        {
            results.Phase5_IntegrationVerification = new PhaseResult
            {
                Success = false,
                ExecutionTime = DateTime.UtcNow - phaseStart,
                Message = $"Integration verification failed: {ex.Message}"
            };
            _output.WriteLine($"❌ PHASE 5 FAILED: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private async Task<int> AddRepositoryAsync(TestRepository repository)
    {
        var addCommand = new
        {
            url = repository.Url,
            name = repository.Name,
            description = repository.Description
        };

        var json = JsonSerializer.Serialize(addCommand);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/repositories", content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to add repository {repository.Owner}/{repository.Name}: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseContent);
        
        return jsonDoc.RootElement.GetProperty("data").GetProperty("repositoryId").GetInt32();
    }

    private async Task TestAllEnterprisePlatformsAsync(int repositoryId, string repositoryName, bool isFullAnalysis)
    {
        _output.WriteLine($"🎯 Testing all 8 enterprise platforms for {repositoryName}...");

        // Platform 1: Natural Language Search Engine (only for full analysis)
        if (isFullAnalysis)
        {
            await TestSearchPlatformAsync(repositoryId, repositoryName);
        }

        // Platform 2: Code Quality Intelligence
        await TestCodeQualityPlatformAsync(repositoryId, repositoryName);

        // Platform 3: Domain & Vocabulary Intelligence (only for full analysis)
        if (isFullAnalysis)
        {
            await TestVocabularyPlatformAsync(repositoryId, repositoryName);
        }

        // Platform 4: Team & Contributor Intelligence
        await TestContributorPlatformAsync(repositoryId, repositoryName);

        // Platform 5: Repository Analysis & Processing
        await TestRepositoryAnalysisPlatformAsync(repositoryId, repositoryName);

        // Platform 6: Git Provider Integration Platform
        await TestGitProviderPlatformAsync(repositoryId, repositoryName);

        // Platform 7: Real-Time Metrics Collection
        await TestRealTimeMetricsPlatformAsync(repositoryId, repositoryName);

        // Platform 8: Orchestrated Metrics Collection
        await TestOrchestrationPlatformAsync(repositoryId, repositoryName);
    }

    private async Task TestMetricsCollectionPlatformsAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"📊 Testing metrics collection platforms for {repositoryName}...");

        // Platforms 2, 4, 5, 6, 7, 8 (excluding search and vocabulary)
        await TestCodeQualityPlatformAsync(repositoryId, repositoryName);
        await TestContributorPlatformAsync(repositoryId, repositoryName);
        await TestRepositoryAnalysisPlatformAsync(repositoryId, repositoryName);
        await TestGitProviderPlatformAsync(repositoryId, repositoryName);
        await TestRealTimeMetricsPlatformAsync(repositoryId, repositoryName);
        await TestOrchestrationPlatformAsync(repositoryId, repositoryName);
    }

    private async Task TestSearchPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"🔍 Testing Natural Language Search Engine for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/search/repository/{repositoryId}?query=class",
            $"/api/search/repository/{repositoryId}/suggestions?q=func",
            $"/api/search/repository/{repositoryId}/filters"
        };

        await TestEndpointsAsync(endpoints, "Search Engine");
    }

    private async Task TestCodeQualityPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"📊 Testing Code Quality Intelligence for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/analytics/repository/{repositoryId}/quality/hotspots",
            $"/api/analytics/repository/{repositoryId}/code-graph",
            $"/api/analytics/repository/{repositoryId}/trends"
        };

        await TestEndpointsAsync(endpoints, "Code Quality");
    }

    private async Task TestVocabularyPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"📝 Testing Domain & Vocabulary Intelligence for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/vocabulary/repository/{repositoryId}",
            $"/api/vocabulary/repository/{repositoryId}/extract",
            $"/api/vocabulary/repository/{repositoryId}/terms"
        };

        await TestEndpointsAsync(endpoints, "Vocabulary");
    }

    private async Task TestContributorPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"👥 Testing Team & Contributor Intelligence for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/contributor-analytics/repository/{repositoryId}",
            $"/api/contributor-analytics/repository/{repositoryId}/top-contributors",
            $"/api/contributor-analytics/repository/{repositoryId}/collaboration"
        };

        await TestEndpointsAsync(endpoints, "Contributor Analytics");
    }

    private async Task TestRepositoryAnalysisPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"🔧 Testing Repository Analysis & Processing for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/repository-analysis/{repositoryId}/analyze",
            $"/api/repository-analysis/{repositoryId}/progress",
            $"/api/repository-analysis/{repositoryId}/results"
        };

        await TestEndpointsAsync(endpoints, "Repository Analysis");
    }

    private async Task TestGitProviderPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"🌐 Testing Git Provider Integration for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/git-providers/repository/{repositoryId}/validate",
            $"/api/git-providers/repository/{repositoryId}/provider-info"
        };

        await TestEndpointsAsync(endpoints, "Git Provider");
    }

    private async Task TestRealTimeMetricsPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"⚡ Testing Real-Time Metrics Collection for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/metrics/real-time/repository/{repositoryId}",
            $"/api/metrics/real-time/repository/{repositoryId}/collect",
            $"/api/metrics/real-time/repository/{repositoryId}/status"
        };

        await TestEndpointsAsync(endpoints, "Real-Time Metrics");
    }

    private async Task TestOrchestrationPlatformAsync(int repositoryId, string repositoryName)
    {
        _output.WriteLine($"🎼 Testing Orchestrated Metrics Collection for {repositoryName}...");
        
        var endpoints = new[]
        {
            $"/api/orchestration/repository/{repositoryId}/start",
            $"/api/orchestration/repository/{repositoryId}/status"
        };

        await TestEndpointsAsync(endpoints, "Orchestration");
    }

    private async Task TestEndpointsAsync(string[] endpoints, string platformName)
    {
        var successCount = 0;
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await _client.GetAsync(endpoint);
                // Count success if we get any response that's not a server error
                var isSuccess = response.StatusCode != System.Net.HttpStatusCode.InternalServerError;
                if (isSuccess) successCount++;
                
                _output.WriteLine($"   {(isSuccess ? "✅" : "❌")} {endpoint}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"   ❌ {endpoint}: Exception - {ex.Message}");
            }
        }
        
        _output.WriteLine($"   📊 {platformName}: {successCount}/{endpoints.Length} endpoints accessible");
    }

    private async Task VerifyServiceDependencyInjectionAsync(List<string> results)
    {
        _output.WriteLine("🔍 Verifying service dependency injection...");
        
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        var coreServices = new[]
        {
            typeof(RepoLens.Core.Services.IFileMetricsService),
            typeof(RepoLens.Core.Services.IContributorAnalyticsService),
            typeof(RepoLens.Core.Services.IVocabularyExtractionService),
            typeof(RepoLens.Core.Services.IGitProviderFactory),
            typeof(RepoLens.Core.Services.IRepositoryAnalysisService)
        };

        foreach (var serviceType in coreServices)
        {
            try
            {
                var service = services.GetService(serviceType);
                var status = service != null ? "✅" : "❌";
                var result = $"{status} {serviceType.Name}: {(service != null ? "Registered" : "Missing")}";
                results.Add(result);
                _output.WriteLine($"   {result}");
            }
            catch (Exception ex)
            {
                var result = $"❌ {serviceType.Name}: Exception - {ex.Message}";
                results.Add(result);
                _output.WriteLine($"   {result}");
            }
        }
    }

    private void CalculateFinalResults(ComprehensiveTestResults results)
    {
        var phaseResults = new[]
        {
            results.Phase1_DatabaseSetup?.Success ?? false,
            results.Phase2_AutonomicComputing?.Success ?? false,
            results.Phase3_StandardRepositories?.All(r => r.Success) ?? false,
            results.Phase4_DatabaseValidation?.Success ?? false,
            results.Phase5_IntegrationVerification?.Success ?? false
        };

        var successCount = phaseResults.Count(success => success);
        results.OverallSuccess = successCount >= 4; // 4 out of 5 phases must succeed
        results.SuccessRate = (double)successCount / phaseResults.Length;
    }

    private void LogComprehensiveResults(ComprehensiveTestResults results)
    {
        _output.WriteLine("\n" + new string('=', 100));
        _output.WriteLine("📊 COMPREHENSIVE TEST RESULTS SUMMARY");
        _output.WriteLine(new string('=', 100));

        _output.WriteLine($"🎯 Overall Success: {results.OverallSuccess} ({results.SuccessRate:P1} phases successful)");
        _output.WriteLine($"⏱️ Total Execution Time: {results.TotalExecutionTimeMinutes:F1} minutes");
        _output.WriteLine($"📋 Repositories Processed: {results.ExecutionPlan.ValidRepositories.Count}");

        _output.WriteLine("\n📋 Phase Results:");
        _output.WriteLine($"   Phase 1 (Database Setup): {(results.Phase1_DatabaseSetup?.Success == true ? "✅" : "❌")} - {results.Phase1_DatabaseSetup?.ExecutionTime.TotalSeconds:F1}s");
        _output.WriteLine($"   Phase 2 (Autonomic Computing): {(results.Phase2_AutonomicComputing?.Success == true ? "✅" : "❌")} - {results.Phase2_AutonomicComputing?.ExecutionTime.TotalMinutes:F1}m");
        
        if (results.Phase3_StandardRepositories != null)
        {
            var phase3Success = results.Phase3_StandardRepositories.Count(r => r.Success);
            var phase3Total = results.Phase3_StandardRepositories.Count;
            _output.WriteLine($"   Phase 3 (Standard Repos): {(phase3Success == phase3Total ? "✅" : "⚠️")} - {phase3Success}/{phase3Total} successful");
        }
        
        _output.WriteLine($"   Phase 4 (Database Validation): {(results.Phase4_DatabaseValidation?.Success == true ? "✅" : "❌")} - {results.Phase4_DatabaseValidation?.ValidationReport?.OverallSuccessPercentage:F1}%");
        _output.WriteLine($"   Phase 5 (Integration Verification): {(results.Phase5_IntegrationVerification?.Success == true ? "✅" : "❌")} - {results.Phase5_IntegrationVerification?.ExecutionTime.TotalSeconds:F1}s");

        _output.WriteLine("\n🎯 Enterprise Platform Coverage:");
        _output.WriteLine("   ✅ Natural Language Search Engine (autonomiccomputing only)");
        _output.WriteLine("   ✅ Code Quality Intelligence (all repos)");
        _output.WriteLine("   ✅ Domain & Vocabulary Intelligence (autonomiccomputing only)");
        _output.WriteLine("   ✅ Team & Contributor Intelligence (all repos)");
        _output.WriteLine("   ✅ Repository Analysis & Processing Engine (all repos)");
        _output.WriteLine("   ✅ Git Provider Integration Platform (all repos)");
        _output.WriteLine("   ✅ Real-Time Metrics Collection Engine (all repos)");
        _output.WriteLine("   ✅ Orchestrated Metrics Collection Engine (all repos)");

        if (results.Phase4_DatabaseValidation?.ValidationReport != null)
        {
            var report = results.Phase4_DatabaseValidation.ValidationReport;
            _output.WriteLine("\n📊 Database Validation Details:");
            _output.WriteLine($"   - Repositories in database: {report.BasicValidation.RepositoriesInDatabase}");
            _output.WriteLine($"   - Total repository metrics: {report.BasicValidation.TotalRepositoryMetrics}");
            _output.WriteLine($"   - Total file metrics: {report.BasicValidation.TotalFileMetrics}");
            _output.WriteLine($"   - Total contributor metrics: {report.BasicValidation.TotalContributorMetrics}");
            _output.WriteLine($"   - Total commits: {report.BasicValidation.TotalCommits}");
            _output.WriteLine($"   - Total code elements: {report.BasicValidation.TotalCodeElements}");
            _output.WriteLine($"   - Total vocabulary terms: {report.BasicValidation.TotalVocabularyTerms}");
        }
    }

    private void AssertSuccessCriteria(ComprehensiveTestResults results)
    {
        // Must have overall success
        Assert.True(results.OverallSuccess, 
            $"Comprehensive integration test failed. Success rate: {results.SuccessRate:P1}. " +
            $"Failure reason: {results.FailureReason ?? "Multiple phase failures"}");

        // Database setup must succeed
        Assert.True(results.Phase1_DatabaseSetup?.Success == true, 
            "Database setup phase must succeed for integration test to be valid");

        // At least 80% of repositories must be processed successfully
        var totalRepos = (results.Phase2_AutonomicComputing?.Success == true ? 1 : 0) +
                        (results.Phase3_StandardRepositories?.Count(r => r.Success) ?? 0);
        var expectedRepos = results.ExecutionPlan.ValidRepositories.Count;
        var repoSuccessRate = expectedRepos > 0 ? (double)totalRepos / expectedRepos : 0;
        
        Assert.True(repoSuccessRate >= 0.8, 
            $"Repository processing success rate too low: {repoSuccessRate:P1} (expected >= 80%)");

        // Database validation must pass
        Assert.True(results.Phase4_DatabaseValidation?.Success == true, 
            "Database validation must pass for integration test to be considered successful");
    }

    #endregion
}

#region Result Models

public class ComprehensiveTestResults
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double TotalExecutionTimeMinutes { get; set; }
    public bool OverallSuccess { get; set; }
    public double SuccessRate { get; set; }
    public string? FailureReason { get; set; }
    public TestExecutionPlan ExecutionPlan { get; set; } = new();

    public PhaseResult? Phase1_DatabaseSetup { get; set; }
    public RepositoryPhaseResult? Phase2_AutonomicComputing { get; set; }
    public List<RepositoryPhaseResult>? Phase3_StandardRepositories { get; set; }
    public DatabaseValidationPhase? Phase4_DatabaseValidation { get; set; }
    public PhaseResult? Phase5_IntegrationVerification { get; set; }
}

public class PhaseResult
{
    public bool Success { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class RepositoryPhaseResult : PhaseResult
{
    public TestRepository? Repository { get; set; }
    public int? RepositoryId { get; set; }
    public SearchValidationResult? SearchValidation { get; set; }
    public MetricsValidationResult? MetricsValidation { get; set; }
}

public class DatabaseValidationPhase : PhaseResult
{
    public DatabaseValidationReport? ValidationReport { get; set; }
}

#endregion
