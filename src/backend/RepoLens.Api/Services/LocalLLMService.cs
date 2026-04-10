using System.Text.Json;
using System.Text.Json.Serialization;

namespace RepoLens.Api.Services;

/// <summary>
/// Local LLM service using CodeLlama via Ollama for natural language code search
/// Provides free, privacy-preserving code understanding capabilities
/// </summary>
public interface ILocalLLMService
{
    Task<bool> IsAvailableAsync();
    Task<StructuredCodeQuery> ProcessNaturalLanguageQueryAsync(string naturalLanguageQuery);
    Task<string> ExplainSearchResultsAsync(IEnumerable<object> searchResults, string originalQuery);
    Task<List<string>> GenerateSearchSuggestionsAsync(string partialQuery);
    Task<CodeQueryIntent> ClassifyQueryIntentAsync(string query);
}

public class LocalLLMService : ILocalLLMService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LocalLLMService> _logger;
    private readonly string _ollamaBaseUrl;
    private readonly string _primaryModel;
    private readonly string _fallbackModel;

    public LocalLLMService(HttpClient httpClient, ILogger<LocalLLMService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ollamaBaseUrl = configuration.GetValue<string>("LLM:OllamaUrl") ?? "http://localhost:11434";
        _primaryModel = configuration.GetValue<string>("LLM:PrimaryModel") ?? "codellama:7b-instruct";
        _fallbackModel = configuration.GetValue<string>("LLM:FallbackModel") ?? "phi3:mini";

        // Configure timeout for LLM requests
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            _logger.LogDebug("Checking Ollama availability at {BaseUrl}", _ollamaBaseUrl);
            
            var response = await _httpClient.GetAsync($"{_ollamaBaseUrl}/api/tags");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var models = JsonSerializer.Deserialize<OllamaModelsResponse>(content);
                
                var hasCodeLlama = models?.Models?.Any(m => m.Name.Contains("codellama")) ?? false;
                
                _logger.LogInformation("Ollama available. CodeLlama model present: {HasCodeLlama}", hasCodeLlama);
                return hasCodeLlama;
            }
            
            _logger.LogWarning("Ollama not available: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check Ollama availability");
            return false;
        }
    }

    public async Task<StructuredCodeQuery> ProcessNaturalLanguageQueryAsync(string naturalLanguageQuery)
    {
        try
        {
            _logger.LogInformation("Processing natural language query: {Query}", naturalLanguageQuery);
            
            var prompt = CreateCodeQueryPrompt(naturalLanguageQuery);
            
            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = _primaryModel,
                Prompt = prompt,
                Stream = false,
                Format = "json",
                Options = new OllamaOptions
                {
                    Temperature = 0.1f, // Low temperature for consistent structured output
                    TopP = 0.9f,
                    NumCtx = 4096
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_ollamaBaseUrl}/api/generate", ollamaRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LLM request failed with status: {StatusCode}", response.StatusCode);
                return CreateFallbackQuery(naturalLanguageQuery);
            }

            var content = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(content);

            if (string.IsNullOrEmpty(ollamaResponse?.Response))
            {
                _logger.LogWarning("Empty response from LLM");
                return CreateFallbackQuery(naturalLanguageQuery);
            }

            // Parse the JSON response from the LLM
            var structuredQuery = JsonSerializer.Deserialize<StructuredCodeQuery>(ollamaResponse.Response);
            
            if (structuredQuery == null)
            {
                _logger.LogWarning("Failed to parse LLM response as structured query");
                return CreateFallbackQuery(naturalLanguageQuery);
            }

            // Enhance and validate the structured query
            structuredQuery = EnhanceStructuredQuery(structuredQuery, naturalLanguageQuery);
            
            _logger.LogInformation("Successfully processed query. Intent: {Intent}, Confidence: {Confidence}", 
                structuredQuery.Intent, structuredQuery.Confidence);
            
            return structuredQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query: {Query}", naturalLanguageQuery);
            return CreateFallbackQuery(naturalLanguageQuery);
        }
    }

    public async Task<string> ExplainSearchResultsAsync(IEnumerable<object> searchResults, string originalQuery)
    {
        try
        {
            var resultsJson = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var prompt = $@"
Explain these search results for the query ""{originalQuery}"" in a helpful way:

Search Results:
{resultsJson}

Provide a brief, technical explanation of what was found and why it's relevant to the query. Focus on:
1. What types of code elements were found
2. Why they match the search criteria
3. Any patterns or insights about the codebase

Keep the explanation concise but informative for a developer audience.
";

            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = _primaryModel,
                Prompt = prompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.3f, // Slightly higher for more natural language
                    TopP = 0.9f,
                    NumCtx = 4096
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_ollamaBaseUrl}/api/generate", ollamaRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(content);
                return ollamaResponse?.Response ?? "Search results found but explanation unavailable.";
            }

            return "Search completed successfully. Results show matching code elements from your repositories.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate result explanation");
            return "Search completed successfully. Review the results for relevant code elements.";
        }
    }

    public async Task<List<string>> GenerateSearchSuggestionsAsync(string partialQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(partialQuery) || partialQuery.Length < 2)
            {
                return GetDefaultSuggestions();
            }

            var prompt = $@"
