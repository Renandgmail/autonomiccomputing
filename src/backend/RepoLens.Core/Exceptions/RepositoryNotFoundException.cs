namespace RepoLens.Core.Exceptions;

/// <summary>
/// Exception thrown when a repository cannot be found at the specified location
/// </summary>
public class RepositoryNotFoundException : RepoLensException
{
    /// <summary>
    /// Initializes a new instance of the RepositoryNotFoundException class
    /// </summary>
    /// <param name="message">The error message</param>
    public RepositoryNotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RepositoryNotFoundException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public RepositoryNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
