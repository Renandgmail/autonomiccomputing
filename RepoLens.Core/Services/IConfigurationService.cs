namespace RepoLens.Core.Services;

public interface IConfigurationService
{
    string GetConnectionString(string name);
    string GetDefaultConnectionString();
    string GetTestConnectionString();
    T GetValue<T>(string key, T defaultValue = default!);
    bool IsProduction { get; }
    bool IsDevelopment { get; }
    bool IsTesting { get; }
}
