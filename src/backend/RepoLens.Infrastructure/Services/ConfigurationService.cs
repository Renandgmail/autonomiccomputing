using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public ConfigurationService(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public bool IsProduction => _environment.IsProduction();
    public bool IsDevelopment => _environment.IsDevelopment();
    public bool IsTesting => _environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);

    public string GetConnectionString(string name)
    {
        return _configuration.GetConnectionString(name) ?? GetDefaultConnectionString();
    }

    public string GetDefaultConnectionString()
    {
        // Priority order: DefaultConnection -> Production -> Development -> Fallback
        return _configuration.GetConnectionString("DefaultConnection") 
            ?? _configuration.GetConnectionString("Production")
            ?? _configuration.GetConnectionString("Development")
            ?? GetFallbackConnectionString();
    }

    public string GetTestConnectionString()
    {
        return _configuration.GetConnectionString("Testing") 
            ?? _configuration.GetConnectionString("Test")
            ?? GetFallbackTestConnectionString();
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        return _configuration.GetValue<T>(key) ?? defaultValue;
    }

    private string GetFallbackConnectionString()
    {
        // Get values from configuration or use defaults
        var host = GetValue("Database:Host", "localhost");
        var port = GetValue("Database:Port", "5432");
        var database = GetValue("Database:Name", "repolens_db");
        var username = GetValue("Database:Username", "postgres");
        var password = GetValue("Database:Password", "postgres");

        return $"Host={host};Database={database};Username={username};Password={password};Port={port}";
    }

    private string GetFallbackTestConnectionString()
    {
        // Get values from configuration or use defaults for testing
        var host = GetValue("Database:Host", "localhost");
        var port = GetValue("Database:Port", "5432");
        var database = GetValue("Database:TestName", "repolens_test");
        var username = GetValue("Database:Username", "postgres");
        var password = GetValue("Database:Password", "postgres");

        return $"Host={host};Database={database};Username={username};Password={password};Port={port}";
    }
}
