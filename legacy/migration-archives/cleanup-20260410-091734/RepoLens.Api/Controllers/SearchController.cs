using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using System.ComponentModel.DataAnnotations;
using IntentType = RepoLens.Core.Services.IntentType;
using QueryIntent = RepoLens.Core.Services.QueryIntent;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for natural language search and query processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // Temporarily disabled for testing
public class SearchController : ControllerBase
{
    private readonly IQueryProcessingService _queryProcessingService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IQueryProcessingService queryProcessingService,
        ILogger<SearchController> logger)
    {
        _queryProcessingService = queryProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Process a natural language query and return relevant code search results
    /// </summary>
    /// <param name="request">Natural language query request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results with intent analysis and ranked relevance</returns>
    /// <response code="200">Returns search results with intent analysis</response>
    /// <response code="400">Invalid query or request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error during query processing</response>
    [HttpPost("query")]
    public async Task<IActionResult> ProcessQuery([FromBody] NaturalLanguageQueryRequest request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query cannot be empty"));
            }

            if (request.Query.Length > 500)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query is too long (max 500 characters)"));
            }

            _logger.LogInformation("Processing natural language query: {Query} for repository: {RepositoryId}", 
                request.Query, request.RepositoryId);

            var result = await _queryProcessingService.ProcessQueryAsync(
                request.Query, 
                request.RepositoryId, 
                ct);

            var response = new
            {
                query = result.OriginalQuery,
                intent = new
                {
                    type = result.Intent.Type.ToString(),
                    action = result.Intent.Action,
                    target = result.Intent.Target,
                    keywords = result.Intent.Keywords,
                    entities = result.Intent.Entities,
                    confidence = result.Intent.ConfidenceScore,
                    parameters = result.Intent.Parameters
                },
                criteria = new
                {
                    keywords = result.Criteria.Keywords,
                    elementTypes = result.Criteria.ElementTypes.Select(t => t.ToString()),
                    languages = result.Criteria.Languages,
                    accessModifiers = result.Criteria.AccessModifiers,
                    isStatic = result.Criteria.IsStatic,
                    isAsync = result.Criteria.IsAsync,
                    sortBy = result.Criteria.SortBy.ToString()
                },
                results = result.Results.Select(r => new
                {
                    id = r.Id,
                    type = r.Type.ToString(),
                    title = r.Title,
                    description = r.Description,
                    filePath = r.FilePath,
                    language = r.Language,
                    startLine = r.StartLine,
                    endLine = r.EndLine,
                    relevanceScore = Math.Round(r.RelevanceScore, 3),
                    metadata = r.Metadata,
                    highlightedContent = r.HighlightedContent
                }),
                summary = new
                {
                    totalCount = result.TotalCount,
                    returnedCount = result.Results.Count,
                    processingTime = $"{result.ProcessingTime.TotalMilliseconds:F1}ms",
                    confidenceScore = Math.Round(result.ConfidenceScore, 3)
                },
                suggestions = result.Suggestions
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query: {Query}", request.Query);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to process query"));
        }
    }

    /// <summary>
    /// Simple search endpoint for basic queries (frontend compatibility)
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of results per page</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results</returns>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Search query cannot be empty"));
            }

            _logger.LogInformation("Processing simple search query: {Query}", q);

            // Convert to natural language query request
            var request = new NaturalLanguageQueryRequest
            {
                Query = q,
                MaxResults = pageSize
            };

            var result = await _queryProcessingService.ProcessQueryAsync(request.Query, request.RepositoryId, ct);

            var searchResults = new
            {
                query = q,
                page = page,
                pageSize = pageSize,
                totalCount = result.TotalCount,
                totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                results = result.Results.Take(pageSize).Select(r => new
                {
                    id = r.Id,
                    type = r.Type.ToString(),
                    title = r.Title,
                    description = r.Description,
                    filePath = r.FilePath,
                    language = r.Language,
                    relevanceScore = Math.Round(r.RelevanceScore, 3),
                    highlightedContent = r.HighlightedContent
                }),
                processingTime = $"{result.ProcessingTime.TotalMilliseconds:F1}ms"
            };

            return Ok(ApiResponse<object>.SuccessResult(searchResults));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search query: {Query}", q);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to process search"));
        }
    }

    /// <summary>
    /// Get search suggestions based on partial query input
    /// </summary>
    /// <param name="q">Partial query text for autocomplete</param>
    /// <param name="limit">Maximum number of suggestions</param>
    /// <param name="repositoryId">Repository context for suggestions</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of suggested queries</returns>
    /// <response code="200">Returns search suggestions</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        [FromQuery] int? repositoryId = null,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query cannot be empty"));
            }

            if (q.Length > 100)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query is too long (max 100 characters)"));
            }

            var suggestions = await _queryProcessingService.GetSuggestionsAsync(q, repositoryId, ct);

            return Ok(ApiResponse<object>.SuccessResult(new
            {
                query = q,
                repositoryId = repositoryId,
                limit = limit,
                suggestions = suggestions.Take(limit).ToList()
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for: {Query}", q);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get suggestions"));
        }
    }

    /// <summary>
    /// Get available search filters for a specific repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Available filters with their possible values</returns>
    /// <response code="200">Returns available search filters</response>
    /// <response code="404">Repository not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("filters/{repositoryId}")]
    public async Task<IActionResult> GetAvailableFilters(
        [FromRoute] int repositoryId,
        CancellationToken ct = default)
    {
        try
        {
            var filters = await _queryProcessingService.GetAvailableFiltersAsync(repositoryId, ct);

            var response = new
            {
                repositoryId = repositoryId,
                filters = new
                {
                    languages = filters.Languages,
                    fileExtensions = filters.FileExtensions,
                    elementTypes = filters.ElementTypes.Select(t => t.ToString()),
                    accessModifiers = filters.AccessModifiers,
                    commonKeywords = filters.CommonKeywords,
                    modificationDateRange = filters.ModificationDateRange != null ? new
                    {
                        earliest = filters.ModificationDateRange.EarliestDate,
                        latest = filters.ModificationDateRange.LatestDate
                    } : null
                }
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filters for repository: {RepositoryId}", repositoryId);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to get filters"));
        }
    }

    /// <summary>
    /// Extract and analyze intent from a natural language query without executing search
    /// </summary>
    /// <param name="request">Query intent analysis request</param>
    /// <returns>Intent analysis with confidence score</returns>
    /// <response code="200">Returns intent analysis</response>
    /// <response code="400">Invalid query</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("intent")]
    public IActionResult AnalyzeIntent([FromBody] QueryIntentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query cannot be empty"));
            }

            if (request.Query.Length > 500)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Query is too long (max 500 characters)"));
            }

            var intent = _queryProcessingService.ExtractIntent(request.Query);

            var response = new
            {
                originalQuery = request.Query,
                intent = new
                {
                    type = intent.Type.ToString(),
                    action = intent.Action,
                    target = intent.Target,
                    keywords = intent.Keywords,
                    entities = intent.Entities,
                    confidence = Math.Round(intent.ConfidenceScore, 3),
                    parameters = intent.Parameters
                },
                explanation = GenerateIntentExplanation(intent),
                suggestions = GenerateQueryImprovements(intent, request.Query)
            };

            return Ok(ApiResponse<object>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing intent for query: {Query}", request.Query);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to analyze intent"));
        }
    }

    /// <summary>
    /// Get example queries for different intent types to help users
    /// </summary>
    /// <returns>Example queries organized by intent type</returns>
    /// <response code="200">Returns example queries</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("examples")]
    public IActionResult GetExampleQueries()
    {
        var examples = new
        {
            find = new[]
            {
                "find all authentication methods",
                "find public classes in TypeScript",
                "find async functions",
                "find error handling code"
            },
            search = new[]
            {
                "search for validation logic",
                "search database queries",
                "search configuration files",
                "search JWT implementation"
            },
            list = new[]
            {
                "list all files",
                "list interfaces",
                "list static methods",
                "list recent changes"
            },
            count = new[]
            {
                "how many classes are there",
                "count public methods",
                "number of TypeScript files",
                "how many async functions"
            },
            analyze = new[]
            {
                "analyze code complexity",
                "check security patterns",
                "review error handling",
                "examine performance code"
            },
            filter = new[]
            {
                "show only C# files",
                "filter by public access",
                "show static utilities",
                "filter recent modifications"
            }
        };

        return Ok(ApiResponse<object>.SuccessResult(examples));
    }

    private string GenerateIntentExplanation(QueryIntent intent)
    {
        return intent.Type switch
        {
            IntentType.Find => $"I understand you want to find specific code elements: '{intent.Target}'",
            IntentType.Search => $"I'll search for code related to: '{intent.Target}'",
            IntentType.List => $"I'll list all instances of: '{intent.Target}'",
            IntentType.Count => $"I'll count the number of: '{intent.Target}'",
            IntentType.Analyze => $"I'll analyze: '{intent.Target}'",
            IntentType.Filter => $"I'll filter results by: '{intent.Target}'",
            IntentType.Compare => $"I'll compare: '{intent.Target}'",
            IntentType.Explain => $"I'll explain: '{intent.Target}'",
            IntentType.Unknown => "I'm not sure what you're looking for. Try being more specific or use keywords like 'find', 'search', or 'list'.",
            _ => "I'll help you explore the codebase."
        };
    }

    private List<string> GenerateQueryImprovements(QueryIntent intent, string originalQuery)
    {
        var suggestions = new List<string>();

        if (intent.ConfidenceScore < 0.6)
        {
            suggestions.Add("Try starting with action words like 'find', 'search', 'list', or 'count'");
            suggestions.Add("Be more specific about what you're looking for");
            suggestions.Add("Include programming language or file type if relevant");
        }

        if (intent.Keywords.Count == 0)
        {
            suggestions.Add("Include technical keywords like 'authentication', 'database', 'error', etc.");
        }

        if (intent.Entities.Count == 0)
        {
            suggestions.Add("Specify code elements like 'class', 'method', 'function', or 'interface'");
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("Your query looks good! You can also try filtering by language or access modifier.");
        }

        return suggestions;
    }
}

// Request models
public class NaturalLanguageQueryRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Query { get; set; } = string.Empty;
    
    public int? RepositoryId { get; set; }
    
    public int MaxResults { get; set; } = 50;
}

public class QueryIntentRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Query { get; set; } = string.Empty;
}
