using RepoLens.Core.Entities;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Providers;

/// <summary>
/// Factory implementation for creating Git provider service instances
/// </summary>
public class GitProviderFactory : IGitProviderFactory
{
    private readonly IEnumerable<IGitProviderService> _providers;

    /// <summary>
    /// Initializes a new instance of the GitProviderFactory
    /// </summary>
    /// <param name="providers">Collection of all available Git provider services</param>
    public GitProviderFactory(IEnumerable<IGitProviderService> providers)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    /// <inheritdoc />
    public IGitProviderService GetProvider(string repositoryUrl)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new ArgumentNullException(nameof(repositoryUrl), "Repository URL cannot be null or empty");
        }

        // Find the first provider that can handle this URL
        var provider = _providers.FirstOrDefault(p => p.CanHandle(repositoryUrl));

        if (provider == null)
        {
            var supportedTypes = string.Join(", ", SupportedProviders);
            throw new NotSupportedException(
                $"No provider found that can handle the URL '{repositoryUrl}'. " +
                $"Supported provider types: {supportedTypes}");
        }

        return provider;
    }

    /// <inheritdoc />
    public IGitProviderService GetProvider(ProviderType providerType)
    {
        if (providerType == ProviderType.Unknown)
        {
            throw new NotSupportedException("Cannot create provider for Unknown provider type");
        }

        var provider = _providers.FirstOrDefault(p => p.ProviderType == providerType);

        if (provider == null)
        {
            var supportedTypes = string.Join(", ", SupportedProviders);
            throw new NotSupportedException(
                $"No provider found for type '{providerType}'. " +
                $"Supported provider types: {supportedTypes}");
        }

        return provider;
    }

    /// <inheritdoc />
    public IEnumerable<ProviderType> SupportedProviders => 
        _providers.Select(p => p.ProviderType).Distinct().Where(t => t != ProviderType.Unknown);
}
