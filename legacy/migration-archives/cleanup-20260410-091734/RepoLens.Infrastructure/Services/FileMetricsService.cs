using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Core.Services;

namespace RepoLens.Infrastructure.Services;

/// <summary>
/// Implementation of comprehensive file metrics calculation service
/// </summary>
public class FileMetricsService : IFileMetricsService
{
    private readonly ILogger<FileMetricsService> _logger;
    private readonly ICommitRepository _commitRepository;

    // Security patterns for vulnerability detection
    private static readonly Dictionary<string, List<string>> SecurityPatterns = new()
    {
        ["SQL_INJECTION"] = new List<string>
        {
            @"(?i)SELECT\s+.*\s+FROM\s+.*\s+WHERE\s+.*\s*\+",
            @"(?i)""SELECT\s+.*\s+FROM\s+.*\s+WHERE\s+.*""\s*\+",
            @"(?i)query\s*\+=",
            @"(?i)sql\s*\+=",
            @"(?i)command\.CommandText\s*\+="
        },
        ["XSS"] = new List<string>
        {
            @"(?i)innerHTML\s*=",
            @"(?i)document\.write\s*\(",
            @"(?i)eval\s*\(",
            @"(?i)\.html\s*\(",
            @"(?i)Response\.Write\s*\("
        },
        ["HARDCODED_SECRETS"] = new List<string>
        {
            @"password\s*=\s*['""][^'""]+['""]",
            @"api_key\s*=\s*['""][^'""]+['""]",
            @"secret\s*=\s*['""][^'""]+['""]",
            @"private_key\s*=\s*['""][^'""]+['""]",
            @"access_token\s*=\s*['""][^'""]+['""]"
        },
        ["WEAK_CRYPTO"] = new List<string>
        {
            @"(?i)MD5\s*\(",
            @"(?i)SHA1\s*\(",
            @"(?i)DES\s*\(",
            @"(?i)RC4\s*\("
        }
    };

    // Code smell patterns
    private static readonly Dictionary<string, List<string>> CodeSmellPatterns = new()
    {
        ["LONG_PARAMETER_LIST"] = new List<string>
        {
            @"\([^)]{100,}\)", // Parameters longer than 100 characters
        },
        ["MAGIC_NUMBERS"] = new List<string>
        {
            @"\b(?<![\w\.])[0-9]{2,}\b(?![\w\.])", // Numbers with 2+ digits not part of identifiers
        },
        ["TODO_FIXME"] = new List<string>
        {
            @"(?i)//\s*(TODO|FIXME|HACK|BUG)",
            @"(?i)/\*\s*(TODO|FIXME|HACK|BUG)",
        },
        ["EMPTY_CATCH"] = new List<string>
        {
            @"catch\s*\([^)]*\)\s*\{\s*\}",
            @"except[^:]*:\s*pass",
        },
        ["DEEP_NESTING"] = new List<string>
        {
            @"\{\s*\n\s*if\s*\([^)]*\)\s*\{\s*\n\s*if\s*\([^)]*\)\s*\{\s*\n\s*if", // 4+ levels of nesting
        }
    };

    public FileMetricsService(ILogger<FileMetricsService> logger, ICommitRepository commitRepository)
    {
        _logger = logger;
        _commitRepository = commitRepository;
    }

