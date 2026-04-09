using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepoLens.Api.Services;
using RepoLens.Core.Services;
using System.Diagnostics;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Semantic Search Controller
/// Provides advanced vector-based semantic search capabilities for code elements
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Demo mode - remove for production
public class SemanticSearchController : ControllerBase
{
    private readonly ISemanticSearchService _semanticSearchService;
    private readonly ILocalLLMService _llmService;
    private readonly ILogger<SemanticSearchController> _logger;

    public SemanticSearchController(
        ISemanticSearchService semanticSearchService,
        ILocalLLMService llmService,
        ILogger<SemanticSearchController> logger)
    {
        _semanticSearchService = semanticSearchService;
        _llmService = llmService;
        _logger = logger;
    }

    /// <summary>
    /// 🧠 SEMANTIC SEARCH: Find code elements using vector similarity
    /// Examples: "authentication logic", "database connection patterns", "error handling"
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SemanticSearch([FromBody] SemanticSearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new 
                { 
                    error = "Search query cannot be empty",
                    examples = new[]
                    {
                        "authentication logic",
                        "database connection patterns", 
                        "error handling mechanisms",
                        "async data processing",
                        "user interface components"
                    }
                });
            }

            _logger.LogInformation("🧠 Semantic search: {Query}", request.Query);

            // Check if semantic search is available
            var available = await _semanticSearchService.IsAvailableAsync();
            if (!available)
            {
                return BadRequest(new
                {
                    error = "Semantic search not available",
                    message = "Vector embedding service is not running",
                    fallback = "Use /api/NaturalLanguageSearch for keyword-based search",
                    setup = new
                    {
                        step1 = "Install sentence-transformers: pip install sentence-transformers",
                        step2 = "Start embedding server: python -m sentence_transformers.SentenceTransformer",
                        step3 = "Configure endpoint in appsettings.json"
                    }
                });
            }

            // Perform semantic search
            var results = await _semanticSearchService.SearchAsync(
                request.Query, 
                request.Limit ?? 50, 
                request.SimilarityThreshold ?? 0.7
            );

            // Get enhanced explanations if LLM is available
            string? explanation = null;
            var enhancedResults = results;
            
            try
            {
                var llmAvailable = await _llmService.IsAvailableAsync();
                if (llmAvailable && results.Any())
                {
                    var explanationTask = _llmService.ExplainSearchResultsAsync(
                        results.Select(r => new { r.Name, r.ElementType, r.SimilarityScore }).ToList(), 
                        request.Query
                    );
                    
                    explanation = await explanationTask.WaitAsync(TimeSpan.FromSeconds(5));
                }
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("LLM explanation timed out");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get LLM explanation");
            }

            stopwatch.Stop();

            var response = new
            {
                // Query info
                query = request.Query,
                searchType = "semantic_vector",
                embeddingModel = "sentence-transformers/all-MiniLM-L6-v2",
                
                // Results
                results = enhancedResults.Select(r => new
                {
                    id = r.CodeElementId,
                    name = r.Name,
                    type = r.ElementType,
                    filePath = r.FilePath,
                    language = r.Language,
                    signature = r.Signature,
                    documentation = r.Documentation,
                    similarityScore = Math.Round(r.SimilarityScore, 3),
                    matchReasons = r.MatchReasons,
                    repository = new
                    {
                        id = r.RepositoryId,
                        name = r.RepositoryName
                    },
                    location = new
                    {
                        startLine = r.StartLine,
                        endLine = r.EndLine
                    }
                }),
                
                // Metadata
                totalResults = results.Count,
                processingTime = $"{stopwatch.ElapsedMilliseconds}ms",
                searchConfig = new
                {
                    similarityThreshold = request.SimilarityThreshold ?? 0.7,
                    maxResults = request.Limit ?? 50,
                    vectorDimensions = 384
                },
                
                // Explanation
                explanation = explanation ?? "Semantic search completed. Results ordered by vector similarity.",
                
                // Features
                features = new[]
                {
                    "✅ Vector embeddings",
                    "✅ Semantic similarity",
                    "✅ Cross-language understanding",
                    "✅ Conceptual matching",
                    "✅ Contextual relevance"
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in semantic search for query: {Query}", request.Query);
            return StatusCode(500, new 
            { 
                error = "Semantic search failed",
                message = ex.Message,
                fallback = "Try /api/NaturalLanguageSearch for keyword-based search"
            });
        }
    }

    /// <summary>
    /// 🔍 Find semantically similar code elements to a given element
    /// </summary>
    [HttpGet("similar/{codeElementId}")]
    public async Task<IActionResult> FindSimilar(int codeElementId, [FromQuery] int limit = 10)
    {
        try
        {
            var available = await _semanticSearchService.IsAvailableAsync();
            if (!available)
            {
                return BadRequest(new { error = "Semantic search not available" });
            }

            var similar = await _semanticSearchService.FindSimilarAsync(codeElementId, limit);
            
            return Ok(new
            {
                targetElementId = codeElementId,
                similarElements = similar.Select(s => new
                {
                    id = s.CodeElementId,
                    name = s.Name,
                    type = s.ElementType,
                    filePath = s.FilePath,
                    language = s.Language,
                    similarityScore = Math.Round(s.SimilarityScore, 3),
                    repository = s.RepositoryName
                }),
                totalFound = similar.Count,
                searchType = "semantic_similarity"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar elements to {Id}", codeElementId);
            return StatusCode(500, new { error = "Failed to find similar elements" });
        }
    }

    /// <summary>
    /// 📊 Get semantic search capabilities and statistics
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var available = await _semanticSearchService.IsAvailableAsync();
            var stats = await _semanticSearchService.GetIndexStatsAsync();
            
            return Ok(new
            {
                semanticSearch = new
                {
                    available = available,
                    status = available ? "✅ Active" : "❌ Unavailable",
                    embeddingModel = stats.EmbeddingModel,
                    vectorDimensions = stats.VectorDimensions
                },
                
                index = new
                {
                    totalElements = stats.TotalIndexedElements,
                    totalEmbeddings = stats.TotalEmbeddings,
                    lastUpdate = stats.LastIndexUpdate,
                    indexStatus = stats.IndexStatus,
                    sizeBytes = stats.IndexSizeBytes,
                    sizeMB = Math.Round(stats.IndexSizeBytes / 1024.0 / 1024.0, 2)
                },
                
                performance = new
                {
                    averageSearchTime = $"{stats.AverageSearchTime.TotalMilliseconds}ms",
                    embeddingCacheEnabled = true,
                    batchProcessing = true
                },
                
                capabilities = new[]
                {
                    "Vector similarity search",
                    "Cross-language semantic understanding", 
                    "Conceptual code matching",
                    "Similar element discovery",
                    "Contextual relevance scoring"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic search status");
            return StatusCode(500, new { error = "Failed to get status" });
        }
    }

    /// <summary>
    /// 🔄 Rebuild the semantic search index
    /// </summary>
    [HttpPost("rebuild-index")]
    public async Task<IActionResult> RebuildIndex()
    {
        try
        {
            _logger.LogInformation("Starting semantic search index rebuild...");
            
            await _semanticSearchService.RebuildIndexAsync();
            
            var stats = await _semanticSearchService.GetIndexStatsAsync();
            
            return Ok(new
            {
                message = "✅ Index rebuild completed",
                stats = new
                {
                    totalElements = stats.TotalIndexedElements,
                    lastUpdate = stats.LastIndexUpdate,
                    indexSizeMB = Math.Round(stats.IndexSizeBytes / 1024.0 / 1024.0, 2)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding semantic search index");
            return StatusCode(500, new { error = "Index rebuild failed" });
        }
    }

    /// <summary>
    /// 🧮 Calculate semantic similarity between two texts
    /// </summary>
    [HttpPost("similarity")]
    public async Task<IActionResult> CalculateSimilarity([FromBody] SimilarityRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Text1) || string.IsNullOrWhiteSpace(request.Text2))
            {
                return BadRequest(new { error = "Both texts are required" });
            }

            var similarity = await _semanticSearchService.CalculateSimilarityAsync(request.Text1, request.Text2);
            
            return Ok(new
            {
                text1 = request.Text1,
                text2 = request.Text2,
                similarity = Math.Round(similarity, 4),
                similarityPercent = Math.Round(similarity * 100, 1) + "%",
                interpretation = GetSimilarityInterpretation(similarity)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity");
            return StatusCode(500, new { error = "Similarity calculation failed" });
        }
    }

    /// <summary>
    /// 📚 Get example queries for semantic search
    /// </summary>
    [HttpGet("examples")]
    public IActionResult GetExamples()
    {
        return Ok(new
        {
            conceptualQueries = new[]
            {
                "user authentication logic",
                "database connection handling", 
                "error logging mechanisms",
                "data validation patterns",
                "caching strategies"
            },
            
            functionalQueries = new[]
            {
                "async data processing",
                "file upload functionality",
                "payment processing logic",
                "notification systems",
                "security middleware"
            },
            
            architecturalQueries = new[]
            {
                "service layer components",
                "dependency injection configuration",
                "API controller patterns",
                "domain model entities",
                "repository implementations"
            },
            
            qualityQueries = new[]
            {
                "error handling best practices",
                "performance optimization code",
                "unit test examples", 
                "logging implementations",
                "configuration management"
            }
        });
    }

    #region Private Methods

    private string GetSimilarityInterpretation(double similarity)
    {
        return similarity switch
        {
            >= 0.9 => "Very High - Nearly identical concepts",
            >= 0.8 => "High - Closely related concepts", 
            >= 0.7 => "Good - Semantically related",
            >= 0.6 => "Moderate - Some conceptual overlap",
            >= 0.5 => "Low - Limited similarity",
            _ => "Very Low - Different concepts"
        };
    }

    #endregion
}

#region Request Models

public class SemanticSearchRequest
{
    public string Query { get; set; } = string.Empty;
    public int? Limit { get; set; }
    public double? SimilarityThreshold { get; set; }
    public int? RepositoryId { get; set; }
    public string[]? Languages { get; set; }
    public string[]? ElementTypes { get; set; }
}

public class SimilarityRequest  
{
    public string Text1 { get; set; } = string.Empty;
    public string Text2 { get; set; } = string.Empty;
}

#endregion
