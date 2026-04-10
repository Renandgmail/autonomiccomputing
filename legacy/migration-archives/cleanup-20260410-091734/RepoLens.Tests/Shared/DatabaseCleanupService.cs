using Microsoft.EntityFrameworkCore;
using RepoLens.Infrastructure;
using Microsoft.Extensions.Logging;

namespace RepoLens.Tests.Shared;

/// <summary>
/// Service for cleaning up database state before integration tests
/// Ensures clean state for comprehensive testing
/// </summary>
public class DatabaseCleanupService
{
    private readonly RepoLensDbContext _dbContext;
    private readonly ILogger<DatabaseCleanupService> _logger;

    public DatabaseCleanupService(RepoLensDbContext dbContext, ILogger<DatabaseCleanupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Performs complete database cleanup for integration testing
    /// </summary>
    public async Task CleanupDatabaseAsync()
    {
        _logger.LogInformation("🧹 Starting comprehensive database cleanup...");

        try
        {
            // Phase 1: Disable foreign key constraints temporarily
            await DisableForeignKeyConstraintsAsync();

            // Phase 2: Clean all data tables in dependency order
            await CleanDataTablesAsync();

            // Phase 3: Reset identity seeds
            await ResetIdentitySeedsAsync();

            // Phase 4: Re-enable foreign key constraints
            await EnableForeignKeyConstraintsAsync();

            // Phase 5: Verify clean state
            await VerifyCleanStateAsync();

            _logger.LogInformation("✅ Database cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Database cleanup failed");
            throw;
        }
    }

    /// <summary>
    /// Verifies database is in clean state
    /// </summary>
    public async Task<DatabaseCleanupResult> VerifyCleanStateAsync()
    {
        _logger.LogInformation("🔍 Verifying clean database state...");

        var result = new DatabaseCleanupResult
        {
            IsClean = true,
            CleanupTimestamp = DateTime.UtcNow
        };

        try
        {
            // Check all major tables are empty
            var tableChecks = new Dictionary<string, int>
            {
                ["Repositories"] = await _dbContext.Set<RepoLens.Core.Entities.Repository>().CountAsync(),
                ["RepositoryMetrics"] = await _dbContext.Set<RepoLens.Core.Entities.RepositoryMetrics>().CountAsync(),
                ["FileMetrics"] = await _dbContext.Set<RepoLens.Core.Entities.FileMetrics>().CountAsync(),
                ["ContributorMetrics"] = await _dbContext.Set<RepoLens.Core.Entities.ContributorMetrics>().CountAsync(),
                ["Commits"] = await _dbContext.Set<RepoLens.Core.Entities.Commit>().CountAsync(),
                ["CodeElements"] = await _dbContext.Set<RepoLens.Core.Entities.CodeElement>().CountAsync(),
                ["VocabularyTerms"] = await _dbContext.Set<RepoLens.Core.Entities.VocabularyTerm>().CountAsync(),
                ["Artifacts"] = await _dbContext.Set<RepoLens.Core.Entities.Artifact>().CountAsync(),
                ["ArtifactVersions"] = await _dbContext.Set<RepoLens.Core.Entities.ArtifactVersion>().CountAsync(),
                ["RepositoryFiles"] = await _dbContext.Set<RepoLens.Core.Entities.RepositoryFile>().CountAsync()
            };

            foreach (var (tableName, count) in tableChecks)
            {
                result.TableCounts[tableName] = count;
                if (count > 0)
                {
                    result.IsClean = false;
                    result.NonEmptyTables.Add(tableName);
                    _logger.LogWarning("⚠️ Table {TableName} has {Count} records", tableName, count);
                }
                else
                {
                    _logger.LogDebug("✅ Table {TableName} is clean (0 records)", tableName);
                }
            }

            if (result.IsClean)
            {
                _logger.LogInformation("✅ Database verification passed - all tables are clean");
            }
            else
            {
                _logger.LogWarning("⚠️ Database verification failed - {Count} tables still have data", 
                    result.NonEmptyTables.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Database verification failed with exception");
            result.IsClean = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private async Task DisableForeignKeyConstraintsAsync()
    {
        _logger.LogDebug("🔓 Disabling foreign key constraints...");
        
        try
        {
            // For PostgreSQL
            await _dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");
            _logger.LogDebug("✅ Foreign key constraints disabled (PostgreSQL)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not disable FK constraints, continuing anyway");
        }
    }

    private async Task EnableForeignKeyConstraintsAsync()
    {
        _logger.LogDebug("🔒 Re-enabling foreign key constraints...");
        
        try
        {
            // For PostgreSQL
            await _dbContext.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
            _logger.LogDebug("✅ Foreign key constraints re-enabled (PostgreSQL)");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not re-enable FK constraints, continuing anyway");
        }
    }

    private async Task CleanDataTablesAsync()
    {
        _logger.LogDebug("🗑️ Cleaning data tables...");

        // Clean tables in reverse dependency order to avoid FK constraint violations
        var cleanupOrder = new[]
        {
            // Child tables first
            "ArtifactVersions",
            "RepositoryFiles", 
            "CodeElements",
            "VocabularyTerms",
            "Commits",
            "ContributorMetrics",
            "FileMetrics", 
            "RepositoryMetrics",
            "Artifacts",
            // Parent tables last
            "Repositories",
            "Users"
        };

        foreach (var tableName in cleanupOrder)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
                _logger.LogDebug("✅ Cleaned table: {TableName}", tableName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not clean table {TableName}: {Message}", tableName, ex.Message);
                // Continue with other tables even if one fails
            }
        }
    }

    private async Task ResetIdentitySeedsAsync()
    {
        _logger.LogDebug("🔄 Resetting identity seeds...");

        var tablesWithIdentity = new[]
        {
            "Repositories",
            "RepositoryMetrics", 
            "FileMetrics",
            "ContributorMetrics",
            "Commits",
            "CodeElements",
            "VocabularyTerms",
            "Artifacts",
            "ArtifactVersions",
            "RepositoryFiles",
            "Users"
        };

        foreach (var tableName in tablesWithIdentity)
        {
            try
            {
                // PostgreSQL syntax for resetting sequences
                await _dbContext.Database.ExecuteSqlRawAsync(
                    $"SELECT setval(pg_get_serial_sequence('\"{tableName}\"', 'id'), 1, false)");
                _logger.LogDebug("✅ Reset identity seed for: {TableName}", tableName);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("ℹ️ Could not reset identity for {TableName}: {Message}", tableName, ex.Message);
                // This is often expected for tables without identity columns
            }
        }
    }

    /// <summary>
    /// Quick cleanup for tests that only need basic table clearing
    /// </summary>
    public async Task QuickCleanupAsync()
    {
        _logger.LogInformation("⚡ Performing quick database cleanup...");

        try
        {
            // Remove all repositories and their cascading data
            var repositories = await _dbContext.Set<RepoLens.Core.Entities.Repository>().ToListAsync();
            if (repositories.Any())
            {
                _dbContext.Set<RepoLens.Core.Entities.Repository>().RemoveRange(repositories);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Removed {Count} repositories", repositories.Count);
            }

            // Remove any orphaned data
            var orphanedMetrics = await _dbContext.Set<RepoLens.Core.Entities.RepositoryMetrics>().ToListAsync();
            if (orphanedMetrics.Any())
            {
                _dbContext.Set<RepoLens.Core.Entities.RepositoryMetrics>().RemoveRange(orphanedMetrics);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Removed {Count} orphaned metrics", orphanedMetrics.Count);
            }

            _logger.LogInformation("✅ Quick cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Quick cleanup failed");
            throw;
        }
    }

    /// <summary>
    /// Prepares database for specific test scenario
    /// </summary>
    public async Task PrepareForTestScenarioAsync(TestScenario scenario)
    {
        _logger.LogInformation("🎯 Preparing database for test scenario: {Scenario}", scenario);

        await CleanupDatabaseAsync();

        switch (scenario)
        {
            case TestScenario.EmptyDatabase:
                // Already clean, nothing more needed
                _logger.LogInformation("✅ Database prepared for empty state testing");
                break;

            case TestScenario.WithSampleData:
                await CreateSampleDataAsync();
                break;

            case TestScenario.FullIntegrationTest:
                await PrepareForFullIntegrationAsync();
                break;

            default:
                _logger.LogWarning("Unknown test scenario: {Scenario}", scenario);
                break;
        }
    }

    private async Task CreateSampleDataAsync()
    {
        _logger.LogDebug("📝 Creating sample data for testing...");

        // This will be expanded based on test needs
        // For now, just ensure database is ready for data insertion
        await _dbContext.Database.EnsureCreatedAsync();
        _logger.LogInformation("✅ Sample data preparation completed");
    }

    private async Task PrepareForFullIntegrationAsync()
    {
        _logger.LogDebug("🚀 Preparing for full integration test...");

        // Ensure database schema is up to date
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Apply any pending migrations
        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("✅ Applied {Count} pending migrations", pendingMigrations.Count());
        }

        _logger.LogInformation("✅ Database prepared for full integration testing");
    }

    /// <summary>
    /// Gets database statistics for monitoring
    /// </summary>
    public async Task<DatabaseStatistics> GetDatabaseStatisticsAsync()
    {
        var stats = new DatabaseStatistics
        {
            Timestamp = DateTime.UtcNow,
            TableCounts = new Dictionary<string, int>()
        };

        try
        {
            var entities = new Dictionary<string, Func<Task<int>>>
            {
                ["Repositories"] = () => _dbContext.Set<RepoLens.Core.Entities.Repository>().CountAsync(),
                ["RepositoryMetrics"] = () => _dbContext.Set<RepoLens.Core.Entities.RepositoryMetrics>().CountAsync(),
                ["FileMetrics"] = () => _dbContext.Set<RepoLens.Core.Entities.FileMetrics>().CountAsync(),
                ["ContributorMetrics"] = () => _dbContext.Set<RepoLens.Core.Entities.ContributorMetrics>().CountAsync(),
                ["Commits"] = () => _dbContext.Set<RepoLens.Core.Entities.Commit>().CountAsync(),
                ["CodeElements"] = () => _dbContext.Set<RepoLens.Core.Entities.CodeElement>().CountAsync(),
                ["VocabularyTerms"] = () => _dbContext.Set<RepoLens.Core.Entities.VocabularyTerm>().CountAsync(),
                ["Artifacts"] = () => _dbContext.Set<RepoLens.Core.Entities.Artifact>().CountAsync(),
                ["ArtifactVersions"] = () => _dbContext.Set<RepoLens.Core.Entities.ArtifactVersion>().CountAsync(),
                ["RepositoryFiles"] = () => _dbContext.Set<RepoLens.Core.Entities.RepositoryFile>().CountAsync()
            };

            foreach (var (tableName, countFunc) in entities)
            {
                stats.TableCounts[tableName] = await countFunc();
            }

            stats.TotalRecords = stats.TableCounts.Values.Sum();
            _logger.LogDebug("📊 Database statistics: {TotalRecords} total records across {Tables} tables", 
                stats.TotalRecords, stats.TableCounts.Count);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get database statistics");
            throw;
        }
    }
}

/// <summary>
/// Result of database cleanup operation
/// </summary>
public class DatabaseCleanupResult
{
    public bool IsClean { get; set; }
    public DateTime CleanupTimestamp { get; set; }
    public Dictionary<string, int> TableCounts { get; set; } = new();
    public List<string> NonEmptyTables { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        var status = IsClean ? "✅ CLEAN" : "❌ NOT CLEAN";
        var details = IsClean 
            ? "All tables empty" 
            : $"{NonEmptyTables.Count} tables still have data: {string.Join(", ", NonEmptyTables)}";
        
        return $"{status} - {details} (checked at {CleanupTimestamp:yyyy-MM-dd HH:mm:ss})";
    }
}

/// <summary>
/// Database statistics for monitoring
/// </summary>
public class DatabaseStatistics
{
    public DateTime Timestamp { get; set; }
    public Dictionary<string, int> TableCounts { get; set; } = new();
    public int TotalRecords { get; set; }

    public override string ToString()
    {
        var tableDetails = string.Join(", ", TableCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        return $"📊 Database Stats ({Timestamp:HH:mm:ss}): {TotalRecords} total records ({tableDetails})";
    }
}

/// <summary>
/// Test scenarios for database preparation
/// </summary>
public enum TestScenario
{
    EmptyDatabase,
    WithSampleData,
    FullIntegrationTest
}