    public async Task<FileMetrics> CalculateFileMetricsAsync(int repositoryId, string filePath, 
        string fileContent, IEnumerable<CodeElement> codeElements, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Calculating comprehensive metrics for file {FilePath}", filePath);

            var language = DetectLanguage(filePath);
            var codeElementsList = codeElements.ToList();

            // Calculate all metrics in parallel for performance
            var complexityTask = CalculateComplexityAsync(fileContent, language, codeElementsList);
            var qualityTask = AnalyzeQualityMetricsAsync(fileContent, language, codeElementsList);
            var securityTask = AnalyzeSecurityAsync(fileContent, filePath, language);
            var performanceTask = AnalyzePerformanceAsync(fileContent, filePath, language, codeElementsList);
            var changeTask = AnalyzeChangePatterns(repositoryId, filePath, cancellationToken);

            await Task.WhenAll(complexityTask, qualityTask, securityTask, performanceTask, changeTask);

            var complexity = await complexityTask;
            var quality = await qualityTask;
            var security = await securityTask;
            var performance = await performanceTask;
            var changes = await changeTask;

            // Calculate overall health score
            var healthScore = CalculateFileHealthScore(complexity, quality, changes);

            var fileMetrics = new FileMetrics
            {
                RepositoryId = repositoryId,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileExtension = Path.GetExtension(filePath),
                PrimaryLanguage = language,
                LastAnalyzed = DateTime.UtcNow,
                
                // Basic metrics
                FileSizeBytes = Encoding.UTF8.GetByteCount(fileContent),
                LineCount = CountLines(fileContent),
                EffectiveLineCount = complexity.EffectiveLines,
                CommentDensity = quality.CommentDensity,
                
                // Complexity metrics
                CyclomaticComplexity = complexity.CyclomaticComplexity,
                CognitiveComplexity = complexity.CognitiveComplexity,
                NestingDepth = complexity.NestingDepth,
                
                // Quality metrics
                MaintainabilityIndex = quality.MaintainabilityIndex,
                TechnicalDebtMinutes = quality.TechnicalDebtMinutes,
                CodeSmellCount = quality.CodeSmellCount,
                
                // Documentation metrics
                DocumentationCoverage = quality.DocumentationCoverage,
                
                // Duplication metrics
                DuplicationPercentage = quality.DuplicationPercentage,
                
                // Change metrics
                ChurnRate = changes.ChurnRate,
                ChangeFrequency = changes.ChangeFrequency,
                TotalCommits = changes.TotalCommits,
                UniqueContributors = changes.UniqueContributors,
                FirstCommit = changes.FirstCommit == default ? DateTime.UtcNow : changes.FirstCommit,
                LastCommit = changes.LastCommit == default ? DateTime.UtcNow : changes.LastCommit,
                LinesAdded = changes.LinesAdded,
                LinesDeleted = changes.LinesDeleted,
                
                // Security metrics
                VulnerabilityCount = security.VulnerabilityCount,
                SecurityHotspots = security.SecurityHotspots,
                ContainsSensitiveData = security.ContainsSensitiveData,
                
                // Health and risk metrics
                BugProneness = CalculateBugProneness(complexity, quality, changes),
                StabilityScore = CalculateStabilityScore(changes),
                MaturityScore = CalculateMaturityScore(changes),
                
                // Method metrics
                MethodCount = codeElementsList.Count(e => e.ElementType == CodeElementType.Method || e.ElementType == CodeElementType.Function),
                ClassCount = codeElementsList.Count(e => e.ElementType == CodeElementType.Class),
                AverageMethodLength = complexity.AverageMethodLength,
                MaxMethodLength = complexity.MaxMethodLength,
                
                // Performance and maintenance
                MaintenanceEffort = CalculatePerformanceImpactScore(performance) * 100,
                RefactoringPriority = quality.TechnicalDebtMinutes / 60.0, // Convert to hours
                
                // File classification
                FileCategory = "Source",
                IsTestFile = filePath.Contains("test", StringComparison.OrdinalIgnoreCase) || 
                           filePath.Contains("spec", StringComparison.OrdinalIgnoreCase),
                IsConfigurationFile = Path.GetExtension(filePath).ToLowerInvariant() is ".json" or ".xml" or ".yml" or ".yaml" or ".toml",
                IsGeneratedCode = fileContent.Contains("// <auto-generated>") || fileContent.Contains("This file was automatically generated")
            };

            stopwatch.Stop();
            _logger.LogDebug("Completed metrics calculation for {FilePath} in {ElapsedMs}ms", 
                filePath, stopwatch.ElapsedMilliseconds);

            return fileMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating file metrics for {FilePath}", filePath);
            
            // Return basic metrics on error
            return new FileMetrics
            {
                RepositoryId = repositoryId,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileExtension = Path.GetExtension(filePath),
                PrimaryLanguage = DetectLanguage(filePath),
                LastAnalyzed = DateTime.UtcNow,
                FileSizeBytes = Encoding.UTF8.GetByteCount(fileContent),
                LineCount = CountLines(fileContent),
                BugProneness = 0.5, // Neutral score on error
                FileCategory = "Source"
            };
        }
    }

    public async Task<ComplexityMetricsResult> CalculateComplexityAsync(string fileContent, string language, 
        IEnumerable<CodeElement> codeElements)
    {
        await Task.CompletedTask; // For async consistency
        
        var lines = fileContent.Split('\n');
        var codeElementsList = codeElements.ToList();
        
        var result = new ComplexityMetricsResult
        {
            LinesOfCode = CountLines(fileContent),
            EffectiveLines = CountEffectiveLines(fileContent, language)
        };

        // Calculate cyclomatic complexity
        result.CyclomaticComplexity = CalculateCyclomaticComplexity(fileContent, language);
        
        // Calculate cognitive complexity (more sophisticated than cyclomatic)
        result.CognitiveComplexity = CalculateCognitiveComplexity(fileContent, language);
        
        // Calculate nesting depth
        result.NestingDepth = CalculateMaxNestingDepth(fileContent, language);
        
        // Calculate method metrics
        var methodLengths = CalculateMethodLengths(codeElementsList, lines);
        result.AverageMethodLength = methodLengths.Any() ? methodLengths.Average() : 0;
        result.MaxMethodLength = methodLengths.Any() ? methodLengths.Max() : 0;

        return result;
    }

    public async Task<QualityMetricsResult> AnalyzeQualityMetricsAsync(string fileContent, string language, 
        IEnumerable<CodeElement> codeElements)
    {
        await Task.CompletedTask; // For async consistency
        
        var result = new QualityMetricsResult();

        // Calculate maintainability index (0-100 scale)
        result.MaintainabilityIndex = CalculateMaintainabilityIndex(fileContent, language, codeElements);
        
        // Detect code smells
        var codeSmells = DetectCodeSmells(fileContent, language);
        result.CodeSmells = codeSmells;
        result.CodeSmellCount = codeSmells.Count;
        
        // Calculate technical debt
        result.TechnicalDebtMinutes = CalculateTechnicalDebt(fileContent, language, codeSmells);
        
        // Calculate comment and documentation metrics
        result.CommentDensity = CalculateCommentDensity(fileContent, language);
        result.DocumentationCoverage = CalculateDocumentationCoverage(fileContent, language, codeElements);
        
        // Calculate duplication metrics
        var duplicationResult = AnalyzeDuplication(fileContent);
        result.DuplicationPercentage = duplicationResult.percentage;
        result.DuplicatedBlocks = duplicationResult.blocks;

        return result;
    }

    public double CalculateFileHealthScore(ComplexityMetricsResult complexityMetrics, 
        QualityMetricsResult qualityMetrics, ChangePatternMetrics? changeMetrics = null)
    {
        var scores = new List<double>();

        // Complexity score (0.0 - 1.0, higher is better)
        var complexityScore = Math.Max(0, 1.0 - (complexityMetrics.CyclomaticComplexity / 50.0));
        scores.Add(complexityScore * 0.25); // 25% weight

        // Quality score based on maintainability index
        var qualityScore = Math.Max(0, qualityMetrics.MaintainabilityIndex / 100.0);
        scores.Add(qualityScore * 0.25); // 25% weight

        // Code smells penalty
        var codeSmellScore = Math.Max(0, 1.0 - (qualityMetrics.CodeSmellCount / 20.0));
        scores.Add(codeSmellScore * 0.20); // 20% weight

        // Documentation score
        var docScore = qualityMetrics.DocumentationCoverage;
        scores.Add(docScore * 0.15); // 15% weight

        // Change stability score
        if (changeMetrics != null)
        {
            var stabilityScore = Math.Max(0, 1.0 - (changeMetrics.ChurnRate / 10.0));
            scores.Add(stabilityScore * 0.15); // 15% weight
        }
        else
        {
            scores.Add(0.7 * 0.15); // Neutral score if no change data
        }

        return Math.Min(1.0, scores.Sum());
    }

    public async Task<ChangePatternMetrics> AnalyzeChangePatterns(int repositoryId, string filePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get commit history for the file
            var commits = await _commitRepository.GetCommitsByRepositoryAsync(repositoryId);
            // Note: Since Commit entity doesn't have FilesChanged, we'll work with all commits for now
            var fileCommits = commits.ToList();

            if (!fileCommits.Any())
            {
                return new ChangePatternMetrics
                {
                    ChurnRate = 0,
                    ChangeFrequency = 0,
                    TotalCommits = 0,
                    UniqueContributors = 0,
                    ContributorBreakdown = new Dictionary<string, int>()
                };
            }

            var result = new ChangePatternMetrics
            {
                TotalCommits = fileCommits.Count,
                FirstCommit = fileCommits.Min(c => c.Timestamp),
                LastCommit = fileCommits.Max(c => c.Timestamp)
            };

            // Calculate contributor metrics
            var contributorGroups = fileCommits.GroupBy(c => c.Author).ToList();
            result.UniqueContributors = contributorGroups.Count();
            result.ContributorBreakdown = contributorGroups.ToDictionary(g => g.Key, g => g.Count());

            // Calculate change frequency (commits per month)
            var timeSpan = result.LastCommit - result.FirstCommit;
            var months = Math.Max(1, timeSpan.TotalDays / 30.44);
            result.ChangeFrequency = result.TotalCommits / months;

            // Calculate churn rate (simplified - based on commit frequency and file size changes)
            result.ChurnRate = CalculateChurnRate(fileCommits);

            // Estimate lines added/deleted (simplified calculation - using placeholder values since Commit entity lacks these properties)
            result.LinesAdded = 0; // TODO: Add LinesAdded property to Commit entity or calculate from git diff
            result.LinesDeleted = 0; // TODO: Add LinesDeleted property to Commit entity or calculate from git diff

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing change patterns for {FilePath}", filePath);
            return new ChangePatternMetrics();
        }
    }

    public async Task<SecurityAnalysisResult> AnalyzeSecurityAsync(string fileContent, string filePath, string language)
    {
        await Task.CompletedTask; // For async consistency
        
        var result = new SecurityAnalysisResult();
        var issues = new List<string>();
        var sensitivePatterns = new List<string>();

        // Analyze security patterns
        foreach (var category in SecurityPatterns)
        {
            foreach (var pattern in category.Value)
            {
                var matches = Regex.Matches(fileContent, pattern, RegexOptions.Multiline);
                if (matches.Count > 0)
                {
                    issues.Add($"{category.Key}: {matches.Count} occurrences");
                    
                    if (category.Key == "HARDCODED_SECRETS")
                    {
                        result.ContainsSensitiveData = true;
                        sensitivePatterns.AddRange(matches.Cast<Match>().Select(m => m.Value));
                    }
                }
            }
        }

        result.SecurityIssues = issues;
        result.SensitiveDataPatterns = sensitivePatterns;
        result.VulnerabilityCount = issues.Count;
        result.SecurityHotspots = CalculateSecurityHotspots(fileContent, language);

        return result;
    }

    public async Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(string fileContent, string filePath, 
        string language, IEnumerable<CodeElement> codeElements)
    {
        await Task.CompletedTask; // For async consistency
        
        var result = new PerformanceAnalysisResult();

        // Estimate compilation impact based on file size and complexity
        result.CompilationImpact = EstimateCompilationImpact(fileContent, language);
        
        // Estimate bundle size contribution
        result.BundleSizeContribution = EstimateBundleSize(fileContent, language);
        
        // Detect CPU-intensive operations
        result.CpuIntensiveOperations = DetectCpuIntensiveOperations(fileContent, language);
        
        // Estimate memory footprint
        result.MemoryFootprint = EstimateMemoryFootprint(fileContent, language, codeElements);
        
        // Generate optimization opportunities
        result.OptimizationOpportunities = GenerateOptimizationSuggestions(fileContent, language, codeElements);

        return result;
    }

    #region Private Helper Methods

    private static string DetectLanguage(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".tsx" => "TypeScript",
            ".jsx" => "JavaScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".h" => "C/C++",
            ".go" => "Go",
            ".rs" => "Rust",
            ".rb" => "Ruby",
            ".php" => "PHP",
            _ => "Unknown"
        };
    }

    private static int CountLines(string content)
    {
        if (string.IsNullOrEmpty(content)) return 0;
        return content.Count(c => c == '\n') + 1;
    }

    private static int CountEffectiveLines(string content, string language)
    {
        var lines = content.Split('\n');
        var effectiveLines = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (IsCommentLine(trimmed, language)) continue;
            if (IsBraceLine(trimmed)) continue;
            
            effectiveLines++;
        }

        return effectiveLines;
    }

    private static bool IsCommentLine(string line, string language)
    {
        return language switch
        {
            "C#" or "JavaScript" or "TypeScript" or "Java" or "C++" or "C" => 
                line.StartsWith("//") || line.StartsWith("/*") || line.StartsWith("*"),
            "Python" => line.StartsWith("#"),
            _ => false
        };
    }

    private static bool IsBraceLine(string line)
    {
        return line is "{" or "}" or "(" or ")" or "[" or "]";
    }

    private double CalculateCyclomaticComplexity(string content, string language)
    {
        var complexity = 1; // Base complexity

        var patterns = language switch
        {
            "C#" or "Java" => new[] { @"\bif\b", @"\belse\b", @"\bfor\b", @"\bforeach\b", @"\bwhile\b", @"\bdo\b", @"\bcase\b", @"\bcatch\b", @"\b\?\s*:", @"\b&&\b", @"\b\|\|\b" },
            "JavaScript" or "TypeScript" => new[] { @"\bif\b", @"\belse\b", @"\bfor\b", @"\bwhile\b", @"\bdo\b", @"\bcase\b", @"\bcatch\b", @"\b\?\s*:", @"\b&&\b", @"\b\|\|\b" },
            "Python" => new[] { @"\bif\b", @"\belif\b", @"\belse\b", @"\bfor\b", @"\bwhile\b", @"\btry\b", @"\bexcept\b", @"\band\b", @"\bor\b" },
            _ => new[] { @"\bif\b", @"\belse\b", @"\bfor\b", @"\bwhile\b", @"\bcase\b" }
        };

        foreach (var pattern in patterns)
        {
            complexity += Regex.Matches(content, pattern, RegexOptions.IgnoreCase).Count;
        }

        return complexity;
    }

    private double CalculateCognitiveComplexity(string content, string language)
    {
        // Cognitive complexity considers nesting levels and specific constructs
        var complexity = 0;
        var nestingLevel = 0;
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            // Count opening braces (increase nesting)
            nestingLevel += trimmed.Count(c => c == '{');
            
            // Cognitive complexity increments
            if (Regex.IsMatch(trimmed, @"\b(if|else if|for|foreach|while|do|case|catch)\b", RegexOptions.IgnoreCase))
            {
                complexity += 1 + nestingLevel;
            }
            
            // Boolean operators add complexity
            complexity += Regex.Matches(trimmed, @"\b(&&|\|\|)\b").Count;
            
            // Count closing braces (decrease nesting)
            nestingLevel -= trimmed.Count(c => c == '}');
            nestingLevel = Math.Max(0, nestingLevel);
        }

        return complexity;
    }

    private double CalculateMaxNestingDepth(string content, string language)
    {
        var maxDepth = 0;
        var currentDepth = 0;

        foreach (var char_ in content)
        {
            if (char_ == '{')
            {
                currentDepth++;
                maxDepth = Math.Max(maxDepth, currentDepth);
            }
            else if (char_ == '}')
            {
                currentDepth--;
            }
        }

        return maxDepth;
    }

    private List<int> CalculateMethodLengths(List<CodeElement> codeElements, string[] lines)
    {
        var methodLengths = new List<int>();

        foreach (var element in codeElements.Where(e => e.ElementType == CodeElementType.Method || e.ElementType == CodeElementType.Function))
        {
            if (element.EndLine > element.StartLine && element.EndLine <= lines.Length)
            {
                methodLengths.Add(element.EndLine - element.StartLine + 1);
            }
        }

        return methodLengths;
    }

    private double CalculateMaintainabilityIndex(string content, string language, IEnumerable<CodeElement> codeElements)
    {
        // Simplified maintainability index calculation based on:
        // - Halstead Volume
        // - Cyclomatic Complexity
        // - Lines of Code
        // - Comment percentage

        var linesOfCode = CountEffectiveLines(content, language);
        var cyclomaticComplexity = CalculateCyclomaticComplexity(content, language);
        var commentDensity = CalculateCommentDensity(content, language);
        
        // Simplified Halstead Volume estimation
        var operators = CountOperators(content, language);
        var operands = CountOperands(content, language);
        var halsteadVolume = (operators + operands) * Math.Log2(CountUniqueOperators(content, language) + CountUniqueOperands(content, language));
        
        // Maintainability Index formula (simplified)
        var maintainabilityIndex = Math.Max(0, 171 - 5.2 * Math.Log(halsteadVolume) - 0.23 * cyclomaticComplexity - 16.2 * Math.Log(linesOfCode) + 50 * Math.Sin(Math.Sqrt(2.4 * commentDensity)));
        
        return Math.Min(100, maintainabilityIndex);
    }

    private List<string> DetectCodeSmells(string content, string language)
    {
        var smells = new List<string>();

        foreach (var category in CodeSmellPatterns)
        {
            foreach (var pattern in category.Value)
            {
                var matches = Regex.Matches(content, pattern, RegexOptions.Multiline);
                if (matches.Count > 0)
                {
                    smells.Add($"{category.Key}: {matches.Count} occurrences");
                }
            }
        }

        return smells;
    }

    private double CalculateTechnicalDebt(string content, string language, List<string> codeSmells)
    {
        // Estimate technical debt in minutes based on various factors
        var debt = 0.0;

        // Base debt from code smells
        debt += codeSmells.Count * 5; // 5 minutes per code smell

        // Debt from complexity
        var complexity = CalculateCyclomaticComplexity(content, language);
        debt += Math.Max(0, complexity - 10) * 2; // 2 minutes per complexity point over 10

        // Debt from file size
        var lines = CountLines(content);
        debt += Math.Max(0, lines - 500) * 0.1; // 0.1 minutes per line over 500

        return debt;
    }

    private double CalculateCommentDensity(string content, string language)
    {
        var lines = content.Split('\n');
        var totalLines = lines.Length;
        var commentLines = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (IsCommentLine(trimmed, language))
            {
                commentLines++;
            }
        }

        return totalLines > 0 ? (double)commentLines / totalLines : 0;
    }

    private double CalculateDocumentationCoverage(string content, string language, IEnumerable<CodeElement> codeElements)
    {
        var publicMembers = codeElements.Where(e => e.AccessModifier == "public").ToList();
        if (!publicMembers.Any()) return 1.0; // No public members = 100% covered

        var documentedMembers = 0;
        var lines = content.Split('\n');

        foreach (var member in publicMembers)
        {
            if (member.StartLine > 1 && member.StartLine <= lines.Length)
            {
                var precedingLine = lines[member.StartLine - 2].Trim();
                if (IsDocumentationComment(precedingLine, language))
                {
                    documentedMembers++;
                }
            }
        }

        return (double)documentedMembers / publicMembers.Count;
    }

    private bool IsDocumentationComment(string line, string language)
    {
        return language switch
        {
            "C#" => line.StartsWith("///"),
            "JavaScript" or "TypeScript" => line.StartsWith("/**"),
            "Python" => line.StartsWith("\"\"\"") || line.StartsWith("'''"),
            "Java" => line.StartsWith("/**"),
            _ => false
        };
    }

    private (double percentage, List<string> blocks) AnalyzeDuplication(string content)
    {
        // Simplified duplication detection
        var lines = content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l.Trim())).ToArray();
        var duplicatedLines = 0;
        var duplicatedBlocks = new List<string>();

        for (int i = 0; i < lines.Length - 3; i++)
        {
            var block = string.Join("\n", lines.Skip(i).Take(4));
            var occurrences = 0;

            for (int j = i + 4; j < lines.Length - 3; j++)
            {
                var compareBlock = string.Join("\n", lines.Skip(j).Take(4));
                if (block.Equals(compareBlock, StringComparison.OrdinalIgnoreCase))
                {
                    occurrences++;
                    if (occurrences == 1) // First duplicate found
                    {
                        duplicatedLines += 4;
                        duplicatedBlocks.Add($"Lines {i + 1}-{i + 4}");
                    }
                }
            }
        }

        var percentage = lines.Length > 0 ? (double)duplicatedLines / lines.Length * 100 : 0;
        return (Math.Min(100, percentage), duplicatedBlocks);
    }

    private double CalculateBugProneness(ComplexityMetricsResult complexity, QualityMetricsResult quality, ChangePatternMetrics changes)
    {
        var proneness = 0.0;

        // High complexity increases bug proneness
        proneness += Math.Min(1.0, complexity.CyclomaticComplexity / 50.0) * 0.3;

        // Code smells increase bug proneness
        proneness += Math.Min(1.0, quality.CodeSmellCount / 10.0) * 0.2;

        // High change frequency can indicate instability
        proneness += Math.Min(1.0, changes.ChangeFrequency / 5.0) * 0.2;

        // Low maintainability increases bug proneness
        proneness += Math.Max(0, (100 - quality.MaintainabilityIndex) / 100.0) * 0.3;

        return Math.Min(1.0, proneness);
    }

    private double CalculateStabilityScore(ChangePatternMetrics changes)
    {
        if (changes.TotalCommits == 0) return 1.0; // No changes = stable

        // Lower change frequency = more stable
        var stabilityScore = Math.Max(0, 1.0 - (changes.ChangeFrequency / 10.0));

        // Lower churn rate = more stable
        stabilityScore += Math.Max(0, 1.0 - (changes.ChurnRate / 5.0));

        return Math.Min(1.0, stabilityScore / 2.0);
    }

    private double CalculateMaturityScore(ChangePatternMetrics changes)
    {
        if (changes.TotalCommits == 0) return 0.5; // No history = neutral

        var ageInDays = (DateTime.UtcNow - changes.FirstCommit).TotalDays;
        var maturityFromAge = Math.Min(1.0, ageInDays / 365.0); // 1 year = full maturity

        var maturityFromCommits = Math.Min(1.0, changes.TotalCommits / 100.0); // 100 commits = mature

        return (maturityFromAge + maturityFromCommits) / 2.0;
    }

    private double CalculatePerformanceImpactScore(PerformanceAnalysisResult performance)
    {
        var impact = 0.0;

        impact += Math.Min(1.0, performance.CompilationImpact / 100.0) * 0.25;
        impact += Math.Min(1.0, performance.BundleSizeContribution / 1000000.0) * 0.25; // 1MB threshold
        impact += Math.Min(1.0, performance.CpuIntensiveOperations / 10.0) * 0.25;
        impact += Math.Min(1.0, performance.MemoryFootprint / 100.0) * 0.25;

        return impact;
    }

    private double CalculateChurnRate(List<Commit> commits)
    {
        if (commits.Count < 2) return 0;

        // TODO: Calculate from actual git diff when LinesAdded/LinesDeleted are available
        // For now, estimate churn based on commit frequency
        var timeSpan = commits.Max(c => c.Timestamp) - commits.Min(c => c.Timestamp);
        var days = Math.Max(1, timeSpan.TotalDays);
        
        // Simple estimation: commits per day as churn rate
        return commits.Count / days;
    }

    private int CalculateSecurityHotspots(string content, string language)
    {
        var hotspots = 0;

        // File operations without proper validation
        hotspots += Regex.Matches(content, @"File\.(Read|Write|Delete)", RegexOptions.IgnoreCase).Count;

        // Network operations
        hotspots += Regex.Matches(content, @"HttpClient|WebRequest|Socket", RegexOptions.IgnoreCase).Count;

        // Database operations
        hotspots += Regex.Matches(content, @"SqlCommand|ExecuteReader|ExecuteNonQuery", RegexOptions.IgnoreCase).Count;

        return hotspots;
    }

    private double EstimateCompilationImpact(string content, string language)
    {
        var impact = CountLines(content) * 0.1; // Base impact from lines
        
        // Complex constructs increase compilation time
        impact += Regex.Matches(content, @"\bclass\b", RegexOptions.IgnoreCase).Count * 2;
        impact += Regex.Matches(content, @"\binterface\b", RegexOptions.IgnoreCase).Count * 1.5;
        impact += Regex.Matches(content, @"\bgeneric\b", RegexOptions.IgnoreCase).Count * 3;

        return impact;
    }

    private long EstimateBundleSize(string content, string language)
    {
        // Rough estimation based on file size and language
        var baseSize = Encoding.UTF8.GetByteCount(content);
        
        return language switch
        {
            "JavaScript" or "TypeScript" => (long)(baseSize * 0.7), // Minification effect
            "C#" => baseSize, // Compiled
            _ => baseSize
        };
    }

    private int DetectCpuIntensiveOperations(string content, string language)
    {
        var operations = 0;

        operations += Regex.Matches(content, @"\bfor\s*\(\s*.*\s*;\s*.*\s*;\s*.*\s*\)", RegexOptions.IgnoreCase).Count;
        operations += Regex.Matches(content, @"\bwhile\s*\(", RegexOptions.IgnoreCase).Count;
        operations += Regex.Matches(content, @"\.Sort\(|\.OrderBy\(", RegexOptions.IgnoreCase).Count;
        operations += Regex.Matches(content, @"Thread\.Sleep|Task\.Delay", RegexOptions.IgnoreCase).Count;

        return operations;
    }

    private double EstimateMemoryFootprint(string content, string language, IEnumerable<CodeElement> codeElements)
    {
        var footprint = 0.0;

        // Base footprint from file size
        footprint += Encoding.UTF8.GetByteCount(content) / 1024.0; // KB

        // Additional footprint from complex types
        footprint += codeElements.Count(e => e.ElementType == CodeElementType.Class) * 10; // KB per class
        footprint += Regex.Matches(content, @"\bList<|ArrayList|Dictionary<|HashMap", RegexOptions.IgnoreCase).Count * 5;

        return footprint;
    }

    private List<string> GenerateOptimizationSuggestions(string content, string language, IEnumerable<CodeElement> codeElements)
    {
        var suggestions = new List<string>();

        // Check for inefficient patterns
        if (Regex.IsMatch(content, @"string\s+\w+\s*=\s*"""";[\s\S]*?\w+\s*\+="))
        {
            suggestions.Add("Consider using StringBuilder instead of string concatenation in loops");
        }

        if (Regex.IsMatch(content, @"\.ToList\(\)\.Count"))
        {
            suggestions.Add("Use .Count() instead of .ToList().Count for better performance");
        }

        if (Regex.Matches(content, @"\bforeach\b", RegexOptions.IgnoreCase).Count > 5)
        {
            suggestions.Add("Consider using parallel processing for multiple foreach loops");
        }

        return suggestions;
    }

    private int CountOperators(string content, string language)
    {
        var operators = new[] { "+", "-", "*", "/", "=", "==", "!=", "<", ">", "&&", "||" };
        return operators.Sum(op => content.Split(new[] { op }, StringSplitOptions.None).Length - 1);
    }

    private int CountOperands(string content, string language)
    {
        // Simplified operand counting
        return Regex.Matches(content, @"\b\w+\b").Count;
    }

    private int CountUniqueOperators(string content, string language)
    {
        var operators = new[] { "+", "-", "*", "/", "=", "==", "!=", "<", ">", "&&", "||" };
        return operators.Count(op => content.Contains(op));
    }

    private int CountUniqueOperands(string content, string language)
    {
        var operands = Regex.Matches(content, @"\b\w+\b").Cast<Match>().Select(m => m.Value).Distinct();
        return operands.Count();
    }

    #endregion
}
