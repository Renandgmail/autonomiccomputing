using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepoLens.Infrastructure;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Integration;

/// <summary>
/// Direct database query test to check repository status
/// </summary>
public class DatabaseQueryTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly RepoLensDbContext _dbContext;

    private const string AUTONOMIC_REPO_URL = "https://github.com/Renandgmail/autonomiccomputing.git";
    private const string AUTONOMIC_REPO_URL_ALT = "https://github.com/Renandgmail/autonomiccomputing";

    public DatabaseQueryTest(ITestOutputHelper output)
    {
        _output = output;
        _serviceProvider = SetupServices();
        _dbContext = _serviceProvider.GetRequiredService<RepoLensDbContext>();
        
        _output.WriteLine("🔍 Direct Database Query for Autonomic Computing Repository");
        _output.WriteLine($"Target URL: {AUTONOMIC_REPO_URL}");
        _output.WriteLine($"Database: repolens_db (Main UI Database)");
    }

    [Fact]
    public async Task QueryDatabase_AutonomicComputingRepository_ShowCurrentStatus()
    {
        _output.WriteLine("\n📋 === DIRECT DATABASE QUERY RESULTS ===");
        
        try
        {
            // Query 1: Check if repository exists with exact URL
            var exactRepository = await _dbContext.Repositories
                .FirstOrDefaultAsync(r => r.Url == AUTONOMIC_REPO_URL);

            // Query 2: Check if repository exists with alternative URL (without .git)
            var altRepository = await _dbContext.Repositories
                .FirstOrDefaultAsync(r => r.Url == AUTONOMIC_REPO_URL_ALT);

            // Query 3: Check if repository exists with partial URL match
            var partialRepository = await _dbContext.Repositories
                .FirstOrDefaultAsync(r => r.Url.Contains("autonomiccomputing"));

            _output.WriteLine("🔍 Repository Search Results:");
            _output.WriteLine($"   • Exact URL match ({AUTONOMIC_REPO_URL}): {(exactRepository != null ? "FOUND" : "NOT FOUND")}");
            _output.WriteLine($"   • Alt URL match ({AUTONOMIC_REPO_URL_ALT}): {(altRepository != null ? "FOUND" : "NOT FOUND")}");
            _output.WriteLine($"   • Partial match (autonomiccomputing): {(partialRepository != null ? "FOUND" : "NOT FOUND")}");

            var repository = exactRepository ?? altRepository ?? partialRepository;

            if (repository != null)
            {
                _output.WriteLine("\n✅ === REPOSITORY FOUND ===");
                await DisplayRepositoryDetailsAsync(repository);
                await DisplayRepositoryMetricsAsync(repository.Id);
                await DisplayVocabularyTermsAsync(repository.Id);
                await DisplayRepositoryFilesAsync(repository.Id);
                await DisplayCodeElementsAsync(repository.Id);
                await DisplayContributorMetricsAsync(repository.Id);
            }
            else
            {
                _output.WriteLine("\n❌ === REPOSITORY NOT FOUND ===");
                await DisplayAllRepositoriesAsync();
                await DisplayDatabaseStatisticsAsync();
            }

            _output.WriteLine("\n📊 === DATABASE SUMMARY ===");
            await DisplayOverallDatabaseStatsAsync();
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Database query failed: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task DisplayRepositoryDetailsAsync(RepoLens.Core.Entities.Repository repository)
    {
        _output.WriteLine($"Repository Details:");
        _output.WriteLine($"   • ID: {repository.Id}");
        _output.WriteLine($"   • Name: {repository.Name}");
        _output.WriteLine($"   • URL: {repository.Url}");
        _output.WriteLine($"   • Description: {repository.Description ?? "No description"}");
        _output.WriteLine($"   • Status: {repository.Status}");
        _output.WriteLine($"   • Type: {repository.Type}");
        _output.WriteLine($"   • Default Branch: {repository.DefaultBranch ?? "Not specified"}");
        _output.WriteLine($"   • Auto Sync: {repository.AutoSync}");
        _output.WriteLine($"   • Sync Interval: {repository.SyncIntervalMinutes} minutes");
        _output.WriteLine($"   • Is Local: {repository.IsLocal}");
        _output.WriteLine($"   • Local Path: {repository.LocalPath ?? "Not specified"}");
        _output.WriteLine($"   • Created: {repository.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        _output.WriteLine($"   • Last Sync: {repository.LastSyncAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
        _output.WriteLine($"   • Last Analysis: {repository.LastAnalysisAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}");
        _output.WriteLine($"   • Owner ID: {repository.OwnerId}");

        if (repository.Tags != null && repository.Tags.Any())
        {
            _output.WriteLine($"   • Tags: {string.Join(", ", repository.Tags)}");
        }

        if (!string.IsNullOrEmpty(repository.Notes))
        {
            _output.WriteLine($"   • Notes: {repository.Notes}");
        }
    }

    private async Task DisplayRepositoryMetricsAsync(int repositoryId)
    {
        var metrics = await _dbContext.RepositoryMetrics
            .Where(m => m.RepositoryId == repositoryId)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        if (metrics != null)
        {
            _output.WriteLine("\n📈 Repository Metrics:");
            _output.WriteLine($"   • Total Files: {metrics.TotalFiles:N0}");
            _output.WriteLine($"   • Total Lines: {metrics.TotalLines:N0}");
            _output.WriteLine($"   • Total Size: {metrics.TotalSize:N0} bytes ({metrics.TotalSize / 1024.0 / 1024.0:F1} MB)");
            _output.WriteLine($"   • Total Commits: {metrics.TotalCommits:N0}");
            _output.WriteLine($"   • Total Contributors: {metrics.TotalContributors}");
            _output.WriteLine($"   • Active Contributors: {metrics.ActiveContributors}");
            _output.WriteLine($"   • Last Commit: {metrics.LastCommitDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}");
            _output.WriteLine($"   • Quality Score: {metrics.QualityScore:F1}/100");
            _output.WriteLine($"   • Security Score: {metrics.SecurityScore:F1}/100");
            _output.WriteLine($"   • Maintainability Index: {metrics.MaintainabilityIndex:F1}/100");
            _output.WriteLine($"   • Technical Debt: {metrics.TechnicalDebt:F1}%");

            if (!string.IsNullOrEmpty(metrics.LanguageDistribution))
            {
                try
                {
                    var languages = JsonSerializer.Deserialize<Dictionary<string, object>>(metrics.LanguageDistribution);
                    _output.WriteLine($"   • Languages: {string.Join(", ", languages.Select(kv => $"{kv.Key}({kv.Value})"))}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"   • Language Distribution: {metrics.LanguageDistribution} (Parse error: {ex.Message})");
                }
            }

            if (!string.IsNullOrEmpty(metrics.ComplexityMetrics))
            {
                _output.WriteLine($"   • Complexity Metrics: {metrics.ComplexityMetrics}");
            }

            _output.WriteLine($"   • Metrics Recorded: {metrics.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            _output.WriteLine($"   • Last Updated: {metrics.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            _output.WriteLine("\n📈 Repository Metrics: ❌ No metrics found");
        }
    }

    private async Task DisplayVocabularyTermsAsync(int repositoryId)
    {
        var vocabularyTerms = await _dbContext.VocabularyTerms
            .Where(v => v.RepositoryId == repositoryId)
            .OrderByDescending(v => v.RelevanceScore)
            .ToListAsync();

        if (vocabularyTerms.Any())
        {
            _output.WriteLine($"\n🧠 Vocabulary Terms ({vocabularyTerms.Count} total):");
            
            var businessTerms = vocabularyTerms.Where(v => v.TermType == RepoLens.Core.Entities.VocabularyTermType.BusinessTerm).ToList();
            var technicalTerms = vocabularyTerms.Where(v => v.TermType == RepoLens.Core.Entities.VocabularyTermType.TechnicalTerm).ToList();
            var domainTerms = vocabularyTerms.Where(v => v.TermType == RepoLens.Core.Entities.VocabularyTermType.DomainSpecific).ToList();

            if (businessTerms.Any())
            {
                _output.WriteLine($"   📊 Business Terms ({businessTerms.Count}):");
                foreach (var term in businessTerms.Take(5))
                {
                    _output.WriteLine($"      • {term.Term} (Score: {term.RelevanceScore:F2}, Freq: {term.Frequency})");
                    _output.WriteLine($"        Context: {term.Context}");
                    if (term.Synonyms != null && term.Synonyms.Any())
                    {
                        _output.WriteLine($"        Synonyms: {string.Join(", ", term.Synonyms)}");
                    }
                }
            }

            if (technicalTerms.Any())
            {
                _output.WriteLine($"   ⚙️ Technical Terms ({technicalTerms.Count}):");
                foreach (var term in technicalTerms.Take(5))
                {
                    _output.WriteLine($"      • {term.Term} (Score: {term.RelevanceScore:F2}, Freq: {term.Frequency})");
                    _output.WriteLine($"        Context: {term.Context}");
                }
            }

            if (domainTerms.Any())
            {
                _output.WriteLine($"   🎯 Domain-Specific Terms ({domainTerms.Count}):");
                foreach (var term in domainTerms.Take(5))
                {
                    _output.WriteLine($"      • {term.Term} (Score: {term.RelevanceScore:F2}, Freq: {term.Frequency})");
                }
            }
        }
        else
        {
            _output.WriteLine("\n🧠 Vocabulary Terms: ❌ No vocabulary terms found");
        }
    }

    private async Task DisplayRepositoryFilesAsync(int repositoryId)
    {
        var files = await _dbContext.RepositoryFiles
            .Where(f => f.RepositoryId == repositoryId)
            .OrderBy(f => f.FilePath)
            .ToListAsync();

        if (files.Any())
        {
            _output.WriteLine($"\n📁 Repository Files ({files.Count} total):");
            
            var groupedByLanguage = files.GroupBy(f => f.Language).OrderByDescending(g => g.Count());
            foreach (var group in groupedByLanguage.Take(5))
            {
                _output.WriteLine($"   • {group.Key}: {group.Count()} files");
                foreach (var file in group.Take(3))
                {
                    _output.WriteLine($"     - {file.FilePath} ({file.LineCount} lines, {file.FileSize} bytes)");
                }
                if (group.Count() > 3)
                {
                    _output.WriteLine($"     ... and {group.Count() - 3} more files");
                }
            }
        }
        else
        {
            _output.WriteLine("\n📁 Repository Files: ❌ No files found");
        }
    }

    private async Task DisplayCodeElementsAsync(int repositoryId)
    {
        var codeElements = await _dbContext.CodeElements
            .Where(c => _dbContext.RepositoryFiles.Any(f => f.Id == c.FileId && f.RepositoryId == repositoryId))
            .ToListAsync();

        if (codeElements.Any())
        {
            _output.WriteLine($"\n🔧 Code Elements ({codeElements.Count} total):");
            
            var groupedByType = codeElements.GroupBy(c => c.ElementType).OrderByDescending(g => g.Count());
            foreach (var group in groupedByType)
            {
                _output.WriteLine($"   • {group.Key}: {group.Count()} elements");
                foreach (var element in group.Take(3))
                {
                    _output.WriteLine($"     - {element.Name} (Line {element.StartLine}, Access: {element.AccessModifier})");
                }
                if (group.Count() > 3)
                {
                    _output.WriteLine($"     ... and {group.Count() - 3} more elements");
                }
            }
        }
        else
        {
            _output.WriteLine("\n🔧 Code Elements: ❌ No code elements found");
        }
    }

    private async Task DisplayContributorMetricsAsync(int repositoryId)
    {
        var contributors = await _dbContext.ContributorMetrics
            .Where(c => c.RepositoryId == repositoryId)
            .OrderByDescending(c => c.CommitCount)
            .ToListAsync();

        if (contributors.Any())
        {
            _output.WriteLine($"\n👥 Contributors ({contributors.Count} total):");
            foreach (var contributor in contributors)
            {
                _output.WriteLine($"   • {contributor.Name} ({contributor.Email})");
                _output.WriteLine($"     - Commits: {contributor.CommitCount}");
                _output.WriteLine($"     - Lines Added: {contributor.LinesAdded:N0}");
                _output.WriteLine($"     - Lines Deleted: {contributor.LinesDeleted:N0}");
                _output.WriteLine($"     - Files Changed: {contributor.FilesChanged}");
                _output.WriteLine($"     - First Commit: {contributor.FirstCommit:yyyy-MM-dd}");
                _output.WriteLine($"     - Last Commit: {contributor.LastCommit:yyyy-MM-dd}");
                _output.WriteLine($"     - Primary: {contributor.IsPrimaryContributor}");
            }
        }
        else
        {
            _output.WriteLine("\n👥 Contributors: ❌ No contributors found");
        }
    }

    private async Task DisplayAllRepositoriesAsync()
    {
        var allRepos = await _dbContext.Repositories
            .OrderBy(r => r.Name)
            .ToListAsync();

        _output.WriteLine($"\n📋 All Repositories in Database ({allRepos.Count} total):");
        
        if (!allRepos.Any())
        {
            _output.WriteLine("   ❌ No repositories found in database");
            return;
        }

        foreach (var repo in allRepos)
        {
            _output.WriteLine($"   • ID: {repo.Id} | Name: {repo.Name}");
            _output.WriteLine($"     URL: {repo.Url}");
            _output.WriteLine($"     Status: {repo.Status} | Type: {repo.Type}");
            _output.WriteLine($"     Created: {repo.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            _output.WriteLine("");
        }
    }

    private async Task DisplayDatabaseStatisticsAsync()
    {
        var totalRepos = await _dbContext.Repositories.CountAsync();
        var activeRepos = await _dbContext.Repositories.CountAsync(r => r.Status == RepoLens.Core.Entities.RepositoryStatus.Active);
        var totalMetrics = await _dbContext.RepositoryMetrics.CountAsync();
        var totalVocabulary = await _dbContext.VocabularyTerms.CountAsync();
        var totalFiles = await _dbContext.RepositoryFiles.CountAsync();
        var totalCodeElements = await _dbContext.CodeElements.CountAsync();
        var totalContributors = await _dbContext.ContributorMetrics.CountAsync();

        _output.WriteLine("\n📊 Database Statistics:");
        _output.WriteLine($"   • Total Repositories: {totalRepos}");
        _output.WriteLine($"   • Active Repositories: {activeRepos}");
        _output.WriteLine($"   • Repository Metrics Records: {totalMetrics}");
        _output.WriteLine($"   • Vocabulary Terms: {totalVocabulary}");
        _output.WriteLine($"   • Repository Files: {totalFiles}");
        _output.WriteLine($"   • Code Elements: {totalCodeElements}");
        _output.WriteLine($"   • Contributor Records: {totalContributors}");
    }

    private async Task DisplayOverallDatabaseStatsAsync()
    {
        var dbStats = new
        {
            TotalRepositories = await _dbContext.Repositories.CountAsync(),
            ActiveRepositories = await _dbContext.Repositories.CountAsync(r => r.Status == RepoLens.Core.Entities.RepositoryStatus.Active),
            TotalMetrics = await _dbContext.RepositoryMetrics.CountAsync(),
            TotalVocabularyTerms = await _dbContext.VocabularyTerms.CountAsync(),
            TotalFiles = await _dbContext.RepositoryFiles.CountAsync(),
            TotalCodeElements = await _dbContext.CodeElements.CountAsync(),
            TotalContributors = await _dbContext.ContributorMetrics.CountAsync(),
            TotalUsers = await _dbContext.Users.CountAsync()
        };

        _output.WriteLine("Overall Database Status:");
        _output.WriteLine($"   • Repositories: {dbStats.TotalRepositories} total, {dbStats.ActiveRepositories} active");
        _output.WriteLine($"   • Metrics: {dbStats.TotalMetrics} records");
        _output.WriteLine($"   • Vocabulary: {dbStats.TotalVocabularyTerms} terms");
        _output.WriteLine($"   • Files: {dbStats.TotalFiles} indexed");
        _output.WriteLine($"   • Code Elements: {dbStats.TotalCodeElements} parsed");
        _output.WriteLine($"   • Contributors: {dbStats.TotalContributors} tracked");
        _output.WriteLine($"   • Users: {dbStats.TotalUsers} registered");

        // Check if this looks like it should show in dashboard
        if (dbStats.TotalRepositories >= 10)
        {
            _output.WriteLine($"\n🎯 Dashboard Display Analysis:");
            _output.WriteLine($"   • Expected UI Count: {dbStats.TotalRepositories} repositories");
            _output.WriteLine($"   • If UI shows only 10: Check pagination/filtering logic");
            _output.WriteLine($"   • If autonomic repo missing: Check repository status and visibility");
        }
    }

    private IServiceProvider SetupServices()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Use MAIN database (same as UI) - repolens_db
        var connectionString = "Host=localhost;Database=repolens_db;Username=postgres;Password=TCEP;Port=5432";
        services.AddDbContext<RepoLensDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors());
        
        return services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        if (_serviceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}
