using System.Collections.Concurrent;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace RepoLens.Infrastructure.Services;

/// <summary>
/// Implementation of repository analysis service following existing RepoLens patterns
/// </summary>
public class RepositoryAnalysisService : IRepositoryAnalysisService
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IFileAnalysisService _fileAnalysisService;
    private readonly IGitProviderFactory _gitProviderFactory;
    private readonly ILogger<RepositoryAnalysisService> _logger;
    
    // In-memory job tracking (in production, use Redis or database)
    private static readonly ConcurrentDictionary<int, AnalysisProgress> _activeJobs = new();
    private static readonly ConcurrentDictionary<int, CancellationTokenSource> _jobCancellationTokens = new();
    private static int _nextJobId = 1;

    public RepositoryAnalysisService(
        IRepositoryRepository repositoryRepository,
        IFileAnalysisService fileAnalysisService,
        IGitProviderFactory gitProviderFactory,
        ILogger<RepositoryAnalysisService> logger)
    {
        _repositoryRepository = repositoryRepository;
        _fileAnalysisService = fileAnalysisService;
        _gitProviderFactory = gitProviderFactory;
        _logger = logger;
    }

    public async Task<int> StartFullAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full analysis for repository {RepositoryId}", repositoryId);

        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
        if (repository == null)
        {
            throw new ArgumentException($"Repository not found: {repositoryId}", nameof(repositoryId));
        }

        // Generate unique job ID
        var jobId = Interlocked.Increment(ref _nextJobId);
        var jobCts = new CancellationTokenSource();
        
        // Create analysis progress tracking
        var progress = new AnalysisProgress
        {
            JobId = jobId,
            RepositoryId = repositoryId,
            RepositoryName = repository.Name,
            Status = "Processing",
            StartTime = DateTime.UtcNow,
            TotalFiles = 0,
            ProcessedFiles = 0
        };

        _activeJobs[jobId] = progress;
        _jobCancellationTokens[jobId] = jobCts;

        // Start background processing
        _ = Task.Run(async () => await ProcessAnalysisJobAsync(jobId, repository, jobCts.Token), cancellationToken);

        _logger.LogInformation("Started analysis job {JobId} for repository {RepositoryName}", jobId, repository.Name);
        return jobId;
    }

    public async Task<int> StartIncrementalAnalysisAsync(int repositoryId, string[]? specificFiles = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting incremental analysis for repository {RepositoryId} with {FileCount} specific files", 
            repositoryId, specificFiles?.Length ?? 0);

        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
        if (repository == null)
        {
            throw new ArgumentException($"Repository not found: {repositoryId}", nameof(repositoryId));
        }

        // For now, delegate to full analysis (incremental logic can be enhanced later)
        return await StartFullAnalysisAsync(repositoryId, cancellationToken);
    }

    public async Task<AnalysisProgress> GetAnalysisProgressAsync(int jobId, CancellationToken cancellationToken = default)
    {
        if (_activeJobs.TryGetValue(jobId, out var progress))
        {
            return progress;
        }

        throw new ArgumentException($"Analysis job not found: {jobId}", nameof(jobId));
    }

    public async Task<bool> StopAnalysisAsync(int jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping analysis job {JobId}", jobId);

        if (_jobCancellationTokens.TryRemove(jobId, out var cts))
        {
            cts.Cancel();
            
            if (_activeJobs.TryGetValue(jobId, out var progress))
            {
                progress.Status = "Cancelled";
                progress.ErrorMessage = "Analysis was cancelled by user request";
            }

            return true;
        }

        return false;
    }

    public async Task<RepositoryAnalysisResult> GetAnalysisResultAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting analysis result for repository {RepositoryId}", repositoryId);

        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
        if (repository == null)
        {
            throw new ArgumentException($"Repository not found: {repositoryId}", nameof(repositoryId));
        }

        // Create result from current repository state
        var result = new RepositoryAnalysisResult
        {
            RepositoryId = repositoryId,
            RepositoryName = repository.Name,
            ScanStatus = repository.ScanStatus ?? "Pending",
            TotalFiles = repository.TotalFiles,
            TotalLines = repository.TotalLines,
            LastAnalysisAt = repository.LastAnalysisAt,
            ScanErrorMessage = repository.ScanErrorMessage,
            SupportedLanguages = _fileAnalysisService.SupportedLanguages.ToList()
        };

        // Calculate statistics from repository files
        if (repository.RepositoryFiles.Any())
        {
            result.LanguageDistribution = repository.RepositoryFiles
                .GroupBy(f => f.Language)
                .ToDictionary(g => g.Key, g => g.Count());

            result.FileTypeDistribution = repository.RepositoryFiles
                .GroupBy(f => f.FileExtension)
                .ToDictionary(g => g.Key, g => g.Count());

            result.TotalCodeElements = repository.RepositoryFiles
                .SelectMany(f => f.CodeElements)
                .Count();

            result.CodeElementDistribution = repository.RepositoryFiles
                .SelectMany(f => f.CodeElements)
                .GroupBy(ce => ce.ElementType)
                .ToDictionary(g => g.Key, g => g.Count());

            result.Files = repository.RepositoryFiles.ToList();
        }

        return result;
    }

    private async Task ProcessAnalysisJobAsync(int jobId, Repository repository, CancellationToken cancellationToken)
    {
        var progress = _activeJobs[jobId];
        
        try
        {
            _logger.LogInformation("Processing analysis job {JobId} for repository {RepositoryName}", jobId, repository.Name);

            // Update repository status
            repository.ScanStatus = "Processing";
            repository.ScanErrorMessage = null;
            await _repositoryRepository.UpdateAsync(repository, cancellationToken);

            // Get repository files using Git service
            var repoPath = await EnsureRepositoryCloned(repository, cancellationToken);
            var files = await ScanRepositoryFiles(repoPath, cancellationToken);

            progress.TotalFiles = files.Count;
            progress.Status = "Processing";

            var processedFiles = 0;
            var totalLines = 0;
            var errors = new List<string>();

            // Process each file
            foreach (var filePath in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    progress.Status = "Cancelled";
                    return;
                }

                try
                {
                    progress.CurrentFile = filePath;
                    
                    // Analyze file if supported
                    if (_fileAnalysisService.IsSupported(Path.GetExtension(filePath)))
                    {
                        var fullPath = Path.Combine(repoPath, filePath);
                        var content = await File.ReadAllTextAsync(fullPath, cancellationToken);
                        
                        var analysisResult = await _fileAnalysisService.AnalyzeFileAsync(filePath, content, cancellationToken);
                        
                        if (analysisResult.Success)
                        {
                            // Create RepositoryFile entity
                            var repositoryFile = new RepositoryFile
                            {
                                RepositoryId = repository.Id,
                                FilePath = filePath,
                                FileName = Path.GetFileName(filePath),
                                FileExtension = Path.GetExtension(filePath),
                                Language = analysisResult.Language,
                                FileSize = analysisResult.FileSize,
                                LineCount = analysisResult.LineCount,
                                LastModified = analysisResult.LastModified,
                                FileHash = analysisResult.FileHash,
                                ProcessingStatus = FileProcessingStatus.Completed,
                                ProcessingTime = analysisResult.ProcessingTimeMs,
                                CodeElements = analysisResult.CodeElements
                            };

                            repository.RepositoryFiles.Add(repositoryFile);
                            totalLines += analysisResult.LineCount;
                        }
                        else
                        {
                            errors.Add($"{filePath}: {analysisResult.ErrorMessage}");
                        }
                    }

                    processedFiles++;
                    progress.ProcessedFiles = processedFiles;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {FilePath} in job {JobId}", filePath, jobId);
                    errors.Add($"{filePath}: {ex.Message}");
                }
            }

            // Update repository with results
            repository.ScanStatus = "Completed";
            repository.TotalFiles = processedFiles;
            repository.TotalLines = totalLines;
            repository.LastAnalysisAt = DateTime.UtcNow;
            
            if (errors.Any())
            {
                repository.ScanErrorMessage = string.Join("; ", errors.Take(5)) + 
                    (errors.Count > 5 ? $" and {errors.Count - 5} more errors" : "");
            }

            await _repositoryRepository.UpdateAsync(repository, cancellationToken);

            // Update progress
            progress.Status = "Completed";
            progress.ProcessingErrors = errors;

            _logger.LogInformation("Completed analysis job {JobId} for repository {RepositoryName}. Processed {ProcessedFiles} files with {ErrorCount} errors", 
                jobId, repository.Name, processedFiles, errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis job {JobId} failed for repository {RepositoryName}", jobId, repository.Name);
            
            progress.Status = "Failed";
            progress.ErrorMessage = ex.Message;

            repository.ScanStatus = "Failed";
            repository.ScanErrorMessage = ex.Message;
            
            try
            {
                await _repositoryRepository.UpdateAsync(repository, cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update repository status after job failure");
            }
        }
        finally
        {
            // Cleanup
            _jobCancellationTokens.TryRemove(jobId, out _);
            
            // Remove job from active jobs after some time to allow status checking
            _ = Task.Delay(TimeSpan.FromHours(1), CancellationToken.None)
                .ContinueWith(_ => _activeJobs.TryRemove(jobId, out var _), TaskScheduler.Default);
        }
    }

    private async Task<string> EnsureRepositoryCloned(Repository repository, CancellationToken cancellationToken)
    {
        // Use existing Git service to ensure repository is available locally
        // This would integrate with the existing GitService implementation
        var tempPath = Path.Combine(Path.GetTempPath(), "RepoLens", $"repo_{repository.Id}");
        
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
            // In a real implementation, this would clone the repository
            // For now, we'll simulate with the current project structure
            _logger.LogInformation("Repository path prepared at {TempPath}", tempPath);
        }

        return tempPath;
    }

    private async Task<List<string>> ScanRepositoryFiles(string repositoryPath, CancellationToken cancellationToken)
    {
        var files = new List<string>();
        var supportedExtensions = new[] { ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".h", ".go", ".rs", ".rb", ".php" };

        // For demo, scan current project files if repository path is temp
        if (repositoryPath.Contains("temp", StringComparison.OrdinalIgnoreCase))
        {
            // Use current project structure for demo
            var currentDir = Directory.GetCurrentDirectory();
            var projectFiles = Directory.GetFiles(currentDir, "*.*", SearchOption.AllDirectories)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Where(f => !f.Contains("bin") && !f.Contains("obj") && !f.Contains("node_modules"))
                .Select(f => Path.GetRelativePath(currentDir, f))
                .ToList();

            files.AddRange(projectFiles);
        }
        else
        {
            // Real repository scanning logic would go here
            if (Directory.Exists(repositoryPath))
            {
                files = Directory.GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Where(f => !f.Contains("bin") && !f.Contains("obj") && !f.Contains("node_modules"))
                    .Select(f => Path.GetRelativePath(repositoryPath, f))
                    .ToList();
            }
        }

        _logger.LogInformation("Found {FileCount} supported files in repository", files.Count);
        return files;
    }
}
