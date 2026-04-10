using Nest;
using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using System.Diagnostics;

namespace RepoLens.Api.Services;

/// <summary>
/// Service for Elasticsearch operations including indexing and searching
/// </summary>
public interface IElasticsearchService
{
    Task<bool> CreateIndexAsync(string indexName);
    Task<bool> IndexCodeElementAsync(CodeElement codeElement);
    Task<bool> BulkIndexCodeElementsAsync(IEnumerable<CodeElement> codeElements);
    Task<ElasticSearchResponse> SearchAsync(ElasticSearchRequest request);
    Task<List<string>> GetSuggestionsAsync(string partialQuery, int limit = 10);
    Task<bool> DeleteIndexAsync(string indexName);
}

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly string _defaultIndexName = "repolens-code";

    public ElasticsearchService(IElasticClient elasticClient, ILogger<ElasticsearchService> logger)
    {
        _elasticClient = elasticClient;
        _logger = logger;
    }

    public async Task<bool> CreateIndexAsync(string indexName)
    {
        try
        {
            var existsResponse = await _elasticClient.Indices.ExistsAsync(indexName);
            if (existsResponse.Exists)
            {
                _logger.LogInformation("Index {IndexName} already exists", indexName);
                return true;
            }

            var createResponse = await _elasticClient.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("code_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "stop", "snowball")
                            )
                            .Custom("path_analyzer", pa => pa
                                .Tokenizer("path_hierarchy")
                                .Filters("lowercase")
                            )
                        )
                    )
                )
                .Map<CodeSearchDocument>(m => m
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t.Name(n => n.Name).Analyzer("standard").Fields(f => f.Keyword(k => k.Name("raw"))))
                        .Text(t => t.Name(n => n.Content).Analyzer("code_analyzer"))
                        .Text(t => t.Name(n => n.FilePath).Analyzer("path_analyzer"))
                        .Completion(c => c.Name(n => n.NameSuggest))
                    )
                )
            );

            if (!createResponse.IsValid)
            {
                _logger.LogError("Failed to create index {IndexName}: {Error}", indexName, createResponse.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully created index {IndexName}", indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", indexName);
            return false;
        }
    }

    public async Task<bool> IndexCodeElementAsync(CodeElement codeElement)
    {
        try
        {
            var document = MapCodeElementToDocument(codeElement);
            
            var response = await _elasticClient.IndexDocumentAsync(document);
            
            if (!response.IsValid)
            {
                _logger.LogError("Failed to index code element {ElementId}: {Error}", codeElement.Id, response.OriginalException?.Message);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing code element {ElementId}", codeElement.Id);
            return false;
        }
    }

    public async Task<bool> BulkIndexCodeElementsAsync(IEnumerable<CodeElement> codeElements)
    {
        try
        {
            var documents = codeElements.Select(MapCodeElementToDocument);
            
            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(_defaultIndexName)
                .IndexMany(documents)
            );

            if (!bulkResponse.IsValid)
            {
                _logger.LogError("Bulk indexing failed: {Error}", bulkResponse.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully bulk indexed {Count} code elements", codeElements.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk indexing");
            return false;
        }
    }

    public async Task<ElasticSearchResponse> SearchAsync(ElasticSearchRequest request)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            var searchResponse = await _elasticClient.SearchAsync<CodeSearchDocument>(s => s
                .Index(_defaultIndexName)
                .Query(q => BuildSearchQuery(q, request))
                .Highlight(h => h
                    .Fields(f => f
                        .Field(fd => fd.Name)
                        .Field(fd => fd.Content)
                        .Field(fd => fd.Signature)
                        .Field(fd => fd.Documentation)
                    )
                    .PreTags("<mark>")
                    .PostTags("</mark>")
                )
                .Aggregations(a => a
                    .Terms("languages", t => t.Field(f => f.Language))
                    .Terms("types", t => t.Field(f => f.Type))
                    .Terms("access_modifiers", t => t.Field(f => f.AccessModifier))
                )
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Sort(so => so.Descending(SortSpecialField.Score))
            );

            stopwatch.Stop();

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Search failed: {Error}", searchResponse.OriginalException?.Message);
                return new ElasticSearchResponse { Query = request.Query };
            }

            var results = searchResponse.Documents.Select((doc, index) => new ElasticSearchResult
            {
                Id = doc.Id,
                Name = doc.Name,
                Type = doc.Type,
                FilePath = doc.FilePath,
                Language = doc.Language,
                Signature = doc.Signature,
                Documentation = doc.Documentation,
                StartLine = doc.StartLine,
                EndLine = doc.EndLine,
                RelevanceScore = (double)(searchResponse.Hits.ElementAt(index).Score ?? 0),
                HighlightedContent = ExtractHighlights(searchResponse.Hits.ElementAt(index)),
                Metadata = new Dictionary<string, object>
                {
                    ["isAsync"] = doc.IsAsync,
                    ["isStatic"] = doc.IsStatic,
                    ["accessModifier"] = doc.AccessModifier,
                    ["lastModified"] = doc.LastModified
                }
            }).ToList();

            var aggregations = new Dictionary<string, List<string>>();
            if (searchResponse.Aggregations.ContainsKey("languages"))
            {
                var languageAgg = searchResponse.Aggregations.Terms("languages");
                aggregations["languages"] = languageAgg.Buckets.Select(b => b.Key).ToList();
            }
            if (searchResponse.Aggregations.ContainsKey("types"))
            {
                var typeAgg = searchResponse.Aggregations.Terms("types");
                aggregations["types"] = typeAgg.Buckets.Select(b => b.Key).ToList();
            }

            return new ElasticSearchResponse
            {
                Query = request.Query,
                TotalHits = (int)(searchResponse.Total),
                Page = request.Page,
                PageSize = request.PageSize,
                ProcessingTime = stopwatch.Elapsed.TotalMilliseconds,
                Results = results,
                Aggregations = aggregations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during search for query: {Query}", request.Query);
            return new ElasticSearchResponse { Query = request.Query };
        }
    }

    public async Task<List<string>> GetSuggestionsAsync(string partialQuery, int limit = 10)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<CodeSearchDocument>(s => s
                .Index(_defaultIndexName)
                .Suggest(su => su
                    .Completion("name_suggestions", c => c
                        .Field(f => f.NameSuggest)
                        .Prefix(partialQuery)
                        .Size(limit)
                    )
                )
                .Size(0)
            );

            if (!response.IsValid)
            {
                _logger.LogError("Suggestions failed: {Error}", response.OriginalException?.Message);
                return new List<string>();
            }

            var suggestions = response.Suggest["name_suggestions"]
                .SelectMany(s => s.Options)
                .Select(o => o.Text)
                .Distinct()
                .ToList();

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for: {Query}", partialQuery);
            return new List<string>();
        }
    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        try
        {
            var response = await _elasticClient.Indices.DeleteAsync(indexName);
            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
            return false;
        }
    }

    private QueryContainer BuildSearchQuery(QueryContainerDescriptor<CodeSearchDocument> q, ElasticSearchRequest request)
    {
        var queries = new List<QueryContainer>();

        // Main search query
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var mainQuery = q.Bool(b => b
                .Should(
                    // Exact name matches (highest boost)
                    sh => sh.Match(m => m.Field(f => f.Name).Query(request.Query).Boost(4.0)),
                    // Fuzzy name matches
                    sh => sh.Fuzzy(f => f.Field(fd => fd.Name).Value(request.Query).Boost(2.0)),
                    // Signature matches
                    sh => sh.Match(m => m.Field(f => f.Signature).Query(request.Query).Boost(3.0)),
                    // Content matches
                    sh => sh.Match(m => m.Field(f => f.Content).Query(request.Query).Boost(1.0)),
                    // Documentation matches
                    sh => sh.Match(m => m.Field(f => f.Documentation).Query(request.Query).Boost(0.5))
                )
                .MinimumShouldMatch(1)
            );

            if (request.FuzzySearch)
            {
                mainQuery = q.Bool(b => b
                    .Should(
                        sh => mainQuery,
                        sh => sh.Fuzzy(f => f.Field(fd => fd.Content).Value(request.Query).Boost(0.8))
                    )
                );
            }

            queries.Add(mainQuery);
        }

        // Apply filters
        var filters = new List<QueryContainer>();

        if (request.RepositoryId.HasValue)
            filters.Add(q.Term(t => t.RepositoryId, request.RepositoryId.Value));

        if (!string.IsNullOrWhiteSpace(request.Language))
            filters.Add(q.Term(t => t.Language, request.Language));

        if (!string.IsNullOrWhiteSpace(request.Type))
            filters.Add(q.Term(t => t.Type, request.Type));

        if (!string.IsNullOrWhiteSpace(request.AccessModifier))
            filters.Add(q.Term(t => t.AccessModifier, request.AccessModifier));

        if (request.IsAsync.HasValue)
            filters.Add(q.Term(t => t.IsAsync, request.IsAsync.Value));

        if (request.IsStatic.HasValue)
            filters.Add(q.Term(t => t.IsStatic, request.IsStatic.Value));

        // Combine queries and filters
        if (queries.Any() && filters.Any())
        {
            return q.Bool(b => b
                .Must(queries.ToArray())
                .Filter(filters.ToArray())
            );
        }
        else if (queries.Any())
        {
            return q.Bool(b => b.Must(queries.ToArray()));
        }
        else if (filters.Any())
        {
            return q.Bool(b => b.Filter(filters.ToArray()));
        }

        return q.MatchAll();
    }

    private CodeSearchDocument MapCodeElementToDocument(CodeElement codeElement)
    {
        return new CodeSearchDocument
        {
            Id = codeElement.Id.ToString(),
            Name = codeElement.Name,
            Type = codeElement.ElementType.ToString(),
            Content = codeElement.FullContent ?? string.Empty,
            Signature = codeElement.Signature ?? string.Empty,
            FilePath = codeElement.RepositoryFile?.FilePath ?? string.Empty,
            Language = codeElement.RepositoryFile?.Language ?? string.Empty,
            AccessModifier = codeElement.AccessModifier ?? string.Empty,
            IsAsync = codeElement.IsAsync,
            IsStatic = codeElement.IsStatic,
            Documentation = codeElement.Documentation ?? string.Empty,
            StartLine = codeElement.StartLine,
            EndLine = codeElement.EndLine,
            RepositoryId = codeElement.RepositoryFile?.RepositoryId ?? 0,
            LastModified = codeElement.RepositoryFile?.LastModified ?? DateTime.UtcNow,
            NameSuggest = new CompletionField
            {
                Input = new[] { codeElement.Name }
            },
            Keywords = ExtractKeywords(codeElement),
            FileInfo = new Models.FileInfo
            {
                FileName = Path.GetFileName(codeElement.RepositoryFile?.FilePath ?? ""),
                FileExtension = Path.GetExtension(codeElement.RepositoryFile?.FilePath ?? ""),
                LastModified = codeElement.RepositoryFile?.LastModified ?? DateTime.UtcNow,
                FileSize = codeElement.RepositoryFile?.FileSize ?? 0
            }
        };
    }

    private List<string> ExtractKeywords(CodeElement codeElement)
    {
        var keywords = new List<string>();
        
        // Add element type
        keywords.Add(codeElement.ElementType.ToString().ToLower());
        
        // Add access modifier
        if (!string.IsNullOrEmpty(codeElement.AccessModifier))
            keywords.Add(codeElement.AccessModifier.ToLower());
        
        // Add async/static flags
        if (codeElement.IsAsync)
            keywords.Add("async");
        if (codeElement.IsStatic)
            keywords.Add("static");
        
        // Extract words from name (camelCase, PascalCase)
        keywords.AddRange(SplitCamelCase(codeElement.Name));
        
        return keywords.Distinct().ToList();
    }

    private List<string> SplitCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var words = new List<string>();
        var currentWord = new System.Text.StringBuilder();

        foreach (var c in input)
        {
            if (char.IsUpper(c) && currentWord.Length > 0)
            {
                words.Add(currentWord.ToString().ToLower());
                currentWord.Clear();
            }
            currentWord.Append(c);
        }

        if (currentWord.Length > 0)
        {
            words.Add(currentWord.ToString().ToLower());
        }

        return words;
    }

    private List<string> ExtractHighlights(IHit<CodeSearchDocument> hit)
    {
        var highlights = new List<string>();
        
        if (hit.Highlight != null)
        {
            foreach (var highlight in hit.Highlight)
            {
                highlights.AddRange(highlight.Value);
            }
        }
        
        return highlights;
    }
}