Complete this partial search query for code search with 5 relevant suggestions:
Partial query: ""{partialQuery}""

Return ONLY a JSON array of strings, like:
[""suggestion 1"", ""suggestion 2"", ""suggestion 3"", ""suggestion 4"", ""suggestion 5""]

Focus on common programming patterns, class/method names, and coding concepts.
";

            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = _fallbackModel, // Use faster model for suggestions
                Prompt = prompt,
                Stream = false,
                Format = "json",
                Options = new OllamaOptions
                {
                    Temperature = 0.4f,
                    TopP = 0.8f,
                    NumCtx = 1024
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_ollamaBaseUrl}/api/generate", ollamaRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(content);
                
                if (!string.IsNullOrEmpty(ollamaResponse?.Response))
                {
                    var suggestions = JsonSerializer.Deserialize<string[]>(ollamaResponse.Response);
                    if (suggestions != null && suggestions.Length > 0)
                    {
                        return suggestions.ToList();
                    }
                }
            }

            return GetDefaultSuggestions();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate search suggestions");
            return GetDefaultSuggestions();
        }
    }

    public async Task<CodeQueryIntent> ClassifyQueryIntentAsync(string query)
    {
        try
        {
            var prompt = $@"
Classify the intent of this code search query:
Query: ""{query}""

Return JSON with:
{{
    ""type"": ""code_search|code_explanation|general_question|count_elements|find_pattern"",
    ""confidence"": 0.95,
    ""requires_structural_analysis"": true|false,
    ""complexity"": ""simple|moderate|complex""
}}
";

            var ollamaRequest = new OllamaGenerateRequest
            {
                Model = _fallbackModel, // Use fast model for classification
                Prompt = prompt,
                Stream = false,
                Format = "json",
                Options = new OllamaOptions
                {
                    Temperature = 0.1f,
                    TopP = 0.9f,
                    NumCtx = 1024
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_ollamaBaseUrl}/api/generate", ollamaRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(content);
                
                if (!string.IsNullOrEmpty(ollamaResponse?.Response))
                {
                    var intent = JsonSerializer.Deserialize<CodeQueryIntent>(ollamaResponse.Response);
                    if (intent != null)
                    {
                        return intent;
                    }
                }
            }

            // Fallback classification
            return new CodeQueryIntent
            {
                Type = "code_search",
                Confidence = 0.5f,
                RequiresStructuralAnalysis = false,
                Complexity = "simple"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to classify query intent");
            return new CodeQueryIntent
            {
                Type = "code_search",
                Confidence = 0.3f,
                RequiresStructuralAnalysis = false,
                Complexity = "simple"
            };
        }
    }

    #region Private Methods

    private string CreateCodeQueryPrompt(string naturalLanguageQuery)
    {
        return $@"
Convert this natural language query about code into structured search parameters.

Query: ""{naturalLanguageQuery}""

Extract and return ONLY valid JSON with this exact structure:
{{
    ""intent"": ""find_class|find_method|search_pattern|explain_code|count_elements"",
    ""target_type"": ""class|method|interface|variable|file|property"",
    ""keywords"": [""extracted"", ""keywords"", ""from"", ""query""],
    ""language_filters"": [""C#"", ""TypeScript"", ""JavaScript""],
    ""access_modifiers"": [""public"", ""private"", ""protected"", ""internal""],
    ""patterns"": [""async"", ""static"", ""abstract"", ""virtual""],
    ""file_patterns"": [""*.cs"", ""*.ts"", ""*.js"", ""*.tsx""],
    ""additional_filters"": {{
        ""is_async"": true,
        ""is_static"": false,
        ""inherits_from"": ""ControllerBase"",
        ""implements"": ""IInterface"",
        ""has_attribute"": ""HttpGet""
    }},
    ""confidence"": 0.95,
    ""search_scope"": ""global|current_file|current_class""
}}

Examples:
- ""find async controllers"" → intent: ""find_class"", keywords: [""controller""], patterns: [""async""], file_patterns: [""*Controller.cs""]
- ""show me TypeScript functions"" → intent: ""find_method"", target_type: ""method"", language_filters: [""TypeScript""]
- ""search for error handling"" → intent: ""search_pattern"", keywords: [""error"", ""handling"", ""exception""]

Return ONLY the JSON, no other text.
";
    }

    private StructuredCodeQuery EnhanceStructuredQuery(StructuredCodeQuery query, string originalQuery)
    {
        // Ensure required fields have defaults
        query.Keywords ??= ExtractSimpleKeywords(originalQuery);
        query.LanguageFilters ??= new List<string>();
        query.AccessModifiers ??= new List<string>();
        query.Patterns ??= new List<string>();
        query.FilePatterns ??= new List<string>();
        query.AdditionalFilters ??= new Dictionary<string, object>();
        
        // Validate confidence
        if (query.Confidence <= 0 || query.Confidence > 1)
        {
            query.Confidence = 0.7f;
        }

        // Set defaults for missing required fields
        if (string.IsNullOrEmpty(query.Intent))
        {
            query.Intent = "search_pattern";
        }

        if (string.IsNullOrEmpty(query.TargetType))
        {
            query.TargetType = InferTargetType(originalQuery);
        }

        // Add intelligent defaults based on common patterns
        if (originalQuery.ToLower().Contains("controller") && !query.FilePatterns.Any())
        {
            query.FilePatterns.Add("*Controller.cs");
        }

        if (originalQuery.ToLower().Contains("component") && !query.FilePatterns.Any())
        {
            query.FilePatterns.AddRange(new[] { "*.tsx", "*.jsx" });
        }

        return query;
    }

    private StructuredCodeQuery CreateFallbackQuery(string naturalLanguageQuery)
    {
        var keywords = ExtractSimpleKeywords(naturalLanguageQuery);
        
        return new StructuredCodeQuery
        {
            Intent = "search_pattern",
            TargetType = InferTargetType(naturalLanguageQuery),
            Keywords = keywords,
            LanguageFilters = new List<string>(),
            AccessModifiers = new List<string>(),
            Patterns = new List<string>(),
            FilePatterns = new List<string>(),
            AdditionalFilters = new Dictionary<string, object>(),
            Confidence = 0.4f,
            SearchScope = "global",
            OriginalQuery = naturalLanguageQuery,
            ProcessingMethod = "fallback"
        };
    }

    private List<string> ExtractSimpleKeywords(string query)
    {
        var commonWords = new HashSet<string> { "find", "search", "show", "get", "list", "all", "the", "a", "an", "in", "of", "for", "with", "that", "and", "or" };
        
        return query.ToLower()
            .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2 && !commonWords.Contains(word))
            .Distinct()
            .ToList();
    }

    private string InferTargetType(string query)
    {
        var lowerQuery = query.ToLower();
        
        if (lowerQuery.Contains("class") || lowerQuery.Contains("controller") || lowerQuery.Contains("service"))
            return "class";
        if (lowerQuery.Contains("method") || lowerQuery.Contains("function"))
            return "method";
        if (lowerQuery.Contains("interface"))
            return "interface";
        if (lowerQuery.Contains("property") || lowerQuery.Contains("field"))
            return "property";
        if (lowerQuery.Contains("variable"))
            return "variable";
        if (lowerQuery.Contains("file"))
            return "file";
            
        return "class"; // Default assumption
    }

    private List<string> GetDefaultSuggestions()
    {
        return new List<string>
        {
            "find all controllers",
            "async methods",
            "public classes",
            "TypeScript interfaces",
            "error handling patterns"
        };
    }

    #endregion
}

#region Models

public class StructuredCodeQuery
{
    public string Intent { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> LanguageFilters { get; set; } = new();
    public List<string> AccessModifiers { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public List<string> FilePatterns { get; set; } = new();
    public Dictionary<string, object> AdditionalFilters { get; set; } = new();
    public float Confidence { get; set; }
    public string SearchScope { get; set; } = "global";
    public string OriginalQuery { get; set; } = string.Empty;
    public string ProcessingMethod { get; set; } = "llm";
}

public class CodeQueryIntent
{
    public string Type { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public bool RequiresStructuralAnalysis { get; set; }
    public string Complexity { get; set; } = string.Empty;
}

public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}

public class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 0.1f;

    [JsonPropertyName("top_p")]
    public float TopP { get; set; } = 0.9f;

    [JsonPropertyName("num_ctx")]
    public int NumCtx { get; set; } = 2048;
}

public class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; set; }
}

public class OllamaModelsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel>? Models { get; set; }
}

public class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; } = string.Empty;
}

#endregion
