using Microsoft.AspNetCore.Mvc;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using RepoLens.Api.Services;
using Microsoft.Extensions.Logging;
using RepoLens.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RepoLens.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ASTAnalysisController : ControllerBase
    {
        private readonly ILogger<ASTAnalysisController> _logger;
        private readonly TypeScriptASTService _typeScriptService;
        private readonly CSharpASTService _csharpService;
        private readonly PythonASTService _pythonService;
        private readonly IASTRepositoryService _astRepositoryService;
        private readonly RepoLensDbContext _context;

        public ASTAnalysisController(
            ILogger<ASTAnalysisController> logger,
            TypeScriptASTService typeScriptService,
            CSharpASTService csharpService,
            PythonASTService pythonService,
            IASTRepositoryService astRepositoryService,
            RepoLensDbContext context)
        {
            _logger = logger;
            _typeScriptService = typeScriptService;
            _csharpService = csharpService;
            _pythonService = pythonService;
            _astRepositoryService = astRepositoryService;
            _context = context;
        }

        /// <summary>
        /// Get complete AST analysis for a repository
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="includeStatements">Include detailed statement analysis</param>
        /// <param name="fileTypes">Filter by specific file types (e.g., .ts, .js)</param>
        /// <param name="forceRefresh">Force re-analysis even if cached data exists</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete AST repository analysis</returns>
        [HttpGet("repository/{repositoryId}/ast-analysis")]
        public async Task<ActionResult<ASTRepositoryAnalysis>> GetRepositoryASTAnalysis(
            int repositoryId,
            [FromQuery] bool includeStatements = false,
            [FromQuery] string[]? fileTypes = null,
            [FromQuery] bool forceRefresh = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting AST analysis for repository {RepositoryId}", repositoryId);

                // Get repository information
                var repository = await GetRepositoryAsync(repositoryId, cancellationToken);
                if (repository == null)
                {
                    return NotFound($"Repository {repositoryId} not found");
                }

                // Check if we have cached analysis and it's not outdated
                if (!forceRefresh)
                {
                    var existingAnalysis = await _astRepositoryService.GetRepositoryAnalysisAsync(repositoryId, cancellationToken);
                    if (existingAnalysis != null)
                    {
                        var isOutdated = await _astRepositoryService.IsAnalysisOutdatedAsync(
                            repositoryId, existingAnalysis.AnalyzedAt, cancellationToken);
                        
                        if (!isOutdated)
                        {
                            _logger.LogInformation("Using cached AST analysis for repository {RepositoryId}", repositoryId);
                            return Ok(existingAnalysis);
                        }
                    }
                }

                // Perform fresh analysis
                var analysis = await AnalyzeRepositoryFiles(repository, fileTypes, includeStatements, cancellationToken);
                
                // Save analysis to database
                var savedAnalysis = await _astRepositoryService.SaveRepositoryAnalysisAsync(analysis, cancellationToken);

                _logger.LogInformation("Completed AST analysis for repository {RepositoryId} with {FileCount} files", 
                    repositoryId, analysis.Files.Count);

                return Ok(savedAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing AST analysis for repository {RepositoryId}", repositoryId);
                return StatusCode(500, new { message = "Internal server error during AST analysis", error = ex.Message });
            }
        }

        /// <summary>
        /// Get AST analysis for a specific file
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="filePath">Relative file path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File AST analysis</returns>
        [HttpGet("repository/{repositoryId}/file")]
        public async Task<ActionResult<ASTFileAnalysis>> GetFileASTAnalysis(
            int repositoryId,
            [FromQuery] string filePath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return BadRequest("File path is required");
                }

                // Get repository information
                var repository = await GetRepositoryAsync(repositoryId, cancellationToken);
                if (repository == null)
                {
                    return NotFound($"Repository {repositoryId} not found");
                }

                // Check if cached analysis exists
                var existingAnalysis = await _astRepositoryService.GetFileAnalysisAsync(repositoryId, filePath, cancellationToken);
                if (existingAnalysis != null)
                {
                    _logger.LogInformation("Using cached file analysis for {FilePath}", filePath);
                    return Ok(existingAnalysis);
                }

                // Perform fresh file analysis
                var fullFilePath = Path.Combine(repository.LocalPath, filePath.TrimStart('/'));
                
                if (!System.IO.File.Exists(fullFilePath))
                {
                    return NotFound($"File {filePath} not found in repository");
                }

                var analysis = await AnalyzeFile(fullFilePath, cancellationToken);
                
                _logger.LogInformation("Completed file analysis for {FilePath} with {StatementCount} statements", 
                    filePath, analysis.Statements.Count);

                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing file {FilePath} in repository {RepositoryId}", filePath, repositoryId);
                return StatusCode(500, new { message = "Internal server error during file analysis", error = ex.Message });
            }
        }

        /// <summary>
        /// Get repository metrics summary
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>AST metrics summary</returns>
        [HttpGet("repository/{repositoryId}/metrics")]
        public async Task<ActionResult<ASTMetrics>> GetRepositoryMetrics(
            int repositoryId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var analysis = await _astRepositoryService.GetRepositoryAnalysisAsync(repositoryId, cancellationToken);
                
                if (analysis == null)
                {
                    return NotFound($"No AST analysis found for repository {repositoryId}");
                }

                return Ok(analysis.Metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for repository {RepositoryId}", repositoryId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get duplicate code blocks for a repository
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="minSimilarity">Minimum similarity score (0-100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of duplicate code blocks</returns>
        [HttpGet("repository/{repositoryId}/duplicates")]
        public async Task<ActionResult<List<DuplicateCodeBlock>>> GetDuplicateCodeBlocks(
            int repositoryId,
            [FromQuery] int minSimilarity = 80,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var analysis = await _astRepositoryService.GetRepositoryAnalysisAsync(repositoryId, cancellationToken);
                
                if (analysis == null)
                {
                    return NotFound($"No AST analysis found for repository {repositoryId}");
                }

                var duplicates = analysis.DuplicateBlocks
                    .Where(d => d.SimilarityScore >= minSimilarity)
                    .OrderByDescending(d => d.SimilarityScore)
                    .ToList();

                return Ok(duplicates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting duplicates for repository {RepositoryId}", repositoryId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get code issues for a repository
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="severity">Filter by severity (critical, high, medium, low)</param>
        /// <param name="category">Filter by category (security, performance, maintainability, reliability)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of code issues</returns>
        [HttpGet("repository/{repositoryId}/issues")]
        public async Task<ActionResult<List<ASTIssue>>> GetRepositoryIssues(
            int repositoryId,
            [FromQuery] string? severity = null,
            [FromQuery] string? category = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var files = await _astRepositoryService.GetFileAnalysesAsync(repositoryId, cancellationToken);
                
                var allIssues = files.SelectMany(f => f.Issues).ToList();
                
                if (!string.IsNullOrWhiteSpace(severity))
                {
                    allIssues = allIssues.Where(i => i.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    allIssues = allIssues.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Sort by severity priority
                var severityOrder = new Dictionary<string, int>
                {
                    ["critical"] = 4,
                    ["high"] = 3,
                    ["medium"] = 2,
                    ["low"] = 1
                };

                allIssues = allIssues.OrderByDescending(i => severityOrder.GetValueOrDefault(i.Severity.ToLower(), 0))
                                   .ThenBy(i => i.Line)
                                   .ToList();

                return Ok(allIssues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting issues for repository {RepositoryId}", repositoryId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete cached AST analysis for a repository
        /// </summary>
        /// <param name="repositoryId">Repository ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpDelete("repository/{repositoryId}/ast-analysis")]
        public async Task<ActionResult> DeleteRepositoryAnalysis(
            int repositoryId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _astRepositoryService.DeleteRepositoryAnalysisAsync(repositoryId, cancellationToken);
                
                _logger.LogInformation("Deleted AST analysis for repository {RepositoryId}", repositoryId);
                
                return Ok(new { message = $"AST analysis deleted for repository {repositoryId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AST analysis for repository {RepositoryId}", repositoryId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private async Task<ASTRepositoryAnalysis> AnalyzeRepositoryFiles(
            Repository repository, 
            string[]? fileTypes, 
            bool includeStatements, 
            CancellationToken cancellationToken)
        {
            var analysis = new ASTRepositoryAnalysis
            {
                RepositoryId = repository.Id,
                AnalyzedAt = DateTime.UtcNow,
                Version = "1.0",
                Files = new List<ASTFileAnalysis>(),
                Dependencies = new List<ASTDependency>(),
                DuplicateBlocks = new List<DuplicateCodeBlock>()
            };

            // Get all files in repository
            var allFiles = GetRepositoryFiles(repository.LocalPath, fileTypes);
            
            _logger.LogInformation("Found {FileCount} files to analyze in repository {RepositoryId}", 
                allFiles.Count, repository.Id);

            // Analyze each file
            foreach (var filePath in allFiles)
            {
                try
                {
                    var fileAnalysis = await AnalyzeFile(filePath, cancellationToken);
                    fileAnalysis.RepositoryAnalysisId = analysis.Id;
                    analysis.Files.Add(fileAnalysis);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to analyze file {FilePath}", filePath);
                    
                    // Add basic file info even on failure
                    var fileInfo = new FileInfo(filePath);
                    analysis.Files.Add(new ASTFileAnalysis
                    {
                        RepositoryAnalysisId = analysis.Id,
                        FilePath = Path.GetRelativePath(repository.LocalPath, filePath),
                        Language = GetLanguageFromExtension(fileInfo.Extension),
                        IsSupported = false,
                        FileSizeBytes = fileInfo.Length,
                        LineCount = 0,
                        LastModified = fileInfo.LastWriteTime,
                        Issues = new List<ASTIssue>
                        {
                            new ASTIssue
                            {
                                Severity = "high",
                                IssueType = "Analysis Error",
                                Category = "reliability",
                                Description = $"Failed to analyze file: {ex.Message}",
                                Recommendation = "Check file syntax and accessibility",
                                Line = 1,
                                RuleId = "AST001"
                            }
                        }
                    });
                }
            }

            // Calculate repository-wide metrics
            analysis.Metrics = CalculateRepositoryMetrics(analysis.Files);
            
            // Detect duplicate code blocks (simplified implementation)
            analysis.DuplicateBlocks = await DetectDuplicateCodeBlocks(analysis.Files, cancellationToken);

            // Calculate dependencies
            analysis.Dependencies = CalculateDependencies(analysis.Files);

            return analysis;
        }

        private async Task<ASTFileAnalysis> AnalyzeFile(string filePath, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            return extension switch
            {
                ".ts" or ".tsx" or ".js" or ".jsx" => await _typeScriptService.AnalyzeTypeScriptFileAsync(filePath, cancellationToken),
                ".cs" => await _csharpService.AnalyzeCSharpFileAsync(filePath, cancellationToken),
                ".py" or ".pyw" => await _pythonService.AnalyzePythonFileAsync(filePath, cancellationToken),
                _ => CreateBasicFileAnalysis(filePath)
            };
        }

        private ASTFileAnalysis CreateBasicFileAnalysis(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return new ASTFileAnalysis
            {
                FilePath = filePath,
                Language = GetLanguageFromExtension(fileInfo.Extension),
                IsSupported = false,
                FileSizeBytes = fileInfo.Length,
                LineCount = 0,
                LastModified = fileInfo.LastWriteTime,
                Metrics = new ASTFileMetrics { QualityScore = 50 }
            };
        }

        private List<string> GetRepositoryFiles(string repositoryPath, string[]? fileTypes)
        {
            var supportedExtensions = fileTypes ?? new[] { ".ts", ".tsx", ".js", ".jsx", ".cs", ".py" };
            
            var files = Directory.GetFiles(repositoryPath, "*.*", SearchOption.AllDirectories)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Where(f => !IsExcludedPath(f))
                .ToList();

            return files;
        }

        private bool IsExcludedPath(string filePath)
        {
            var excludedPaths = new[] { "node_modules", "bin", "obj", ".git", "dist", "build", "__pycache__" };
            return excludedPaths.Any(excluded => filePath.Contains(excluded, StringComparison.OrdinalIgnoreCase));
        }

        private ASTMetrics CalculateRepositoryMetrics(List<ASTFileAnalysis> files)
        {
            var supportedFiles = files.Where(f => f.IsSupported).ToList();

            return new ASTMetrics
            {
                TotalFiles = supportedFiles.Count,
                TotalLinesOfCode = supportedFiles.Sum(f => f.Metrics?.LinesOfCode ?? 0),
                TotalStatements = supportedFiles.Sum(f => f.Statements.Count),
                TotalClasses = supportedFiles.Sum(f => f.Classes.Count),
                TotalMethods = supportedFiles.Sum(f => f.Methods.Count),
                AverageComplexity = supportedFiles.Any() ? supportedFiles.Average(f => f.Metrics?.Complexity ?? 1) : 1,
                MaxComplexity = supportedFiles.Any() ? (int)supportedFiles.Max(f => f.Metrics?.Complexity ?? 1) : 1,
                TotalIssues = supportedFiles.Sum(f => f.Issues.Count),
                CriticalIssues = supportedFiles.Sum(f => f.Issues.Count(i => i.Severity == "critical")),
                HighIssues = supportedFiles.Sum(f => f.Issues.Count(i => i.Severity == "high")),
                MediumIssues = supportedFiles.Sum(f => f.Issues.Count(i => i.Severity == "medium")),
                LowIssues = supportedFiles.Sum(f => f.Issues.Count(i => i.Severity == "low")),
                CodeDuplicationPercentage = 0, // Will be calculated after duplicate detection
                CircularDependencies = 0, // Will be calculated during dependency analysis
                TechnicalDebtHours = CalculateTechnicalDebt(supportedFiles)
            };
        }

        private double CalculateTechnicalDebt(List<ASTFileAnalysis> files)
        {
            // Simple technical debt estimation based on issues and complexity
            double debt = 0;

            foreach (var file in files)
            {
                foreach (var issue in file.Issues)
                {
                    debt += issue.Severity switch
                    {
                        "critical" => 8, // 8 hours to fix
                        "high" => 4,     // 4 hours to fix
                        "medium" => 2,   // 2 hours to fix
                        "low" => 0.5,    // 30 minutes to fix
                        _ => 1
                    };
                }

                // Add complexity-based debt
                var avgComplexity = file.Metrics?.Complexity ?? 1;
                if (avgComplexity > 10)
                {
                    debt += (avgComplexity - 10) * 2; // 2 hours per complexity point over 10
                }
            }

            return Math.Round(debt, 1);
        }

        private async Task<List<DuplicateCodeBlock>> DetectDuplicateCodeBlocks(
            List<ASTFileAnalysis> files, 
            CancellationToken cancellationToken)
        {
            // Simplified duplicate detection - compare method signatures and structure
            // TODO: Implement proper AST-based semantic duplicate detection
            var duplicates = new List<DuplicateCodeBlock>();

            // Group methods by similar signatures
            var methodGroups = files
                .SelectMany(f => f.Methods.Select(m => new { File = f, Method = m }))
                .Where(fm => fm.Method.Signature.Length > 10) // Skip very short methods
                .GroupBy(fm => NormalizeSignature(fm.Method.Signature))
                .Where(g => g.Count() > 1); // Only groups with duplicates

            foreach (var group in methodGroups)
            {
                var groupId = Guid.NewGuid().ToString();
                foreach (var item in group)
                {
                    duplicates.Add(new DuplicateCodeBlock
                    {
                        GroupId = groupId,
                        FilePath = item.File.FilePath,
                        StartLine = item.Method.StartLine,
                        EndLine = item.Method.EndLine,
                        CodeBlock = item.Method.Signature,
                        Hash = ComputeHash(item.Method.Signature),
                        SimilarityScore = 90, // High similarity for same signature
                        LinesOfCode = item.Method.LinesOfCode,
                        DuplicateType = "similar"
                    });
                }
            }

            return duplicates;
        }

        private List<ASTDependency> CalculateDependencies(List<ASTFileAnalysis> files)
        {
            var dependencies = new List<ASTDependency>();

            foreach (var file in files)
            {
                foreach (var import in file.Imports)
                {
                    // Find target file for this import
                    var targetFile = files.FirstOrDefault(f => 
                        f.FilePath.EndsWith(import.Module + ".ts") ||
                        f.FilePath.EndsWith(import.Module + ".tsx") ||
                        f.FilePath.EndsWith(import.Module + ".js") ||
                        f.FilePath.EndsWith(import.Module + ".jsx"));

                    if (targetFile != null)
                    {
                        dependencies.Add(new ASTDependency
                        {
                            SourceFile = file.FilePath,
                            TargetFile = targetFile.FilePath,
                            DependencyType = "import",
                            Weight = 1,
                            IsCircular = false // TODO: Detect circular dependencies
                        });
                    }
                }
            }

            return dependencies;
        }

        private string GetLanguageFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".ts" => "typescript",
                ".tsx" => "typescript",
                ".js" => "javascript",
                ".jsx" => "javascript",
                ".cs" => "csharp",
                ".py" => "python",
                _ => "unknown"
            };
        }

        private string NormalizeSignature(string signature)
        {
            // Simple normalization - remove whitespace and parameter names
            return signature.Replace(" ", "").Replace("\t", "").Replace("\n", "");
        }

        private string ComputeHash(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hash)[..16]; // First 16 characters
        }

        private async Task<Repository?> GetRepositoryAsync(int repositoryId, CancellationToken cancellationToken)
        {
            return await _context.Repositories
                .FirstOrDefaultAsync(r => r.Id == repositoryId, cancellationToken);
        }
    }
}
