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
        context.Repositories.Update(repository);
        await context.SaveChangesAsync(ct);
    }
}