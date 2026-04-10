using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class RepositoryRepository(RepoLensDbContext context) : IRepositoryRepository
{
    public async Task<Repository?> GetByUrlAsync(string url, CancellationToken ct = default)
    {
        return await context.Repositories
            .FirstOrDefaultAsync(r => r.Url == url, ct);
    }

    public async Task<Repository> AddAsync(Repository repository, CancellationToken ct = default)
    {
        context.Repositories.Add(repository);
        await context.SaveChangesAsync(ct);
        return repository;
    }

    public async Task UpdateAsync(Repository repository, CancellationToken ct = default)
    {
        repository.UpdatedAt = DateTime.UtcNow;
        context.Repositories.Update(repository);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Repository>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Repositories
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<Repository?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Repositories
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Repository?> GetByIdWithMetricsAsync(int id, CancellationToken ct = default)
    {
        return await context.Repositories
            .Include(r => r.Metrics)
            .Include(r => r.ContributorMetrics)
            .Include(r => r.FileMetrics)
            .Include(r => r.Artifacts)
            .Include(r => r.Owner)
            .Include(r => r.Organization)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repository = await GetByIdAsync(id, ct);
        if (repository != null)
        {
            context.Repositories.Remove(repository);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        return await context.Repositories.CountAsync(ct);
    }

    public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
    {
        // Consider active if LastSyncCommit is not empty (has been processed)
        return await context.Repositories
            .Where(r => !string.IsNullOrEmpty(r.LastSyncCommit))
            .CountAsync(ct);
    }

    public async Task<IEnumerable<Repository>> GetRecentlyUpdatedAsync(int count, CancellationToken ct = default)
    {
        return await context.Repositories
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }
}
