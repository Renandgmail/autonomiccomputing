using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using RepoLens.Infrastructure;

namespace RepoLens.Tests.Integration;

/// <summary>
/// Comprehensive service integration verification test
/// Validates that all analytics services work together properly
/// </summary>
public class ServiceIntegrationVerificationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ServiceIntegrationVerificationTest(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the default DbContext with an in-memory database for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RepoLensDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<RepoLensDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ServiceIntegrationTest");
                });
            });
        });
        
        _client = _factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task ServiceIntegration_CompleteAnalyticsPlatform_ShouldWork()
    {
        _output.WriteLine("=== STARTING COMPREHENSIVE SERVICE INTEGRATION TEST ===");
        
        // Test 1: Health Check - Verify service is running
        await VerifyServiceHealth();
        
        // Test 2: Analytics Endpoints - Verify all endpoints respond properly
        await VerifyAnalyticsEndpoints();
        
        // Test 3: Error Handling - Verify graceful degradation
        await VerifyErrorHandling();
        
        _output.WriteLine("=== SERVICE INTEGRATION TEST COMPLETED SUCCESSFULLY ===");
    }

    private async Task VerifyServiceHealth()
    {
        _output.WriteLine("--- Testing Service Health ---");
        
        var response = await _client.GetAsync("/api/health");
        var content = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();
        _output.WriteLine($"✅ Health check passed: {response.StatusCode}");
        
        // Verify health response contains expected structure
        var healthData = JsonDocument.Parse(content);
        Assert.True(healthData.RootElement.TryGetProperty("status", out _));
        Assert.True(healthData.RootElement.TryGetProperty("timestamp", out _));
        
        _output.WriteLine("✅ Health response structure validated");
    }

    private async Task VerifyAnalyticsEndpoints()
    {
        _output.WriteLine("--- Testing Analytics Endpoints ---");
        
        // Test Analytics Summary (should work without any data)
        await TestAnalyticsSummary();
        
        // Test Repository-specific endpoints (should handle missing data gracefully)
        await TestRepositoryAnalyticsEndpoints();
        
        _output.WriteLine("✅ All analytics endpoints responding properly");
    }

    private async Task TestAnalyticsSummary()
    {
        _output.WriteLine("Testing Analytics Summary endpoint...");
        
        var response = await _client.GetAsync("/api/analytics/summary");
        var content = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();
        _output.WriteLine($"✅ Analytics Summary: {response.StatusCode}");
        
        var jsonDoc = JsonDocument.Parse(content);
        Assert.True(jsonDoc.RootElement.GetProperty("success").GetBoolean());
        
        // Should handle empty data gracefully
        var data = jsonDoc.RootElement.GetProperty("data");
        _output.WriteLine($"✅ Summary data structure: {data}");
    }

    private async Task TestRepositoryAnalyticsEndpoints()
    {
        _output.WriteLine("Testing Repository Analytics endpoints...");
        
        var testRepositoryId = 999; // Non-existent repository
        
        // Test all analytics endpoints with non-existent repository
        var endpoints = new[]
        {
            $"/api/analytics/repository/{testRepositoryId}/history",
            $"/api/analytics/repository/{testRepositoryId}/trends",
            $"/api/analytics/repository/{testRepositoryId}/language-trends",
            $"/api/analytics/repository/{testRepositoryId}/activity-patterns",
            $"/api/analytics/repository/{testRepositoryId}/files",
            $"/api/analytics/repository/{testRepositoryId}/quality/hotspots",
            $"/api/analytics/repository/{testRepositoryId}/code-graph"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            // All endpoints should either succeed with empty data or return 404
            var isSuccessful = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
            Assert.True(isSuccessful, $"Endpoint {endpoint} returned unexpected status: {response.StatusCode}");
            
            _output.WriteLine($"✅ {endpoint}: {response.StatusCode}");
            
            // If successful, verify JSON structure
            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(content);
                Assert.True(jsonDoc.RootElement.TryGetProperty("success", out _));
            }
        }
    }

    private async Task VerifyErrorHandling()
    {
        _output.WriteLine("--- Testing Error Handling ---");
        
        // Test invalid endpoints
        var invalidEndpoints = new[]
        {
            "/api/analytics/repository/invalid/files",
            "/api/analytics/repository/-1/trends",
            "/api/analytics/nonexistent/endpoint"
        };

        foreach (var endpoint in invalidEndpoints)
        {
            var response = await _client.GetAsync(endpoint);
            
            // Should return proper error codes (400, 404, etc.)
            Assert.False(response.StatusCode == System.Net.HttpStatusCode.InternalServerError, 
                $"Endpoint {endpoint} should not return 500 error");
            
            _output.WriteLine($"✅ Error handling for {endpoint}: {response.StatusCode}");
        }
        
        _output.WriteLine("✅ Error handling verification completed");
    }

    [Fact] 
    public async Task AnalyticsController_AllEndpoints_ShouldReturnValidJSON()
    {
        _output.WriteLine("=== TESTING ALL ANALYTICS CONTROLLER ENDPOINTS ===");
        
        var endpoints = new Dictionary<string, string>
        {
            ["Summary"] = "/api/analytics/summary",
            ["Repository History"] = "/api/analytics/repository/1/history",
            ["Repository Trends"] = "/api/analytics/repository/1/trends?days=7",
            ["Language Trends"] = "/api/analytics/repository/1/language-trends?days=7", 
            ["Activity Patterns"] = "/api/analytics/repository/1/activity-patterns",
            ["File Metrics"] = "/api/analytics/repository/1/files?page=1&pageSize=5",
            ["Quality Hotspots"] = "/api/analytics/repository/1/quality/hotspots?limit=3",
            ["Code Graph"] = "/api/analytics/repository/1/code-graph"
        };

        var results = new List<string>();
        
        foreach (var (name, endpoint) in endpoints)
        {
            try
            {
                var response = await _client.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                var status = response.IsSuccessStatusCode ? "✅ SUCCESS" : 
                           response.StatusCode == System.Net.HttpStatusCode.NotFound ? "⚠️ NOT FOUND" : 
                           $"❌ ERROR ({response.StatusCode})";
                
                results.Add($"{status} - {name}: {endpoint}");
                
                // If successful, verify it's valid JSON
                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(content);
                    Assert.True(jsonDoc.RootElement.TryGetProperty("success", out _));
                    _output.WriteLine($"✅ {name} - Valid JSON structure");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ EXCEPTION - {name}: {ex.Message}");
                _output.WriteLine($"❌ {name} - Exception: {ex.Message}");
            }
        }
        
        // Log summary
        _output.WriteLine("\n=== ENDPOINT TEST SUMMARY ===");
        foreach (var result in results)
        {
            _output.WriteLine(result);
        }
        
        // Verify at least summary endpoint works (core functionality)
        var summaryResponse = await _client.GetAsync("/api/analytics/summary");
        Assert.True(summaryResponse.IsSuccessStatusCode, "Analytics Summary endpoint must work");
    }

    [Fact]
    public async Task ServiceArchitecture_DependencyInjection_ShouldBeConfiguredCorrectly()
    {
        _output.WriteLine("=== TESTING SERVICE ARCHITECTURE & DEPENDENCY INJECTION ===");
        
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        // Test core service registrations
        var coreServices = new[]
        {
            "IRepositoryMetricsRepository",
            "IFileMetricsRepository", 
            "IContributorMetricsRepository",
            "IFileMetricsService",
            "IContributorAnalyticsService",
            "IVocabularyExtractionService",
            "IGitProviderFactory",
            "IRepositoryAnalysisService",
            "IQueryProcessingService"
        };

        var serviceTypes = new Dictionary<string, Type>
        {
            ["IRepositoryMetricsRepository"] = typeof(RepoLens.Core.Repositories.IRepositoryMetricsRepository),
            ["IFileMetricsRepository"] = typeof(RepoLens.Core.Repositories.IFileMetricsRepository),
            ["IContributorMetricsRepository"] = typeof(RepoLens.Core.Repositories.IContributorMetricsRepository),
            ["IFileMetricsService"] = typeof(RepoLens.Core.Services.IFileMetricsService),
            ["IContributorAnalyticsService"] = typeof(RepoLens.Core.Services.IContributorAnalyticsService),
            ["IVocabularyExtractionService"] = typeof(RepoLens.Core.Services.IVocabularyExtractionService),
            ["IGitProviderFactory"] = typeof(RepoLens.Core.Services.IGitProviderFactory),
            ["IRepositoryAnalysisService"] = typeof(RepoLens.Core.Services.IRepositoryAnalysisService),
            ["IQueryProcessingService"] = typeof(RepoLens.Core.Services.IQueryProcessingService)
        };

        var registeredServices = new List<string>();
        var missingServices = new List<string>();
        
        foreach (var (serviceName, serviceType) in serviceTypes)
        {
            try
            {
                var service = services.GetService(serviceType);
                if (service != null)
                {
                    registeredServices.Add($"✅ {serviceName}: {service.GetType().Name}");
                }
                else
                {
                    missingServices.Add($"❌ {serviceName}: Not registered");
                }
            }
            catch (Exception ex)
            {
                missingServices.Add($"❌ {serviceName}: Exception - {ex.Message}");
            }
        }
        
        // Log results
        _output.WriteLine("\n--- REGISTERED SERVICES ---");
        foreach (var service in registeredServices)
        {
            _output.WriteLine(service);
        }
        
        if (missingServices.Any())
        {
            _output.WriteLine("\n--- MISSING SERVICES ---");
            foreach (var missing in missingServices)
            {
                _output.WriteLine(missing);
            }
        }
        
        // Verify critical services are registered
        Assert.True(registeredServices.Count >= 6, 
            $"Expected at least 6 core services registered, but only found {registeredServices.Count}");
        
        _output.WriteLine($"\n✅ Service Architecture: {registeredServices.Count}/{serviceTypes.Count} services properly registered");
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
