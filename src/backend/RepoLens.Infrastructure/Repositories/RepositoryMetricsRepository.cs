using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class RepositoryMetricsRepository : IRepositoryMetricsRepository
{
    private readonly RepoLensDbContext _context;

    public RepositoryMetricsRepository(RepoLensDbContext context)
    {
        _context = context;
    }

    public async Task<RepositoryMetrics?> GetLatestMetricsAsync(int repositoryId)
    {
        return await _context.RepositoryMetrics
            .Where(rm => rm.RepositoryId == repositoryId)
            .OrderByDescending(rm => rm.MeasurementDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<RepositoryMetrics>> GetMetricsHistoryAsync(int repositoryId, DateTime startDate, DateTime endDate)
    {
        return await _context.RepositoryMetrics
            .Where(rm => rm.RepositoryId == repositoryId && 
                        rm.MeasurementDate >= startDate && 
                        rm.MeasurementDate <= endDate)
            .OrderBy(rm => rm.MeasurementDate)
            .ToListAsync();
    }

    public async Task<RepositoryMetrics> SaveMetricsAsync(RepositoryMetrics metrics)
    {
        // Check if metrics for this date already exist
        var existing = await _context.RepositoryMetrics
            .FirstOrDefaultAsync(rm => rm.RepositoryId == metrics.RepositoryId && 
                                      rm.MeasurementDate.Date == metrics.MeasurementDate.Date);

        if (existing != null)
        {
            // Update existing metrics manually to avoid ID conflicts
            existing.TotalFiles = metrics.TotalFiles;
            existing.RepositorySizeBytes = metrics.RepositorySizeBytes;
            existing.BinaryFileCount = metrics.BinaryFileCount;
            existing.TextFileCount = metrics.TextFileCount;
            existing.LanguageDistribution = metrics.LanguageDistribution;
            existing.FileTypeDistribution = metrics.FileTypeDistribution;
            existing.CommitsLastWeek = metrics.CommitsLastWeek;
            existing.CommitsLastMonth = metrics.CommitsLastMonth;
            existing.CommitsLastQuarter = metrics.CommitsLastQuarter;
            existing.DevelopmentVelocity = metrics.DevelopmentVelocity;
            existing.HourlyActivityPattern = metrics.HourlyActivityPattern;
            existing.DailyActivityPattern = metrics.DailyActivityPattern;
            existing.ActiveContributors = metrics.ActiveContributors;
            existing.TotalContributors = metrics.TotalContributors;
            existing.BusFactor = metrics.BusFactor;
            existing.TotalLinesOfCode = metrics.TotalLinesOfCode;
            existing.EffectiveLinesOfCode = metrics.EffectiveLinesOfCode;
            existing.CommentLines = metrics.CommentLines;
            existing.BlankLines = metrics.BlankLines;
            existing.CommentRatio = metrics.CommentRatio;
            existing.MaintainabilityIndex = metrics.MaintainabilityIndex;
            existing.LineCoveragePercentage = metrics.LineCoveragePercentage;
            existing.DocumentationCoverage = metrics.DocumentationCoverage;
            existing.BuildSuccessRate = metrics.BuildSuccessRate;
            existing.SecurityVulnerabilities = metrics.SecurityVulnerabilities;
            existing.AverageCyclomaticComplexity = metrics.AverageCyclomaticComplexity;
            existing.TotalMethods = metrics.TotalMethods;
            existing.TotalClasses = metrics.TotalClasses;
            existing.AverageCommitSize = metrics.AverageCommitSize;
            existing.LinesAddedLastWeek = metrics.LinesAddedLastWeek;
            existing.FilesChangedLastWeek = metrics.FilesChangedLastWeek;
            existing.MeasurementDate = metrics.MeasurementDate;
            
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // Add new metrics
            _context.RepositoryMetrics.Add(metrics);
            await _context.SaveChangesAsync();
            return metrics;
        }
    }

    public async Task<List<RepositoryMetrics>> GetAllLatestMetricsAsync()
    {
        return await _context.RepositoryMetrics
            .Where(rm => rm.MeasurementDate == 
                        _context.RepositoryMetrics
                            .Where(rm2 => rm2.RepositoryId == rm.RepositoryId)
                            .Max(rm2 => rm2.MeasurementDate))
            .ToListAsync();
    }

    public async Task<bool> HasMetricsForDateAsync(int repositoryId, DateTime date)
    {
        return await _context.RepositoryMetrics
            .AnyAsync(rm => rm.RepositoryId == repositoryId && 
                           rm.MeasurementDate.Date == date.Date);
    }

    public async Task<List<RepositoryMetrics>> GetTrendDataAsync(int repositoryId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _context.RepositoryMetrics
            .Where(rm => rm.RepositoryId == repositoryId && 
                        rm.MeasurementDate >= startDate)
            .OrderBy(rm => rm.MeasurementDate)
            .ToListAsync();
    }

    public async Task<Dictionary<string, object>> GetSummaryTrendsAsync(int repositoryId, int days = 30)
    {
        var trendData = await GetTrendDataAsync(repositoryId, days);
        
        if (!trendData.Any())
        {
            return new Dictionary<string, object>();
        }

        var latest = trendData.Last();
        var earliest = trendData.First();
        
        var commitsTrend = latest.CommitsLastMonth - earliest.CommitsLastMonth;
        var filesTrend = latest.TotalFiles - earliest.TotalFiles;
        var sizeTrend = latest.RepositorySizeBytes - earliest.RepositorySizeBytes;
        var qualityTrend = latest.CodeQualityScore - earliest.CodeQualityScore;
        
        return new Dictionary<string, object>
        {
            ["commits_trend"] = commitsTrend,
            ["files_trend"] = filesTrend,
            ["size_trend"] = sizeTrend,
            ["quality_trend"] = Math.Round(qualityTrend, 2),
            ["period_days"] = days,
            ["data_points"] = trendData.Count,
            ["latest_measurement"] = latest.MeasurementDate,
            ["earliest_measurement"] = earliest.MeasurementDate
        };
    }
}
