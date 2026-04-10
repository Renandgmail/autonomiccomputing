using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class FileMetricsRepository : IFileMetricsRepository
{
    private readonly RepoLensDbContext _context;

    public FileMetricsRepository(RepoLensDbContext context)
    {
        _context = context;
    }

    public async Task<FileMetrics?> GetFileMetricsAsync(int repositoryId, string filePath)
    {
        return await _context.FileMetrics
            .FirstOrDefaultAsync(fm => fm.RepositoryId == repositoryId && fm.FilePath == filePath);
    }

    public async Task<List<FileMetrics>> GetRepositoryFileMetricsAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .OrderByDescending(fm => fm.LastAnalyzed)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetHotspotFilesAsync(int repositoryId, int count = 20)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.IsHotspot)
            .OrderByDescending(fm => fm.CyclomaticComplexity)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetHighRiskFilesAsync(int repositoryId, int count = 20)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.CyclomaticComplexity > 10)
            .OrderByDescending(fm => fm.CyclomaticComplexity)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetFilesByLanguageAsync(int repositoryId, string language)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.PrimaryLanguage == language)
            .OrderByDescending(fm => fm.LineCount)
            .ToListAsync();
    }

    public async Task<FileMetrics> SaveFileMetricsAsync(FileMetrics metrics)
    {
        // Check if file metrics already exists to avoid duplicates
        var existingMetrics = await _context.FileMetrics
            .FirstOrDefaultAsync(fm => fm.RepositoryId == metrics.RepositoryId && fm.FilePath == metrics.FilePath);
        
        if (existingMetrics != null)
        {
            // Update existing metrics
            UpdateFileMetrics(existingMetrics, metrics);
            await _context.SaveChangesAsync();
            return existingMetrics;
        }

        _context.FileMetrics.Add(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<List<FileMetrics>> SaveFileMetricsBatchAsync(List<FileMetrics> metrics)
    {
        if (!metrics.Any()) return metrics;

        var repositoryId = metrics.First().RepositoryId;
        var filePaths = metrics.Select(fm => fm.FilePath).ToList();
        
        // Get existing file metrics to avoid duplicates
        var existingMetrics = await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && filePaths.Contains(fm.FilePath))
            .ToListAsync();

        var existingPaths = existingMetrics.Select(em => em.FilePath).ToHashSet();
        var newMetrics = metrics.Where(fm => !existingPaths.Contains(fm.FilePath)).ToList();

        // Update existing metrics
        foreach (var existing in existingMetrics)
        {
            var updated = metrics.FirstOrDefault(fm => fm.FilePath == existing.FilePath);
            if (updated != null)
            {
                UpdateFileMetrics(existing, updated);
            }
        }

        // Add new metrics
        if (newMetrics.Any())
        {
            _context.FileMetrics.AddRange(newMetrics);
        }

        await _context.SaveChangesAsync();
        return await GetRepositoryFileMetricsAsync(repositoryId);
    }

    public async Task<Dictionary<string, int>> GetLanguageDistributionAsync(int repositoryId)
    {
        var distribution = await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .GroupBy(fm => fm.PrimaryLanguage)
            .Select(g => new { Language = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Language, x => x.Count);

        return distribution;
    }

    public async Task<Dictionary<string, double>> GetComplexityDistributionAsync(int repositoryId)
    {
        var metrics = await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .ToListAsync();

        return new Dictionary<string, double>
        {
            ["Low"] = metrics.Count(fm => fm.CyclomaticComplexity <= 5),
            ["Medium"] = metrics.Count(fm => fm.CyclomaticComplexity > 5 && fm.CyclomaticComplexity <= 10),
            ["High"] = metrics.Count(fm => fm.CyclomaticComplexity > 10 && fm.CyclomaticComplexity <= 20),
            ["VeryHigh"] = metrics.Count(fm => fm.CyclomaticComplexity > 20)
        };
    }

    public async Task<List<FileMetrics>> GetFilesNeedingRefactoringAsync(int repositoryId, double minComplexity = 10.0)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.CyclomaticComplexity >= minComplexity)
            .OrderByDescending(fm => fm.CyclomaticComplexity)
            .ToListAsync();
    }

    // Legacy methods for compatibility
    public async Task<FileMetrics> AddAsync(FileMetrics fileMetrics)
    {
        // Check if file metrics already exists to avoid duplicates
        var existingMetrics = await _context.FileMetrics
            .FirstOrDefaultAsync(fm => fm.RepositoryId == fileMetrics.RepositoryId && fm.FilePath == fileMetrics.FilePath);
        
        if (existingMetrics != null)
        {
            // Update existing metrics
            UpdateFileMetrics(existingMetrics, fileMetrics);
            await _context.SaveChangesAsync();
            return existingMetrics;
        }

        _context.FileMetrics.Add(fileMetrics);
        await _context.SaveChangesAsync();
        return fileMetrics;
    }

    public async Task<List<FileMetrics>> AddRangeAsync(List<FileMetrics> fileMetrics)
    {
        if (!fileMetrics.Any()) return fileMetrics;

        var repositoryId = fileMetrics.First().RepositoryId;
        var filePaths = fileMetrics.Select(fm => fm.FilePath).ToList();
        
        // Get existing file metrics to avoid duplicates
        var existingMetrics = await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && filePaths.Contains(fm.FilePath))
            .ToListAsync();

        var existingPaths = existingMetrics.Select(em => em.FilePath).ToHashSet();
        var newMetrics = fileMetrics.Where(fm => !existingPaths.Contains(fm.FilePath)).ToList();

        // Update existing metrics
        foreach (var existing in existingMetrics)
        {
            var updated = fileMetrics.FirstOrDefault(fm => fm.FilePath == existing.FilePath);
            if (updated != null)
            {
                UpdateFileMetrics(existing, updated);
            }
        }

        // Add new metrics
        if (newMetrics.Any())
        {
            _context.FileMetrics.AddRange(newMetrics);
        }

        await _context.SaveChangesAsync();
        return await GetByRepositoryAsync(repositoryId);
    }

    public async Task<List<FileMetrics>> GetByRepositoryAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .OrderByDescending(fm => fm.LastAnalyzed)
            .ToListAsync();
    }

    public async Task<FileMetrics?> GetByFilePathAsync(int repositoryId, string filePath)
    {
        return await _context.FileMetrics
            .FirstOrDefaultAsync(fm => fm.RepositoryId == repositoryId && fm.FilePath == filePath);
    }

    public async Task<List<FileMetrics>> GetByLanguageAsync(int repositoryId, string language)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.PrimaryLanguage == language)
            .OrderByDescending(fm => fm.LineCount)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetByExtensionAsync(int repositoryId, string extension)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.FileExtension == extension)
            .OrderByDescending(fm => fm.LineCount)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetHotspotsAsync(int repositoryId, int count = 20)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.IsHotspot)
            .OrderByDescending(fm => fm.CyclomaticComplexity)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetTestFilesAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.IsTestFile)
            .OrderByDescending(fm => fm.TestCoverage)
            .ToListAsync();
    }

    public async Task<List<FileMetrics>> GetConfigurationFilesAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId && fm.IsConfigurationFile)
            .OrderBy(fm => fm.FileName)
            .ToListAsync();
    }

    public async Task<Dictionary<string, object>> GetFileSummaryAsync(int repositoryId)
    {
        var metrics = await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .ToListAsync();

        if (!metrics.Any())
        {
            return new Dictionary<string, object>();
        }

        var languageDistribution = metrics
            .GroupBy(fm => fm.PrimaryLanguage)
            .ToDictionary(g => g.Key, g => g.Count());

        var extensionDistribution = metrics
            .GroupBy(fm => fm.FileExtension)
            .ToDictionary(g => g.Key, g => g.Count());

        return new Dictionary<string, object>
        {
            ["TotalFiles"] = metrics.Count,
            ["TotalLinesOfCode"] = metrics.Sum(fm => fm.LineCount),
            ["TotalEffectiveLines"] = metrics.Sum(fm => fm.EffectiveLineCount),
            ["TotalCommentLines"] = metrics.Sum(fm => fm.CommentLineCount),
            ["TotalBlankLines"] = metrics.Sum(fm => fm.BlankLineCount),
            ["AverageComplexity"] = metrics.Average(fm => fm.CyclomaticComplexity),
            ["TotalFileSize"] = metrics.Sum(fm => fm.FileSizeBytes),
            ["TestFiles"] = metrics.Count(fm => fm.IsTestFile),
            ["ConfigFiles"] = metrics.Count(fm => fm.IsConfigurationFile),
            ["HotspotFiles"] = metrics.Count(fm => fm.IsHotspot),
            ["AverageMaintainability"] = metrics.Average(fm => fm.MaintainabilityIndex),
            ["LanguageDistribution"] = languageDistribution,
            ["ExtensionDistribution"] = extensionDistribution,
            ["AverageTestCoverage"] = metrics.Where(fm => fm.IsTestFile).DefaultIfEmpty().Average(fm => fm?.TestCoverage ?? 0)
        };
    }

    public async Task<bool> HasMetricsAsync(int repositoryId, string filePath)
    {
        return await _context.FileMetrics
            .AnyAsync(fm => fm.RepositoryId == repositoryId && fm.FilePath == filePath);
    }

    public async Task<int> GetFileCountAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .CountAsync(fm => fm.RepositoryId == repositoryId);
    }

    public async Task<DateTime?> GetLastAnalyzedDateAsync(int repositoryId)
    {
        return await _context.FileMetrics
            .Where(fm => fm.RepositoryId == repositoryId)
            .OrderByDescending(fm => fm.LastAnalyzed)
            .Select(fm => fm.LastAnalyzed)
            .FirstOrDefaultAsync();
    }

    private void UpdateFileMetrics(FileMetrics existing, FileMetrics updated)
    {
        existing.FileSizeBytes = updated.FileSizeBytes;
        existing.LineCount = updated.LineCount;
        existing.EffectiveLineCount = updated.EffectiveLineCount;
        existing.CommentLineCount = updated.CommentLineCount;
        existing.BlankLineCount = updated.BlankLineCount;
        existing.CyclomaticComplexity = updated.CyclomaticComplexity;
        existing.MaintainabilityIndex = updated.MaintainabilityIndex;
        existing.LastAnalyzed = updated.LastAnalyzed;
        existing.LastCommit = updated.LastCommit;
        existing.IsHotspot = updated.IsHotspot;
        existing.TestCoverage = updated.TestCoverage;
        existing.ContributorBreakdown = updated.ContributorBreakdown;
        existing.ChangePatterns = updated.ChangePatterns;
        existing.DependencyGraph = updated.DependencyGraph;
        existing.IssueHistory = updated.IssueHistory;
    }
}
