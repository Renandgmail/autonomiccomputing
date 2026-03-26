using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

public interface IMetricsCollectionService
{
    Task<RepositoryMetrics> CollectRepositoryMetricsAsync(int repositoryId);
    Task<List<ContributorMetrics>> CollectContributorMetricsAsync(int repositoryId, DateTime periodStart, DateTime periodEnd);
    Task<List<FileMetrics>> CollectFileMetricsAsync(int repositoryId);
    Task CollectAllMetricsAsync(int repositoryId);
    Task<bool> ShouldCollectMetricsAsync(int repositoryId);
}
