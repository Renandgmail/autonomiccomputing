using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;

namespace RepoLens.Infrastructure.Services;

/// <summary>
/// Implementation of natural language query processing service
/// Uses rule-based approach without external ML dependencies
/// </summary>
public class QueryProcessingService : IQueryProcessingService
{
    private readonly RepoLensDbContext _context;
    private readonly ILogger<QueryProcessingService> _logger;
    
    // Intent recognition patterns
    private static readonly Dictionary<IntentType, List<string>> IntentPatterns = new()
    {
        {
            IntentType.Find,
            new List<string>
            {
                @"find\s+(?:all\s+)?(.+)",
                @"show\s+(?:me\s+)?(?:all\s+)?(.+)",
                @"get\s+(?:all\s+)?(.+)",
                @"locate\s+(.+)"
            }
        },
        {
            IntentType.Search,
            new List<string>
            {
                @"search\s+(?:for\s+)?(.+)",
                @"look\s+(?:for\s+)?(.+)",
                @"query\s+(.+)",
                @"where\s+(?:is\s+)?(.+)"
            }
        },
        {
            IntentType.List,
            new List<string>
            {
                @"list\s+(?:all\s+)?(.+)",
                @"show\s+(?:all\s+)?(.+)",
                @"display\s+(.+)"
            }
        },
        {
            IntentType.Count,
            new List<string>
            {
                @"how\s+many\s+(.+)",
                @"count\s+(?:of\s+)?(.+)",
                @"number\s+of\s+(.+)"
            }
        },
        {
            IntentType.Analyze,
            new List<string>
            {
                @"analyze\s+(.+)",
                @"check\s+(.+)",
                @"review\s+(.+)",
                @"examine\s+(.+)"
            }
        }
    };

    // Entity patterns for code elements
    private static readonly Dictionary<CodeElementType, List<string>> ElementPatterns = new()
    {
        {
            CodeElementType.Class,
            new List<string> { "class", "classes", "object", "objects" }
        },
        {
            CodeElementType.Method,
            new List<string> { "method", "methods", "function", "functions", "procedure", "procedures" }
        },
        {
            CodeElementType.Property,
            new List<string> { "property", "properties", "field", "fields", "attribute", "attributes" }
        },
        {
            CodeElementType.Interface,
            new List<string> { "interface", "interfaces", "contract", "contracts" }
        },
        {
            CodeElementType.Namespace,
            new List<string> { "namespace", "namespaces", "module", "modules", "package", "packages" }
        }
    };

    // Language patterns
    private static readonly Dictionary<string, List<string>> LanguagePatterns = new()
    {
        { "C#", new List<string> { "c#", "csharp", "c sharp", "dotnet", ".net" } },
        { "TypeScript", new List<string> { "typescript", "ts", "angular", "react" } },
        { "JavaScript", new List<string> { "javascript", "js", "node", "nodejs" } },
        { "Python", new List<string> { "python", "py", "django", "flask" } },
        { "Java", new List<string> { "java", "spring", "maven", "gradle" } },
        { "SQL", new List<string> { "sql", "database", "query", "stored procedure" } }
    };

    // Access modifier patterns
    private static readonly Dictionary<string, List<string>> AccessModifierPatterns = new()
    {
        { "public", new List<string> { "public", "exposed", "external", "api" } },
        { "private", new List<string> { "private", "internal", "hidden", "secret" } },
        { "protected", new List<string> { "protected", "inherited" } },
        { "static", new List<string> { "static", "shared", "utility" } }
    };

