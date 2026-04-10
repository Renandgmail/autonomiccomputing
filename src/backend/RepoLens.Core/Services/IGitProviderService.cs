using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Defines the contract for Git repository provider services that can collect metrics
/// from different Git hosting platforms (GitHub, GitLab, etc.) or local repositories
/// </summary>
public interface IGitProviderService
{
    /// <summary>
    /// Gets the provider type this service handles
    /// </summary>
    ProviderType ProviderType { get; }

    /// <summary>
    /// Determines if this service can handle the given repository URL
    /// </summary>
    /// <param name="repositoryUrl">The repository URL to check</param>
    /// <returns>True if this service can handle the URL, false otherwise</returns>
    bool CanHandle(string repositoryUrl);

    /// <summary>
    /// Collects comprehensive repository metrics for the given repository
    /// </summary>
    /// <param name="context">The repository context containing all necessary information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing repository metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provider cannot handle this repository type</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails</exception>
    Task<RepositoryMetrics> CollectMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Collects per-contributor metrics for the given repository
    /// </summary>
    /// <param name="context">The repository context containing all necessary information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing contributor metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provider cannot handle this repository type</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails</exception>
    Task<IReadOnlyList<ContributorMetrics>> CollectContributorMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Collects per-file metrics for the given repository
    /// </summary>
    /// <param name="context">The repository context containing all necessary information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing file metrics</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provider cannot handle this repository type</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails</exception>
    Task<IReadOnlyList<FileMetrics>> CollectFileMetricsAsync(
        RepositoryContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Validates that the repository URL is accessible and the service can connect to it
    /// </summary>
    /// <param name="repositoryUrl">The repository URL to validate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation containing validation results</returns>
    /// <exception cref="ArgumentNullException">Thrown when repositoryUrl is null</exception>
    Task<ProviderValidationResult> ValidateAccessAsync(
        string repositoryUrl,
        CancellationToken ct = default);
}
