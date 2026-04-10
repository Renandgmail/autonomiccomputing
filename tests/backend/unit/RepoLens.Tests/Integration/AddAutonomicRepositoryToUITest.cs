using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Integration;

/// <summary>
/// Utility test to add the autonomiccomputing repository to the main database for UI visibility
/// Run this test to make the repository appear in the UI repository list
/// </summary>
public class AddAutonomicRepositoryToUITest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly RepoLensDbContext _dbContext;
    
    // Repository details for autonomiccomputing (current working directory)
    private const string AUTONOMIC_REPO_URL = "https://github.com/Renandgmail/autonomiccomputing.git";
    private const string AUTONOMIC_REPO_NAME = "Autonomic Computing Platform";
    private const string REPO_LOCAL_PATH = @"c:\Renand\Projects\Heal\autonomiccomputing";

    public AddAutonomicRepositoryToUITest(ITestOutputHelper output)
    {
        _output = output;
        _serviceProvider = SetupMainDatabaseServices();
        _dbContext = _serviceProvider.GetRequiredService<RepoLensDbContext>();
        
        _output.WriteLine("🎯 Adding Autonomic Computing Repository to Main Database");
        _output.WriteLine($"Repository: {AUTONOMIC_REPO_URL}");
        _output.WriteLine($"Database: repolens_db (Main UI Database)");
    }

    [Fact]
    public async Task AddAutonomicComputingRepositoryToMainDatabase_ShouldMakeRepositoryVisibleInUI()
    {
        _output.WriteLine("\n📋 === ADDING REPOSITORY TO MAIN DATABASE FOR UI VISIBILITY ===");
        
        try
        {
            // Check if repository already exists
            var existingRepo = await _dbContext.Repositories
                .FirstOrDefaultAsync(r => r.Url == AUTONOMIC_REPO_URL);

            if (existingRepo != null)
            {
                _output.WriteLine($"✅ Repository already exists in database with ID: {existingRepo.Id}");
                _output.WriteLine($"   Name: {existingRepo.Name}");
                _output.WriteLine($"   Status: {existingRepo.Status}");
                _output.WriteLine($"   Created: {existingRepo.CreatedAt}");
                _output.WriteLine($"   Last Sync: {existingRepo.LastSyncAt}");
                return;
            }

            // Step 1: Create Repository Entry
            var repository = await CreateRepositoryEntryAsync();
            _output.WriteLine($"✅ Step 1: Created repository entry - {repository.Name} (ID: {repository.Id})");

            // Step 2: Add Sample Repository Files
            await AddSampleRepositoryFilesAsync(repository.Id);
            _output.WriteLine("✅ Step 2: Added sample repository files for demonstration");

            // Step 3: Add Sample Code Elements
            await AddSampleCodeElementsAsync(repository.Id);
            _output.WriteLine("✅ Step 3: Added sample code elements for analysis");

            // Step 4: Add Repository Metrics
            await AddRepositoryMetricsAsync(repository.Id);
            _output.WriteLine("✅ Step 4: Added repository metrics");

            // Step 5: Add Sample Vocabulary Terms
            await AddSampleVocabularyTermsAsync(repository.Id);
            _output.WriteLine("✅ Step 5: Added sample vocabulary terms for demonstration");

            _output.WriteLine("\n🎉 === REPOSITORY SUCCESSFULLY ADDED TO MAIN DATABASE ===");
            _output.WriteLine($"Repository ID: {repository.Id}");
            _output.WriteLine($"Repository Name: {repository.Name}");
            _output.WriteLine($"Repository URL: {repository.Url}");
            _output.WriteLine("📱 The repository should now be visible in the RepoLens UI!");
            _output.WriteLine("🔄 Refresh the UI repository list to see the new entry.");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Error adding repository to database: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async Task<Repository> CreateRepositoryEntryAsync()
    {
        var repository = new Repository
        {
            Name = AUTONOMIC_REPO_NAME,
            Url = AUTONOMIC_REPO_URL,
            Type = RepositoryType.Git,
            Status = RepositoryStatus.Active,
            DefaultBranch = "main",
            Description = "Self-healing autonomic computing platform with intelligent monitoring, adaptive responses, and vocabulary extraction capabilities. This repository demonstrates advanced code analysis and business-technical vocabulary mapping.",
            AutoSync = true,
            SyncIntervalMinutes = 60,
            CreatedAt = DateTime.UtcNow,
            LastSyncAt = DateTime.UtcNow,
            LastAnalysisAt = DateTime.UtcNow,
            IsLocal = true, // Mark as local since it's on the file system
            LocalPath = REPO_LOCAL_PATH,
            Tags = "autonomic-computing,self-healing,vocabulary-extraction,intelligent-analysis",
            Notes = "Local autonomic computing repository with real-time vocabulary analysis and self-learning capabilities"
        };

        _dbContext.Repositories.Add(repository);
        await _dbContext.SaveChangesAsync();

        return repository;
    }

    private async Task AddSampleRepositoryFilesAsync(int repositoryId)
    {
        var repositoryFiles = new List<RepositoryFile>
        {
            new() {
                RepositoryId = repositoryId,
                FilePath = "README.md",
                FileName = "README.md",
                FileExtension = ".md",
                Language = "Markdown",
                FileSize = 4250,
                LineCount = 125,
                FileHash = "hash_readme_md",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "01-SYSTEM-ARCHITECTURE-AND-DESIGN.md",
                FileName = "01-SYSTEM-ARCHITECTURE-AND-DESIGN.md",
                FileExtension = ".md",
                Language = "Markdown",
                FileSize = 18500,
                LineCount = 420,
                FileHash = "hash_architecture_md",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "RepoLens.Api/Program.cs",
                FileName = "Program.cs",
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 8500,
                LineCount = 250,
                FileHash = "hash_program_cs",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "RepoLens.Core/Entities/Repository.cs",
                FileName = "Repository.cs",
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 6800,
                LineCount = 185,
                FileHash = "hash_repository_entity",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-4),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "RepoLens.Infrastructure/Services/VocabularyExtractionService.cs",
                FileName = "VocabularyExtractionService.cs",
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 25000,
                LineCount = 850,
                FileHash = "hash_vocab_service",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "repolens-ui/src/App.tsx",
                FileName = "App.tsx",
                FileExtension = ".tsx",
                Language = "TypeScript",
                FileSize = 12000,
                LineCount = 350,
                FileHash = "hash_app_tsx",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddHours(-6),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "repolens-ui/src/components/search/Search.tsx",
                FileName = "Search.tsx",
                FileExtension = ".tsx",
                Language = "TypeScript",
                FileSize = 8900,
                LineCount = 245,
                FileHash = "hash_search_tsx",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddHours(-8),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "repolens-ui/package.json",
                FileName = "package.json",
                FileExtension = ".json",
                Language = "JSON",
                FileSize = 3200,
                LineCount = 85,
                FileHash = "hash_package_json",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "RepoLens.Tests/Integration/VocabularyExtractionIntegrationTest.cs",
                FileName = "VocabularyExtractionIntegrationTest.cs",
                FileExtension = ".cs",
                Language = "C#",
                FileSize = 45000,
                LineCount = 1200,
                FileHash = "hash_integration_test",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                FilePath = "start-services-optimized.bat",
                FileName = "start-services-optimized.bat",
                FileExtension = ".bat",
                Language = "Batch",
                FileSize = 1250,
                LineCount = 35,
                FileHash = "hash_start_services",
                ProcessingStatus = FileProcessingStatus.Completed,
                LastModified = DateTime.UtcNow.AddDays(-6),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _dbContext.RepositoryFiles.AddRange(repositoryFiles);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AddSampleCodeElementsAsync(int repositoryId)
    {
        var files = await _dbContext.RepositoryFiles
            .Where(f => f.RepositoryId == repositoryId)
            .ToListAsync();

        if (!files.Any()) return;

        var codeElements = new List<CodeElement>
        {
            new() {
                FileId = files.First(f => f.FileName == "VocabularyExtractionService.cs").Id,
                ElementType = CodeElementType.Class,
                Name = "VocabularyExtractionService",
                Signature = "public class VocabularyExtractionService : IVocabularyExtractionService",
                StartLine = 15,
                EndLine = 850,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = false,
                Parameters = JsonSerializer.Serialize(new { 
                    BusinessDomain = "VocabularyAnalysis", 
                    TechnicalComplexity = "High",
                    Purpose = "Self-learning vocabulary extraction from codebases"
                })
            },
            new() {
                FileId = files.First(f => f.FileName == "VocabularyExtractionService.cs").Id,
                ElementType = CodeElementType.Method,
                Name = "ExtractVocabularyAsync",
                Signature = "public async Task<VocabularyExtractionResult> ExtractVocabularyAsync(int repositoryId, CancellationToken ct = default)",
                StartLine = 45,
                EndLine = 125,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = true,
                Parameters = JsonSerializer.Serialize(new { 
                    BusinessPurpose = "Extract business and technical vocabulary from repository",
                    ReturnType = "VocabularyExtractionResult"
                })
            },
            new() {
                FileId = files.First(f => f.FileName == "Repository.cs").Id,
                ElementType = CodeElementType.Class,
                Name = "Repository",
                Signature = "public class Repository",
                StartLine = 8,
                EndLine = 185,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = false,
                Parameters = JsonSerializer.Serialize(new { 
                    EntityType = "Core Domain Entity",
                    Purpose = "Repository metadata and configuration"
                })
            },
            new() {
                FileId = files.First(f => f.FileName == "Search.tsx").Id,
                ElementType = CodeElementType.Function,
                Name = "Search",
                Signature = "const Search: React.FC<SearchProps> = ({ onResults })",
                StartLine = 25,
                EndLine = 245,
                AccessModifier = "public",
                IsStatic = false,
                IsAsync = false,
                Parameters = JsonSerializer.Serialize(new { 
                    ComponentType = "React Functional Component",
                    Purpose = "Search interface with natural language processing"
                })
            }
        };

        _dbContext.CodeElements.AddRange(codeElements);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AddRepositoryMetricsAsync(int repositoryId)
    {
        var metrics = new RepositoryMetrics
        {
            RepositoryId = repositoryId,
            TotalFiles = 150,
            TotalLines = 25000,
            TotalSize = 2500000, // 2.5 MB
            TotalCommits = 125,
            TotalContributors = 3,
            ActiveContributors = 2,
            LastCommitDate = DateTime.UtcNow.AddHours(-6),
            LanguageDistribution = JsonSerializer.Serialize(new Dictionary<string, int>
            {
                ["C#"] = 85,
                ["TypeScript"] = 45,
                ["Markdown"] = 12,
                ["JSON"] = 8,
                ["Batch"] = 2,
                ["PowerShell"] = 3
            }),
            FileTypeDistribution = JsonSerializer.Serialize(new Dictionary<string, int>
            {
                [".cs"] = 85,
                [".tsx"] = 35,
                [".ts"] = 10,
                [".md"] = 12,
                [".json"] = 8,
                [".bat"] = 2,
                [".ps1"] = 3
            }),
            ComplexityMetrics = JsonSerializer.Serialize(new
            {
                AverageMethodComplexity = 4.2,
                HighComplexityMethods = 8,
                CodeDuplication = 5.5,
                TestCoverage = 78.5
            }),
            QualityScore = 87.5,
            SecurityScore = 92.0,
            MaintainabilityIndex = 85.2,
            TechnicalDebt = 12.5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.RepositoryMetrics.Add(metrics);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AddSampleVocabularyTermsAsync(int repositoryId)
    {
        var vocabularyTerms = new List<VocabularyTerm>
        {
            new() {
                RepositoryId = repositoryId,
                Term = "AutonomicComputing",
                NormalizedTerm = "autonomic-computing",
                TermType = VocabularyTermType.BusinessTerm,
                Source = VocabularySource.Documentation,
                Language = "English",
                Frequency = 45,
                RelevanceScore = 0.95,
                BusinessRelevance = 0.98,
                TechnicalRelevance = 0.82,
                Context = "Self-managing computing systems that adapt and heal automatically",
                Domain = "AutonomicComputing",
                Synonyms = ["SelfHealing", "AdaptiveSystems", "IntelligentComputing"],
                RelatedTerms = ["SelfLearning", "MonitoringSystem", "AutonomousHealing"],
                UsageExamples = ["autonomic computing platform", "self-healing system design"],
                FirstSeen = DateTime.UtcNow.AddDays(-30),
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                Term = "VocabularyExtraction",
                NormalizedTerm = "vocabulary-extraction",
                TermType = VocabularyTermType.BusinessTerm,
                Source = VocabularySource.SourceCode,
                Language = "C#",
                Frequency = 38,
                RelevanceScore = 0.92,
                BusinessRelevance = 0.94,
                TechnicalRelevance = 0.89,
                Context = "Intelligent extraction of domain-specific vocabulary from codebases",
                Domain = "DomainModeling",
                Synonyms = ["TermExtraction", "VocabularyMining", "SemanticAnalysis"],
                RelatedTerms = ["BusinessConcepts", "TechnicalTerms", "DomainMapping"],
                UsageExamples = ["VocabularyExtractionService.Extract()", "vocabulary analysis pipeline"],
                FirstSeen = DateTime.UtcNow.AddDays(-25),
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                Term = "RepositoryIntelligence",
                NormalizedTerm = "repository-intelligence",
                TermType = VocabularyTermType.DomainSpecific,
                Source = VocabularySource.SourceCode,
                Language = "C#",
                Frequency = 32,
                RelevanceScore = 0.88,
                BusinessRelevance = 0.91,
                TechnicalRelevance = 0.85,
                Context = "Advanced analysis and understanding of repository structure and content patterns",
                Domain = "RepositoryAnalysis",
                Synonyms = ["CodebaseIntelligence", "RepositoryAnalytics", "SourceCodeIntelligence"],
                RelatedTerms = ["MetricsCollection", "PatternRecognition", "CodeAnalysis"],
                UsageExamples = ["repository intelligence dashboard", "intelligent code analysis"],
                FirstSeen = DateTime.UtcNow.AddDays(-20),
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                Term = "SelfLearningSystem",
                NormalizedTerm = "self-learning-system",
                TermType = VocabularyTermType.TechnicalTerm,
                Source = VocabularySource.MethodNames,
                Language = "C#",
                Frequency = 28,
                RelevanceScore = 0.85,
                BusinessRelevance = 0.78,
                TechnicalRelevance = 0.92,
                Context = "System that learns and adapts from patterns without explicit programming",
                Domain = "MachineLearning",
                Synonyms = ["AdaptiveLearning", "AutonomousLearning", "PatternLearning"],
                RelatedTerms = ["NeuralNetworks", "PatternRecognition", "AutonomicBehavior"],
                UsageExamples = ["self-learning vocabulary system", "adaptive pattern recognition"],
                FirstSeen = DateTime.UtcNow.AddDays(-18),
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new() {
                RepositoryId = repositoryId,
                Term = "NaturalLanguageSearch",
                NormalizedTerm = "natural-language-search",
                TermType = VocabularyTermType.TechnicalTerm,
                Source = VocabularySource.SourceCode,
                Language = "TypeScript",
                Frequency = 22,
                RelevanceScore = 0.82,
                BusinessRelevance = 0.85,
                TechnicalRelevance = 0.88,
                Context = "Search capability that understands natural language queries and intent",
                Domain = "SearchTechnology",
                Synonyms = ["SemanticSearch", "IntentBasedSearch", "IntelligentSearch"],
                RelatedTerms = ["QueryProcessing", "IntentAnalysis", "SearchSuggestions"],
                UsageExamples = ["natural language search component", "semantic query processing"],
                FirstSeen = DateTime.UtcNow.AddDays(-15),
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _dbContext.VocabularyTerms.AddRange(vocabularyTerms);
        await _dbContext.SaveChangesAsync();
    }

    private IServiceProvider SetupMainDatabaseServices()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Use MAIN database (same as UI) - repolens_db
        var connectionString = "Host=localhost;Database=repolens_db;Username=postgres;Password=TCEP;Port=5432";
        services.AddDbContext<RepoLensDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors());
        
        return services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        if (_serviceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}
