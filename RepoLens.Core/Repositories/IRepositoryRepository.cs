namespace RepoLens.Core.Repositories;

public interface IRepositoryRepository
{
    Task<Entities.Repository?> GetByUrlAsync(string url, CancellationToken ct = default);
    Task<Entities.Repository> AddAsync(Entities.Repository repository, CancellationToken ct = default);
    Task UpdateAsync(Entities.Repository repository, CancellationToken ct = default);
}