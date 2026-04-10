using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Integration;

/// <summary>
/// IT-001: A user adds a public GitHub repository by URL.
/// The system validates it, creates the DB record, collects metrics, and returns a health score.
/// </summary>
[Trait("Category", "Integration")]
public class GitHubRepositoryFlowIntegrationTest : IClassFixture<GitHubTestWebApplicationFactory>, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;
    private readonly GitHubTestWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly RepoLensDbContext _dbContext;
    private readonly List<TestLogEntry> _testLogs;

    // Test repository - using a small, stable public repository
    private const string TEST_GITHUB_URL = "https://github.com/octocat/Hello-World";
    private const string TEST_REPO_NAME = "Hello-World-Test";

    public GitHubRepositoryFlowIntegrationTest(GitHubTestWebApplicationFactory factory, ITestOutputHelper output)
    {
        _output = output;
        _factory = factory;
        _client = factory.CreateClient();
        
        // Skip test if no GitHub token available
        var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new SkipException("GITHUB_TOKEN environment variable not set - skipping GitHub integration test");
        }

        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();
        _testLogs = _scope.ServiceProvider.GetRequiredService<TestLoggerSink>().Entries;
        
        _output.WriteLine($"🚀 Starting IT-001: GitHub Repository Flow Integration Test");
        _output.WriteLine($"Test Repository: {TEST_GITHUB_URL}");
    }

    [Fact(DisplayName = "IT-001: Add GitHub Repository - Full Workflow")]
    public async Task AddRepository_WithValidGitHubUrl_CreatesRepositoryAndCollectsMetrics()
    {
        // ARRANGE — set up the system state
        var jwt = await AuthenticateTestUserAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        // Ensure repository doesn't already exist
        await CleanupExistingRepositoryAsync(TEST_GITHUB_URL);

        _output.WriteLine("✅ ARRANGE: Test user authenticated and system prepared");

        // ACT — perform the operation via HTTP
        var addRepositoryRequest = new
        {
            url = TEST_GITHUB_URL,
            name = TEST_REPO_NAME
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(addRepositoryRequest),
            Encoding.UTF8,
            "application/json");

        _output.WriteLine($"🔄 ACT: Adding repository {TEST_GITHUB_URL}");
        var response = await _client.PostAsync("/api/repositories", requestContent);

        // ASSERT HTTP — verify the response
        _output.WriteLine($"📤 HTTP Response Status: {response.StatusCode}");
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"📤 HTTP Response Body: {responseContent}");

        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        // Verify response structure
        Assert.True(responseData.TryGetProperty("id", out var idProperty));
        Assert.True(responseData.TryGetProperty("name", out var nameProperty));
        Assert.True(responseData.TryGetProperty("url", out var urlProperty));
        Assert.True(responseData.TryGetProperty("providerType", out var providerTypeProperty));
        Assert.True(responseData.TryGetProperty("status", out var statusProperty));

        var repositoryId = idProperty.GetInt32();
        var returnedName = nameProperty.GetString();
        var returnedUrl = urlProperty.GetString();
        var providerType = providerTypeProperty.GetString();
        var status = statusProperty.GetString();

        Assert.Equal(TEST_REPO_NAME, returnedName);
        Assert.Equal(TEST_GITHUB_URL, returnedUrl);
        Assert.Equal("GitHub", providerType);
        Assert.True(status == "Pending" || status == "Processing" || status == "Completed", 
            $"Expected status to be Pending, Processing, or Completed, but was {status}");

        _output.WriteLine($"✅ ASSERT HTTP: Repository created with ID {repositoryId}, status: {status}");

        // ASSERT DB — query the database directly and verify state
        _output.WriteLine("🔍 ASSERT DB: Verifying database state...");

        // Verify repository was created
        var dbRepository = await _dbContext.Repositories
            .Where(r => r.Url == TEST_GITHUB_URL)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(dbRepository);
        Assert.Equal(ProviderType.GitHub, dbRepository.ProviderType);
        Assert.NotEqual(RepositoryStatus.Error, dbRepository.Status);
        
        _output.WriteLine($"✅ Repository in DB: ID={dbRepository.Id}, ProviderType={dbRepository.ProviderType}, Status={dbRepository.Status}");

        // Wait a moment for metrics collection to complete (if async)
        await Task.Delay(2000);

        // Verify metrics were collected
        var metricsCount = await _dbContext.RepositoryMetrics
            .CountAsync(m => m.RepositoryId == repositoryId);

        Assert.True(metricsCount >= 1, $"Expected at least 1 metrics record, found {metricsCount}");
        _output.WriteLine($"✅ Metrics records found: {metricsCount}");

        // Verify specific metrics values
        var latestMetrics = await _dbContext.RepositoryMetrics
            .Where(m => m.RepositoryId == repositoryId)
            .OrderByDescending(m => m.MeasurementDate)
            .FirstOrDefaultAsync();

        Assert.NotNull(latestMetrics);
        Assert.True(latestMetrics.TotalContributors > 0, 
            $"Expected TotalContributors > 0, got {latestMetrics.TotalContributors}");
        Assert.True(latestMetrics.TotalFiles > 0, 
            $"Expected TotalFiles > 0, got {latestMetrics.TotalFiles}");
        Assert.True(latestMetrics.ProjectHealthScore >= 0 && latestMetrics.ProjectHealthScore <= 100, 
            $"Expected health score between 0-100, got {latestMetrics.ProjectHealthScore}");

        _output.WriteLine($"✅ Latest Metrics: Contributors={latestMetrics.TotalContributors}, " +
                         $"Files={latestMetrics.TotalFiles}, HealthScore={latestMetrics.ProjectHealthScore:F1}");

        // ASSERT LOGS — verify expected log entries were emitted
        _output.WriteLine("📝 ASSERT LOGS: Verifying log entries...");

        var startMetricsLogs = _testLogs.Where(log => 
            log.Level == LogLevel.Information && 
            log.Message.Contains("Starting metrics collection", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var completeMetricsLogs = _testLogs.Where(log => 
            log.Level == LogLevel.Information && 
            (log.Message.Contains("Metrics collection completed", StringComparison.OrdinalIgnoreCase) ||
             log.Message.Contains("Collected", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var errorLogs = _testLogs.Where(log => 
            log.Level >= LogLevel.Error && 
            log.Message.Contains(repositoryId.ToString()))
            .ToList();

        Assert.True(startMetricsLogs.Any(), "Expected to find 'Starting metrics collection' log entry");
        Assert.True(completeMetricsLogs.Any(), "Expected to find metrics completion log entry");
        Assert.Empty(errorLogs);

        _output.WriteLine($"✅ Found {startMetricsLogs.Count} start logs, {completeMetricsLogs.Count} completion logs, {errorLogs.Count} error logs");

        foreach (var log in startMetricsLogs.Take(2))
        {
            _output.WriteLine($"   📝 Start Log: {log.Message}");
        }

        foreach (var log in completeMetricsLogs.Take(2))
        {
            _output.WriteLine($"   📝 Complete Log: {log.Message}");
        }

        _output.WriteLine("🎉 IT-001: Full GitHub repository workflow completed successfully!");
    }

    private async Task<string> AuthenticateTestUserAsync()
    {
        // Register a test user
        var registerRequest = new
        {
            email = $"test-user-{Guid.NewGuid()}@github-test.com",
            password = "TestPassword123!",
            firstName = "Test",
            lastName = "User"
        };

        var registerContent = new StringContent(
            JsonSerializer.Serialize(registerRequest),
            Encoding.UTF8,
            "application/json");

        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        
        if (registerResponse.StatusCode == HttpStatusCode.Created || registerResponse.StatusCode == HttpStatusCode.OK)
        {
            var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
            var registerData = JsonSerializer.Deserialize<JsonElement>(registerResponseContent);
            
            if (registerData.TryGetProperty("token", out var tokenProperty))
            {
                return tokenProperty.GetString()!;
            }
        }

        // If registration doesn't return token, try login
        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        
        return loginData.GetProperty("token").GetString()!;
    }

    private async Task CleanupExistingRepositoryAsync(string url)
    {
        var existingRepo = await _dbContext.Repositories
            .FirstOrDefaultAsync(r => r.Url == url);

        if (existingRepo != null)
        {
            _dbContext.Repositories.Remove(existingRepo);
            await _dbContext.SaveChangesAsync();
            _output.WriteLine("🧹 Cleaned up existing repository from previous test");
        }
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _client?.Dispose();
    }
}

/// <summary>
/// Custom test application factory for GitHub integration tests
/// </summary>
public class GitHubTestWebApplicationFactory : IDisposable
{
    public IServiceProvider Services { get; private set; }

    public GitHubTestWebApplicationFactory()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add test database
        services.AddDbContext<RepoLensDbContext>(options =>
        {
            options.UseInMemoryDatabase($"github_test_db_{Guid.NewGuid()}");
        });

        // Add test logger sink
        services.AddSingleton<TestLoggerSink>();

        Services = services.BuildServiceProvider();
        
        // Initialize database
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();
        dbContext.Database.EnsureCreated();
    }

    public HttpClient CreateClient()
    {
        // For now, return a basic HttpClient for API testing
        // In a full implementation, this would create a test server
        return new HttpClient();
    }

    public void Dispose()
    {
        if (Services is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}

/// <summary>
/// Test log entry for verification
/// </summary>
public class TestLogEntry
{
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Test logger sink for capturing log entries
/// </summary>
public class TestLoggerSink : ILogger
{
    public List<TestLogEntry> Entries { get; } = new();

    public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Entries.Add(new TestLogEntry
        {
            Level = logLevel,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    private class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
