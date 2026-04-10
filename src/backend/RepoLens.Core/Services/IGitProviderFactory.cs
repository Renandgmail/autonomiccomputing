using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Factory interface for creating Git provider service instances based on repository URLs
/// </summary>
public interface IGitProviderFactory
{
    /// <summary>
    /// Gets the appropriate Git provider service for the given repository URL
    /// </summary>
    /// <param name="repositoryUrl">The repository URL to analyze</param>
    /// <returns>A Git provider service that can handle the URL</returns>
    /// <exception cref="ArgumentNullException">Thrown when repositoryUrl is null or empty</exception>
    /// <exception cref="NotSupportedException">Thrown when no provider can handle the URL</exception>
    IGitProviderService GetProvider(string repositoryUrl);

    /// <summary>
    /// Gets the Git provider service for the specified provider type
    /// </summary>
    /// <param name="providerType">The provider type to get a service for</param>
    /// <returns>A Git provider service for the specified type</returns>
    /// <exception cref="NotSupportedException">Thrown when the provider type is not supported</exception>
    IGitProviderService GetProvider(ProviderType providerType);

    /// <summary>
    /// Gets a collection of all supported provider types
    /// </summary>
    IEnumerable<ProviderType> SupportedProviders { get; }
}
