namespace RepoLens.Core.Repositories;

public interface IArtifactRepository
{
    Task<Entities.Artifact?> GetByPathAsync(int repositoryId, string path, CancellationToken ct = default);
    Task<Entities.Artifact> AddAsync(Entities.Artifact artifact, CancellationToken ct = default);
    Task<Entities.ArtifactVersion?> GetByHashAsync(int artifactId, string contentHash, CancellationToken ct = default);
    Task<Entities.ArtifactVersion> AddVersionAsync(Entities.ArtifactVersion artifactVersion, CancellationToken ct = default);
}