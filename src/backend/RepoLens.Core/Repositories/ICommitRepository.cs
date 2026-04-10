using RepoLens.Core.Entities;

namespace RepoLens.Core.Repositories;

public interface ICommitRepository
{
    Task<Commit> AddAsync(Commit commit);
    Task<List<Commit>> AddRangeAsync(List<Commit> commits);
    Task<List<Commit>> GetCommitsByRepositoryAsync(int repositoryId);
    Task<List<Commit>> GetCommitsByRepositoryAsync(int repositoryId, DateTime since, DateTime until);
    Task<Commit?> GetCommitByShaAsync(string sha);
    Task<Commit?> GetCommitByShaAsync(int repositoryId, string sha);
    Task<bool> CommitExistsAsync(int repositoryId, string sha);
    Task<int> GetCommitCountAsync(int repositoryId);
    Task<List<Commit>> GetRecentCommitsAsync(int repositoryId, int count = 100);
    Task<DateTime?> GetLastCommitDateAsync(int repositoryId);
}
