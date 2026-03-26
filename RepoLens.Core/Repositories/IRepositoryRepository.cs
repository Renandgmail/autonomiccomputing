namespace RepoLens.Core.Repositories;

public interface IRepositoryRepository
{
    Task<Entities.Repository?> GetByUrlAsync(string url, CancellationToken ct = default);
    Task<Entities.Repository> AddAsync(Entities.Repository repository, CancellationToken ct = default);
    Task UpdateAsync(Entities.Repository repository, CancellationToken ct = default);
    
    // Additional methods for UI support
    Task<IEnumerable<Entities.Repository>> GetAllAsync(CancellationToken ct = default);
    Task<Entities.Repository?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Entities.Repository?> GetByIdWithMetricsAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<int> GetActiveCountAsync(CancellationToken ct = default);
    Task<IEnumerable<Entities.Repository>> GetRecentlyUpdatedAsync(int count, CancellationToken ct = default);
}
