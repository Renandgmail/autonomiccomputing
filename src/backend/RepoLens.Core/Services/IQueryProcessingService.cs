using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Service interface for processing natural language queries and converting them to structured searches
/// </summary>
public interface IQueryProcessingService
{
    /// <summary>
    /// Process a natural language query and return structured search results
    /// </summary>
    /// <param name="query">Natural language query from user</param>
    /// <param name="repositoryId">Repository to search within (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with ranked relevance</returns>
    Task<QueryResult> ProcessQueryAsync(string query, int? repositoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract intent from a natural language query
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <returns>Classified intent with confidence score</returns>
    QueryIntent ExtractIntent(string query);

    /// <summary>
    /// Get search suggestions based on partial query input
    /// </summary>
    /// <param name="partialQuery">Partial query text</param>
    /// <param name="repositoryId">Repository context (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested queries</returns>
    Task<IEnumerable<string>> GetSuggestionsAsync(string partialQuery, int? repositoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available search filters for the given repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available filters with their values</returns>
    Task<SearchFilters> GetAvailableFiltersAsync(int repositoryId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of processing a natural language query
/// </summary>
public class QueryResult
{
    public string OriginalQuery { get; set; } = string.Empty;
    public QueryIntent Intent { get; set; } = new();
    public SearchCriteria Criteria { get; set; } = new();
    public List<SearchResultItem> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// Extracted intent from natural language query
/// </summary>
public class QueryIntent
{
    public IntentType Type { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// Types of query intents
/// </summary>
public enum IntentType
{
    Find,           // "find all classes", "show me functions"
    Search,         // "search for authentication", "look for error handling"
    List,           // "list all files", "show repositories"
    Analyze,        // "analyze code quality", "show complexity"
    Compare,        // "compare implementations", "difference between"
    Explain,        // "explain this pattern", "what does this do"
    Count,          // "how many functions", "count classes"
    Filter,         // "filter by language", "show only public methods"
    Unknown
}

/// <summary>
/// Structured search criteria derived from natural language
/// </summary>
public class SearchCriteria
{
    public List<string> Keywords { get; set; } = new();
    public List<CodeElementType> ElementTypes { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> AccessModifiers { get; set; } = new();
    public string FilePattern { get; set; } = string.Empty;
    public bool? IsStatic { get; set; }
    public bool? IsAsync { get; set; }
    public int? MinLines { get; set; }
    public int? MaxLines { get; set; }
    public DateTime? ModifiedAfter { get; set; }
    public DateTime? ModifiedBefore { get; set; }
    public SortOrder SortBy { get; set; } = SortOrder.Relevance;
}

/// <summary>
/// Sort orders for search results
/// </summary>
public enum SortOrder
{
    Relevance,
    Name,
    Size,
    Modified,
    Complexity,
    Language
}

/// <summary>
/// Individual search result item
/// </summary>
public class SearchResultItem
{
    public int Id { get; set; }
    public SearchResultType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public double RelevanceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> HighlightedContent { get; set; } = new();
}

/// <summary>
/// Types of search results
/// </summary>
public enum SearchResultType
{
    File,
    Class,
    Method,
    Function,
    Property,
    Interface,
    Namespace,
    Variable,
    Constant
}

/// <summary>
/// Available search filters for a repository
/// </summary>
public class SearchFilters
{
    public List<string> Languages { get; set; } = new();
    public List<string> FileExtensions { get; set; } = new();
    public List<CodeElementType> ElementTypes { get; set; } = new();
    public List<string> AccessModifiers { get; set; } = new();
    public List<string> CommonKeywords { get; set; } = new();
    public DateRange ModificationDateRange { get; set; } = new();
}

/// <summary>
/// Date range for filtering
/// </summary>
public class DateRange
{
    public DateTime? EarliestDate { get; set; }
    public DateTime? LatestDate { get; set; }
}
