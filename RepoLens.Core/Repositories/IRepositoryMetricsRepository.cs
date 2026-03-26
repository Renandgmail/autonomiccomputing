using RepoLens.Core.Entities;

namespace RepoLens.Core.Repositories;

public interface IRepositoryMetricsRepository
{
    Task<RepositoryMetrics?> GetLatestMetricsAsync(int repositoryId);
    Task<List<RepositoryMetrics>> GetMetricsHistoryAsync(int repositoryId, DateTime startDate, DateTime endDate);
    Task<RepositoryMetrics> SaveMetricsAsync(RepositoryMetrics metrics);
    Task<List<RepositoryMetrics>> GetAllLatestMetricsAsync();
    Task<bool> HasMetricsForDateAsync(int repositoryId, DateTime date);
    
    // New methods for trend analysis
    Task<List<RepositoryMetrics>> GetTrendDataAsync(int repositoryId, int days = 30);
    Task<Dictionary<string, object>> GetSummaryTrendsAsync(int repositoryId, int days = 30);
}
