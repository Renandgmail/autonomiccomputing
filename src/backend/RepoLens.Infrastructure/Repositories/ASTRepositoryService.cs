using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Repositories
{
    public class ASTRepositoryService : IASTRepositoryService
    {
        private readonly RepoLensDbContext _context;
        private readonly ILogger<ASTRepositoryService> _logger;
        private readonly TimeSpan _analysisValidityPeriod = TimeSpan.FromHours(24); // Analysis valid for 24 hours

        public ASTRepositoryService(RepoLensDbContext context, ILogger<ASTRepositoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ASTRepositoryAnalysis?> GetRepositoryAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving AST analysis for repository {RepositoryId}", repositoryId);

                return await _context.ASTRepositoryAnalyses
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Statements)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Classes)
                            .ThenInclude(c => c.Methods)
                                .ThenInclude(m => m.Parameters)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Classes)
                            .ThenInclude(c => c.Properties)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Methods)
                            .ThenInclude(m => m.Parameters)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Methods)
                            .ThenInclude(m => m.Issues)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Imports)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Exports)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Issues)
                    .Include(ara => ara.Files)
                        .ThenInclude(f => f.Metrics)
                    .Include(ara => ara.Dependencies)
                    .Include(ara => ara.DuplicateBlocks)
                    .Include(ara => ara.Metrics)
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving AST analysis for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        public async Task<ASTFileAnalysis?> GetFileAnalysisAsync(int repositoryId, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving file analysis for {FilePath} in repository {RepositoryId}", filePath, repositoryId);

                // First, get the repository analysis
                var repositoryAnalysis = await _context.ASTRepositoryAnalyses
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (repositoryAnalysis == null)
                {
                    _logger.LogWarning("No repository analysis found for repository {RepositoryId}", repositoryId);
                    return null;
                }

                // Get the specific file analysis
                return await _context.ASTFileAnalyses
                    .Include(afa => afa.Statements)
                    .Include(afa => afa.Classes)
                        .ThenInclude(c => c.Methods)
                            .ThenInclude(m => m.Parameters)
                    .Include(afa => afa.Classes)
                        .ThenInclude(c => c.Properties)
                    .Include(afa => afa.Methods)
                        .ThenInclude(m => m.Parameters)
                    .Include(afa => afa.Methods)
                        .ThenInclude(m => m.Issues)
                    .Include(afa => afa.Imports)
                    .Include(afa => afa.Exports)
                    .Include(afa => afa.Issues)
                    .Include(afa => afa.Metrics)
                    .Where(afa => afa.RepositoryAnalysisId == repositoryAnalysis.Id && 
                                  afa.FilePath == filePath)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file analysis for {FilePath} in repository {RepositoryId}", filePath, repositoryId);
                throw;
            }
        }

        public async Task<List<ASTFileAnalysis>> GetFileAnalysesAsync(int repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving all file analyses for repository {RepositoryId}", repositoryId);

                // First, get the repository analysis
                var repositoryAnalysis = await _context.ASTRepositoryAnalyses
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (repositoryAnalysis == null)
                {
                    _logger.LogWarning("No repository analysis found for repository {RepositoryId}", repositoryId);
                    return new List<ASTFileAnalysis>();
                }

                return await _context.ASTFileAnalyses
                    .Include(afa => afa.Issues)
                    .Include(afa => afa.Metrics)
                    .Where(afa => afa.RepositoryAnalysisId == repositoryAnalysis.Id)
                    .OrderBy(afa => afa.FilePath)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file analyses for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        public async Task<ASTRepositoryAnalysis> SaveRepositoryAnalysisAsync(ASTRepositoryAnalysis analysis, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Saving AST analysis for repository {RepositoryId} with {FileCount} files", 
                    analysis.RepositoryId, analysis.Files.Count);

                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Remove existing analysis for this repository
                    var existingAnalysis = await _context.ASTRepositoryAnalyses
                        .Where(ara => ara.RepositoryId == analysis.RepositoryId)
                        .ToListAsync(cancellationToken);

                    if (existingAnalysis.Any())
                    {
                        _logger.LogInformation("Removing {Count} existing analyses for repository {RepositoryId}", 
                            existingAnalysis.Count, analysis.RepositoryId);
                        _context.ASTRepositoryAnalyses.RemoveRange(existingAnalysis);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    // Add the new analysis
                    _context.ASTRepositoryAnalyses.Add(analysis);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Set IDs for related entities
                    foreach (var file in analysis.Files)
                    {
                        file.RepositoryAnalysisId = analysis.Id;
                        
                        // Set file analysis ID for nested entities
                        foreach (var statement in file.Statements)
                            statement.FileAnalysisId = file.Id;
                        
                        foreach (var cls in file.Classes)
                        {
                            cls.FileAnalysisId = file.Id;
                            foreach (var method in cls.Methods)
                            {
                                method.ClassId = cls.Id;
                                method.FileAnalysisId = file.Id;
                                foreach (var param in method.Parameters)
                                    param.MethodId = method.Id;
                            }
                            foreach (var prop in cls.Properties)
                                prop.ClassId = cls.Id;
                        }
                        
                        foreach (var method in file.Methods.Where(m => m.ClassId == null))
                        {
                            method.FileAnalysisId = file.Id;
                            foreach (var param in method.Parameters)
                                param.MethodId = method.Id;
                        }
                        
                        foreach (var import in file.Imports)
                            import.FileAnalysisId = file.Id;
                        
                        foreach (var export in file.Exports)
                            export.FileAnalysisId = file.Id;
                        
                        foreach (var issue in file.Issues)
                            issue.FileAnalysisId = file.Id;
                        
                        // Note: file.Metrics is a value object, not an entity with FK
                    }

                    // Set IDs for repository-level entities
                    foreach (var dependency in analysis.Dependencies)
                        dependency.RepositoryAnalysisId = analysis.Id;
                    
                    foreach (var duplicate in analysis.DuplicateBlocks)
                        duplicate.RepositoryAnalysisId = analysis.Id;
                    
                    // Note: analysis.Metrics is a value object, not an entity with FK

                    // Save all entities in batches for better performance
                    await SaveInBatchesAsync(analysis, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("Successfully saved AST analysis for repository {RepositoryId}", analysis.RepositoryId);
                    return analysis;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving AST analysis for repository {RepositoryId}", analysis.RepositoryId);
                throw;
            }
        }

        public async Task<bool> IsAnalysisOutdatedAsync(int repositoryId, DateTime analysisDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow - _analysisValidityPeriod;
                var isOutdated = analysisDate < cutoffDate;

                _logger.LogInformation("Repository {RepositoryId} analysis from {AnalysisDate} is {Status}", 
                    repositoryId, analysisDate, isOutdated ? "outdated" : "current");

                return isOutdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if analysis is outdated for repository {RepositoryId}", repositoryId);
                return true; // Default to outdated on error to trigger fresh analysis
            }
        }

        public async Task DeleteRepositoryAnalysisAsync(int repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting AST analysis for repository {RepositoryId}", repositoryId);

                var existingAnalyses = await _context.ASTRepositoryAnalyses
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .ToListAsync(cancellationToken);

                if (existingAnalyses.Any())
                {
                    _context.ASTRepositoryAnalyses.RemoveRange(existingAnalyses);
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    _logger.LogInformation("Deleted {Count} analyses for repository {RepositoryId}", 
                        existingAnalyses.Count, repositoryId);
                }
                else
                {
                    _logger.LogInformation("No existing analyses found for repository {RepositoryId}", repositoryId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AST analysis for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        public async Task<List<ASTIssue>> GetRepositoryIssuesAsync(int repositoryId, string? severity = null, string? category = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving issues for repository {RepositoryId} with filters: severity={Severity}, category={Category}", 
                    repositoryId, severity, category);

                var repositoryAnalysis = await _context.ASTRepositoryAnalyses
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (repositoryAnalysis == null)
                {
                    return new List<ASTIssue>();
                }

                var query = _context.ASTIssues
                    .Include(i => i.FileAnalysis)
                    .Include(i => i.Method)
                    .Where(i => i.FileAnalysis.RepositoryAnalysisId == repositoryAnalysis.Id);

                if (!string.IsNullOrWhiteSpace(severity))
                {
                    query = query.Where(i => i.Severity == severity);
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(i => i.Category == category);
                }

                // Sort by severity priority, then by line number
                var severityOrder = new Dictionary<string, int>
                {
                    ["critical"] = 4,
                    ["high"] = 3,
                    ["medium"] = 2,
                    ["low"] = 1
                };

                var issues = await query.ToListAsync(cancellationToken);
                
                return issues.OrderByDescending(i => severityOrder.GetValueOrDefault(i.Severity.ToLower(), 0))
                           .ThenBy(i => i.FileAnalysis.FilePath)
                           .ThenBy(i => i.Line)
                           .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving issues for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        public async Task<List<DuplicateCodeBlock>> GetDuplicateCodeBlocksAsync(int repositoryId, int minSimilarity = 80, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving duplicate code blocks for repository {RepositoryId} with min similarity {MinSimilarity}%", 
                    repositoryId, minSimilarity);

                var repositoryAnalysis = await _context.ASTRepositoryAnalyses
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (repositoryAnalysis == null)
                {
                    return new List<DuplicateCodeBlock>();
                }

                return await _context.DuplicateCodeBlocks
                    .Where(dcb => dcb.RepositoryAnalysisId == repositoryAnalysis.Id && 
                                  dcb.SimilarityScore >= minSimilarity)
                    .OrderByDescending(dcb => dcb.SimilarityScore)
                    .ThenBy(dcb => dcb.GroupId)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving duplicate code blocks for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        public async Task<ASTMetrics?> GetRepositoryMetricsAsync(int repositoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var repositoryAnalysis = await _context.ASTRepositoryAnalyses
                    .Include(ara => ara.Metrics)
                    .Where(ara => ara.RepositoryId == repositoryId)
                    .OrderByDescending(ara => ara.AnalyzedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                return repositoryAnalysis?.Metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics for repository {RepositoryId}", repositoryId);
                throw;
            }
        }

        private async Task SaveInBatchesAsync(ASTRepositoryAnalysis analysis, CancellationToken cancellationToken)
        {
            const int batchSize = 100;

            // Save file analyses in batches
            for (int i = 0; i < analysis.Files.Count; i += batchSize)
            {
                var batch = analysis.Files.Skip(i).Take(batchSize);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("Saved batch {BatchNumber} of {TotalFiles} files for repository {RepositoryId}", 
                    i / batchSize + 1, analysis.Files.Count, analysis.RepositoryId);
            }

            // Save dependencies in batches
            for (int i = 0; i < analysis.Dependencies.Count; i += batchSize)
            {
                var batch = analysis.Dependencies.Skip(i).Take(batchSize);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Save duplicate blocks in batches
            for (int i = 0; i < analysis.DuplicateBlocks.Count; i += batchSize)
            {
                var batch = analysis.DuplicateBlocks.Skip(i).Take(batchSize);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Final save for any remaining changes
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
