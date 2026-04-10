using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SearchApiDemo
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string? _baseUrl = "http://localhost:5179";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("🌟" + new string('=', 80) + "🌟");
            Console.WriteLine("🌟  SEARCH API DEMONSTRATION - TESTING ALL ENDPOINTS           🌟");
            Console.WriteLine("🌟" + new string('=', 80) + "🌟");
            Console.WriteLine();
            
            try
            {
                // Test if API is running
                Console.WriteLine("📡 Testing API connectivity...");
                var healthResponse = await _httpClient.GetAsync($"{_baseUrl}/api/health");
                if (!healthResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("❌ API not responding. Please start the RepoLens API first.");
                    Console.WriteLine("💡 Run: dotnet run --project RepoLens.Api");
                    return;
                }
                Console.WriteLine("✅ API is running!");
                Console.WriteLine();
                
                // Demo all search endpoints
                await DemoBasicSearch();
                await DemoNaturalLanguageQuery();
                await DemoSearchSuggestions();
                await DemoSearchFilters();
                await DemoIntentAnalysis();
                
                Console.WriteLine("🎉 SEARCH API DEMO COMPLETED SUCCESSFULLY!");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Demo failed: {ex.Message}");
                Console.WriteLine("💡 Make sure the RepoLens API is running on http://localhost:3000");
            }
        }
        
        private static async Task DemoBasicSearch()
        {
            Console.WriteLine("🔍 BASIC SEARCH API TEST");
            Console.WriteLine(new string('─', 50));
            
            var queries = new[] { "controller", "search", "async", "interface" };
            
            foreach (var query in queries)
            {
                var endpoint = $"{_baseUrl}/api/search?q={Uri.EscapeDataString(query)}&page=1&pageSize=5";
                
                Console.WriteLine($"📤 GET {endpoint}");
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"📥 Status: {response.StatusCode}");
                Console.WriteLine($"📋 Response:");
                try
                {
                    var json = JsonDocument.Parse(content);
                    var prettyJson = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(prettyJson);
                }
                catch
                {
                    Console.WriteLine(content);
                }
                Console.WriteLine();
            }
        }
        
        private static async Task DemoNaturalLanguageQuery()
        {
            Console.WriteLine("🤖 NATURAL LANGUAGE QUERY API TEST");
            Console.WriteLine(new string('─', 50));
            
            var queries = new[]
            {
                "find all controllers",
                "search for async methods",
                "show me TypeScript components"
            };
            
            foreach (var query in queries)
            {
                var request = new
                {
                    Query = query,
                    RepositoryId = 1,
                    MaxResults = 5
                };
                
                var endpoint = $"{_baseUrl}/api/search/query";
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"📤 POST {endpoint}");
                Console.WriteLine($"📋 Request Body: {json}");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"📥 Status: {response.StatusCode}");
                Console.WriteLine($"📋 Response:");
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var prettyJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(prettyJson);
                }
                catch
                {
                    Console.WriteLine(responseContent);
                }
                Console.WriteLine();
            }
        }
        
        private static async Task DemoSearchSuggestions()
        {
            Console.WriteLine("💡 SEARCH SUGGESTIONS API TEST");
            Console.WriteLine(new string('─', 50));
            
            var partialQueries = new[] { "search", "async", "control" };
            
            foreach (var partial in partialQueries)
            {
                var endpoint = $"{_baseUrl}/api/search/suggestions?q={Uri.EscapeDataString(partial)}&limit=5&repositoryId=1";
                
                Console.WriteLine($"📤 GET {endpoint}");
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"📥 Status: {response.StatusCode}");
                Console.WriteLine($"📋 Response:");
                try
                {
                    var json = JsonDocument.Parse(content);
                    var prettyJson = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(prettyJson);
                }
                catch
                {
                    Console.WriteLine(content);
                }
                Console.WriteLine();
            }
        }
        
        private static async Task DemoSearchFilters()
        {
            Console.WriteLine("🔧 SEARCH FILTERS API TEST");
            Console.WriteLine(new string('─', 50));
            
            var endpoint = $"{_baseUrl}/api/search/filters/1";
            
            Console.WriteLine($"📤 GET {endpoint}");
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Status: {response.StatusCode}");
            Console.WriteLine($"📋 Response:");
            try
            {
                var json = JsonDocument.Parse(content);
                var prettyJson = JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(prettyJson);
            }
            catch
            {
                Console.WriteLine(content);
            }
            Console.WriteLine();
        }
        
        private static async Task DemoIntentAnalysis()
        {
            Console.WriteLine("🧠 INTENT ANALYSIS API TEST");
            Console.WriteLine(new string('─', 50));
            
            var queries = new[]
            {
                "find authentication methods",
                "count public classes",
                "analyze error handling"
            };
            
            foreach (var query in queries)
            {
                var request = new { Query = query };
                var endpoint = $"{_baseUrl}/api/search/intent";
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"📤 POST {endpoint}");
                Console.WriteLine($"📋 Request Body: {json}");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"📥 Status: {response.StatusCode}");
                Console.WriteLine($"📋 Response:");
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var prettyJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(prettyJson);
                }
                catch
                {
                    Console.WriteLine(responseContent);
                }
                Console.WriteLine();
            }
        }
    }
}
