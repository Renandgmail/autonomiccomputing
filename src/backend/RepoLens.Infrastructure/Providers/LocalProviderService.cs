using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Exceptions;
using RepoLens.Core.Services;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using GitRepository = LibGit2Sharp.Repository;
using RepoNotFoundException = RepoLens.Core.Exceptions.RepositoryNotFoundException;

namespace RepoLens.Infrastructure.Providers;

/// <summary>
/// Git provider service for local repositories using LibGit2Sharp
/// Handles file://, absolute, and relative paths with security controls
/// </summary>
public class LocalProviderService : IGitProviderService
{
    private readonly ILogger<LocalProviderService> _logger;
    private readonly IConfiguration _configuration;
    
    // Default allowed paths - can be overridden via configuration
    private readonly HashSet<string> _defaultAllowedPaths = new()
    {
        @"C:\repos",
        @"C:\projects", 
        @"C:\dev",
        @"/home/repos",
        @"/home/projects",
        @"/home/dev",
        @"./repos",
        @"../repos",
        @"./test-fixtures"
    };

    public LocalProviderService(ILogger<LocalProviderService> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public ProviderType ProviderType => ProviderType.Local;

    public bool CanHandle(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return false;

        // Handle file:// protocol
        if (repositoryUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            return true;

        // Handle absolute paths (Unix/Linux/macOS: starts with /)
        if (repositoryUrl.StartsWith("/"))
            return true;

        // Handle Windows absolute paths (C:\, D:\, etc.)
        if (repositoryUrl.Length >= 3 && char.IsLetter(repositoryUrl[0]) && 
            repositoryUrl[1] == ':' && (repositoryUrl[2] == '\\' || repositoryUrl[2] == '/'))
            return true;

        // Handle relative paths (./ or ../)
        if (repositoryUrl.StartsWith("./") || repositoryUrl.StartsWith("../"))
            return true;

        return false;
    }

    public async Task<ProviderValidationResult> ValidateAccessAsync(string repositoryUrl, CancellationToken ct = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(repositoryUrl);

            if (!CanHandle(repositoryUrl))
            {
                return ProviderValidationResult.Failure(
                    "Repository URL is not a valid local path format");
            }

            var normalizedPath = NormalizePath(repositoryUrl);
            
            // Security check: Validate path is in allowed list
            if (!IsPathAllowed(normalizedPath))
            {
                _logger.LogWarning("Access denied to path outside allowed directories: {Path}", normalizedPath);
                return ProviderValidationResult.Failure(
                    "Access denied: Repository path is outside allowed directories",
                    $"Path: {normalizedPath}");
            }

            // Check if path exists
            if (!Directory.Exists(normalizedPath))
            {
                return ProviderValidationResult.Failure(
                    "Repository path does not exist",
                    $"Path: {normalizedPath}");
            }

            // Check if it's a Git repository
            if (!GitRepository.IsValid(normalizedPath))
            {
                return ProviderValidationResult.Failure(
                    "Path is not a valid Git repository",
                    $"Path: {normalizedPath}");
            }

            _logger.LogInformation("Successfully validated local repository: {Path}", normalizedPath);
            return ProviderValidationResult.Success($"Valid Git repository at: {normalizedPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate local repository: {Url}", repositoryUrl);
            return ProviderValidationResult.Failure(
                "Failed to validate repository access",
                ex.Message);
        }
    }

    public async Task<RepositoryMetrics> CollectMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Local)
            throw new InvalidOperationException($"LocalProviderService cannot handle {context.ProviderType} repositories");

        var normalizedPath = NormalizePath(context.Url);
        
        if (!IsPathAllowed(normalizedPath))
            throw new SecurityException($"Access denied to path: {normalizedPath}");

        if (!Directory.Exists(normalizedPath))
            throw new RepoNotFoundException($"Repository not found at path: {normalizedPath}");

