using RepoLens.Core.Entities;

namespace RepoLens.Core.Repositories;

public interface IFileMetricsRepository
{
    Task<FileMetrics?> GetFileMetricsAsync(int repositoryId, string filePath);
    Task<List<FileMetrics>> GetRepositoryFileMetricsAsync(int repositoryId);
    Task<List<FileMetrics>> GetHotspotFilesAsync(int repositoryId, int count = 20);
    Task<List<FileMetrics>> GetHighRiskFilesAsync(int repositoryId, int count = 20);
    Task<List<FileMetrics>> GetFilesByLanguageAsync(int repositoryId, string language);
    Task<FileMetrics> SaveFileMetricsAsync(FileMetrics metrics);
    Task<List<FileMetrics>> SaveFileMetricsBatchAsync(List<FileMetrics> metrics);
    Task<Dictionary<string, int>> GetLanguageDistributionAsync(int repositoryId);
    Task<Dictionary<string, double>> GetComplexityDistributionAsync(int repositoryId);
    Task<List<FileMetrics>> GetFilesNeedingRefactoringAsync(int repositoryId, double minComplexity = 10.0);
}
