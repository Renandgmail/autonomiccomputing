using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepoLens.Api.Models;
using RepoLens.Api.Services;
using RepoLens.Infrastructure;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Enhanced search controller using Elasticsearch for fast, intelligent code search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ElasticSearchController : ControllerBase
{
    private readonly IElasticsearchService _elasticSearchService;
    private readonly RepoLensDbContext _dbContext;
    private readonly ILogger<ElasticSearchController> _logger;

    public ElasticSearchController(
        IElasticsearchService elasticSearchService,
        RepoLensDbContext dbContext,
        ILogger<ElasticSearchController> logger)
    {
        _elasticSearchService = elasticSearchService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// DEMO: Enhanced search endpoint that works without authentication
    /// Shows real search results with class names, method names, etc.
    /// </summary>
    /// <param name="q">Search query</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Results per page</param>
    /// <param name="language">Filter by programming language</param>
    /// <param name="type">Filter by code element type</param>
    /// <param name="repositoryId">Filter by repository</param>
    /// <returns>Enhanced search results with highlighting and relevance scoring</returns>
    [HttpGet("demo")]
    [AllowAnonymous]  // No authentication required for demo
    public async Task<IActionResult> DemoSearch(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? language = null,
        [FromQuery] string? type = null,
        [FromQuery] int? repositoryId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new { error = "Search query cannot be empty", example = "?q=controller" });
            }

            _logger.LogInformation("Demo search for: {Query}", q);

            // Try Elasticsearch first, fallback to database if not available
            try
            {
                var elasticRequest = new ElasticSearchRequest
                {
                    Query = q,
                    Page = page,
                    PageSize = pageSize,
                    Language = language,
                    Type = type,
                    RepositoryId = repositoryId,
                    FuzzySearch = true
                };

                var elasticResults = await _elasticSearchService.SearchAsync(elasticRequest);
                
                if (elasticResults.Results.Any())
                {
                    return Ok(new
                    {
                        searchEngine = "Elasticsearch",
                        query = q,
                        totalHits = elasticResults.TotalHits,
                        page = elasticResults.Page,
                        pageSize = elasticResults.PageSize,
                        processingTime = $"{elasticResults.ProcessingTime:F1}ms",
                        results = elasticResults.Results.Select(r => new
                        {
                            id = r.Id,
                            name = r.Name,
                            type = r.Type,
                            filePath = r.FilePath,
                            language = r.Language,
                            signature = r.Signature,
                            documentation = r.Documentation,
                            startLine = r.StartLine,
                            endLine = r.EndLine,
                            relevanceScore = Math.Round(r.RelevanceScore, 3),
                            highlighted = r.HighlightedContent,
                            metadata = r.Metadata
                        }),
                        aggregations = elasticResults.Aggregations,
                        suggestions = elasticResults.Suggestions
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Elasticsearch unavailable, falling back to database search");
            }

            // Fallback to direct database search
            var query = _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .Where(ce => ce.Name.Contains(q) || 
                            (ce.Signature != null && ce.Signature.Contains(q)) ||
                            (ce.FullContent != null && ce.FullContent.Contains(q)));

            if (repositoryId.HasValue)
                query = query.Where(ce => ce.RepositoryFile!.RepositoryId == repositoryId.Value);

            if (!string.IsNullOrEmpty(language))
                query = query.Where(ce => ce.RepositoryFile!.Language == language);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(ce => ce.ElementType.ToString() == type);

            var totalCount = await query.CountAsync();
            
            var results = await query
                .OrderByDescending(ce => ce.Name.Contains(q) ? 1 : 0)  // Exact name matches first
                .ThenBy(ce => ce.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ce => new
                {
                    id = ce.Id.ToString(),
                    name = ce.Name,
                    type = ce.ElementType.ToString(),
                    filePath = ce.RepositoryFile!.FilePath,
                    language = ce.RepositoryFile.Language,
                    signature = ce.Signature ?? "",
                    documentation = ce.Documentation ?? "",
                    startLine = ce.StartLine,
                    endLine = ce.EndLine,
                    relevanceScore = ce.Name.Contains(q) ? 1.0 : 0.5,
                    highlighted = new[] { ce.Name },
                    isAsync = ce.IsAsync,
                    isStatic = ce.IsStatic,
                    accessModifier = ce.AccessModifier ?? "",
                    lastModified = ce.RepositoryFile.LastModified
                })
                .ToListAsync();

            return Ok(new
            {
                searchEngine = "Database",
                query = q,
                totalHits = totalCount,
                page = page,
                pageSize = pageSize,
                processingTime = "N/A",
                results = results,
                message = "🚀 This is a demo search! Install Elasticsearch for enhanced features like fuzzy search, highlighting, and relevance scoring."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during demo search for query: {Query}", q);
            return StatusCode(500, new { error = "Search failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Enhanced search with full Elasticsearch features (requires authentication)
    /// </summary>
    [HttpPost("search")]
    [Authorize]
    public async Task<IActionResult> EnhancedSearch([FromBody] ElasticSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Search query cannot be empty"));
            }

            _logger.LogInformation("Enhanced search for: {Query}", request.Query);

            var results = await _elasticSearchService.SearchAsync(request);
            
            return Ok(ApiResponse<ElasticSearchResponse>.SuccessResult(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced search for query: {Query}", request.Query);
            return StatusCode(500, ApiResponse<object>.ErrorResult("Enhanced search failed"));
        }
    }

    /// <summary>
    /// Get intelligent search suggestions (works without authentication)
    /// </summary>
    [HttpGet("suggestions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest(new { error = "Query must be at least 2 characters" });
            }

            var suggestions = await _elasticSearchService.GetSuggestionsAsync(q, limit);
            
            return Ok(new
            {
                query = q,
                suggestions = suggestions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for: {Query}", q);
            
            // Fallback to database search for suggestions
            var dbSuggestions = await _dbContext.CodeElements
                .Where(ce => ce.Name.StartsWith(q))
                .Select(ce => ce.Name)
                .Distinct()
                .Take(limit)
                .ToListAsync();
            
            return Ok(new
            {
                query = q,
                suggestions = dbSuggestions,
                fallback = true
            });
        }
    }

    /// <summary>
    /// Index repository code into Elasticsearch for enhanced search (admin only)
    /// </summary>
    [HttpPost("index/{repositoryId}")]
    [Authorize] // Add admin role check if needed
    public async Task<IActionResult> IndexRepository(int repositoryId)
    {
        try
        {
            _logger.LogInformation("Starting indexing for repository: {RepositoryId}", repositoryId);

            // Create index if it doesn't exist
            await _elasticSearchService.CreateIndexAsync("repolens-code");

            // Get all code elements for the repository
            var codeElements = await _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .Where(ce => ce.RepositoryFile!.RepositoryId == repositoryId)
                .ToListAsync();

            if (!codeElements.Any())
            {
                return NotFound(new { error = $"No code elements found for repository {repositoryId}" });
            }

            // Bulk index all code elements
            var success = await _elasticSearchService.BulkIndexCodeElementsAsync(codeElements);
            
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to index repository" });
            }

            return Ok(new
            {
                message = "Repository successfully indexed",
                repositoryId = repositoryId,
                indexedElements = codeElements.Count,
                searchAvailable = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing repository: {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "Indexing failed", message = ex.Message });
        }
    }

    /// <summary>
    /// Get search statistics and available filters
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSearchStats()
    {
        try
        {
            var stats = await _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .GroupBy(ce => ce.RepositoryFile!.Language)
                .Select(g => new
                {
                    language = g.Key,
                    count = g.Count(),
                    types = g.GroupBy(ce => ce.ElementType)
                        .Select(tg => new { type = tg.Key.ToString(), count = tg.Count() })
                        .ToList()
                })
                .ToListAsync();

            var totalElements = await _dbContext.CodeElements.CountAsync();
            var totalRepositories = await _dbContext.Repositories.CountAsync();
            var totalFiles = await _dbContext.RepositoryFiles.CountAsync();

            return Ok(new
            {
                totalElements = totalElements,
                totalRepositories = totalRepositories,
                totalFiles = totalFiles,
                languageBreakdown = stats,
                searchTips = new[]
                {
                    "Try searching for 'async methods' or 'public classes'",
                    "Use specific class names like 'Controller' or 'Service'",
                    "Search for patterns like 'error handling' or 'validation'",
                    "Filter by language: C#, TypeScript, JavaScript, etc."
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search stats");
            return StatusCode(500, new { error = "Failed to get search statistics" });
        }
    }
}
