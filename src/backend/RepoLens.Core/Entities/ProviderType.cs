namespace RepoLens.Core.Entities;

/// <summary>
/// Represents the type of Git repository provider
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// Unknown or unsupported provider
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// GitHub (github.com)
    /// </summary>
    GitHub = 1,
    
    /// <summary>
    /// GitLab (gitlab.com or self-hosted)
    /// </summary>
    GitLab = 2,
    
    /// <summary>
    /// Bitbucket (bitbucket.org)
    /// </summary>
    Bitbucket = 3,
    
    /// <summary>
    /// Azure DevOps (dev.azure.com or TFS)
    /// </summary>
    AzureDevOps = 4,
    
    /// <summary>
    /// Local file system repository
    /// </summary>
    Local = 5
}