    // Common technical keywords
    private static readonly HashSet<string> TechnicalKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "authentication", "authorization", "login", "password", "token", "jwt", "oauth",
        "encryption", "decryption", "hash", "security", "validation", "sanitization",
        "database", "sql", "query", "repository", "entity", "model", "dto",
        "api", "endpoint", "controller", "service", "middleware", "filter",
        "async", "await", "promise", "callback", "event", "handler",
        "exception", "error", "logging", "debugging", "trace", "audit",
        "configuration", "settings", "environment", "deployment", "build",
        "cache", "redis", "memory", "performance", "optimization", "benchmark",
        "test", "unit", "integration", "mock", "stub", "fixture",
        "notification", "email", "sms", "push", "webhook", "queue",
        "file", "upload", "download", "stream", "io", "parser", "serializer"
    };

    public QueryProcessingService(RepoLensDbContext context, ILogger<QueryProcessingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<QueryResult> ProcessQueryAsync(string query, int? repositoryId = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Processing query: {Query}", query);

            // Step 1: Extract intent from the query
            var intent = ExtractIntent(query);
            
            // Step 2: Convert intent to search criteria
            var criteria = ConvertIntentToSearchCriteria(intent, query);
            
            // Step 3: Execute search against database
            var results = await ExecuteSearchAsync(criteria, repositoryId, cancellationToken);
            
            // Step 4: Rank and format results
            var rankedResults = RankResults(results, intent, criteria);
            
            stopwatch.Stop();
            
            var queryResult = new QueryResult
            {
                OriginalQuery = query,
                Intent = intent,
                Criteria = criteria,
                Results = rankedResults.Take(50).ToList(), // Limit to top 50 results
                TotalCount = results.Count,
                ProcessingTime = stopwatch.Elapsed,
                ConfidenceScore = intent.ConfidenceScore,
                Suggestions = await GenerateSuggestionsAsync(query, repositoryId, cancellationToken)
            };

            _logger.LogInformation("Query processed successfully in {ElapsedMs}ms. Found {ResultCount} results",
                stopwatch.ElapsedMilliseconds, results.Count);

            return queryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", query);
            
            stopwatch.Stop();
            return new QueryResult
            {
                OriginalQuery = query,
                Intent = new QueryIntent { Type = IntentType.Unknown, ConfidenceScore = 0.0 },
                ProcessingTime = stopwatch.Elapsed,
                Results = new List<SearchResultItem>(),
                Suggestions = new List<string> { "Try a simpler query", "Check spelling", "Use specific keywords" }
            };
        }
    }

    public QueryIntent ExtractIntent(string query)
    {
        var normalizedQuery = query.ToLowerInvariant().Trim();
        var intent = new QueryIntent
        {
            Type = IntentType.Unknown,
            ConfidenceScore = 0.0
        };

        // Try to match against intent patterns
        foreach (var (intentType, patterns) in IntentPatterns)
        {
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(normalizedQuery, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    intent.Type = intentType;
                    intent.Target = match.Groups[1].Value.Trim();
                    intent.ConfidenceScore = 0.8; // High confidence for pattern match
                    break;
                }
            }
            if (intent.Type != IntentType.Unknown) break;
        }

        // If no pattern matched, use keyword analysis
        if (intent.Type == IntentType.Unknown)
        {
            intent = AnalyzeKeywords(normalizedQuery);
        }

        // Extract entities and keywords
        intent.Keywords = ExtractKeywords(normalizedQuery);
        intent.Entities = ExtractEntities(normalizedQuery);
        intent.Parameters = ExtractParameters(normalizedQuery);

        return intent;
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string partialQuery, int? repositoryId = null, CancellationToken cancellationToken = default)
    {
        var suggestions = new List<string>();
        var normalizedQuery = partialQuery.ToLowerInvariant().Trim();

        // Add intent-based suggestions
        if (normalizedQuery.Length >= 2)
        {
            if (normalizedQuery.StartsWith("find") || normalizedQuery.StartsWith("show"))
            {
                suggestions.AddRange(new[]
                {
                    "find all classes",
                    "find authentication methods",
                    "show error handling",
                    "find async functions",
                    "show public interfaces"
                });
            }
            else if (normalizedQuery.StartsWith("search"))
            {
                suggestions.AddRange(new[]
                {
                    "search for validation",
                    "search database queries",
                    "search configuration",
                    "search logging"
                });
            }
            else if (normalizedQuery.StartsWith("list"))
            {
                suggestions.AddRange(new[]
                {
                    "list all files",
                    "list TypeScript classes",
                    "list public methods",
                    "list recent changes"
                });
            }
        }

        // Add repository-specific suggestions
        if (repositoryId.HasValue)
        {
            var repoSuggestions = await GetRepositorySpecificSuggestionsAsync(repositoryId.Value, normalizedQuery, cancellationToken);
            suggestions.AddRange(repoSuggestions);
        }

        // Add technical keyword suggestions
        var matchingKeywords = TechnicalKeywords
            .Where(k => k.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(k => $"find {k} code");
        suggestions.AddRange(matchingKeywords);

        return suggestions.Distinct().Take(10);
    }

    public async Task<SearchFilters> GetAvailableFiltersAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        var filters = new SearchFilters();

        var repository = await _context.Repositories
            .Include(r => r.RepositoryFiles)
            .ThenInclude(f => f.CodeElements)
            .FirstOrDefaultAsync(r => r.Id == repositoryId, cancellationToken);

        if (repository == null)
        {
            return filters;
        }

        // Extract available languages
        filters.Languages = repository.RepositoryFiles
            .Where(f => !string.IsNullOrEmpty(f.Language))
            .Select(f => f.Language)
            .Distinct()
            .OrderBy(l => l)
            .ToList();

        // Extract file extensions
        filters.FileExtensions = repository.RepositoryFiles
            .Where(f => !string.IsNullOrEmpty(f.FileExtension))
            .Select(f => f.FileExtension)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        // Extract element types
        filters.ElementTypes = repository.RepositoryFiles
            .SelectMany(f => f.CodeElements)
            .Select(e => e.ElementType)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        // Extract access modifiers
        filters.AccessModifiers = repository.RepositoryFiles
            .SelectMany(f => f.CodeElements)
            .Where(e => !string.IsNullOrEmpty(e.AccessModifier))
            .Select(e => e.AccessModifier)
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        // Get modification date range
        var files = repository.RepositoryFiles.Where(f => f.LastModified != null).ToList();
        if (files.Any())
        {
            filters.ModificationDateRange = new DateRange
            {
                EarliestDate = files.Min(f => f.LastModified),
                LatestDate = files.Max(f => f.LastModified)
            };
        }

        // Extract common keywords from code elements
        var elementNames = repository.RepositoryFiles
            .SelectMany(f => f.CodeElements)
            .Select(e => e.Name)
            .Where(n => !string.IsNullOrEmpty(n));

        filters.CommonKeywords = ExtractCommonTerms(elementNames)
            .Take(50)
            .ToList();

        return filters;
    }

    private QueryIntent AnalyzeKeywords(string query)
    {
        var intent = new QueryIntent { Type = IntentType.Search, ConfidenceScore = 0.5 };

        // Check for count-related words
        if (Regex.IsMatch(query, @"\b(how many|count|number)\b", RegexOptions.IgnoreCase))
        {
            intent.Type = IntentType.Count;
            intent.ConfidenceScore = 0.7;
        }
        // Check for analysis words
        else if (Regex.IsMatch(query, @"\b(analyze|check|review|quality|complexity)\b", RegexOptions.IgnoreCase))
        {
            intent.Type = IntentType.Analyze;
            intent.ConfidenceScore = 0.7;
        }
        // Check for listing words
        else if (Regex.IsMatch(query, @"\b(list|all|every)\b", RegexOptions.IgnoreCase))
        {
            intent.Type = IntentType.List;
            intent.ConfidenceScore = 0.6;
        }

        return intent;
    }

    private List<string> ExtractKeywords(string query)
    {
        var keywords = new List<string>();
        var words = Regex.Split(query, @"\W+")
            .Where(w => w.Length > 2)
            .ToList();

        // Add technical keywords
        keywords.AddRange(words.Where(w => TechnicalKeywords.Contains(w)));

        // Add other significant words (excluding common stop words)
        var stopWords = new HashSet<string> { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "man", "may", "new", "now", "old", "see", "two", "way", "who", "boy", "did", "its", "let", "put", "say", "she", "too", "use" };
        keywords.AddRange(words.Where(w => !stopWords.Contains(w.ToLowerInvariant()) && w.Length > 3));

        return keywords.Distinct().ToList();
    }

    private List<string> ExtractEntities(string query)
    {
        var entities = new List<string>();

        // Extract code element types
        foreach (var (elementType, patterns) in ElementPatterns)
        {
            if (patterns.Any(p => query.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                entities.Add(elementType.ToString());
            }
        }

        // Extract languages
        foreach (var (language, patterns) in LanguagePatterns)
        {
            if (patterns.Any(p => query.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                entities.Add(language);
            }
        }

        return entities.Distinct().ToList();
    }

    private Dictionary<string, string> ExtractParameters(string query)
    {
        var parameters = new Dictionary<string, string>();

        // Extract access modifiers
        foreach (var (modifier, patterns) in AccessModifierPatterns)
        {
            if (patterns.Any(p => query.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                parameters["AccessModifier"] = modifier;
                break;
            }
        }

        // Extract async/static indicators
        if (Regex.IsMatch(query, @"\b(async|asynchronous)\b", RegexOptions.IgnoreCase))
        {
            parameters["IsAsync"] = "true";
        }

        if (Regex.IsMatch(query, @"\b(static|shared|utility)\b", RegexOptions.IgnoreCase))
        {
            parameters["IsStatic"] = "true";
        }

        return parameters;
    }

    private SearchCriteria ConvertIntentToSearchCriteria(QueryIntent intent, string originalQuery)
    {
        var criteria = new SearchCriteria
        {
            Keywords = intent.Keywords
        };

        // Map entities to search criteria
        foreach (var entity in intent.Entities)
        {
            if (Enum.TryParse<CodeElementType>(entity, true, out var elementType))
            {
                criteria.ElementTypes.Add(elementType);
            }
            else if (LanguagePatterns.ContainsKey(entity))
            {
                criteria.Languages.Add(entity);
            }
        }

        // Apply parameters
        foreach (var (key, value) in intent.Parameters)
        {
            switch (key)
            {
                case "AccessModifier":
                    criteria.AccessModifiers.Add(value);
                    break;
                case "IsAsync":
                    criteria.IsAsync = bool.Parse(value);
                    break;
                case "IsStatic":
                    criteria.IsStatic = bool.Parse(value);
                    break;
            }
        }

        // Set sort order based on intent
        criteria.SortBy = intent.Type switch
        {
            IntentType.Count => SortOrder.Name,
            IntentType.Analyze => SortOrder.Complexity,
            IntentType.List => SortOrder.Name,
            _ => SortOrder.Relevance
        };

        return criteria;
    }

    private async Task<List<SearchResultItem>> ExecuteSearchAsync(SearchCriteria criteria, int? repositoryId, CancellationToken cancellationToken)
    {
        var query = _context.RepositoryFiles
            .Include(f => f.CodeElements)
            .AsQueryable();

        // Filter by repository
        if (repositoryId.HasValue)
        {
            query = query.Where(f => f.RepositoryId == repositoryId.Value);
        }

        // Filter by languages
        if (criteria.Languages.Any())
        {
            query = query.Where(f => criteria.Languages.Contains(f.Language));
        }

        var files = await query.ToListAsync(cancellationToken);
        var results = new List<SearchResultItem>();

        foreach (var file in files)
        {
            // Add file-level results
            if (ShouldIncludeFile(file, criteria))
            {
                results.Add(new SearchResultItem
                {
                    Id = file.Id,
                    Type = SearchResultType.File,
                    Title = file.FileName,
                    Description = $"{file.Language} file with {file.CodeElements.Count} elements",
                    FilePath = file.FilePath,
                    Language = file.Language,
                    RelevanceScore = CalculateRelevanceScore(file, null, criteria),
                    Metadata = new Dictionary<string, object>
                    {
                        { "FileSize", file.FileSize },
                        { "LineCount", file.LineCount },
                        { "ElementCount", file.CodeElements.Count }
                    }
                });
            }

            // Add code element results
            foreach (var element in file.CodeElements)
            {
                if (ShouldIncludeElement(element, criteria))
                {
                    results.Add(new SearchResultItem
                    {
                        Id = element.Id,
                        Type = MapCodeElementType(element.ElementType),
                        Title = element.Name,
                        Description = GenerateElementDescription(element),
                        FilePath = file.FilePath,
                        Language = file.Language,
                        StartLine = element.StartLine,
                        EndLine = element.EndLine,
                        RelevanceScore = CalculateRelevanceScore(file, element, criteria),
                        Metadata = new Dictionary<string, object>
                        {
                            { "ElementType", element.ElementType.ToString() },
                            { "AccessModifier", element.AccessModifier ?? "unknown" },
                            { "IsStatic", element.IsStatic },
                            { "IsAsync", element.IsAsync }
                        }
                    });
                }
            }
        }

        return results;
    }

    private bool ShouldIncludeFile(RepositoryFile file, SearchCriteria criteria)
    {
        // Check keywords in filename and path
        if (criteria.Keywords.Any())
        {
            var fileText = $"{file.FileName} {file.FilePath}".ToLowerInvariant();
            var hasKeywords = criteria.Keywords.Any(k => fileText.Contains(k.ToLowerInvariant()));
            if (!hasKeywords) return false;
        }

        // Check file pattern
        if (!string.IsNullOrEmpty(criteria.FilePattern))
        {
            if (!Regex.IsMatch(file.FileName, criteria.FilePattern, RegexOptions.IgnoreCase))
                return false;
        }

        // Check modification date
        if (criteria.ModifiedAfter.HasValue && file.LastModified < criteria.ModifiedAfter)
            return false;
        if (criteria.ModifiedBefore.HasValue && file.LastModified > criteria.ModifiedBefore)
            return false;

        return true;
    }

    private bool ShouldIncludeElement(CodeElement element, SearchCriteria criteria)
    {
        // Check element types
        if (criteria.ElementTypes.Any() && !criteria.ElementTypes.Contains(element.ElementType))
            return false;

        // Check access modifiers
        if (criteria.AccessModifiers.Any() && !criteria.AccessModifiers.Contains(element.AccessModifier ?? ""))
            return false;

        // Check static/async flags
        if (criteria.IsStatic.HasValue && element.IsStatic != criteria.IsStatic.Value)
            return false;
        if (criteria.IsAsync.HasValue && element.IsAsync != criteria.IsAsync.Value)
            return false;

        // Check keywords in element name and signature
        if (criteria.Keywords.Any())
        {
            var elementText = $"{element.Name} {element.Signature} {element.Parameters}".ToLowerInvariant();
            var hasKeywords = criteria.Keywords.Any(k => elementText.Contains(k.ToLowerInvariant()));
            if (!hasKeywords) return false;
        }

        return true;
    }

    private double CalculateRelevanceScore(RepositoryFile file, CodeElement? element, SearchCriteria criteria)
    {
        double score = 0.5; // Base score

        var searchText = element != null 
            ? $"{element.Name} {element.Signature} {element.Parameters}".ToLowerInvariant()
            : $"{file.FileName} {file.FilePath}".ToLowerInvariant();

        // Keyword matching score
        foreach (var keyword in criteria.Keywords)
        {
            var keywordLower = keyword.ToLowerInvariant();
            if (searchText.Contains(keywordLower))
            {
                score += 0.2;
                // Exact name match gets higher score
                if (element?.Name.Equals(keyword, StringComparison.OrdinalIgnoreCase) == true)
                {
                    score += 0.3;
                }
            }
        }

        // Element type match
        if (element != null && criteria.ElementTypes.Contains(element.ElementType))
        {
            score += 0.15;
        }

        // Language match
        if (criteria.Languages.Contains(file.Language))
        {
            score += 0.1;
        }

        // Access modifier match
        if (element != null && criteria.AccessModifiers.Contains(element.AccessModifier ?? ""))
        {
            score += 0.1;
        }

        return Math.Min(score, 1.0);
    }

    private List<SearchResultItem> RankResults(List<SearchResultItem> results, QueryIntent intent, SearchCriteria criteria)
    {
        return criteria.SortBy switch
        {
            SortOrder.Relevance => results.OrderByDescending(r => r.RelevanceScore).ToList(),
            SortOrder.Name => results.OrderBy(r => r.Title).ToList(),
            SortOrder.Modified => results.OrderByDescending(r => r.Metadata.ContainsKey("LastModified") ? r.Metadata["LastModified"] : DateTime.MinValue).ToList(),
            SortOrder.Language => results.OrderBy(r => r.Language).ThenByDescending(r => r.RelevanceScore).ToList(),
            _ => results.OrderByDescending(r => r.RelevanceScore).ToList()
        };
    }

    private async Task<List<string>> GenerateSuggestionsAsync(string query, int? repositoryId, CancellationToken cancellationToken)
    {
        var suggestions = new List<string>();

        // Add related technical terms
        var queryWords = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in queryWords)
        {
            var relatedTerms = TechnicalKeywords
                .Where(k => k.Contains(word, StringComparison.OrdinalIgnoreCase) && k != word)
                .Take(2);
            suggestions.AddRange(relatedTerms.Select(t => $"search for {t}"));
        }

        return suggestions.Take(5).ToList();
    }

    private async Task<List<string>> GetRepositorySpecificSuggestionsAsync(int repositoryId, string partialQuery, CancellationToken cancellationToken)
    {
        var suggestions = new List<string>();

        try
        {
            // Get common element names from the repository
            var elementNames = await _context.RepositoryFiles
                .Where(f => f.RepositoryId == repositoryId)
                .SelectMany(f => f.CodeElements)
                .Where(e => e.Name.StartsWith(partialQuery, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Name)
                .Distinct()
                .Take(5)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(elementNames.Select(name => $"find {name}"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get repository-specific suggestions for repository {RepositoryId}", repositoryId);
        }

        return suggestions;
    }

    private List<string> ExtractCommonTerms(IEnumerable<string> names)
    {
        var words = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in names)
        {
            // Split camelCase and PascalCase
            var splitName = Regex.Split(name, @"(?=[A-Z])")
                .Where(s => s.Length > 2)
                .ToList();

            foreach (var word in splitName)
            {
                var cleanWord = Regex.Replace(word, @"[^\w]", "").ToLowerInvariant();
                if (cleanWord.Length > 2)
                {
                    words[cleanWord] = words.GetValueOrDefault(cleanWord, 0) + 1;
                }
            }
        }

        return words
            .Where(kvp => kvp.Value > 1) // Only include terms that appear multiple times
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    private SearchResultType MapCodeElementType(CodeElementType elementType)
    {
        return elementType switch
        {
            CodeElementType.Class => SearchResultType.Class,
            CodeElementType.Method => SearchResultType.Method,
            CodeElementType.Function => SearchResultType.Function,
            CodeElementType.Property => SearchResultType.Property,
            CodeElementType.Interface => SearchResultType.Interface,
            CodeElementType.Namespace => SearchResultType.Namespace,
            CodeElementType.Variable => SearchResultType.Variable,
            CodeElementType.Field => SearchResultType.Variable,
            _ => SearchResultType.Method
        };
    }

    private string GenerateElementDescription(CodeElement element)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(element.AccessModifier))
            parts.Add(element.AccessModifier);

        if (element.IsStatic)
            parts.Add("static");

        if (element.IsAsync)
            parts.Add("async");

        parts.Add(element.ElementType.ToString().ToLowerInvariant());

        if (!string.IsNullOrEmpty(element.ReturnType))
            parts.Add($"returns {element.ReturnType}");

        return string.Join(" ", parts);
    }
}
