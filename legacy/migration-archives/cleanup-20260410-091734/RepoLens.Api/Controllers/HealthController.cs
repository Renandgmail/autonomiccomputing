using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Services;
using RepoLens.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace RepoLens.Api.Controllers;

/// <summary>
/// P5-003: Health check endpoints
/// Provides health status for database, storage, and provider configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Route("health")] // Additional route for Docker health checks
public class HealthController : ControllerBase
{
    private readonly RepoLensDbContext _dbContext;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        RepoLensDbContext dbContext,
        IGitProviderFactory gitProviderFactory,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _gitProviderFactory = gitProviderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthStatus = new HealthStatus
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetApplicationVersion(),
                Uptime = GetUptime(),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            // Check database connectivity
            healthStatus.Database = await CheckDatabaseHealth();
            
            // Check storage accessibility
            healthStatus.Storage = CheckStorageHealth();
            
            // Check provider configuration
            healthStatus.Providers = CheckProviderConfiguration();

            // Determine overall status
            var isHealthy = healthStatus.Database.IsHealthy && 
                           healthStatus.Storage.IsHealthy && 
                           healthStatus.Providers.IsHealthy;

            healthStatus.Status = isHealthy ? "Healthy" : "Unhealthy";

            var statusCode = isHealthy ? 200 : 503;
            return StatusCode(statusCode, healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            return StatusCode(503, new HealthStatus
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Detailed health check with additional diagnostics
    /// </summary>
    /// <returns>Detailed health status</returns>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        var healthStatus = await GetDetailedHealthStatus();
        var statusCode = healthStatus.Status == "Healthy" ? 200 : 503;
        return StatusCode(statusCode, healthStatus);
    }

    /// <summary>
    /// Readiness probe for Kubernetes/Docker
    /// </summary>
    /// <returns>Simple ready/not ready response</returns>
    [HttpGet("ready")]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            // Check if application is ready to serve requests
            var canConnectToDb = await _dbContext.Database.CanConnectAsync();
            
            if (canConnectToDb)
            {
                return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
            }
            else
            {
                return StatusCode(503, new { status = "Not Ready", reason = "Database not accessible" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { status = "Not Ready", reason = ex.Message });
        }
    }

    /// <summary>
    /// Liveness probe for Kubernetes/Docker
    /// </summary>
    /// <returns>Simple alive/dead response</returns>
    [HttpGet("live")]
    public IActionResult GetLiveness()
    {
        // Basic liveness check - if this endpoint responds, the app is alive
        return Ok(new { status = "Alive", timestamp = DateTime.UtcNow });
    }

    private async Task<ComponentHealth> CheckDatabaseHealth()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                return new ComponentHealth
                {
                    IsHealthy = false,
                    Status = "Cannot connect to database",
                    ResponseTime = 0
                };
            }

            var startTime = DateTime.UtcNow;
            var userCount = await _dbContext.Users.CountAsync();
            var repositoryCount = await _dbContext.Repositories.CountAsync();
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ComponentHealth
            {
                IsHealthy = true,
                Status = "Connected",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    { "UserCount", userCount },
                    { "RepositoryCount", repositoryCount },
                    { "DatabaseProvider", _dbContext.Database.ProviderName ?? "Unknown" }
                }
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                IsHealthy = false,
                Status = $"Database error: {ex.Message}",
                ResponseTime = 0
            };
        }
    }

    private ComponentHealth CheckStorageHealth()
    {
        try
        {
            // Check if storage paths are accessible
            var tempPath = Path.GetTempPath();
            var testFile = Path.Combine(tempPath, $"health_check_{Guid.NewGuid()}.tmp");
            
            var startTime = DateTime.UtcNow;
            System.IO.File.WriteAllText(testFile, "health check");
            var content = System.IO.File.ReadAllText(testFile);
            System.IO.File.Delete(testFile);
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            var isHealthy = content == "health check";

            return new ComponentHealth
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "Accessible" : "Storage test failed",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    { "TempPath", tempPath },
                    { "AvailableSpace", GetAvailableDiskSpace(tempPath) }
                }
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                IsHealthy = false,
                Status = $"Storage error: {ex.Message}",
                ResponseTime = 0
            };
        }
    }

    private ComponentHealth CheckProviderConfiguration()
    {
        try
        {
            var providerTypes = new[] { "https://github.com/test/repo", "file:///test/path" };
            var workingProviders = new List<string>();
            var failedProviders = new List<string>();

            foreach (var testUrl in providerTypes)
            {
                try
                {
                    var provider = _gitProviderFactory.GetProvider(testUrl);
                    workingProviders.Add(provider.GetType().Name);
                }
                catch (Exception ex)
                {
                    failedProviders.Add($"{testUrl}: {ex.Message}");
                }
            }

            var isHealthy = workingProviders.Count > 0;

            return new ComponentHealth
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "Providers configured" : "No working providers",
                ResponseTime = 0,
                Details = new Dictionary<string, object>
                {
                    { "WorkingProviders", workingProviders },
                    { "FailedProviders", failedProviders }
                }
            };
        }
        catch (Exception ex)
        {
            return new ComponentHealth
            {
                IsHealthy = false,
                Status = $"Provider configuration error: {ex.Message}",
                ResponseTime = 0
            };
        }
    }

    private async Task<DetailedHealthStatus> GetDetailedHealthStatus()
    {
        var healthStatus = new DetailedHealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetApplicationVersion(),
            Uptime = GetUptime(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        // System information
        healthStatus.System = new SystemHealth
        {
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = Environment.WorkingSet,
            OSVersion = Environment.OSVersion.ToString(),
            MachineName = Environment.MachineName
        };

        // Check all components
        healthStatus.Database = await CheckDatabaseHealth();
        healthStatus.Storage = CheckStorageHealth();
        healthStatus.Providers = CheckProviderConfiguration();

        // Determine overall status
        var isHealthy = healthStatus.Database.IsHealthy && 
                       healthStatus.Storage.IsHealthy && 
                       healthStatus.Providers.IsHealthy;

        healthStatus.Status = isHealthy ? "Healthy" : "Unhealthy";

        return healthStatus;
    }

    private string GetApplicationVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString() ?? "Unknown";
    }

    private TimeSpan GetUptime()
    {
        return DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
    }

    private long GetAvailableDiskSpace(string path)
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(path) ?? path);
            return drive.AvailableFreeSpace;
        }
        catch
        {
            return -1;
        }
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Version { get; set; }
    public TimeSpan? Uptime { get; set; }
    public string? Environment { get; set; }
    public ComponentHealth Database { get; set; } = new();
    public ComponentHealth Storage { get; set; } = new();
    public ComponentHealth Providers { get; set; } = new();
    public string? Error { get; set; }
}

public class DetailedHealthStatus : HealthStatus
{
    public SystemHealth System { get; set; } = new();
}

public class ComponentHealth
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public double ResponseTime { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

public class SystemHealth
{
    public int ProcessorCount { get; set; }
    public long WorkingSet { get; set; }
    public string OSVersion { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
}
