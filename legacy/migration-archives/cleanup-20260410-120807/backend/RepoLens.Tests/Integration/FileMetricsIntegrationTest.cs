using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using RepoLens.Infrastructure;
using Xunit;
using System.Text.Json;

namespace RepoLens.Tests.Integration;

public class FileMetricsIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FileMetricsIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FileMetricsService_CalculateFileMetricsAsync_WithCSharpFile_ReturnsComprehensiveMetrics()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var fileMetricsService = scope.ServiceProvider.GetRequiredService<IFileMetricsService>();
        
        var repository = await CreateTestRepositoryAsync();
        var csharpFileContent = @"
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    /// <summary>
    /// Test class for file metrics calculation
    /// </summary>
    public class TestService
    {
        private readonly ILogger<TestService> _logger;
        
        public TestService(ILogger<TestService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Complex method with multiple branches for testing cyclomatic complexity
        /// </summary>
        public async Task<string> ProcessDataAsync(string input, int option)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException(""Input cannot be null or empty"");
                
            var result = string.Empty;
            
            // Multiple if statements to increase complexity
            if (option == 1)
            {
                result = input.ToUpper();
            }
            else if (option == 2)
            {
                result = input.ToLower();
            }
            else if (option == 3)
            {
                result = input.Trim();
            }
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (char.IsLetter(input[i]))
                    {
                        result += input[i];
                    }
                }
            }
            
            // Nested conditions for cognitive complexity
            if (result.Length > 10)
            {
                if (result.Contains(""test""))
                {
                    result = ""TESTED_"" + result;
                }
            }
            
            return await Task.FromResult(result);
        }
        
        // TODO: This method needs refactoring
        public string LongParameterMethod(string param1, int param2, bool param3, double param4, 
            DateTime param5, List<string> param6, Dictionary<string, object> param7)
        {
            return $""{param1}_{param2}_{param3}_{param4}_{param5}_{param6?.Count}_{param7?.Count}"";
        }
    }
}";

        var codeElements = new List<CodeElement>
        {
            new CodeElement { ElementType = CodeElementType.Class, Name = "TestService", StartLine = 10, EndLine = 60 },
            new CodeElement { ElementType = CodeElementType.Method, Name = "ProcessDataAsync", StartLine = 22, EndLine = 52, AccessModifier = "public" },
            new CodeElement { ElementType = CodeElementType.Method, Name = "LongParameterMethod", StartLine = 55, EndLine = 59, AccessModifier = "public" }
        };

        // Act
        var metrics = await fileMetricsService.CalculateFileMetricsAsync(
            repository.Id, 
            "src/TestService.cs", 
            csharpFileContent, 
            codeElements);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(repository.Id, metrics.RepositoryId);
        Assert.Equal("src/TestService.cs", metrics.FilePath);
        Assert.Equal("TestService.cs", metrics.FileName);
        Assert.Equal(".cs", metrics.FileExtension);
        Assert.Equal("C#", metrics.PrimaryLanguage);
        
        // Basic metrics
        Assert.True(metrics.FileSizeBytes > 0);
        Assert.True(metrics.LineCount > 50);
        Assert.True(metrics.EffectiveLineCount > 0);
        Assert.True(metrics.CommentDensity > 0);
        
        // Complexity metrics
        Assert.True(metrics.CyclomaticComplexity > 5);
        Assert.True(metrics.CognitiveComplexity > 0);
        Assert.True(metrics.NestingDepth > 1);
        
        // Quality metrics
        Assert.True(metrics.MaintainabilityIndex > 0);
        Assert.True(metrics.TechnicalDebtMinutes > 0);
        Assert.True(metrics.CodeSmellCount > 0);
        
        // Method metrics
        Assert.Equal(2, metrics.MethodCount);
        Assert.Equal(1, metrics.ClassCount);
        Assert.True(metrics.AverageMethodLength > 0);
        Assert.True(metrics.MaxMethodLength > 10);
        
        // File classification
        Assert.False(metrics.IsTestFile);
        Assert.False(metrics.IsConfigurationFile);
        Assert.False(metrics.IsGeneratedCode);
    }

    [Fact]
    public async Task FileMetricsService_AnalyzeSecurityAsync_DetectsVulnerabilities()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var fileMetricsService = scope.ServiceProvider.GetRequiredService<IFileMetricsService>();
        
        var vulnerableCode = @"
public class VulnerableCode 
{
    private string connectionString = ""Server=localhost;User=sa;Password=admin123;"";
    private string apiKey = ""sk-1234567890abcdef"";
    
    public DataTable GetUserData(string userId)
    {
        // SQL Injection vulnerability
        string query = ""SELECT * FROM Users WHERE Id = "" + userId;
        
        using var command = new SqlCommand(query);
        return command.ExecuteQuery();
    }
    
    public void WriteToPage(string userInput)
    {
        // XSS vulnerability
        Response.Write(""<div>"" + userInput + ""</div>"");
        page.innerHTML = userInput;
    }
    
