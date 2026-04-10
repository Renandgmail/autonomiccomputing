using LibGit2Sharp;
using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

public interface IGitService
{
    Task<LibGit2Sharp.Repository> OpenOrCloneRepositoryAsync(string repositoryUrl, string localPath, CancellationToken ct = default);
    Task<IEnumerable<RepoLens.Core.Entities.Commit>> GetCommitsAsync(LibGit2Sharp.Repository repository, int repositoryId, CancellationToken ct = default);
    Task<byte[]> GetBlobContentAsync(LibGit2Sharp.Repository repository, string commitSha, string filePath, CancellationToken ct = default);
}
