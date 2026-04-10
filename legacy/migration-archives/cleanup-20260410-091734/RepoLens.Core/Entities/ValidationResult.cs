namespace RepoLens.Core.Entities;

/// <summary>
/// Represents the result of a Git provider validation operation
/// </summary>
public class ProviderValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation was successful
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets additional details about the validation result
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <param name="details">Optional details about the validation</param>
    /// <returns>A successful validation result</returns>
    public static ProviderValidationResult Success(string? details = null)
    {
        return new ProviderValidationResult
        {
            IsValid = true,
            Details = details
        };
    }
    
    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="details">Optional additional details</param>
    /// <returns>A failed validation result</returns>
    public static ProviderValidationResult Failure(string errorMessage, string? details = null)
    {
        return new ProviderValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            Details = details
        };
    }
}