    public string HashPassword(string password)
    {
        // Weak cryptography
        return MD5.ComputeHash(password);
    }
}";

        // Act
        var result = await fileMetricsService.AnalyzeSecurityAsync(vulnerableCode, "VulnerableCode.cs", "C#");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.VulnerabilityCount > 0);
        Assert.True(result.ContainsSensitiveData);
        Assert.True(result.SecurityHotspots > 0);
        Assert.True(result.SecurityIssues.Any(i => i.Contains("SQL_INJECTION")));
        Assert.True(result.SecurityIssues.Any(i => i.Contains("HARDCODED_SECRETS")));
    }

    [Fact]
    public async Task FileMetricsService_AnalyzeQualityMetricsAsync_DetectsCodeSmells()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var fileMetricsService = scope.ServiceProvider.GetRequiredService<IFileMetricsService>();
        
        var codeWithSmells = @"
public class SmellExample 
{
    // Magic numbers
    public void BadMethod(string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8)
    {
        var result = p1.Length * 42 + 123; // Magic numbers
        
        try 
        {
            // Some operation
        }
        catch (Exception ex)
        {
            // Empty catch block
        }
        
        // TODO: Fix this method
        // FIXME: Remove magic numbers
        
        if (result > 100)
        {
            if (result < 200)
            {
                if (result != 150)
                {
                    if (result % 2 == 0) // Deep nesting
                    {
                        Console.WriteLine(result);
                    }
                }
            }
        }
    }
}";

        // Act
        var result = await fileMetricsService.AnalyzeQualityMetricsAsync(codeWithSmells, "C#", new List<CodeElement>());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CodeSmellCount > 0);
        Assert.True(result.CodeSmells.Any(s => s.Contains("LONG_PARAMETER_LIST")));
        Assert.True(result.CodeSmells.Any(s => s.Contains("TODO_FIXME")));
        Assert.True(result.TechnicalDebtMinutes > 0);
    }

    [Fact]
    public async Task FileMetricsService_CalculateComplexityAsync_WithVariousLanguages_ReturnsLanguageSpecificResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var fileMetricsService = scope.ServiceProvider.GetRequiredService<IFileMetricsService>();
        
        var languages = new[]
        {
            ("C#", GenerateCSharpContent()),
            ("Python", GeneratePythonContent()),
            ("JavaScript", GenerateJavaScriptContent())
        };

        foreach (var (language, content) in languages)
        {
            // Act
            var result = await fileMetricsService.CalculateComplexityAsync(content, language, new List<CodeElement>());

            // Assert
            Assert.NotNull(result);
            Assert.True(result.LinesOfCode > 0);
            Assert.True(result.CyclomaticComplexity > 0);
        }
    }

    [Fact]
    public async Task FileMetricsService_CalculateFileHealthScore_ReturnsValidScore()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var fileMetricsService = scope.ServiceProvider.GetRequiredService<IFileMetricsService>();
        
        var complexity = new ComplexityMetricsResult
        {
            CyclomaticComplexity = 8,
            CognitiveComplexity = 12,
            LinesOfCode = 150,
            EffectiveLines = 120
        };

        var quality = new QualityMetricsResult
        {
            MaintainabilityIndex = 75,
            CodeSmellCount = 3,
            DocumentationCoverage = 0.8,
            TechnicalDebtMinutes = 45
        };

        var changes = new ChangePatternMetrics
        {
            ChurnRate = 2.5,
            ChangeFrequency = 1.2,
            TotalCommits = 15
        };

        // Act
        var healthScore = fileMetricsService.CalculateFileHealthScore(complexity, quality, changes);

        // Assert
        Assert.True(healthScore >= 0 && healthScore <= 1);
        Assert.True(healthScore > 0);
    }

    private async Task<Repository> CreateTestRepositoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RepoLensDbContext>();

        var repository = new Repository
        {
            Name = $"TestRepo{Guid.NewGuid():N}",
            Url = $"https://github.com/test/repo{Guid.NewGuid():N}",
            Description = "Test repository for file metrics integration test",
            DefaultBranch = "main",
            CreatedAt = DateTime.UtcNow,
            Status = RepositoryStatus.Active,
            IsPrivate = false,
            AutoSync = true,
            SyncIntervalMinutes = 60,
            ProviderType = ProviderType.GitHub,
            OwnerId = 1 // Assuming test user exists
        };

        context.Repositories.Add(repository);
        await context.SaveChangesAsync();

        return repository;
    }

    private string GenerateCSharpContent() => @"
public class TestClass 
{
    public void Method()
    {
        if (true)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }
    }
}";

    private string GeneratePythonContent() => @"
def test_function():
    if True:
        for i in range(10):
            if i > 5:
                print(i)
            elif i < 3:
                continue
            else:
                break
";

    private string GenerateJavaScriptContent() => @"
function testFunction() {
    if (true) {
        for (let i = 0; i < 10; i++) {
            if (i > 5) {
                console.log(i);
            } else if (i < 3) {
                continue;
            } else {
                break;
            }
        }
    }
}";
}