        try
        {
            _logger.LogInformation("Collecting repository metrics for: {Path}", normalizedPath);

            using var repo = new GitRepository(normalizedPath);
            
            var metrics = new RepositoryMetrics
            {
                RepositoryId = context.RepositoryId,
                MeasurementDate = DateTime.UtcNow
            };

            // Basic repository info
            await PopulateBasicMetricsAsync(repo, metrics, ct);
            
            // Code analysis
            await PopulateCodeMetricsAsync(normalizedPath, metrics, ct);
            
            // Activity metrics
            await PopulateActivityMetricsAsync(repo, metrics, ct);

            _logger.LogInformation(
                "Collected metrics for repository {RepositoryId}: {TotalLines} lines, {CommitCount} commits", 
                context.RepositoryId, metrics.TotalLinesOfCode, metrics.CommitsLastQuarter);

            return metrics;
        }
        catch (Exception ex) when (!(ex is SecurityException || ex is RepoNotFoundException))
        {
            _logger.LogError(ex, "Failed to collect metrics for repository: {Path}", normalizedPath);
            throw new GitException($"Failed to collect repository metrics: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<ContributorMetrics>> CollectContributorMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Local)
            throw new InvalidOperationException($"LocalProviderService cannot handle {context.ProviderType} repositories");

        var normalizedPath = NormalizePath(context.Url);
        
        if (!IsPathAllowed(normalizedPath))
            throw new SecurityException($"Access denied to path: {normalizedPath}");

        if (!Directory.Exists(normalizedPath))
            throw new RepoNotFoundException($"Repository not found at path: {normalizedPath}");

        try
        {
            _logger.LogInformation("Collecting contributor metrics for: {Path}", normalizedPath);

            using var repo = new GitRepository(normalizedPath);
            var contributors = new List<ContributorMetrics>();
            var periodStart = DateTime.UtcNow.AddDays(-90);
            var periodEnd = DateTime.UtcNow;

            // Group commits by contributor
            var contributorCommits = repo.Commits
                .Where(c => c.Author.When.DateTime >= periodStart && c.Author.When.DateTime <= periodEnd)
                .GroupBy(c => new { c.Author.Name, c.Author.Email })
                .ToList();

            foreach (var contributorGroup in contributorCommits)
            {
                var commits = contributorGroup.ToList();
                var contributor = new ContributorMetrics
                {
                    RepositoryId = context.RepositoryId,
                    ContributorName = contributorGroup.Key.Name,
                    ContributorEmail = contributorGroup.Key.Email,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    
                    CommitCount = commits.Count,
                    WorkingDays = commits.Select(c => c.Author.When.Date).Distinct().Count(),
                    FirstContribution = commits.Min(c => c.Author.When.DateTime),
                    LastContribution = commits.Max(c => c.Author.When.DateTime),
                    
                    // Calculate activity patterns
                    HourlyActivityPattern = JsonSerializer.Serialize(
                        commits.GroupBy(c => c.Author.When.Hour)
                               .ToDictionary(g => g.Key.ToString(), g => g.Count())),
                               
                    // Basic contribution metrics
                    ContributionPercentage = (double)commits.Count / contributorCommits.Sum(g => g.Count()) * 100,
                    CommitFrequency = (double)commits.Count / 90, // commits per day over 90 days
                    AverageCommitSize = commits.Average(c => c.Tree.Count), // approximate
                    
                    // Contributor classification
                    IsCoreContributor = commits.Count >= 10,
                    IsNewContributor = (DateTime.UtcNow - commits.Min(c => c.Author.When.DateTime)).TotalDays <= 30
                };

                contributors.Add(contributor);
                ct.ThrowIfCancellationRequested();
            }

            _logger.LogInformation("Collected metrics for {Count} contributors", contributors.Count);
            return contributors.AsReadOnly();
        }
        catch (Exception ex) when (!(ex is SecurityException || ex is RepoNotFoundException))
        {
            _logger.LogError(ex, "Failed to collect contributor metrics for repository: {Path}", normalizedPath);
            throw new GitException($"Failed to collect contributor metrics: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<FileMetrics>> CollectFileMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Local)
            throw new InvalidOperationException($"LocalProviderService cannot handle {context.ProviderType} repositories");

        var normalizedPath = NormalizePath(context.Url);
        
        if (!IsPathAllowed(normalizedPath))
            throw new SecurityException($"Access denied to path: {normalizedPath}");

        if (!Directory.Exists(normalizedPath))
            throw new RepoNotFoundException($"Repository not found at path: {normalizedPath}");

        try
        {
            _logger.LogInformation("Collecting file metrics for: {Path}", normalizedPath);

            using var repo = new GitRepository(normalizedPath);
            var fileMetrics = new List<FileMetrics>();

            // Get all files in the latest commit
            var latestCommit = repo.Head.Tip;
            if (latestCommit == null)
            {
                _logger.LogWarning("No commits found in repository: {Path}", normalizedPath);
                return fileMetrics.AsReadOnly();
            }

            foreach (var treeEntry in latestCommit.Tree)
            {
                if (treeEntry.TargetType == TreeEntryTargetType.Blob)
                {
                    var filePath = treeEntry.Path;
                    var blob = (Blob)treeEntry.Target;
                    
                    var fileMetric = new FileMetrics
                    {
                        RepositoryId = context.RepositoryId,
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        FileExtension = Path.GetExtension(filePath),
                        LastAnalyzed = DateTime.UtcNow,
                        
                        FileSizeBytes = blob.Size,
                        PrimaryLanguage = DetermineLanguageFromExtension(Path.GetExtension(filePath)),
                        
                        // Count commits that touched this file
                        TotalCommits = repo.Commits.Where(c => c.Tree[filePath] != null).Count(),
                        
                        // Basic file categorization
                        IsTestFile = IsTestFile(filePath),
                        IsGeneratedCode = IsGeneratedCode(filePath),
                        FileCategory = DetermineFileCategory(filePath)
                    };

                    // Calculate lines if it's a text file
                    if (blob.IsBinary == false)
                    {
                        var content = blob.GetContentText();
                        var lines = content.Split('\n');
                        fileMetric.LineCount = lines.Length;
                        fileMetric.BlankLineCount = lines.Count(string.IsNullOrWhiteSpace);
                        fileMetric.CommentLineCount = CountCommentLines(lines, fileMetric.FileExtension);
                        fileMetric.EffectiveLineCount = fileMetric.LineCount - fileMetric.BlankLineCount - fileMetric.CommentLineCount;
                    }

                    fileMetrics.Add(fileMetric);
                    ct.ThrowIfCancellationRequested();
                }
            }

            _logger.LogInformation("Collected metrics for {Count} files", fileMetrics.Count);
            return fileMetrics.AsReadOnly();
        }
        catch (Exception ex) when (!(ex is SecurityException || ex is RepoNotFoundException))
        {
            _logger.LogError(ex, "Failed to collect file metrics for repository: {Path}", normalizedPath);
            throw new GitException($"Failed to collect file metrics: {ex.Message}", ex);
        }
    }

    #region Private Helper Methods

    private string NormalizePath(string path)
    {
        // Handle file:// URLs
        if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            path = path[7..]; // Remove "file://"
        }

        // Convert to absolute path if relative
        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(path);
        }

        // Normalize path separators
        return Path.GetFullPath(path);
    }

    private bool IsPathAllowed(string normalizedPath)
    {
        var allowedPaths = GetAllowedPaths();
        
        return allowedPaths.Any(allowedPath =>
        {
            var normalizedAllowedPath = Path.GetFullPath(allowedPath);
            return normalizedPath.StartsWith(normalizedAllowedPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    private HashSet<string> GetAllowedPaths()
    {
        var configPaths = _configuration.GetSection("LocalRepositories:AllowedPaths").Get<string[]>();
        if (configPaths?.Length > 0)
        {
            return configPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return _defaultAllowedPaths;
    }

    private async Task PopulateBasicMetricsAsync(GitRepository repo, RepositoryMetrics metrics, CancellationToken ct)
    {
        // Commit counts
        var allCommits = repo.Commits.ToList();
        var now = DateTime.UtcNow;
        
        metrics.CommitsLastWeek = allCommits.Count(c => (now - c.Author.When.DateTime).TotalDays <= 7);
        metrics.CommitsLastMonth = allCommits.Count(c => (now - c.Author.When.DateTime).TotalDays <= 30);
        metrics.CommitsLastQuarter = allCommits.Count(c => (now - c.Author.When.DateTime).TotalDays <= 90);

        // Contributor counts
        var allContributors = allCommits
            .Select(c => new { c.Author.Name, c.Author.Email })
            .Distinct()
            .ToList();
            
        metrics.TotalContributors = allContributors.Count;
        metrics.ActiveContributors = allCommits
            .Where(c => (now - c.Author.When.DateTime).TotalDays <= 30)
            .Select(c => new { c.Author.Name, c.Author.Email })
            .Distinct()
            .Count();

        await Task.CompletedTask; // Make async for consistency
    }

    private async Task PopulateCodeMetricsAsync(string repoPath, RepositoryMetrics metrics, CancellationToken ct)
    {
        var files = Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories)
            .Where(f => !IsGitInternalFile(f))
            .ToList();

        metrics.TotalFiles = files.Count;
        metrics.TotalDirectories = Directory.GetDirectories(repoPath, "*", SearchOption.AllDirectories).Length;

        var totalLines = 0;
        var totalBytes = 0L;
        var languageStats = new Dictionary<string, int>();

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var fileInfo = new FileInfo(file);
                totalBytes += fileInfo.Length;

                var extension = Path.GetExtension(file);
                var language = DetermineLanguageFromExtension(extension);
                
                if (languageStats.ContainsKey(language))
                    languageStats[language]++;
                else
                    languageStats[language] = 1;

                // Count lines for text files
                if (IsTextFile(file))
                {
                    var lines = await File.ReadAllLinesAsync(file, ct);
                    totalLines += lines.Length;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Skipped file analysis for: {File}", file);
            }
        }

        metrics.TotalLinesOfCode = totalLines;
        metrics.EffectiveLinesOfCode = (int)(totalLines * 0.8); // Rough estimate
        metrics.RepositorySizeBytes = totalBytes;
        metrics.AverageFileSize = files.Count > 0 ? (double)totalBytes / files.Count : 0;

        // Language distribution as JSON
        metrics.LanguageDistribution = JsonSerializer.Serialize(languageStats);
    }

    private async Task PopulateActivityMetricsAsync(GitRepository repo, RepositoryMetrics metrics, CancellationToken ct)
    {
        var commits = repo.Commits.ToList();
        
        if (commits.Any())
        {
            var hourlyPattern = commits
                .GroupBy(c => c.Author.When.Hour)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());
                
            var dailyPattern = commits
                .GroupBy(c => c.Author.When.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            metrics.HourlyActivityPattern = JsonSerializer.Serialize(hourlyPattern);
            metrics.DailyActivityPattern = JsonSerializer.Serialize(dailyPattern);
        }

        await Task.CompletedTask; // Make async for consistency
    }

    private static string DetermineLanguageFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".h" or ".hpp" => "C/C++ Header",
            ".css" => "CSS",
            ".html" or ".htm" => "HTML",
            ".json" => "JSON",
            ".xml" => "XML",
            ".yml" or ".yaml" => "YAML",
            ".md" => "Markdown",
            ".sql" => "SQL",
            ".sh" => "Shell",
            ".ps1" => "PowerShell",
            _ => "Other"
        };
    }

    private static bool IsTestFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var directory = Path.GetDirectoryName(filePath)?.ToLowerInvariant() ?? "";
        
        return fileName.Contains("test") || 
               fileName.Contains("spec") || 
               directory.Contains("test") ||
               directory.Contains("spec");
    }

    private static bool IsGeneratedCode(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        return fileName.Contains("generated") ||
               fileName.Contains(".g.") ||
               fileName.EndsWith(".designer.cs");
    }

    private static string DetermineFileCategory(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" or ".js" or ".ts" or ".py" or ".java" or ".cpp" or ".c" => "Source",
            ".h" or ".hpp" => "Header",
            ".css" or ".scss" or ".less" => "Stylesheet",
            ".html" or ".htm" or ".aspx" or ".razor" => "Markup",
            ".json" or ".xml" or ".yml" or ".yaml" or ".config" => "Configuration",
            ".md" or ".txt" or ".rst" => "Documentation",
            ".sql" => "Database",
            ".png" or ".jpg" or ".gif" or ".svg" or ".ico" => "Image",
            _ => "Other"
        };
    }

