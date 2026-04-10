using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

public interface ISemanticSearchService
{
    Task<bool> IsAvailableAsync();
    Task<List<SemanticSearchResult>> SearchAsync(string query, int limit, double similarityThreshold);
    Task<List<SemanticSearchResult>> FindSimilarAsync(int codeElementId, int limit);
    Task<SemanticSearchStats> GetIndexStatsAsync();
    Task RebuildIndexAsync();
    Task<double> CalculateSimilarityAsync(string text1, string text2);
}

public class SemanticSearchResult
{
    public int CodeElementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ElementType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public List<string> MatchReasons { get; set; } = new();
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class SemanticSearchStats
{
    public string EmbeddingModel { get; set; } = string.Empty;
    public int VectorDimensions { get; set; }
    public int TotalIndexedElements { get; set; }
    public int TotalEmbeddings { get; set; }
    public DateTime LastIndexUpdate { get; set; }
    public string IndexStatus { get; set; } = string.Empty;
    public long IndexSizeBytes { get; set; }
    public TimeSpan AverageSearchTime { get; set; }
}
