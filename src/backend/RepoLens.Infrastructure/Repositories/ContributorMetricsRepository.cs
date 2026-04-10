using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class ContributorMetricsRepository : IContributorMetricsRepository
{
    private readonly RepoLensDbContext _context;

    public ContributorMetricsRepository(RepoLensDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContributorMetrics>> GetContributorMetricsAsync(int repositoryId, DateTime periodStart, DateTime periodEnd)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId 
                      && cm.PeriodStart >= periodStart 
                      && cm.PeriodEnd <= periodEnd)
            .OrderByDescending(cm => cm.CommitCount)
            .ToListAsync();
    }

    public async Task<List<ContributorMetrics>> GetTopContributorsAsync(int repositoryId, int count = 10)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId)
            .OrderByDescending(cm => cm.CommitCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<ContributorMetrics?> GetContributorMetricsAsync(int repositoryId, string contributorEmail, DateTime periodStart)
    {
        return await _context.ContributorMetrics
            .FirstOrDefaultAsync(cm => cm.RepositoryId == repositoryId 
                                    && cm.ContributorEmail == contributorEmail 
                                    && cm.PeriodStart == periodStart);
    }

    public async Task<ContributorMetrics> SaveContributorMetricsAsync(ContributorMetrics metrics)
    {
        _context.ContributorMetrics.Add(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<List<ContributorMetrics>> SaveContributorMetricsBatchAsync(List<ContributorMetrics> metrics)
    {
        if (!metrics.Any()) return metrics;

        _context.ContributorMetrics.AddRange(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<List<string>> GetActiveContributorsAsync(int repositoryId, DateTime since)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId && cm.LastContribution >= since)
            .Select(cm => cm.ContributorEmail)
            .Distinct()
            .ToListAsync();
    }

    public async Task<double> GetBusFactorAsync(int repositoryId)
    {
        var contributors = await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId)
            .OrderByDescending(cm => cm.CommitCount)
            .ToListAsync();

        if (!contributors.Any()) return 0;

        var totalContributions = contributors.Sum(c => c.CommitCount);
        var cumulativeContributions = 0;
        var factor = 0;
        
        foreach (var contributor in contributors)
        {
            cumulativeContributions += contributor.CommitCount;
            factor++;
            if (cumulativeContributions >= totalContributions * 0.5) break;
        }
        
        return Math.Max(1, factor);
    }

    // Legacy methods for compatibility
    public async Task<ContributorMetrics> AddAsync(ContributorMetrics contributorMetrics)
    {
        _context.ContributorMetrics.Add(contributorMetrics);
        await _context.SaveChangesAsync();
        return contributorMetrics;
    }

    public async Task<List<ContributorMetrics>> AddRangeAsync(List<ContributorMetrics> contributorMetrics)
    {
        if (!contributorMetrics.Any()) return contributorMetrics;

        _context.ContributorMetrics.AddRange(contributorMetrics);
        await _context.SaveChangesAsync();
        return contributorMetrics;
    }

    public async Task<List<ContributorMetrics>> GetByRepositoryAsync(int repositoryId)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId)
            .OrderByDescending(cm => cm.CommitCount)
            .ToListAsync();
    }

    public async Task<List<ContributorMetrics>> GetByRepositoryAsync(int repositoryId, DateTime startDate, DateTime endDate)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId 
                      && cm.PeriodStart >= startDate 
                      && cm.PeriodEnd <= endDate)
            .OrderByDescending(cm => cm.CommitCount)
            .ToListAsync();
    }

    public async Task<ContributorMetrics?> GetByContributorAsync(int repositoryId, string contributorEmail, DateTime periodStart)
    {
        return await _context.ContributorMetrics
            .FirstOrDefaultAsync(cm => cm.RepositoryId == repositoryId 
                                    && cm.ContributorEmail == contributorEmail 
                                    && cm.PeriodStart == periodStart);
    }


    public async Task<int> GetContributorCountAsync(int repositoryId)
    {
        return await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId)
            .Select(cm => cm.ContributorEmail)
            .Distinct()
            .CountAsync();
    }

    public async Task<bool> HasMetricsAsync(int repositoryId, string contributorEmail, DateTime periodStart)
    {
        return await _context.ContributorMetrics
            .AnyAsync(cm => cm.RepositoryId == repositoryId 
                         && cm.ContributorEmail == contributorEmail 
                         && cm.PeriodStart == periodStart);
    }

    public async Task<Dictionary<string, int>> GetContributionSummaryAsync(int repositoryId)
    {
        var metrics = await _context.ContributorMetrics
            .Where(cm => cm.RepositoryId == repositoryId)
            .ToListAsync();

        return new Dictionary<string, int>
        {
            ["TotalContributors"] = metrics.Select(cm => cm.ContributorEmail).Distinct().Count(),
            ["TotalCommits"] = metrics.Sum(cm => cm.CommitCount),
            ["TotalLinesAdded"] = metrics.Sum(cm => cm.LinesAdded),
            ["TotalLinesDeleted"] = metrics.Sum(cm => cm.LinesDeleted),
            ["TotalFilesModified"] = metrics.Sum(cm => cm.FilesModified),
            ["ActiveContributors"] = metrics.Count(cm => cm.LastContribution >= DateTime.UtcNow.AddDays(-30)),
            ["CoreContributors"] = metrics.Count(cm => cm.IsCoreContributor)
        };
    }
}
