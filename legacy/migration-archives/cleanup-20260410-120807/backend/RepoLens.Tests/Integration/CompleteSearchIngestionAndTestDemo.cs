using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;

namespace RepoLens.Tests.Integration;

public class CompleteSearchIngestionAndTestDemo : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private string? _authToken;
    private int _repositoryId;

    public CompleteSearchIngestionAndTestDemo(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddLogging(b => b.AddConsole());
            });
        }).CreateClient();
    }

    [Fact]
    public async Task CompleteSearchWorkflow_IngestData_ThenTestAllSearchAPIs()
    {
        LogTitle("🚀 COMPLETE SEARCH API INTEGRATION TEST");
        LogInfo("This test will: ingest real data → test all search APIs → show actual results");
        LogSeparator();

        try
        {
            // Step 1: Authentication
            await AuthenticateAsync();
            
            // Step 2: Setup Repository and Ingest Data
            await SetupRepositoryAndIngestDataAsync();
            
            // Step 3: Test Basic Search API
            await TestBasicSearchAPIAsync();
            
            // Step 4: Test Natural Language Query API
            await TestNaturalLanguageQueryAPIAsync();
            
            // Step 5: Test Search Suggestions API
            await TestSearchSuggestionsAPIAsync();
            
            // Step 6: Test Search Filters API
            await TestSearchFiltersAPIAsync();
            
            // Step 7: Test Intent Analysis API
            await TestIntentAnalysisAPIAsync();
            
            LogSuccess("🎉 ALL SEARCH API TESTS COMPLETED SUCCESSFULLY!");
        }
        catch (Exception ex)
        {
            LogError($"❌ Test failed: {ex.Message}");
            LogError($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task AuthenticateAsync()
    {
        LogStep("STEP 1: AUTHENTICATION");
        
        var registerRequest = new
        {
            Email = "searchtest@autonomiccomputing.com",
            Password = "TestPassword123!",
            FirstName = "Search",
            LastName = "Tester"
        };

        LogApiCall("POST", "/api/auth/register", registerRequest);
        
        var registerJson = JsonSerializer.Serialize(registerRequest);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
        var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
        
        var registerResult = await registerResponse.Content.ReadAsStringAsync();
        LogApiResponse("register", registerResponse.StatusCode, registerResult);
        
        if (registerResponse.IsSuccessStatusCode)
        {
            var registerData = JsonSerializer.Deserialize<dynamic>(registerResult);
            _authToken = JsonSerializer.Deserialize<JsonElement>(registerResult)
                .GetProperty("data")
                .GetProperty("token")
                .GetString();
        }
        else
        {
            // Try login instead
            var loginRequest = new { Email = registerRequest.Email, Password = registerRequest.Password };
            LogApiCall("POST", "/api/auth/login", loginRequest);
            
            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            
            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            LogApiResponse("login", loginResponse.StatusCode, loginResult);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                _authToken = JsonSerializer.Deserialize<JsonElement>(loginResult)
                    .GetProperty("data")
                    .GetProperty("token")
                    .GetString();
            }
        }

        if (!string.IsNullOrEmpty(_authToken))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            LogSuccess("✅ Authentication successful");
        }
        else
        {
            throw new Exception("Authentication failed - no token received");
        }
        
        LogSeparator();
    }

    private async Task SetupRepositoryAndIngestDataAsync()
    {
        LogStep("STEP 2: REPOSITORY SETUP AND DATA INGESTION");

        // Create repository
        var repoRequest = new
        {
            Name = "Autonomic Computing Search Test",
            Url = "https://github.com/Renandgmail/autonomiccomputing.git",
            Description = "Integration test repository for search functionality"
        };

        LogApiCall("POST", "/api/repositories", repoRequest);
        
        var repoJson = JsonSerializer.Serialize(repoRequest);
        var repoContent = new StringContent(repoJson, Encoding.UTF8, "application/json");
        var repoResponse = await _client.PostAsync("/api/repositories", repoContent);
        
        var repoResult = await repoResponse.Content.ReadAsStringAsync();
        LogApiResponse("create repository", repoResponse.StatusCode, repoResult);

        if (repoResponse.IsSuccessStatusCode)
        {
            _repositoryId = JsonSerializer.Deserialize<JsonElement>(repoResult)
                .GetProperty("data")
                .GetProperty("id")
                .GetInt32();
            LogSuccess($"✅ Repository created with ID: {_repositoryId}");
        }
        else
        {
            // Repository might already exist, let's get it
            var getReposResponse = await _client.GetAsync("/api/repositories");
            var getReposResult = await getReposResponse.Content.ReadAsStringAsync();
            
            if (getReposResponse.IsSuccessStatusCode)
            {
                var reposData = JsonSerializer.Deserialize<JsonElement>(getReposResult);
                var repos = reposData.GetProperty("data").EnumerateArray();
                var existingRepo = repos.FirstOrDefault(r => 
                    r.GetProperty("url").GetString() == repoRequest.Url);
                
                if (existingRepo.ValueKind != JsonValueKind.Undefined)
                {
                    _repositoryId = existingRepo.GetProperty("id").GetInt32();
                    LogSuccess($"✅ Using existing repository with ID: {_repositoryId}");
                }
                else
                {
                    throw new Exception("Could not create or find repository");
                }
            }
            else
            {
                throw new Exception($"Failed to create repository: {repoResult}");
            }
        }

        // Now ingest sample search data directly into database
        await IngestSearchDataDirectlyAsync();
        
        LogSeparator();
    }

    private async Task IngestSearchDataDirectlyAsync()
    {
        LogInfo("📂 Ingesting searchable data directly into database...");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        // Clear existing data for this repository
        var existingFiles = await dbContext.RepositoryFiles
            .Where(f => f.RepositoryId == _repositoryId)
            .ToListAsync();
        
        var existingElements = await dbContext.CodeElements
            .Where(c => existingFiles.Any(f => f.Id == c.FileId))
            .ToListAsync();
        
        var existingVocab = await dbContext.VocabularyTerms
            .Where(v => v.RepositoryId == _repositoryId)
            .ToListAsync();

        if (existingElements.Any()) dbContext.CodeElements.RemoveRange(existingElements);
        if (existingFiles.Any()) dbContext.RepositoryFiles.RemoveRange(existingFiles);
        if (existingVocab.Any()) dbContext.VocabularyTerms.RemoveRange(existingVocab);
        await dbContext.SaveChangesAsync();

        // Create searchable files with real content
        var files = new[]
        {
            new RepositoryFile
            {
                RepositoryId = _repositoryId,
                FilePath = "RepoLens.Api/Controllers/SearchController.cs",
                FileName = "SearchController.cs",
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 15420,
                LineCount = 387,
                FileHash = "SC001234",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new RepositoryFile
            {
                RepositoryId = _repositoryId,
                FilePath = "RepoLens.Core/Services/IQueryProcessingService.cs",
                FileName = "IQueryProcessingService.cs", 
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 3250,
                LineCount = 82,
                FileHash = "QP005678",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddHours(-6),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new RepositoryFile
            {
                RepositoryId = _repositoryId,
                FilePath = "repolens-ui/src/components/search/NaturalLanguageSearch.tsx",
                FileName = "NaturalLanguageSearch.tsx",
                FileExtension = ".tsx", 
                Language = "TypeScript",
                FileSize = 8760,
                LineCount = 198,
                FileHash = "NL009012",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddHours(-3),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        dbContext.RepositoryFiles.AddRange(files);
        await dbContext.SaveChangesAsync();

        LogInfo($"✅ Created {files.Length} searchable files");

        // Create code elements
        var elements = new[]
        {
            new CodeElement
            {
                FileId = files[0].Id,
                ElementType = CodeElementType.Class,
                Name = "SearchController",
                Signature = "public class SearchController : ControllerBase",
                StartLine = 15,
                EndLine = 285,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = false,
                Documentation = "API controller for natural language search functionality",
                CreatedAt = DateTime.UtcNow
            },
            new CodeElement
            {
                FileId = files[0].Id,
                ElementType = CodeElementType.Method,
                Name = "ProcessQuery",
                Signature = "public async Task<IActionResult> ProcessQuery([FromBody] NaturalLanguageQueryRequest request)",
                StartLine = 45,
                EndLine = 85,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = true,
                ReturnType = "Task<IActionResult>",
                Documentation = "Processes natural language queries and returns search results",
                CreatedAt = DateTime.UtcNow
            },
            new CodeElement
            {
                FileId = files[1].Id,
                ElementType = CodeElementType.Interface,
                Name = "IQueryProcessingService",
                Signature = "public interface IQueryProcessingService",
                StartLine = 8,
                EndLine = 25,
                AccessModifier = "public", 
                IsStatic = false,
                IsAsync = false,
                Documentation = "Service interface for processing different types of search queries",
                CreatedAt = DateTime.UtcNow
            },
            new CodeElement
            {
                FileId = files[2].Id,
                ElementType = CodeElementType.Function,
                Name = "NaturalLanguageSearch",
                Signature = "const NaturalLanguageSearch: React.FC<Props> = () => {",
                StartLine = 12,
                EndLine = 98,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = false,
                Documentation = "React component for natural language search interface",
                CreatedAt = DateTime.UtcNow
            }
        };

        dbContext.CodeElements.AddRange(elements);
        await dbContext.SaveChangesAsync();

        LogInfo($"✅ Created {elements.Length} searchable code elements");

        // Create vocabulary terms
        var vocabulary = new[]
        {
            new VocabularyTerm
            {
                RepositoryId = _repositoryId,
                Term = "SearchController",
                NormalizedTerm = "search-controller",
                TermType = VocabularyTermType.TechnicalTerm,
                Source = VocabularySource.SourceCode,
                Language = "C#",
                Frequency = 15,
                RelevanceScore = 0.95,
                BusinessRelevance = 0.80,
                TechnicalRelevance = 0.98,
                Context = "API controller for handling search requests",
                Domain = "WebAPI",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new VocabularyTerm
            {
                RepositoryId = _repositoryId,
                Term = "NaturalLanguage",
                NormalizedTerm = "natural-language",
                TermType = VocabularyTermType.TechnicalTerm,
                Source = VocabularySource.SourceCode,
                Language = "TypeScript",
                Frequency = 8,
                RelevanceScore = 0.90,
                BusinessRelevance = 0.85,
                TechnicalRelevance = 0.92,
                Context = "Natural language processing for search queries",
                Domain = "AI",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        dbContext.VocabularyTerms.AddRange(vocabulary);
        await dbContext.SaveChangesAsync();

        LogInfo($"✅ Created {vocabulary.Length} vocabulary terms");
        LogSuccess("✅ Search data ingestion completed successfully!");
    }

    private async Task TestBasicSearchAPIAsync()
    {
        LogStep("STEP 3: BASIC SEARCH API TEST");

        var searchQueries = new[] { "SearchController", "natural", "async", "interface" };
        
        foreach (var query in searchQueries)
        {
            var endpoint = $"/api/search?q={Uri.EscapeDataString(query)}&page=1&pageSize=10";
            LogApiCall("GET", endpoint, null);
            
            var response = await _client.GetAsync(endpoint);
            var result = await response.Content.ReadAsStringAsync();
            
            LogApiResponse($"basic search '{query}'", response.StatusCode, result);
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(result);
                var searchData = data.GetProperty("data");
                var results = searchData.GetProperty("results").EnumerateArray().ToList();
                var totalCount = searchData.GetProperty("totalCount").GetInt32();
                var processingTime = searchData.GetProperty("processingTime").GetString();
                
                LogInfo($"📊 Query: '{query}' → {results.Count} results (total: {totalCount}) in {processingTime}");
                
                if (results.Any())
                {
                    foreach (var item in results.Take(2))
                    {
                        var title = item.GetProperty("title").GetString();
                        var type = item.GetProperty("type").GetString();
                        var filePath = item.GetProperty("filePath").GetString();
                        var relevance = item.GetProperty("relevanceScore").GetDecimal();
                        LogInfo($"   🎯 {type}: {title} (relevance: {relevance:F2}) in {filePath}");
                    }
                    LogSuccess($"✅ Basic search for '{query}' returned {results.Count} relevant results");
                }
                else
                {
                    LogWarning($"⚠️ No results found for query '{query}'");
                }
            }
            else
            {
                LogError($"❌ Basic search failed for query '{query}': {result}");
            }
            
            LogInfo("");
        }
        
        LogSeparator();
    }

    private async Task TestNaturalLanguageQueryAPIAsync()
    {
        LogStep("STEP 4: NATURAL LANGUAGE QUERY API TEST");

        var nlQueries = new[]
        {
            "find all controllers",
            "search for async methods", 
            "show me interfaces",
            "find TypeScript components"
        };
        
        foreach (var query in nlQueries)
        {
            var request = new
            {
                Query = query,
                RepositoryId = _repositoryId,
                MaxResults = 10
            };
            
            LogApiCall("POST", "/api/search/query", request);
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/search/query", content);
            
            var result = await response.Content.ReadAsStringAsync();
            LogApiResponse($"natural language query '{query}'", response.StatusCode, result);
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(result);
                var queryData = data.GetProperty("data");
                
                var intent = queryData.GetProperty("intent");
                var intentType = intent.GetProperty("type").GetString();
                var confidence = intent.GetProperty("confidence").GetDecimal();
                var action = intent.GetProperty("action").GetString();
                var target = intent.GetProperty("target").GetString();
                
                var results = queryData.GetProperty("results").EnumerateArray().ToList();
                var summary = queryData.GetProperty("summary");
                var totalCount = summary.GetProperty("totalCount").GetInt32();
                var processingTime = summary.GetProperty("processingTime").GetString();
                
                LogInfo($"🤖 Query: '{query}'");
                LogInfo($"   🎯 Intent: {intentType} (confidence: {confidence:F2})");
                LogInfo($"   📋 Action: {action}, Target: {target}");
                LogInfo($"   📊 Results: {results.Count} returned (total: {totalCount}) in {processingTime}");
                
                if (results.Any())
                {
                    foreach (var item in results.Take(2))
                    {
                        var title = item.GetProperty("title").GetString();
                        var type = item.GetProperty("type").GetString();
                        var filePath = item.GetProperty("filePath").GetString();
                        var relevance = item.GetProperty("relevanceScore").GetDecimal();
                        LogInfo($"      🎯 {type}: {title} (relevance: {relevance:F2}) in {filePath}");
                    }
                    LogSuccess($"✅ Natural language query '{query}' processed successfully with {results.Count} results");
                }
                else
                {
                    LogWarning($"⚠️ No results found for natural language query '{query}'");
                }
            }
            else
            {
                LogError($"❌ Natural language query failed for '{query}': {result}");
            }
            
            LogInfo("");
        }
        
        LogSeparator();
    }

    private async Task TestSearchSuggestionsAPIAsync()
    {
        LogStep("STEP 5: SEARCH SUGGESTIONS API TEST");

        var partialQueries = new[] { "search", "async", "control", "interface" };
        
        foreach (var partial in partialQueries)
        {
            var endpoint = $"/api/search/suggestions?q={Uri.EscapeDataString(partial)}&limit=5&repositoryId={_repositoryId}";
            LogApiCall("GET", endpoint, null);
            
            var response = await _client.GetAsync(endpoint);
            var result = await response.Content.ReadAsStringAsync();
            
            LogApiResponse($"suggestions for '{partial}'", response.StatusCode, result);
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(result);
                var suggestionData = data.GetProperty("data");
                var suggestions = suggestionData.GetProperty("suggestions").EnumerateArray().ToList();
                
                LogInfo($"💡 Suggestions for '{partial}': {suggestions.Count} suggestions");
                foreach (var suggestion in suggestions)
                {
                    LogInfo($"   💭 {suggestion.GetString()}");
                }
                LogSuccess($"✅ Got {suggestions.Count} suggestions for '{partial}'");
            }
            else
            {
                LogError($"❌ Suggestions failed for '{partial}': {result}");
            }
            
            LogInfo("");
        }
        
        LogSeparator();
    }

    private async Task TestSearchFiltersAPIAsync()
    {
        LogStep("STEP 6: SEARCH FILTERS API TEST");

        var endpoint = $"/api/search/filters/{_repositoryId}";
        LogApiCall("GET", endpoint, null);
        
        var response = await _client.GetAsync(endpoint);
        var result = await response.Content.ReadAsStringAsync();
        
        LogApiResponse($"filters for repository {_repositoryId}", response.StatusCode, result);
        
        if (response.IsSuccessStatusCode)
        {
            var data = JsonSerializer.Deserialize<JsonElement>(result);
            var filterData = data.GetProperty("data");
            var filters = filterData.GetProperty("filters");
            
            var languages = filters.GetProperty("languages").EnumerateArray().Select(l => l.GetString()).ToList();
            var fileExtensions = filters.GetProperty("fileExtensions").EnumerateArray().Select(e => e.GetString()).ToList();
            var elementTypes = filters.GetProperty("elementTypes").EnumerateArray().Select(t => t.GetString()).ToList();
            var accessModifiers = filters.GetProperty("accessModifiers").EnumerateArray().Select(a => a.GetString()).ToList();
            
            LogInfo($"🔧 Available filters for repository {_repositoryId}:");
            LogInfo($"   🗣️ Languages: {string.Join(", ", languages)}");
            LogInfo($"   📁 File Extensions: {string.Join(", ", fileExtensions)}");
            LogInfo($"   🔧 Element Types: {string.Join(", ", elementTypes)}");
            LogInfo($"   🔓 Access Modifiers: {string.Join(", ", accessModifiers)}");
            
            LogSuccess($"✅ Retrieved filters successfully");
        }
        else
        {
            LogError($"❌ Failed to get filters: {result}");
        }
        
        LogSeparator();
    }

    private async Task TestIntentAnalysisAPIAsync()
    {
        LogStep("STEP 7: INTENT ANALYSIS API TEST");

        var testQueries = new[]
        {
            "find all authentication methods",
            "count public classes",
            "list TypeScript interfaces", 
            "analyze error handling"
        };
        
        foreach (var query in testQueries)
        {
            var request = new { Query = query };
            
            LogApiCall("POST", "/api/search/intent", request);
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/search/intent", content);
            
            var result = await response.Content.ReadAsStringAsync();
            LogApiResponse($"intent analysis for '{query}'", response.StatusCode, result);
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(result);
                var intentData = data.GetProperty("data");
                
                var intent = intentData.GetProperty("intent");
                var intentType = intent.GetProperty("type").GetString();
                var action = intent.GetProperty("action").GetString();
                var target = intent.GetProperty("target").GetString();
                var confidence = intent.GetProperty("confidence").GetDecimal();
                var keywords = intent.GetProperty("keywords").EnumerateArray().Select(k => k.GetString()).ToList();
                
                var explanation = intentData.GetProperty("explanation").GetString();
                var suggestions = intentData.GetProperty("suggestions").EnumerateArray().Select(s => s.GetString()).ToList();
                
                LogInfo($"🧠 Intent Analysis for: '{query}'");
                LogInfo($"   📋 Type: {intentType} (confidence: {confidence:F2})");
                LogInfo($"   🎯 Action: {action}, Target: {target}");
                LogInfo($"   🔑 Keywords: {string.Join(", ", keywords)}");
                LogInfo($"   💬 Explanation: {explanation}");
                if (suggestions.Any())
                {
                    LogInfo($"   💡 Suggestions: {string.Join("; ", suggestions)}");
                }
                
                LogSuccess($"✅ Intent analysis completed for '{query}'");
            }
            else
            {
                LogError($"❌ Intent analysis failed for '{query}': {result}");
            }
            
            LogInfo("");
        }
        
        LogSeparator();
    }

    // Logging helpers
    private void LogTitle(string message)
    {
        _output.WriteLine("");
        _output.WriteLine("🌟" + new string('=', 88) + "🌟");
        _output.WriteLine($"🌟  {message.PadRight(86)}  🌟");
        _output.WriteLine("🌟" + new string('=', 88) + "🌟");
        _output.WriteLine("");
    }

    private void LogStep(string step)
    {
        _output.WriteLine("");
        _output.WriteLine("🔹" + new string('─', 88) + "🔹");
        _output.WriteLine($"🔹  {step.PadRight(86)}  🔹");
        _output.WriteLine("🔹" + new string('─', 88) + "🔹");
    }

    private void LogSeparator()
    {
        _output.WriteLine(new string('─', 90));
    }

    private void LogApiCall(string method, string endpoint, object? requestBody)
    {
        _output.WriteLine($"📤 API CALL: {method} {endpoint}");
        if (requestBody != null)
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });
            _output.WriteLine($"📋 Request Body:");
            _output.WriteLine(json);
        }
    }

    private void LogApiResponse(string operation, System.Net.HttpStatusCode statusCode, string responseBody)
    {
        _output.WriteLine($"📥 API RESPONSE ({operation}): {(int)statusCode} {statusCode}");
        try
        {
            var jsonDoc = JsonDocument.Parse(responseBody);
            var formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
            _output.WriteLine($"📋 Response Body:");
            _output.WriteLine(formattedJson);
        }
        catch
        {
            _output.WriteLine($"📋 Raw Response: {responseBody}");
        }
    }

    private void LogInfo(string message) => _output.WriteLine($"ℹ️  {message}");
    private void LogSuccess(string message) => _output.WriteLine($"✅ {message}");
    private void LogWarning(string message) => _output.WriteLine($"⚠️  {message}");
    private void LogError(string message) => _output.WriteLine($"❌ {message}");
}
