using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;

namespace RepoLens.Infrastructure.Repositories;

public class CommitRepository : ICommitRepository
{
    private readonly RepoLensDbContext _context;

    public CommitRepository(RepoLensDbContext context)
    {
        _context = context;
    }

    public async Task<Commit> AddAsync(Commit commit)
    {
        // Check if commit already exists to avoid duplicates
        var existingCommit = await _context.Commits
            .FirstOrDefaultAsync(c => c.RepositoryId == commit.RepositoryId && c.Sha == commit.Sha);
        
        if (existingCommit != null)
        {
            return existingCommit;
        }

        _context.Commits.Add(commit);
        await _context.SaveChangesAsync();
        return commit;
    }

    public async Task<List<Commit>> AddRangeAsync(List<Commit> commits)
    {
        if (!commits.Any()) return commits;

        var repositoryId = commits.First().RepositoryId;
        var commitShas = commits.Select(c => c.Sha).ToList();
        
        // Get existing commits to avoid duplicates
        var existingCommits = await _context.Commits
            .Where(c => c.RepositoryId == repositoryId && commitShas.Contains(c.Sha))
            .Select(c => c.Sha)
            .ToListAsync();

        // Filter out existing commits
        var newCommits = commits
            .Where(c => !existingCommits.Contains(c.Sha))
            .ToList();

        if (newCommits.Any())
        {
            _context.Commits.AddRange(newCommits);
            await _context.SaveChangesAsync();
        }

        // Return all commits (new + existing)
        return await GetCommitsByRepositoryAsync(repositoryId, commitShas);
    }

    public async Task<List<Commit>> GetCommitsByRepositoryAsync(int repositoryId)
    {
        return await _context.Commits
            .Where(c => c.RepositoryId == repositoryId)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Commit>> GetCommitsByRepositoryAsync(int repositoryId, DateTime since, DateTime until)
    {
        return await _context.Commits
            .Where(c => c.RepositoryId == repositoryId && c.Timestamp >= since && c.Timestamp <= until)
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();
    }

    public async Task<Commit?> GetCommitByShaAsync(string sha)
    {
        return await _context.Commits
            .FirstOrDefaultAsync(c => c.Sha == sha);
    }

    public async Task<Commit?> GetCommitByShaAsync(int repositoryId, string sha)
    {
        return await _context.Commits
            .FirstOrDefaultAsync(c => c.RepositoryId == repositoryId && c.Sha == sha);
    }

    public async Task<bool> CommitExistsAsync(int repositoryId, string sha)
    {
        return await _context.Commits
            .AnyAsync(c => c.RepositoryId == repositoryId && c.Sha == sha);
    }

    public async Task<int> GetCommitCountAsync(int repositoryId)
    {
        return await _context.Commits
            .CountAsync(c => c.RepositoryId == repositoryId);
    }

    public async Task<List<Commit>> GetRecentCommitsAsync(int repositoryId, int count = 100)
    {
        return await _context.Commits
            .Where(c => c.RepositoryId == repositoryId)
            .OrderByDescending(c => c.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<DateTime?> GetLastCommitDateAsync(int repositoryId)
    {
        return await _context.Commits
            .Where(c => c.RepositoryId == repositoryId)
            .OrderByDescending(c => c.Timestamp)
            .Select(c => c.Timestamp)
            .FirstOrDefaultAsync();
    }

    private async Task<List<Commit>> GetCommitsByRepositoryAsync(int repositoryId, List<string> shas)
    {
        return await _context.Commits
            .Where(c => c.RepositoryId == repositoryId && shas.Contains(c.Sha))
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync();
    }
}
