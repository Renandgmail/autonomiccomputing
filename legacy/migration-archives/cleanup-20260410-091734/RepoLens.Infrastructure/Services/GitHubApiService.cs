using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;

namespace RepoLens.Infrastructure.Services;

public interface IGitHubApiService
{
    Task<GitHubRepository> GetRepositoryAsync(string owner, string repo);
    Task<List<GitHubCommit>> GetCommitsAsync(string owner, string repo, int page = 1, int perPage = 100);
    Task<List<GitHubContributor>> GetContributorsAsync(string owner, string repo);
    Task<List<GitHubLanguage>> GetLanguagesAsync(string owner, string repo);
    Task<List<GitHubFile>> GetRepositoryContentsAsync(string owner, string repo, string path = "");
    Task<GitHubRateLimit> GetRateLimitAsync();
}

public class GitHubApiService : IGitHubApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubApiService> _logger;
    private readonly string? _accessToken;
    private readonly JsonSerializerOptions _jsonOptions;

    public GitHubApiService(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _accessToken = configuration["GitHub:AccessToken"];
        
        // Configure HTTP client
        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RepoLens", "1.0"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        
        if (!string.IsNullOrEmpty(_accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            _logger.LogInformation("GitHub API configured with authentication token");
        }
        else
        {
            _logger.LogWarning("GitHub API configured without authentication - rate limits will apply");
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<GitHubRepository> GetRepositoryAsync(string owner, string repo)
    {
        _logger.LogInformation("Fetching repository information for {Owner}/{Repo}", owner, repo);
        
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var repository = JsonSerializer.Deserialize<GitHubRepository>(content, _jsonOptions)!;
        
        _logger.LogInformation("Retrieved repository: {Name}, Stars: {Stars}, Forks: {Forks}, Size: {Size}KB", 
            repository.Name, repository.StargazersCount, repository.ForksCount, repository.Size);
            
        return repository;
    }

    public async Task<List<GitHubCommit>> GetCommitsAsync(string owner, string repo, int page = 1, int perPage = 100)
    {
        _logger.LogInformation("Fetching commits for {Owner}/{Repo}, page {Page}", owner, repo, page);
        
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/commits?page={page}&per_page={perPage}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var commits = JsonSerializer.Deserialize<List<GitHubCommit>>(content, _jsonOptions)!;
        
        _logger.LogInformation("Retrieved {Count} commits from page {Page}", commits.Count, page);
        return commits;
    }

    public async Task<List<GitHubContributor>> GetContributorsAsync(string owner, string repo)
    {
        _logger.LogInformation("Fetching contributors for {Owner}/{Repo}", owner, repo);
        
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/contributors");
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var contributors = JsonSerializer.Deserialize<List<GitHubContributor>>(content, _jsonOptions)!;
        
        _logger.LogInformation("Retrieved {Count} contributors", contributors.Count);
        return contributors;
    }

    public async Task<List<GitHubLanguage>> GetLanguagesAsync(string owner, string repo)
    {
        _logger.LogInformation("Fetching languages for {Owner}/{Repo}", owner, repo);
        
        var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/languages");
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var languagesDict = JsonSerializer.Deserialize<Dictionary<string, int>>(content, _jsonOptions)!;
        
        var languages = languagesDict.Select(kvp => new GitHubLanguage
        {
            Name = kvp.Key,
            Bytes = kvp.Value
        }).ToList();
        
        _logger.LogInformation("Retrieved {Count} languages", languages.Count);
        return languages;
    }

    public async Task<List<GitHubFile>> GetRepositoryContentsAsync(string owner, string repo, string path = "")
    {
        _logger.LogInformation("Fetching repository contents for {Owner}/{Repo} at path '{Path}'", owner, repo, path);
        
        var url = string.IsNullOrEmpty(path) 
            ? $"repos/{owner}/{repo}/contents"
            : $"repos/{owner}/{repo}/contents/{path}";
            
        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var files = JsonSerializer.Deserialize<List<GitHubFile>>(content, _jsonOptions)!;
        
        _logger.LogInformation("Retrieved {Count} files/directories from path '{Path}'", files.Count, path);
        return files;
    }

    public async Task<GitHubRateLimit> GetRateLimitAsync()
    {
        var response = await _httpClient.GetAsync("rate_limit");
        await EnsureSuccessStatusCodeAsync(response);
        
        var content = await response.Content.ReadAsStringAsync();
        var rateLimit = JsonSerializer.Deserialize<GitHubRateLimitResponse>(content, _jsonOptions)!;
        
        return rateLimit.Rate;
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            
            _logger.LogError("GitHub API request failed with status {StatusCode}: {Error}", 
                statusCode, errorContent);
                
            if (statusCode == 403)
            {
                var rateLimitRemaining = response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining) 
                    ? remaining.FirstOrDefault() : "unknown";
                var rateLimitReset = response.Headers.TryGetValues("X-RateLimit-Reset", out var reset) 
                    ? reset.FirstOrDefault() : "unknown";
                    
                throw new HttpRequestException($"GitHub API rate limit exceeded. Remaining: {rateLimitRemaining}, Reset at: {rateLimitReset}");
            }
            
            response.EnsureSuccessStatusCode();
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// GitHub API Response Models
public class GitHubRepository
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Private { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = string.Empty;
    public int Size { get; set; }
    public int StargazersCount { get; set; }
    public int WatchersCount { get; set; }
    public int ForksCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime PushedAt { get; set; }
    public GitHubUser Owner { get; set; } = new();
}

public class GitHubCommit
{
    public string Sha { get; set; } = string.Empty;
    public GitHubCommitDetails Commit { get; set; } = new();
    public GitHubUser Author { get; set; } = new();
    public GitHubUser Committer { get; set; } = new();
}

public class GitHubCommitDetails
{
    public GitHubAuthor Author { get; set; } = new();
    public GitHubAuthor Committer { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class GitHubAuthor
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class GitHubUser
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class GitHubContributor : GitHubUser
{
    public int Contributions { get; set; }
}

public class GitHubLanguage
{
    public string Name { get; set; } = string.Empty;
    public int Bytes { get; set; }
}

public class GitHubFile
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "file" or "dir"
    public int Size { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
}

public class GitHubRateLimit
{
    public int Limit { get; set; }
    public int Used { get; set; }
    public int Remaining { get; set; }
    public int Reset { get; set; }
}

public class GitHubRateLimitResponse
{
    public GitHubRateLimit Rate { get; set; } = new();
}
