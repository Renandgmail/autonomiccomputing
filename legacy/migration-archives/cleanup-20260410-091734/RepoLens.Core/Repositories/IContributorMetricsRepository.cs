using RepoLens.Core.Entities;

namespace RepoLens.Core.Repositories;

public interface IContributorMetricsRepository
{
    Task<List<ContributorMetrics>> GetContributorMetricsAsync(int repositoryId, DateTime periodStart, DateTime periodEnd);
    Task<List<ContributorMetrics>> GetTopContributorsAsync(int repositoryId, int count = 10);
    Task<ContributorMetrics?> GetContributorMetricsAsync(int repositoryId, string contributorEmail, DateTime periodStart);
    Task<ContributorMetrics> SaveContributorMetricsAsync(ContributorMetrics metrics);
    Task<List<ContributorMetrics>> SaveContributorMetricsBatchAsync(List<ContributorMetrics> metrics);
    Task<List<string>> GetActiveContributorsAsync(int repositoryId, DateTime since);
    Task<double> GetBusFactorAsync(int repositoryId);
}
