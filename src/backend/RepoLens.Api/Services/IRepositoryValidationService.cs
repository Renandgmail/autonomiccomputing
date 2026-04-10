using RepoLens.Api.Models;
using RepoLens.Core.Entities;

namespace RepoLens.Api.Services;

public interface IRepositoryValidationService
{
    ValidationResult ValidateUrlFormat(string url);
    Task<bool> ValidateRepositoryAccessAsync(string url);
    string ExtractRepositoryNameFromUrl(string url);
    ProviderType DetectProviderType(string url);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public bool IsGitRepository { get; set; }
    public string? ErrorMessage { get; set; }

    public static ValidationResult Success() => new() { IsValid = true, IsGitRepository = true };
    public static ValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
}
