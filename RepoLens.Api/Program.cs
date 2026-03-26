using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;
using RepoLens.Infrastructure;
using RepoLens.Infrastructure.Repositories;
using RepoLens.Infrastructure.Services;
using RepoLens.Infrastructure.Git;
using RepoLens.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container - API only
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add API documentation
// builder.Services.AddEndpointsApiExplorer(); // Temporarily disabled
// builder.Services.AddSwaggerGen(); // Temporarily disabled due to compatibility issue

// Configure Entity Framework with PostgreSQL for production
builder.Services.AddDbContext<RepoLensDbContext>((serviceProvider, options) =>
{
    var configService = serviceProvider.GetService<IConfigurationService>();
    var connectionString = configService?.GetDefaultConnectionString() 
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=repolens_db;Username=postgres;Password=TCEP;Port=5432";
    options.UseNpgsql(connectionString);
});

// Configure Identity for API
builder.Services.AddIdentity<User, Role>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    
    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<RepoLensDbContext>()
.AddDefaultTokenProviders();

// Configure JWT authentication for API
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "RepoLens.Api",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "RepoLens.Web",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["JwtSettings:SecretKey"] ?? "DefaultSecretKey123!"))
    };
});

builder.Services.AddAuthorization();

// Register repository services
builder.Services.AddScoped<IRepositoryRepository, RepositoryRepository>();
builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();
builder.Services.AddScoped<IRepositoryMetricsRepository, RepositoryMetricsRepository>();

// Register configuration service
builder.Services.AddScoped<IConfigurationService, RepoLens.Infrastructure.Services.ConfigurationService>();

// Register infrastructure services
builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddScoped<IMetricsCollectionService, MetricsCollectionService>();

// Register HttpClient for API calls
builder.Services.AddHttpClient();

// Register Git provider services
builder.Services.AddScoped<IGitHubApiService, GitHubApiService>();

// Register all Git provider implementations
builder.Services.AddScoped<IGitProviderService, RepoLens.Infrastructure.Providers.GitHubProviderService>();
builder.Services.AddScoped<IGitProviderService, RepoLens.Infrastructure.Providers.LocalProviderService>();
builder.Services.AddScoped<IGitProviderService, RepoLens.Infrastructure.Providers.GitLabProviderService>();
builder.Services.AddScoped<IGitProviderService, RepoLens.Infrastructure.Providers.BitbucketProviderService>();
builder.Services.AddScoped<IGitProviderService, RepoLens.Infrastructure.Providers.AzureDevOpsProviderService>();

// Register Git provider factory
builder.Services.AddScoped<IGitProviderFactory, RepoLens.Infrastructure.Providers.GitProviderFactory>();

// Register the REAL metrics collection services for actual GitHub integration (legacy - will be replaced by providers)
builder.Services.AddScoped<IRealMetricsCollectionService, RealMetricsCollectionService>();

// Register additional repositories needed for real metrics
builder.Services.AddScoped<IContributorMetricsRepository, ContributorMetricsRepository>();
builder.Services.AddScoped<IFileMetricsRepository, FileMetricsRepository>();
builder.Services.AddScoped<ICommitRepository, CommitRepository>();

// Configure local storage service
builder.Services.AddSingleton<StorageService>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILogger<StorageService>>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    
    var storagePath = configuration.GetValue<string>("Storage:LocalPath") 
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RepoLens", "storage");
    
    return new StorageService(storagePath, logger);
});

// Register validation services
builder.Services.AddScoped<RepoLens.Api.Services.IRepositoryValidationService, RepoLens.Api.Services.RepositoryValidationService>();

// Register SignalR notification service
builder.Services.AddScoped<RepoLens.Api.Hubs.IMetricsNotificationService, RepoLens.Api.Hubs.MetricsNotificationService>();


// Add CORS support for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger(); // Temporarily disabled
    // app.UseSwaggerUI(); // Temporarily disabled
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("ReactApp");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RepoLens.Api.Hubs.MetricsHub>("/hubs/metrics");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // Apply any pending migrations
        await context.Database.MigrateAsync();
        
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
        // Continue startup even if database initialization fails
    }
}

app.Run();
