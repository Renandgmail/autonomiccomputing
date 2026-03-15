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
}