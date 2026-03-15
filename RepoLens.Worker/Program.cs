using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Infrastructure;
using RepoLens.Infrastructure.Repositories;
using RepoLens.Infrastructure.Git;
using RepoLens.Infrastructure.Storage;
using RepoLens.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Wrap this call to fix warning CA1416
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog();
}

builder.Logging.AddEventSourceLogger();
builder.Logging.AddEventLog();
builder.Logging.AddEventSourceLogger();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Configure services
builder.Services.AddDbContext<RepoLensDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IRepositoryRepository, RepositoryRepository>();
builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();
builder.Services.AddSingleton<GitService>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddScoped<IngestionService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RepoLens Worker started");

        using var scope = _serviceProvider.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<IngestionService>();

        try
        {
            // Example repository to ingest
            var repositoryUrl = "https://github.com/example/repo.git";
            var localPath = Path.Combine(Path.GetTempPath(), "repolens", "example-repo");

            await ingestionService.IngestRepositoryAsync(repositoryUrl, localPath, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during repository ingestion");
        }

        _logger.LogInformation("RepoLens Worker stopped");
    }
}
