namespace RepoLens.Core.Entities;

/// <summary>
/// Contains all the data a provider needs to work with a specific repository
/// </summary>
/// <param name="RepositoryId">The database ID of the repository</param>
/// <param name="Url">The repository URL or path</param>
/// <param name="ProviderType">The type of Git provider</param>
/// <param name="AuthToken">Authentication token for remote repositories (null for public repos or local paths)</param>
/// <param name="LocalClonePath">Local path where the repository is cloned (set after cloning)</param>
/// <param name="Owner">Repository owner/organization name (null for local repos)</param>
/// <param name="RepoName">Repository name (null for local repos - use path instead)</param>
public record RepositoryContext(
    int RepositoryId,
    string Url,
    ProviderType ProviderType,
    string? AuthToken,
    string? LocalClonePath,
    string? Owner,
    string? RepoName
);
