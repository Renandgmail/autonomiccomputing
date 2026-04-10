namespace RepoLens.Core.Repositories;

public interface IArtifactRepository
{
    Task<Entities.Artifact?> GetByPathAsync(int repositoryId, string path, CancellationToken ct = default);
    Task<Entities.Artifact> AddAsync(Entities.Artifact artifact, CancellationToken ct = default);
    Task<Entities.ArtifactVersion?> GetByHashAsync(int artifactId, string contentHash, CancellationToken ct = default);
    Task<Entities.ArtifactVersion> AddVersionAsync(Entities.ArtifactVersion artifactVersion, CancellationToken ct = default);
    
    // Additional methods for UI support
    Task<IEnumerable<Entities.Artifact>> GetAllAsync(CancellationToken ct = default);
    Task<Entities.Artifact?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Entities.Artifact>> GetByRepositoryIdAsync(int? repositoryId, CancellationToken ct = default);
    Task<IEnumerable<Entities.Artifact>> SearchByPathAsync(string searchTerm, int? repositoryId, CancellationToken ct = default);
    Task<int> GetCountByRepositoryAsync(int repositoryId, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<Entities.ArtifactVersion?> GetLatestVersionAsync(int artifactId, string? commitSha = null, CancellationToken ct = default);
    Task<IEnumerable<Entities.ArtifactVersion>> GetVersionsByArtifactIdAsync(int artifactId, CancellationToken ct = default);
    Task<IEnumerable<Entities.Commit>> GetCommitsByRepositoryAsync(int repositoryId, int page, int pageSize, CancellationToken ct = default);
}
