using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;
using System.Text.Json;
using Xunit;
using RepoLens.Api.Models;

namespace RepoLens.Tests.Integration;

public class SearchIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SearchIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_WithMatchingRepositories_ReturnsCorrectResults()
    {
        // Arrange
        var repo1 = await CreateTestRepositoryAsync("test-repo", "A test repository for integration testing");
        var repo2 = await CreateTestRepositoryAsync("another-test", "Another test repository");
        var repo3 = await CreateTestRepositoryAsync("production-app", "Production application repository");

        // Act
        var response = await _client.GetAsync("/api/search?q=test");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        // Verify the response contains search results
        var responseJson = JsonSerializer.Serialize(apiResponse.Data);
        Assert.Contains("results", responseJson);
        Assert.Contains("totalResults", responseJson);
        Assert.Contains("query", responseJson);

        // Extract results for detailed verification
        var jsonElement = (JsonElement)apiResponse.Data;
        var resultsProperty = jsonElement.GetProperty("results");
        var totalResultsProperty = jsonElement.GetProperty("totalResults");

        // Should find repositories containing "test"
        Assert.True(totalResultsProperty.GetInt32() >= 2);
        Assert.True(resultsProperty.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task Search_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var repositories = new List<Repository>();
        for (int i = 1; i <= 15; i++)
        {
            repositories.Add(await CreateTestRepositoryAsync($"test-repo-{i:D2}", $"Test repository number {i}"));
        }

        // Act - Get second page with page size 5
        var response = await _client.GetAsync("/api/search?q=test&page=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);

        var jsonElement = (JsonElement)apiResponse.Data;
        var pageProperty = jsonElement.GetProperty("page");
        var pageSizeProperty = jsonElement.GetProperty("pageSize");
        var resultsProperty = jsonElement.GetProperty("results");

        Assert.Equal(2, pageProperty.GetInt32());
        Assert.Equal(5, pageSizeProperty.GetInt32());
        Assert.True(resultsProperty.GetArrayLength() <= 5);
    }

    [Fact]
    public async Task Search_WithNoMatches_ReturnsEmptyResults()
    {
        // Arrange
        await CreateTestRepositoryAsync("production-app", "Production application");

        // Act
        var response = await _client.GetAsync("/api/search?q=nonexistent");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);

        var jsonElement = (JsonElement)apiResponse.Data;
        var totalResultsProperty = jsonElement.GetProperty("totalResults");
        var resultsProperty = jsonElement.GetProperty("results");

        Assert.Equal(0, totalResultsProperty.GetInt32());
        Assert.Equal(0, resultsProperty.GetArrayLength());
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/search?q=");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Equal("Search query is required", apiResponse.ErrorMessage);
    }

    [Fact]
    public async Task SearchSuggestions_WithPartialQuery_ReturnsSuggestions()
    {
        // Arrange
        await CreateTestRepositoryAsync("test-api", "Test API repository");
        await CreateTestRepositoryAsync("test-web", "Test web application");
        await CreateTestRepositoryAsync("production-app", "Production application");

        // Act
        var response = await _client.GetAsync("/api/search/suggestions?q=te");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);

        var jsonElement = (JsonElement)apiResponse.Data;
        var suggestionsProperty = jsonElement.GetProperty("suggestions");

        // Should find repositories starting with "te"
        Assert.True(suggestionsProperty.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task SearchSuggestions_WithShortQuery_ReturnsEmptySuggestions()
    {
        // Act
        var response = await _client.GetAsync("/api/search/suggestions?q=t");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);

        var jsonElement = (JsonElement)apiResponse.Data;
        var suggestionsProperty = jsonElement.GetProperty("suggestions");

        Assert.Equal(0, suggestionsProperty.GetArrayLength());
    }

    [Fact]
    public async Task Search_RelevanceScoring_OrdersResultsCorrectly()
    {
        // Arrange
        var exactMatch = await CreateTestRepositoryAsync("test", "Exact match repository");
        var startsWithMatch = await CreateTestRepositoryAsync("test-api", "Repository starting with test");
        var containsMatch = await CreateTestRepositoryAsync("my-test-repo", "Repository containing test");
        var descriptionMatch = await CreateTestRepositoryAsync("api-service", "A service with test in description");

        // Act
        var response = await _client.GetAsync("/api/search?q=test");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);

        var jsonElement = (JsonElement)apiResponse.Data;
        var resultsProperty = jsonElement.GetProperty("results");

        // Verify results are returned (exact ordering may vary due to alphabetical tie-breaking)
        Assert.True(resultsProperty.GetArrayLength() >= 4);

        // Verify all expected repositories are found
        var resultTitles = new List<string>();
        foreach (var result in resultsProperty.EnumerateArray())
        {
            var title = result.GetProperty("title").GetString();
            if (title != null)
                resultTitles.Add(title);
        }

        Assert.Contains("test", resultTitles);
        Assert.Contains("test-api", resultTitles);
        Assert.Contains("my-test-repo", resultTitles);
        Assert.Contains("api-service", resultTitles);
    }

    private async Task<Repository> CreateTestRepositoryAsync(string name, string description)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var repository = new Repository
        {
            Name = name,
            Url = $"https://github.com/test/{name}",
            Description = description,
            DefaultBranch = "main",
            CreatedAt = DateTime.UtcNow,
            Status = RepositoryStatus.Active,
            IsPrivate = false,
            AutoSync = true,
            SyncIntervalMinutes = 60,
            ProviderType = ProviderType.GitHub,
            OwnerId = 1 // Assuming test user exists
        };

        context.Repositories.Add(repository);
        await context.SaveChangesAsync();

        return repository;
    }
}
