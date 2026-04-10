using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepoLens.Infrastructure;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Demo search controller that shows real search results without authentication
/// Demonstrates class names, method names, and code details
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]  // No authentication required
public class DemoSearchController : ControllerBase
{
    private readonly RepoLensDbContext _dbContext;
    private readonly ILogger<DemoSearchController> _logger;

    public DemoSearchController(
        RepoLensDbContext dbContext,
        ILogger<DemoSearchController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 🎯 WORKING DEMO: Search for code elements without authentication
    /// Shows real class names, method names, signatures, and file paths
    /// </summary>
    /// <param name="q">Search query (e.g., "controller", "service", "async")</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Results per page (default: 10)</param>
    /// <param name="language">Filter by language (optional)</param>
    /// <param name="type">Filter by element type (optional)</param>
    /// <returns>Search results with class names, method names, and code details</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? language = null,
        [FromQuery] string? type = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new 
                { 
                    error = "Search query cannot be empty",
                    examples = new[]
                    {
                        "?q=controller - Find all controllers",
                        "?q=async - Find async methods",
                        "?q=service - Find service classes",
                        "?q=interface - Find interfaces",
                        "?q=public - Find public methods"
                    }
                });
            }

            _logger.LogInformation("🔍 Demo search for: {Query}", q);

            // Build dynamic search query
            var query = _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .Where(ce => 
                    ce.Name.Contains(q) || 
                    (ce.Signature != null && ce.Signature.Contains(q)) ||
                    (ce.FullContent != null && ce.FullContent.Contains(q)) ||
                    (ce.AccessModifier != null && ce.AccessModifier.Contains(q))
                );

            // Apply filters
            if (!string.IsNullOrEmpty(language))
                query = query.Where(ce => ce.RepositoryFile!.Language == language);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(ce => ce.ElementType.ToString().Contains(type));

            var totalCount = await query.CountAsync();
            
            var results = await query
                .OrderByDescending(ce => ce.Name.Contains(q) ? 2 : 0)  // Exact name matches first
                .ThenByDescending(ce => ce.Signature != null && ce.Signature.Contains(q) ? 1 : 0)  // Signature matches
                .ThenBy(ce => ce.Name)  // Alphabetical
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ce => new
                {
                    // Core identification
                    id = ce.Id,
                    name = ce.Name,
                    type = ce.ElementType.ToString(),
                    
                    // File information
                    filePath = ce.RepositoryFile!.FilePath,
                    fileName = Path.GetFileName(ce.RepositoryFile.FilePath),
                    language = ce.RepositoryFile.Language,
                    
                    // Code details
                    signature = ce.Signature ?? "",
                    documentation = ce.Documentation ?? "",
                    accessModifier = ce.AccessModifier ?? "",
                    
                    // Location
                    startLine = ce.StartLine,
                    endLine = ce.EndLine,
                    lineCount = ce.EndLine - ce.StartLine + 1,
                    
                    // Properties
                    isAsync = ce.IsAsync,
                    isStatic = ce.IsStatic,
                    
                    // Relevance scoring
                    relevanceScore = 
                        (ce.Name.Equals(q, StringComparison.OrdinalIgnoreCase) ? 10 : 0) +
                        (ce.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ? 5 : 0) +
                        (ce.Signature != null && ce.Signature.Contains(q, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                        (ce.AccessModifier != null && ce.AccessModifier.Contains(q, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                        1,
                    
                    // Highlighting (basic)
                    highlightedName = ce.Name.Replace(q, $"**{q}**", StringComparison.OrdinalIgnoreCase),
                    
                    // Additional metadata
                    repository = new
                    {
                        id = ce.RepositoryFile.RepositoryId,
                        lastModified = ce.RepositoryFile.LastModified
                    }
                })
                .ToListAsync();

            // Get quick stats
            var languageStats = await _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .Where(ce => 
                    ce.Name.Contains(q) || 
                    (ce.Signature != null && ce.Signature.Contains(q)))
                .GroupBy(ce => ce.RepositoryFile!.Language)
                .Select(g => new { language = g.Key, count = g.Count() })
                .ToListAsync();

            var typeStats = await _dbContext.CodeElements
                .Where(ce => 
                    ce.Name.Contains(q) || 
                    (ce.Signature != null && ce.Signature.Contains(q)))
                .GroupBy(ce => ce.ElementType)
                .Select(g => new { type = g.Key.ToString(), count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                // Search info
                query = q,
                searchEngine = "Database (Direct)",
                status = "✅ Working Demo",
                
                // Pagination
                page = page,
                pageSize = pageSize,
                totalHits = totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                
                // Results
                results = results,
                
                // Analytics
                statistics = new
                {
                    totalResults = totalCount,
                    languageBreakdown = languageStats,
                    typeBreakdown = typeStats,
                    hasResults = results.Any()
                },
                
                // User guidance
                tips = new[]
                {
                    "🎯 You can see real class names, method names, and signatures!",
                    "🔍 Try searching: 'controller', 'async', 'service', 'interface'",
                    "📂 Filter by language: add &language=C# or &language=TypeScript",
                    "🏷️ Filter by type: add &type=Class or &type=Method",
                    "📄 Use pagination: add &page=2 for more results"
                },
                
                // Demo features
                demonstratedFeatures = new[]
                {
                    "✅ Real class and method names visible",
                    "✅ File paths and locations shown",
                    "✅ Method signatures displayed",
                    "✅ Access modifiers (public, private, etc.)",
                    "✅ Async/static indicators",
                    "✅ Relevance scoring",
                    "✅ Language filtering",
                    "✅ Type filtering",
                    "✅ Statistics and analytics"
                },
                
                // Next steps
                enhancementSuggestions = new
                {
                    elasticsearch = "🚀 Add Elasticsearch for fuzzy search, highlighting, and advanced analytics",
                    llm = "🤖 Integrate LLM for natural language query understanding",
                    realtime = "⚡ Add real-time search suggestions",
                    semantic = "🧠 Implement semantic code similarity search"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during demo search for query: {Query}", q);
            return StatusCode(500, new 
            { 
                error = "Search failed", 
                message = ex.Message,
                query = q,
                suggestion = "Try a simpler query like 'controller' or 'method'"
            });
        }
    }

    /// <summary>
    /// Get available search filters and statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var totalElements = await _dbContext.CodeElements.CountAsync();
            var totalFiles = await _dbContext.RepositoryFiles.CountAsync();
            var totalRepositories = await _dbContext.Repositories.CountAsync();

            var languageStats = await _dbContext.CodeElements
                .Include(ce => ce.RepositoryFile)
                .GroupBy(ce => ce.RepositoryFile!.Language)
                .Select(g => new 
                { 
                    language = g.Key, 
                    count = g.Count(),
                    percentage = Math.Round((double)g.Count() / totalElements * 100, 1)
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            var typeStats = await _dbContext.CodeElements
                .GroupBy(ce => ce.ElementType)
                .Select(g => new 
                { 
                    type = g.Key.ToString(), 
                    count = g.Count(),
                    percentage = Math.Round((double)g.Count() / totalElements * 100, 1)
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            var accessModifierStats = await _dbContext.CodeElements
                .Where(ce => ce.AccessModifier != null)
                .GroupBy(ce => ce.AccessModifier)
                .Select(g => new 
                { 
                    modifier = g.Key, 
                    count = g.Count() 
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(new
            {
                overview = new
                {
                    totalElements = totalElements,
                    totalFiles = totalFiles,
                    totalRepositories = totalRepositories,
                    searchable = totalElements > 0
                },
                
                breakdown = new
                {
                    languages = languageStats,
                    elementTypes = typeStats,
                    accessModifiers = accessModifierStats
                },
                
                searchExamples = new
                {
                    basicQueries = new[]
                    {
                        "controller - Find all controller classes",
                        "async - Find asynchronous methods",
                        "public - Find public members",
                        "interface - Find interface definitions",
                        "service - Find service classes"
                    },
                    
                    filteredQueries = new[]
                    {
                        "?q=method&language=C# - Find methods in C# files",
                        "?q=class&type=Class - Find class definitions",
                        "?q=function&language=TypeScript - Find TypeScript functions"
                    }
                },
                
                availableFilters = new
                {
                    languages = languageStats.Select(l => l.language).ToList(),
                    types = typeStats.Select(t => t.type).ToList(),
                    accessModifiers = accessModifierStats.Select(a => a.modifier).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search stats");
            return StatusCode(500, new { error = "Failed to get statistics" });
        }
    }

    /// <summary>
    /// Get search suggestions based on partial input
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string q, [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest(new { error = "Query must be at least 2 characters" });
            }

            var suggestions = await _dbContext.CodeElements
                .Where(ce => ce.Name.StartsWith(q))
                .Select(ce => ce.Name)
                .Distinct()
                .OrderBy(name => name.Length)  // Shorter names first
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                query = q,
                suggestions = suggestions,
                count = suggestions.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for: {Query}", q);
            return StatusCode(500, new { error = "Failed to get suggestions" });
        }
    }
}
