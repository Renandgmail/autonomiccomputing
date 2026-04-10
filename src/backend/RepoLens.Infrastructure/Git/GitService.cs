using LibGit2Sharp;
using RepoLens.Core.Entities;
using RepoLens.Core.Exceptions;
using RepoLens.Core.Services;
using System.IO;

namespace RepoLens.Infrastructure.Git;

public class GitService : IGitService
{
    public async Task<LibGit2Sharp.Repository> OpenOrCloneRepositoryAsync(string repositoryUrl, string localPath, CancellationToken ct = default)
    {
        try
        {
            if (Directory.Exists(localPath))
            {
                return new LibGit2Sharp.Repository(localPath);
            }
            else
            {
                LibGit2Sharp.Repository.Clone(repositoryUrl, localPath);
                return new LibGit2Sharp.Repository(localPath);
            }
        }
        catch (Exception ex)
        {
            throw new GitException($"Failed to open or clone repository {repositoryUrl}", ex);
        }
    }

    public async Task<IEnumerable<RepoLens.Core.Entities.Commit>> GetCommitsAsync(LibGit2Sharp.Repository repository, int repositoryId, CancellationToken ct = default)
    {
        try
        {
            var commits = repository.Commits
                .OrderByDescending(c => c.Author.When)
                .Select(c => new RepoLens.Core.Entities.Commit
                {
                    Sha = c.Sha,
                    RepositoryId = repositoryId,
                    Author = c.Author.Name,
                    Timestamp = c.Author.When.DateTime,
                    Message = c.Message
                });

            return commits;
        }
        catch (Exception ex)
        {
            throw new GitException("Failed to retrieve commits", ex);
        }
    }

    public async Task<byte[]> GetBlobContentAsync(LibGit2Sharp.Repository repository, string commitSha, string filePath, CancellationToken ct = default)
    {
        try
        {
            var commit = repository.Lookup<LibGit2Sharp.Commit>(commitSha);
            var treeEntry = commit[filePath];
            
            if (treeEntry.TargetType == TreeEntryTargetType.Blob)
            {
                var blob = (Blob)treeEntry.Target;
                using var stream = blob.GetContentStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, ct);
                return memoryStream.ToArray();
            }

            throw new GitException($"File {filePath} not found in commit {commitSha}");
        }
        catch (Exception ex)
        {
            throw new GitException($"Failed to retrieve blob content for {filePath} in commit {commitSha}", ex);
        }
    }
}
