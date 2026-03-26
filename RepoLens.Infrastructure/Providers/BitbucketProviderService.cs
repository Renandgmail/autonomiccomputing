using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Providers;

/// <summary>
/// Bitbucket provider service for collecting metrics from Bitbucket repositories
/// This is a stub implementation for future development
/// </summary>
public class BitbucketProviderService : IGitProviderService
{
    private readonly ILogger<BitbucketProviderService> _logger;

    public BitbucketProviderService(ILogger<BitbucketProviderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ProviderType ProviderType => ProviderType.Bitbucket;

    public bool CanHandle(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return false;

        return repositoryUrl.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase) ||
               repositoryUrl.Contains("bitbucket.com", StringComparison.OrdinalIgnoreCase);
    }

    public Task<ProviderValidationResult> ValidateAccessAsync(string repositoryUrl, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repositoryUrl);

        if (!CanHandle(repositoryUrl))
        {
            return Task.FromResult(ProviderValidationResult.Failure(
                "Repository URL is not a valid Bitbucket repository URL"));
        }

        // For now, return a stub implementation
        _logger.LogWarning("Bitbucket provider is not yet implemented. Repository validation skipped for: {Url}", repositoryUrl);
        
        return Task.FromResult(ProviderValidationResult.Failure(
            "Bitbucket provider is not yet implemented",
            "This is a stub implementation. Bitbucket support will be added in a future release."));
    }

    public Task<RepositoryMetrics> CollectMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Bitbucket)
            throw new InvalidOperationException($"BitbucketProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Bitbucket metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Bitbucket provider is not yet implemented. " +
            "This is a stub implementation. Bitbucket support will be added in a future release.");
    }

    public Task<IReadOnlyList<ContributorMetrics>> CollectContributorMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Bitbucket)
            throw new InvalidOperationException($"BitbucketProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Bitbucket contributor metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Bitbucket provider is not yet implemented. " +
            "This is a stub implementation. Bitbucket support will be added in a future release.");
    }

    public Task<IReadOnlyList<FileMetrics>> CollectFileMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.Bitbucket)
            throw new InvalidOperationException($"BitbucketProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Bitbucket file metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Bitbucket provider is not yet implemented. " +
            "This is a stub implementation. Bitbucket support will be added in a future release.");
    }
}
