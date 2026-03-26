using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Providers;

/// <summary>
/// Azure DevOps provider service for collecting metrics from Azure DevOps repositories
/// This is a stub implementation for future development
/// </summary>
public class AzureDevOpsProviderService : IGitProviderService
{
    private readonly ILogger<AzureDevOpsProviderService> _logger;

    public AzureDevOpsProviderService(ILogger<AzureDevOpsProviderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ProviderType ProviderType => ProviderType.AzureDevOps;

    public bool CanHandle(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return false;

        return repositoryUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase) ||
               repositoryUrl.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase) ||
               repositoryUrl.Contains("azure.com", StringComparison.OrdinalIgnoreCase);
    }

    public Task<ProviderValidationResult> ValidateAccessAsync(string repositoryUrl, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repositoryUrl);

        if (!CanHandle(repositoryUrl))
        {
            return Task.FromResult(ProviderValidationResult.Failure(
                "Repository URL is not a valid Azure DevOps repository URL"));
        }

        // For now, return a stub implementation
        _logger.LogWarning("Azure DevOps provider is not yet implemented. Repository validation skipped for: {Url}", repositoryUrl);
        
        return Task.FromResult(ProviderValidationResult.Failure(
            "Azure DevOps provider is not yet implemented",
            "This is a stub implementation. Azure DevOps support will be added in a future release."));
    }

    public Task<RepositoryMetrics> CollectMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.AzureDevOps)
            throw new InvalidOperationException($"AzureDevOpsProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Azure DevOps metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Azure DevOps provider is not yet implemented. " +
            "This is a stub implementation. Azure DevOps support will be added in a future release.");
    }

    public Task<IReadOnlyList<ContributorMetrics>> CollectContributorMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.AzureDevOps)
            throw new InvalidOperationException($"AzureDevOpsProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Azure DevOps contributor metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Azure DevOps provider is not yet implemented. " +
            "This is a stub implementation. Azure DevOps support will be added in a future release.");
    }

    public Task<IReadOnlyList<FileMetrics>> CollectFileMetricsAsync(RepositoryContext context, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ProviderType != ProviderType.AzureDevOps)
            throw new InvalidOperationException($"AzureDevOpsProviderService cannot handle {context.ProviderType} repositories");

        _logger.LogWarning("Azure DevOps file metrics collection is not yet implemented for repository: {Url}", context.Url);
        
        throw new NotImplementedException(
            "Azure DevOps provider is not yet implemented. " +
            "This is a stub implementation. Azure DevOps support will be added in a future release.");
    }
}
