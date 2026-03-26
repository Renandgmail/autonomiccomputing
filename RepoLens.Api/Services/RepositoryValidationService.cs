using RepoLens.Api.Models;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using System.Text.RegularExpressions;

namespace RepoLens.Api.Services;

public class RepositoryValidationService : IRepositoryValidationService
{
    private readonly IGitService _gitService;
    private readonly ILogger<RepositoryValidationService> _logger;

    // Provider detection patterns based on architecture specification
    private readonly Dictionary<ProviderType, string[]> _providerPatterns = new()
    {
        [ProviderType.GitHub] = new[]
        {
            @"^https?://github\.com/[\w\.\-]+/[\w\.\-]+(\.git)?/?$", // GitHub HTTPS
            @"^git@github\.com:[\w\.\-]+/[\w\.\-]+(\.git)?/?$",     // GitHub SSH
            @"^github\.com/[\w\.\-]+/[\w\.\-]+(\.git)?/?$"          // GitHub without protocol
        },
        [ProviderType.GitLab] = new[]
        {
            @"^https?://gitlab\.com/[\w\.\-/]+(\.git)?/?$",         // GitLab HTTPS
            @"^git@gitlab\.com:[\w\.\-/]+(\.git)?/?$",              // GitLab SSH
            @"^gitlab\.com/[\w\.\-/]+(\.git)?/?$"                   // GitLab without protocol
        },
        [ProviderType.Bitbucket] = new[]
        {
            @"^https?://bitbucket\.org/[\w\.\-]+/[\w\.\-]+(\.git)?/?$", // Bitbucket HTTPS
            @"^git@bitbucket\.org:[\w\.\-]+/[\w\.\-]+(\.git)?/?$"      // Bitbucket SSH
        },
        [ProviderType.AzureDevOps] = new[]
        {
            @"^https?://dev\.azure\.com/[\w\.\-]+/[\w\.\-]+/_git/[\w\.\-]+/?$", // Azure DevOps new format
            @"^https?://[\w\.\-]+\.visualstudio\.com/[\w\.\-]+/_git/[\w\.\-]+/?$" // Azure DevOps legacy format
        }
    };

    public RepositoryValidationService(IGitService gitService, ILogger<RepositoryValidationService> logger)
    {
        _gitService = gitService;
        _logger = logger;
    }

    public ProviderType DetectProviderType(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ProviderType.Unknown;

        // Check each provider pattern
        foreach (var (providerType, patterns) in _providerPatterns)
        {
            if (patterns.Any(pattern => Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase)))
            {
                return providerType;
            }
        }

        // Check for local paths as fallback (from architecture specification)
        if (IsLocalPath(url))
        {
            return ProviderType.Local;
        }

        return ProviderType.Unknown;
    }

    public ValidationResult ValidateUrlFormat(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ValidationResult.Failure("Repository URL cannot be empty");

        var providerType = DetectProviderType(url);
        
        if (providerType != ProviderType.Unknown)
            return ValidationResult.Success();

        return ValidationResult.Failure("Invalid Git repository URL format. Supported: GitHub, GitLab, Bitbucket, Azure DevOps, Local paths");
    }

    public async Task<bool> ValidateRepositoryAccessAsync(string url)
    {
        _logger.LogInformation("Validating repository access for URL: {Url}", url);

        var formatValidation = ValidateUrlFormat(url);
        if (!formatValidation.IsValid)
        {
            _logger.LogWarning("URL format validation failed: {Error}", formatValidation.ErrorMessage);
            return false;
        }

        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "repolens-validation", Guid.NewGuid().ToString());
            using var repo = await _gitService.OpenOrCloneRepositoryAsync(url, tempPath);
            
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            
            _logger.LogInformation("Repository access validation successful for URL: {Url}", url);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repository access validation failed for URL: {Url}", url);
            return false;
        }
    }

    public string ExtractRepositoryNameFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
                return "Unknown Repository";

            // Handle local paths first (before URI parsing which might throw)
            if (IsLocalPath(url))
            {
                var path = url.StartsWith("file://") ? url[7..] : url;
                var segments = path.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries);
                return segments.LastOrDefault() ?? "Unknown Repository";
            }

            // Handle SSH URLs like git@github.com:user/repo.git
            if (url.StartsWith("git@"))
            {
                var sshPattern = @"git@[^:]+:(.+)";
                var match = Regex.Match(url, sshPattern);
                if (match.Success)
                {
                    var path = match.Groups[1].Value;
                    var sshSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    var sshLastSegment = sshSegments.LastOrDefault() ?? "Unknown Repository";
                    return sshLastSegment.EndsWith(".git") ? sshLastSegment[..^4] : sshLastSegment;
                }
            }

            // Handle HTTPS URLs
            var uri = new Uri(url);
            var httpsSegments = uri.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var httpsLastSegment = httpsSegments.LastOrDefault() ?? "Unknown Repository";
            
            return httpsLastSegment.EndsWith(".git") 
                ? httpsLastSegment[..^4] 
                : httpsLastSegment;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract repository name from URL: {Url}", url);
            return "Unknown Repository";
        }
    }

    /// <summary>
    /// Determines if the given URL represents a local file path
    /// Based on architecture specification patterns
    /// </summary>
    private bool IsLocalPath(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // file:// protocol
        if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            return true;

        // Absolute path starting with / (Unix/Linux/macOS)
        if (url.StartsWith("/"))
            return true;

        // Relative path starting with ./ or ../
        if (url.StartsWith("./") || url.StartsWith("../"))
            return true;

        // Windows absolute path (C:\, D:\, etc.)
        if (url.Length >= 3 && char.IsLetter(url[0]) && url[1] == ':' && (url[2] == '\\' || url[2] == '/'))
            return true;

        return false;
    }
}
