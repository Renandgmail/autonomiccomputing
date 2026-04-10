using RepoLens.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace RepoLens.Api.Services
{
    public interface IPythonASTService
    {
        Task<ASTFileAnalysis> AnalyzePythonFileAsync(string filePath, CancellationToken cancellationToken);
        Task<bool> IsPythonFileAsync(string filePath);
        Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken);
    }

    public class PythonASTService : IPythonASTService
    {
        private readonly ILogger<PythonASTService> _logger;

        // Common Python patterns for analysis
        private static readonly Regex ClassPattern = new(@"^class\s+(\w+)(?:\([^)]*\))?:", RegexOptions.Multiline);
        private static readonly Regex FunctionPattern = new(@"^(\s*)def\s+(\w+)\s*\([^)]*\):", RegexOptions.Multiline);
        private static readonly Regex ImportPattern = new(@"^(?:from\s+(\S+)\s+)?import\s+(.+)", RegexOptions.Multiline);
        
        public PythonASTService(ILogger<PythonASTService> logger)
        {
            _logger = logger;
        }

        public async Task<ASTFileAnalysis> AnalyzePythonFileAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting Python analysis for file: {FilePath}", filePath);

                if (!await IsPythonFileAsync(filePath))
                {
                    throw new ArgumentException($"File {filePath} is not a Python file");
                }

                var content = await File.ReadAllTextAsync(filePath, cancellationToken);
                var lines = content.Split('\n');
                var fileInfo = new FileInfo(filePath);

                var analysis = new ASTFileAnalysis
                {
                    FilePath = filePath,
                    Language = "python",
                    IsSupported = true,
                    FileSizeBytes = fileInfo.Length,
                    LineCount = lines.Length,
                    LastModified = fileInfo.LastWriteTime,
                    Classes = AnalyzeClasses(content),
                    Methods = AnalyzeFunctions(content),
                    Imports = AnalyzeImports(content),
                    Statements = AnalyzeStatements(lines),
                    Issues = new List<ASTIssue>()
                };

                // Calculate metrics
                analysis.Metrics = CalculateFileMetrics(analysis, content);

                // Analyze code issues
                analysis.Issues = await AnalyzeCodeIssuesAsync(filePath, analysis, cancellationToken);

                _logger.LogDebug("Completed Python analysis for {FilePath}: {ClassCount} classes, {FunctionCount} functions, {IssueCount} issues",
                    filePath, analysis.Classes.Count, analysis.Methods.Count, analysis.Issues.Count);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing Python file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<bool> IsPythonFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".py" || extension == ".pyw";
        }

        public async Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken)
        {
            var issues = new List<ASTIssue>();
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var lines = content.Split('\n');

            // Security issues
            issues.AddRange(AnalyzeSecurityIssues(lines));
            
            // Performance issues  
            issues.AddRange(AnalyzePerformanceIssues(lines));
            
            // Maintainability issues
            issues.AddRange(AnalyzeMaintainabilityIssues(analysis, lines));
            
            // Reliability issues
            issues.AddRange(AnalyzeReliabilityIssues(lines));

            return issues;
        }

        private List<ASTClass> AnalyzeClasses(string content)
        {
            var classes = new List<ASTClass>();
            var matches = ClassPattern.Matches(content);

            foreach (Match match in matches)
            {
                var className = match.Groups[1].Value;
                var lineNumber = content.Substring(0, match.Index).Split('\n').Length;
                
                // Find class end (simple heuristic)
                var classEndLine = FindClassEnd(content, match.Index);

                classes.Add(new ASTClass
                {
                    Name = className,
                    FullName = className,
                    StartLine = lineNumber,
                    EndLine = classEndLine,
                    AccessModifier = "public", // Python doesn't have explicit access modifiers
                    IsAbstract = content.Contains($"class {className}") && content.Contains("abc.ABC"),
                    BaseClass = ExtractBaseClasses(match.Value).FirstOrDefault(),
                    Methods = new List<ASTMethod>(),
                    Properties = new List<ASTProperty>()
                });
            }

            return classes;
        }

        private List<ASTMethod> AnalyzeFunctions(string content)
        {
            var methods = new List<ASTMethod>();
            var matches = FunctionPattern.Matches(content);

            foreach (Match match in matches)
            {
                var indentation = match.Groups[1].Value;
                var functionName = match.Groups[2].Value;
                var lineNumber = content.Substring(0, match.Index).Split('\n').Length;
                
                // Find function end
                var functionEndLine = FindFunctionEnd(content, match.Index, indentation.Length);
                var linesOfCode = functionEndLine - lineNumber + 1;

                // Calculate complexity (simplified)
                var functionContent = GetContentBetweenLines(content, lineNumber, functionEndLine);
                var complexity = CalculateCyclomaticComplexity(functionContent);

                methods.Add(new ASTMethod
                {
                    Name = functionName,
                    StartLine = lineNumber,
                    EndLine = functionEndLine,
                    AccessModifier = DetermineAccessModifier(functionName),
                    IsStatic = false, // Would need more sophisticated analysis
                    IsAsync = functionContent.Contains("async ") || functionContent.Contains("await "),
                    ReturnType = "object", // Python is dynamically typed
                    Parameters = CreateParameterObjects(ExtractParameters(match.Value)),
                    Signature = match.Value.Trim(),
                    LinesOfCode = linesOfCode,
                    CyclomaticComplexity = complexity
                });
            }

            return methods;
        }

        private List<ASTParameter> CreateParameterObjects(List<string> parameterNames)
        {
            var parameters = new List<ASTParameter>();
            for (int i = 0; i < parameterNames.Count; i++)
            {
                var paramName = parameterNames[i];
                parameters.Add(new ASTParameter
                {
                    Name = paramName,
                    Type = "object", // Python is dynamically typed
                    Position = i,
                    IsOptional = paramName.Contains("=")
                });
            }
            return parameters;
        }

        private List<ASTImport> AnalyzeImports(string content)
        {
            var imports = new List<ASTImport>();
            var matches = ImportPattern.Matches(content);

            foreach (Match match in matches)
            {
                var fromModule = match.Groups[1].Value;
                var importedItems = match.Groups[2].Value;
                var lineNumber = content.Substring(0, match.Index).Split('\n').Length;

                if (!string.IsNullOrEmpty(fromModule))
                {
                    // from module import items
                    imports.Add(new ASTImport
                    {
                        Module = fromModule,
                        ImportedSymbols = importedItems.Split(',').Select(s => s.Trim()).ToList(),
                        IsNamespaceImport = false,
                        Line = lineNumber
                    });
                }
                else
                {
                    // import module
                    imports.Add(new ASTImport
                    {
                        Module = importedItems.Trim(),
                        ImportedSymbols = new List<string>(),
                        IsNamespaceImport = true,
                        Line = lineNumber
                    });
                }
            }

            return imports;
        }

        private List<ASTStatement> AnalyzeStatements(string[] lines)
        {
            var statements = new List<ASTStatement>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var statementType = DetermineStatementType(line);
                statements.Add(new ASTStatement
                {
                    Type = statementType,
                    CodeSnippet = line,
                    Line = i + 1
                });
            }

            return statements;
        }

        private ASTFileMetrics CalculateFileMetrics(ASTFileAnalysis analysis, string content)
        {
            var lines = content.Split('\n');
            var codeLines = lines.Count(l => !string.IsNullOrWhiteSpace(l.Trim()) && !l.Trim().StartsWith('#'));

            var complexity = analysis.Methods.Any() ? 
                analysis.Methods.Average(m => (double)m.CyclomaticComplexity) : 1;

            var qualityScore = CalculateQualityScore(analysis);

            return new ASTFileMetrics
            {
                LinesOfCode = codeLines,
                Statements = analysis.Statements.Count,
                Classes = analysis.Classes.Count,
                Methods = analysis.Methods.Count,
                Complexity = complexity,
                Issues = analysis.Issues.Count,
                QualityScore = qualityScore
            };
        }

        private List<ASTIssue> AnalyzeSecurityIssues(string[] lines)
        {
            var issues = new List<ASTIssue>();

            // Check for common security issues
            var securityPatterns = new[]
            {
                new { Pattern = new Regex(@"eval\s*\("), Rule = "PY001", Message = "Use of eval() function", Severity = "critical" },
                new { Pattern = new Regex(@"exec\s*\("), Rule = "PY002", Message = "Use of exec() function", Severity = "critical" },
                new { Pattern = new Regex(@"subprocess\.call\s*\([^)]*shell\s*=\s*True"), Rule = "PY003", Message = "Shell injection vulnerability", Severity = "high" },
                new { Pattern = new Regex(@"pickle\.loads?\s*\("), Rule = "PY004", Message = "Unsafe pickle deserialization", Severity = "high" },
                new { Pattern = new Regex(@"random\.random\(\)"), Rule = "PY005", Message = "Use cryptographically secure random for security purposes", Severity = "medium" },
                new { Pattern = new Regex(@"md5\("), Rule = "PY006", Message = "MD5 is cryptographically weak", Severity = "medium" }
            };

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                foreach (var pattern in securityPatterns)
                {
                    if (pattern.Pattern.IsMatch(line))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = pattern.Severity,
                            IssueType = "Security Vulnerability",
                            Category = "security",
                            Description = pattern.Message,
                            Recommendation = GetSecurityRecommendation(pattern.Rule),
                            Line = lineIndex + 1,
                            RuleId = pattern.Rule
                        });
                    }
                }
            }

            return issues;
        }

        private List<ASTIssue> AnalyzePerformanceIssues(string[] lines)
        {
            var issues = new List<ASTIssue>();

            // Check for performance issues
            var performancePatterns = new[]
            {
                new { Pattern = new Regex(@"\.append\s*\([^)]*\)\s*$"), Rule = "PY101", Message = "Consider using list comprehension instead of append in loop", Severity = "low" },
                new { Pattern = new Regex(@"for\s+\w+\s+in\s+range\s*\(\s*len\s*\("), Rule = "PY102", Message = "Use enumerate() instead of range(len())", Severity = "low" },
                new { Pattern = new Regex(@"\.keys\(\)\s*$"), Rule = "PY103", Message = "Unnecessary .keys() call when iterating dictionary", Severity = "low" }
            };

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                foreach (var pattern in performancePatterns)
                {
                    if (pattern.Pattern.IsMatch(line))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = pattern.Severity,
                            IssueType = "Performance Issue",
                            Category = "performance",
                            Description = pattern.Message,
                            Recommendation = GetPerformanceRecommendation(pattern.Rule),
                            Line = lineIndex + 1,
                            RuleId = pattern.Rule
                        });
                    }
                }
            }

            return issues;
        }

        private List<ASTIssue> AnalyzeMaintainabilityIssues(ASTFileAnalysis analysis, string[] lines)
        {
            var issues = new List<ASTIssue>();

            // Function complexity issues
            foreach (var method in analysis.Methods)
            {
                if (method.CyclomaticComplexity > 10)
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = method.CyclomaticComplexity > 20 ? "high" : "medium",
                        IssueType = "Complexity Issue",
                        Category = "maintainability",
                        Description = $"Function '{method.Name}' has high complexity ({method.CyclomaticComplexity})",
                        Recommendation = "Consider breaking this function into smaller, more focused functions",
                        Line = method.StartLine,
                        RuleId = "PY201"
                    });
                }

                if (method.LinesOfCode > 50)
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = method.LinesOfCode > 100 ? "high" : "medium",
                        IssueType = "Size Issue",
                        Category = "maintainability",
                        Description = $"Function '{method.Name}' is too long ({method.LinesOfCode} lines)",
                        Recommendation = "Consider breaking this function into smaller functions",
                        Line = method.StartLine,
                        RuleId = "PY202"
                    });
                }
            }

            // File size issues
            if (analysis.LineCount > 1000)
            {
                issues.Add(new ASTIssue
                {
                    Severity = analysis.LineCount > 2000 ? "high" : "medium",
                    IssueType = "File Size Issue",
                    Category = "maintainability",
                    Description = $"File is very large ({analysis.LineCount} lines)",
                    Recommendation = "Consider splitting this file into multiple modules",
                    Line = 1,
                    RuleId = "PY203"
                });
            }

            return issues;
        }

        private List<ASTIssue> AnalyzeReliabilityIssues(string[] lines)
        {
            var issues = new List<ASTIssue>();

            // Check for reliability issues
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];

                // Bare except clauses
                if (Regex.IsMatch(line, @"except\s*:"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "medium",
                        IssueType = "Exception Handling",
                        Category = "reliability",
                        Description = "Bare except clause catches all exceptions",
                        Recommendation = "Specify the exception type(s) you want to catch",
                        Line = lineIndex + 1,
                        RuleId = "PY301"
                    });
                }

                // TODO comments
                if (line.Contains("TODO") || line.Contains("FIXME") || line.Contains("HACK"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "low",
                        IssueType = "Technical Debt",
                        Category = "maintainability",
                        Description = "TODO/FIXME comment found",
                        Recommendation = "Address this technical debt item",
                        Line = lineIndex + 1,
                        RuleId = "PY302"
                    });
                }
            }

            return issues;
        }

        private string DetermineStatementType(string line)
        {
            if (line.StartsWith("if ")) return "conditional";
            if (line.StartsWith("for ") || line.StartsWith("while ")) return "loop";
            if (line.StartsWith("try:")) return "exception";
            if (line.StartsWith("def ")) return "function";
            if (line.StartsWith("class ")) return "class";
            if (line.StartsWith("import ") || line.StartsWith("from ")) return "import";
            if (line.StartsWith("return ")) return "return";
            if (line.Contains("=") && !line.Contains("==") && !line.Contains("!=")) return "assignment";
            
            return "expression";
        }

        private string DetermineAccessModifier(string functionName)
        {
            if (functionName.StartsWith("__") && functionName.EndsWith("__"))
                return "special"; // Magic methods
            if (functionName.StartsWith("_"))
                return "protected"; // Convention for protected/private
            
            return "public";
        }

        private List<string> ExtractBaseClasses(string classDefinition)
        {
            var match = Regex.Match(classDefinition, @"class\s+\w+\s*\(\s*([^)]+)\s*\):");
            if (match.Success)
            {
                return match.Groups[1].Value.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
            return new List<string>();
        }

        private List<string> ExtractParameters(string functionDefinition)
        {
            var match = Regex.Match(functionDefinition, @"def\s+\w+\s*\(\s*([^)]*)\s*\):");
            if (match.Success)
            {
                var paramStr = match.Groups[1].Value;
                return paramStr.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }
            return new List<string>();
        }

        private int FindClassEnd(string content, int startIndex)
        {
            var lines = content.Substring(startIndex).Split('\n');
            var classIndentation = GetIndentationLevel(lines[0]);
            
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var lineIndentation = GetIndentationLevel(line);
                    if (lineIndentation <= classIndentation && !line.Trim().StartsWith('#'))
                    {
                        return content.Substring(0, startIndex).Split('\n').Length + i - 1;
                    }
                }
            }
            
            return content.Split('\n').Length;
        }

        private int FindFunctionEnd(string content, int startIndex, int functionIndentation)
        {
            var lines = content.Substring(startIndex).Split('\n');
            
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var lineIndentation = GetIndentationLevel(line);
                    if (lineIndentation <= functionIndentation && !line.Trim().StartsWith('#'))
                    {
                        return content.Substring(0, startIndex).Split('\n').Length + i - 1;
                    }
                }
            }
            
            return content.Split('\n').Length;
        }

        private int GetIndentationLevel(string line)
        {
            return line.Length - line.TrimStart().Length;
        }

        private string GetContentBetweenLines(string content, int startLine, int endLine)
        {
            var lines = content.Split('\n');
            var selectedLines = lines.Skip(startLine - 1).Take(endLine - startLine + 1);
            return string.Join('\n', selectedLines);
        }

        private int CalculateCyclomaticComplexity(string functionContent)
        {
            // Count decision points
            var complexity = 1; // Base complexity
            
            var decisionPatterns = new[]
            {
                @"\bif\b", @"\belif\b", @"\bfor\b", @"\bwhile\b",
                @"\btry\b", @"\bexcept\b", @"\band\b", @"\bor\b"
            };

            foreach (var pattern in decisionPatterns)
            {
                complexity += Regex.Matches(functionContent, pattern).Count;
            }

            return complexity;
        }

        private double CalculateQualityScore(ASTFileAnalysis analysis)
        {
            var score = 100.0;
            
            // Penalize for issues
            score -= analysis.Issues.Count(i => i.Severity == "critical") * 20;
            score -= analysis.Issues.Count(i => i.Severity == "high") * 10;
            score -= analysis.Issues.Count(i => i.Severity == "medium") * 5;
            score -= analysis.Issues.Count(i => i.Severity == "low") * 2;
            
            return Math.Max(0, Math.Min(100, score));
        }

        private string GetSecurityRecommendation(string ruleId)
        {
            return ruleId switch
            {
                "PY001" => "Replace eval() with safer alternatives like ast.literal_eval() or specific parsing",
                "PY002" => "Replace exec() with safer alternatives or proper code design",
                "PY003" => "Use shell=False and pass arguments as list, or validate input thoroughly",
                "PY004" => "Use safer serialization formats like JSON, or validate pickle data source",
                "PY005" => "Use secrets module for cryptographically secure random numbers",
                "PY006" => "Use SHA-256 or other secure hash algorithms instead of MD5",
                _ => "Follow security best practices for this pattern"
            };
        }

        private string GetPerformanceRecommendation(string ruleId)
        {
            return ruleId switch
            {
                "PY101" => "Use list comprehension: [expression for item in iterable]",
                "PY102" => "Use enumerate(): for i, item in enumerate(sequence)",
                "PY103" => "Iterate directly over dictionary: for key in dict",
                _ => "Follow Python performance best practices"
            };
        }
    }
}
