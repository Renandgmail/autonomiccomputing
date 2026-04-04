using Nest;

namespace RepoLens.Api.Models;

/// <summary>
/// Elasticsearch document representing searchable code elements
/// </summary>
[ElasticsearchType(RelationName = "code-element")]
public class CodeSearchDocument
{
    [Keyword]
    public string Id { get; set; } = string.Empty;
    
    [Text(Analyzer = "standard")]
    public string Name { get; set; } = string.Empty;
    
    [Keyword]
    public string Type { get; set; } = string.Empty;
    
    [Text(Analyzer = "code_analyzer")]
    public string Content { get; set; } = string.Empty;
    
    [Text(Analyzer = "standard")]
    public string Signature { get; set; } = string.Empty;
    
    [Text(Analyzer = "path_analyzer")]
    public string FilePath { get; set; } = string.Empty;
    
    [Keyword]
    public string Language { get; set; } = string.Empty;
    
    [Keyword]
    public string AccessModifier { get; set; } = string.Empty;
    
    [Boolean]
    public bool IsAsync { get; set; }
    
    [Boolean]
    public bool IsStatic { get; set; }
    
    [Text(Analyzer = "standard")]
    public string Documentation { get; set; } = string.Empty;
    
    [Number]
    public int StartLine { get; set; }
    
    [Number]
    public int EndLine { get; set; }
    
    [Number]
    public int RepositoryId { get; set; }
    
    [Date]
    public DateTime LastModified { get; set; }
    
    // For auto-complete functionality
    [Completion]
    public CompletionField NameSuggest { get; set; } = new();
    
    // Tokenized content for better search
    [Text(Analyzer = "keyword")]
    public List<string> Keywords { get; set; } = new();
    
    // Nested object for file information
    [Nested]
    public FileInfo FileInfo { get; set; } = new();
}

[ElasticsearchType(RelationName = "file-info")]
public class FileInfo
{
    [Text]
    public string FileName { get; set; } = string.Empty;
    
    [Keyword]
    public string FileExtension { get; set; } = string.Empty;
    
    [Date]
    public DateTime LastModified { get; set; }
    
    [Number]
    public long FileSize { get; set; }
}

/// <summary>
/// Search result model for API responses
/// </summary>
public class ElasticSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> HighlightedContent { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Search request model
/// </summary>
public class ElasticSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int? RepositoryId { get; set; }
    public string? Language { get; set; }
    public string? Type { get; set; }
    public string? AccessModifier { get; set; }
    public bool? IsAsync { get; set; }
    public bool? IsStatic { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool FuzzySearch { get; set; } = true;
}

/// <summary>
/// Search response model
/// </summary>
public class ElasticSearchResponse
{
    public string Query { get; set; } = string.Empty;
    public int TotalHits { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public double ProcessingTime { get; set; }
    public List<ElasticSearchResult> Results { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public Dictionary<string, List<string>> Aggregations { get; set; } = new();
}

/// <summary>
/// Demo search result model for testing
/// </summary>
public class SearchResultDemo
{
    public string Query { get; set; } = string.Empty;
    public int TotalHits { get; set; }
    public int Count => TotalHits; // Property that tests expect
    public List<object> Results { get; set; } = new();
    public string SearchEngine { get; set; } = string.Empty;
    public double ProcessingTime { get; set; }
}
