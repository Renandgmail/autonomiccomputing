using RepoLens.Core.Entities;

namespace RepoLens.Core.Services
{
    public interface IASTAnalysisService
    {
        Task<ASTRepositoryAnalysis> AnalyzeRepositoryAsync(int repositoryId, string repositoryPath, CancellationToken cancellationToken = default);
        Task<ASTFileAnalysis> AnalyzeFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<List<DuplicateCodeBlock>> DetectDuplicateCodeAsync(List<ASTFileAnalysis> files, CancellationToken cancellationToken = default);
        Task<List<ASTDependency>> CalculateDependenciesAsync(List<ASTFileAnalysis> files, CancellationToken cancellationToken = default);
        Task<ASTMetrics> CalculateRepositoryMetricsAsync(List<ASTFileAnalysis> files, CancellationToken cancellationToken = default);
    }

    public interface ITypeScriptASTService
    {
        Task<ASTFileAnalysis> AnalyzeTypeScriptFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<bool> IsTypeScriptFileAsync(string filePath);
        Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken = default);
    }

    public interface ICSharpASTService
    {
        Task<ASTFileAnalysis> AnalyzeCSharpFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<bool> IsCSharpFileAsync(string filePath);
        Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken = default);
    }

    public interface IPythonASTService
    {
        Task<ASTFileAnalysis> AnalyzePythonFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<bool> IsPythonFileAsync(string filePath);
        Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken = default);
    }

    public interface IASTRepositoryService
    {
        Task<ASTRepositoryAnalysis?> GetRepositoryAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default);
        Task<ASTRepositoryAnalysis> SaveRepositoryAnalysisAsync(ASTRepositoryAnalysis analysis, CancellationToken cancellationToken = default);
        Task<List<ASTFileAnalysis>> GetFileAnalysesAsync(int repositoryAnalysisId, CancellationToken cancellationToken = default);
        Task<ASTFileAnalysis?> GetFileAnalysisAsync(int repositoryAnalysisId, string filePath, CancellationToken cancellationToken = default);
        Task DeleteRepositoryAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default);
        Task<bool> IsAnalysisOutdatedAsync(int repositoryId, DateTime lastModified, CancellationToken cancellationToken = default);
    }
}