    private static int CountCommentLines(string[] lines, string extension)
    {
        var commentCount = 0;
        var inBlockComment = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Handle different comment styles based on file extension
            switch (extension.ToLowerInvariant())
            {
                case ".cs" or ".js" or ".ts" or ".cpp" or ".java":
                    if (inBlockComment)
                    {
                        commentCount++;
                        if (trimmedLine.Contains("*/"))
                            inBlockComment = false;
                    }
                    else if (trimmedLine.StartsWith("//") || 
                             trimmedLine.StartsWith("/*"))
                    {
                        commentCount++;
                        if (trimmedLine.StartsWith("/*") && !trimmedLine.Contains("*/"))
                            inBlockComment = true;
                    }
                    break;
                    
                case ".py":
                    if (trimmedLine.StartsWith("#"))
                        commentCount++;
                    break;
                    
                case ".html" or ".xml":
                    if (trimmedLine.Contains("<!--"))
                        commentCount++;
                    break;
            }
        }
        
        return commentCount;
    }

    private static bool IsTextFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var textExtensions = new[] 
        { 
            ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".hpp",
            ".css", ".html", ".htm", ".xml", ".json", ".yml", ".yaml", 
            ".md", ".txt", ".sql", ".sh", ".ps1", ".config", ".gitignore"
        };
        
        return textExtensions.Contains(extension);
    }

    private static bool IsGitInternalFile(string filePath)
    {
        return filePath.Contains("\\.git\\") || filePath.Contains("/.git/");
    }

    #endregion
}
