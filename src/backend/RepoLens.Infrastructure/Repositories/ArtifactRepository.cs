using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class ArtifactRepository(RepoLensDbContext context) : IArtifactRepository
{
    public async Task<Artifact?> GetByPathAsync(int repositoryId, string path, CancellationToken ct = default)
    {
        return await context.Artifacts
            .FirstOrDefaultAsync(a => a.RepositoryId == repositoryId && a.Path == path, ct);
    }

    public async Task<Artifact> AddAsync(Artifact artifact, CancellationToken ct = default)
    {
        context.Artifacts.Add(artifact);
        await context.SaveChangesAsync(ct);
        return artifact;
    }

    public async Task<ArtifactVersion?> GetByHashAsync(int artifactId, string contentHash, CancellationToken ct = default)
    {
        return await context.ArtifactVersions
            .FirstOrDefaultAsync(av => av.ArtifactId == artifactId && av.ContentHash == contentHash, ct);
    }

    public async Task<ArtifactVersion> AddVersionAsync(ArtifactVersion artifactVersion, CancellationToken ct = default)
    {
        context.ArtifactVersions.Add(artifactVersion);
        await context.SaveChangesAsync(ct);
        return artifactVersion;
    }

    public async Task<IEnumerable<Artifact>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Artifacts.ToListAsync(ct);
    }

    public async Task<Artifact?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Artifacts
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IEnumerable<Artifact>> GetByRepositoryIdAsync(int? repositoryId, CancellationToken ct = default)
    {
        var query = context.Artifacts.AsQueryable();
        
        if (repositoryId.HasValue)
        {
            query = query.Where(a => a.RepositoryId == repositoryId.Value);
        }
        
        return await query.OrderBy(a => a.Path).ToListAsync(ct);
    }

    public async Task<IEnumerable<Artifact>> SearchByPathAsync(string searchTerm, int? repositoryId, CancellationToken ct = default)
    {
        var query = context.Artifacts.AsQueryable();
        
        if (repositoryId.HasValue)
        {
            query = query.Where(a => a.RepositoryId == repositoryId.Value);
        }
        
        query = query.Where(a => a.Path.Contains(searchTerm));
        
        return await query.OrderBy(a => a.Path).ToListAsync(ct);
    }

    public async Task<int> GetCountByRepositoryAsync(int repositoryId, CancellationToken ct = default)
    {
        return await context.Artifacts
            .Where(a => a.RepositoryId == repositoryId)
            .CountAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await context.Artifacts.CountAsync(ct);
    }

    public async Task<ArtifactVersion?> GetLatestVersionAsync(int artifactId, string? commitSha = null, CancellationToken ct = default)
    {
        var query = context.ArtifactVersions
            .Where(av => av.ArtifactId == artifactId);
        
        if (!string.IsNullOrEmpty(commitSha))
        {
            query = query.Where(av => av.CommitSha == commitSha);
        }
        
        return await query.OrderByDescending(av => av.Id).FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<ArtifactVersion>> GetVersionsByArtifactIdAsync(int artifactId, CancellationToken ct = default)
    {
        return await context.ArtifactVersions
            .Where(av => av.ArtifactId == artifactId)
            .OrderByDescending(av => av.Id)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Commit>> GetCommitsByRepositoryAsync(int repositoryId, int page, int pageSize, CancellationToken ct = default)
    {
        return await context.Commits
            .Where(c => c.RepositoryId == repositoryId)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}
