using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using System.Text.RegularExpressions;

namespace RepoLens.Api.Services
{
    public class CSharpASTService : ICSharpASTService
    {
        private readonly ILogger<CSharpASTService> _logger;
        private static readonly string[] CSharpExtensions = { ".cs" };

        public CSharpASTService(ILogger<CSharpASTService> logger)
        {
            _logger = logger;
        }

        public async Task<ASTFileAnalysis> AnalyzeCSharpFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Analyzing C# file: {FilePath}", filePath);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileInfo = new FileInfo(filePath);
                var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);

                // Create the analysis result
                var analysis = new ASTFileAnalysis
                {
                    FilePath = filePath,
                    Language = "csharp",
                    IsSupported = true,
                    FileSizeBytes = fileInfo.Length,
                    LineCount = sourceCode.Split('\n').Length,
                    LastModified = fileInfo.LastWriteTime
                };

                // Parse C# code using Roslyn
                var tree = CSharpSyntaxTree.ParseText(sourceCode, cancellationToken: cancellationToken);
                var root = tree.GetCompilationUnitRoot(cancellationToken);

                // Create compilation for semantic model
                var compilation = CSharpCompilation.Create("TempAssembly")
                    .AddReferences(GetMetadataReferences())
                    .AddSyntaxTrees(tree);

                var semanticModel = compilation.GetSemanticModel(tree);

                // Extract AST elements
                analysis.Statements = ExtractStatements(root);
                analysis.Classes = ExtractClasses(root, semanticModel);
                analysis.Methods = ExtractMethods(root, semanticModel);
                analysis.Imports = ExtractImports(root);
                analysis.Exports = ExtractExports(root);

                // Calculate metrics
                analysis.Metrics = CalculateFileMetrics(analysis, sourceCode);

                // Analyze code issues
                analysis.Issues = await AnalyzeCodeIssuesAsync(filePath, analysis, cancellationToken);

                _logger.LogInformation("Successfully analyzed C# file: {FilePath} with {StatementCount} statements", 
                    filePath, analysis.Statements.Count);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing C# file: {FilePath}", filePath);
                
                // Return basic analysis even on error
                var fileInfo = new FileInfo(filePath);
                return new ASTFileAnalysis
                {
                    FilePath = filePath,
                    Language = "csharp",
                    IsSupported = false,
                    FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
                    LineCount = 0,
                    LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue,
                    Issues = new List<ASTIssue>
                    {
                        new ASTIssue
                        {
                            Severity = "high",
                            IssueType = "Analysis Error",
                            Category = "reliability",
                            Description = $"Failed to analyze C# file: {ex.Message}",
                            Recommendation = "Check file syntax and ensure it's valid C# code",
                            Line = 1,
                            RuleId = "CS_AST001"
                        }
                    }
                };
            }
        }

        public Task<bool> IsCSharpFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return Task.FromResult(CSharpExtensions.Contains(extension));
        }

        public async Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken = default)
        {
            var issues = new List<ASTIssue>();

            try
            {
                var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);
                var lines = sourceCode.Split('\n');

                // Rule 1: Security Issues
                await AnalyzeSecurityIssues(issues, lines, sourceCode);

                // Rule 2: Performance Issues
                AnalyzePerformanceIssues(issues, analysis);

                // Rule 3: Code Quality Issues
                AnalyzeQualityIssues(issues, lines, analysis);

                // Rule 4: .NET Specific Issues
                AnalyzeDotNetSpecificIssues(issues, sourceCode);

                _logger.LogInformation("Found {IssueCount} code issues in C# file {FilePath}", issues.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing code issues for C# file: {FilePath}", filePath);
            }

            return issues;
        }

        private async Task AnalyzeSecurityIssues(List<ASTIssue> issues, string[] lines, string sourceCode)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // SQL injection patterns
                if (line.Contains("ExecuteNonQuery") && line.Contains("+") && !line.Contains("//"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "critical",
                        IssueType = "SQL Injection",
                        Category = "security",
                        Description = "Potential SQL injection vulnerability - string concatenation detected",
                        Recommendation = "Use parameterized queries (SqlParameter) or Entity Framework",
                        Line = i + 1,
                        RuleId = "CS_SEC001",
                        MoreInfoUrl = "https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/sql-injection"
                    });
                }

                // Hardcoded connection strings
                if ((line.Contains("ConnectionString") || line.Contains("connectionString")) && 
                    (line.Contains("Server=") || line.Contains("Data Source=")) && 
                    !line.Contains("//"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "high",
                        IssueType = "Hardcoded Connection String",
                        Category = "security",
                        Description = "Hardcoded connection string detected",
                        Recommendation = "Use appsettings.json or environment variables for connection strings",
                        Line = i + 1,
                        RuleId = "CS_SEC002"
                    });
                }

                // Insecure cryptography
                if (line.Contains("MD5") || line.Contains("SHA1") || line.Contains("DES"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "high",
                        IssueType = "Weak Cryptography",
                        Category = "security",
                        Description = "Use of weak or deprecated cryptographic algorithm",
                        Recommendation = "Use SHA256, AES, or other modern cryptographic algorithms",
                        Line = i + 1,
                        RuleId = "CS_SEC003"
                    });
                }

                // Unsafe reflection
                if (line.Contains("Assembly.LoadFrom") || line.Contains("Activator.CreateInstance"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "medium",
                        IssueType = "Unsafe Reflection",
                        Category = "security",
                        Description = "Dynamic assembly loading or object creation detected",
                        Recommendation = "Validate input and use secure reflection patterns",
                        Line = i + 1,
                        RuleId = "CS_SEC004"
                    });
                }

                // Hardcoded secrets
                var secretPatterns = new[]
                {
                    @"password\s*=\s*""[^""]+""",
                    @"apikey\s*=\s*""[^""]+""",
                    @"secret\s*=\s*""[^""]+""",
                    @"token\s*=\s*""[^""]+"""
                };

                foreach (var pattern in secretPatterns)
                {
                    if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "high",
                            IssueType = "Hardcoded Secrets",
                            Category = "security",
                            Description = "Hardcoded credential or secret detected",
                            Recommendation = "Use secure configuration management (Azure Key Vault, etc.)",
                            Line = i + 1,
                            RuleId = "CS_SEC005"
                        });
                        break;
                    }
                }
            }
        }

        private void AnalyzePerformanceIssues(List<ASTIssue> issues, ASTFileAnalysis analysis)
        {
            foreach (var method in analysis.Methods)
            {
                // High cyclomatic complexity
                if (method.CyclomaticComplexity > 15)
                {
                    issues.Add(new ASTIssue
                    {
                        MethodId = method.Id,
                        Severity = method.CyclomaticComplexity > 20 ? "high" : "medium",
                        IssueType = "High Complexity",
                        Category = "maintainability",
                        Description = $"Method '{method.Name}' has high cyclomatic complexity ({method.CyclomaticComplexity})",
                        Recommendation = "Consider breaking down into smaller, focused methods",
                        Line = method.StartLine,
                        RuleId = "CS_PERF001"
                    });
                }

                // Long methods
                if (method.LinesOfCode > 50)
                {
                    issues.Add(new ASTIssue
                    {
                        MethodId = method.Id,
                        Severity = method.LinesOfCode > 100 ? "high" : "medium",
                        IssueType = "Long Method",
                        Category = "maintainability",
                        Description = $"Method '{method.Name}' is too long ({method.LinesOfCode} lines)",
                        Recommendation = "Extract smaller, cohesive methods following SRP",
                        Line = method.StartLine,
                        RuleId = "CS_PERF002"
                    });
                }

                // Too many parameters
                if (method.Parameters.Count > 7)
                {
                    issues.Add(new ASTIssue
                    {
                        MethodId = method.Id,
                        Severity = "medium",
                        IssueType = "Too Many Parameters",
                        Category = "maintainability",
                        Description = $"Method '{method.Name}' has too many parameters ({method.Parameters.Count})",
                        Recommendation = "Consider parameter objects or dependency injection",
                        Line = method.StartLine,
                        RuleId = "CS_PERF003"
                    });
                }
            }

            // Large classes
            foreach (var cls in analysis.Classes)
            {
                if (cls.Methods.Count > 20)
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "medium",
                        IssueType = "Large Class",
                        Category = "maintainability",
                        Description = $"Class '{cls.Name}' has too many methods ({cls.Methods.Count})",
                        Recommendation = "Consider splitting into smaller, more focused classes",
                        Line = cls.StartLine,
                        RuleId = "CS_PERF004"
                    });
                }
            }
        }

        private void AnalyzeQualityIssues(List<ASTIssue> issues, string[] lines, ASTFileAnalysis analysis)
        {
            // TODO comments
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].ToLower();
                if (line.Contains("todo") || line.Contains("fixme") || line.Contains("hack"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = line.Contains("hack") ? "medium" : "low",
                        IssueType = "Technical Debt Comment",
                        Category = "maintainability",
                        Description = "TODO, FIXME, or HACK comment found",
                        Recommendation = "Address the technical debt item or create a proper issue",
                        Line = i + 1,
                        RuleId = "CS_QUAL001"
                    });
                }
            }

            // Empty catch blocks
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("catch") && i + 2 < lines.Length && 
                    lines[i + 1].Trim() == "{" && lines[i + 2].Trim() == "}")
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "medium",
                        IssueType = "Empty Catch Block",
                        Category = "reliability",
                        Description = "Empty catch block detected - exceptions are being silently ignored",
                        Recommendation = "Add proper exception handling or logging",
                        Line = i + 1,
                        RuleId = "CS_QUAL002"
                    });
                }
            }
        }

        private void AnalyzeDotNetSpecificIssues(List<ASTIssue> issues, string sourceCode)
        {
            var lines = sourceCode.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Missing ConfigureAwait(false)
                if (line.Contains("await ") && !line.Contains("ConfigureAwait") && !line.Contains("//"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "low",
                        IssueType = "Missing ConfigureAwait",
                        Category = "performance",
                        Description = "Consider using ConfigureAwait(false) for library code",
                        Recommendation = "Add .ConfigureAwait(false) to avoid deadlocks",
                        Line = i + 1,
                        RuleId = "CS_NET001"
                    });
                }

                // String concatenation in loops
                if ((line.Contains("for ") || line.Contains("while ") || line.Contains("foreach ")) &&
                    sourceCode.Substring(sourceCode.IndexOf(line)).Contains("+=") &&
                    sourceCode.Substring(sourceCode.IndexOf(line)).Contains("string"))
                {
                    issues.Add(new ASTIssue
                    {
                        Severity = "medium",
                        IssueType = "String Concatenation in Loop",
                        Category = "performance",
                        Description = "String concatenation in loop detected",
                        Recommendation = "Use StringBuilder for multiple string concatenations",
                        Line = i + 1,
                        RuleId = "CS_NET002"
                    });
                }

                // IDisposable not disposed
                if (line.Contains("new FileStream") || line.Contains("new SqlConnection") ||
                    line.Contains("new HttpClient"))
                {
                    var nextFewLines = string.Join(" ", lines.Skip(i).Take(10));
                    if (!nextFewLines.Contains("using") && !nextFewLines.Contains(".Dispose()"))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "medium",
                            IssueType = "Resource Not Disposed",
                            Category = "reliability",
                            Description = "IDisposable resource may not be properly disposed",
                            Recommendation = "Use 'using' statement or explicit Dispose() call",
                            Line = i + 1,
                            RuleId = "CS_NET003"
                        });
                    }
                }
            }
        }

        private List<ASTStatement> ExtractStatements(CompilationUnitSyntax root)
        {
            var statements = new List<ASTStatement>();
            var statementNodes = root.DescendantNodes().OfType<StatementSyntax>();

            foreach (var statement in statementNodes)
            {
                var lineSpan = statement.GetLocation().GetLineSpan();
                
                statements.Add(new ASTStatement
                {
                    StatementId = Guid.NewGuid().ToString(),
                    Type = GetStatementType(statement),
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1,
                    StartPosition = statement.SpanStart,
                    EndPosition = statement.Span.End,
                    CodeSnippet = statement.ToString().Substring(0, Math.Min(200, statement.ToString().Length)),
                    Complexity = CalculateStatementComplexity(statement),
                    Dependencies = new List<string>()
                });
            }

            return statements;
        }

        private List<ASTClass> ExtractClasses(CompilationUnitSyntax root, SemanticModel semanticModel)
        {
            var classes = new List<ASTClass>();
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations)
            {
                var lineSpan = classDecl.GetLocation().GetLineSpan();
                var symbolInfo = semanticModel.GetDeclaredSymbol(classDecl);

                var astClass = new ASTClass
                {
                    Name = classDecl.Identifier.ValueText,
                    FullName = symbolInfo?.ToDisplayString() ?? classDecl.Identifier.ValueText,
                    StartLine = lineSpan.StartLinePosition.Line + 1,
                    EndLine = lineSpan.EndLinePosition.Line + 1,
                    AccessModifier = GetAccessModifier(classDecl.Modifiers),
                    IsAbstract = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    IsInterface = false,
                    BaseClass = GetBaseClass(classDecl),
                    Interfaces = GetImplementedInterfaces(classDecl),
                    Methods = new List<ASTMethod>(),
                    Properties = new List<ASTProperty>()
                };

                // Extract methods
                var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    astClass.Methods.Add(ExtractMethod(method, semanticModel));
                }

                // Extract properties
                var properties = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                foreach (var property in properties)
                {
                    astClass.Properties.Add(ExtractProperty(property));
                }

                classes.Add(astClass);
            }

            return classes;
        }

        private List<ASTMethod> ExtractMethods(CompilationUnitSyntax root, SemanticModel semanticModel)
        {
            var methods = new List<ASTMethod>();
            var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var methodDecl in methodDeclarations)
            {
                methods.Add(ExtractMethod(methodDecl, semanticModel));
            }

            return methods;
        }

        private ASTMethod ExtractMethod(MethodDeclarationSyntax methodDecl, SemanticModel semanticModel)
        {
            var lineSpan = methodDecl.GetLocation().GetLineSpan();
            
            return new ASTMethod
            {
                Name = methodDecl.Identifier.ValueText,
                Signature = GetMethodSignature(methodDecl),
                StartLine = lineSpan.StartLinePosition.Line + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                AccessModifier = GetAccessModifier(methodDecl.Modifiers),
                IsStatic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsAsync = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)),
                ReturnType = methodDecl.ReturnType.ToString(),
                Parameters = ExtractParameters(methodDecl),
                CyclomaticComplexity = CalculateMethodComplexity(methodDecl),
                LinesOfCode = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line + 1,
                CalledMethods = ExtractMethodCalls(methodDecl),
                Issues = new List<ASTIssue>()
            };
        }

        private ASTProperty ExtractProperty(PropertyDeclarationSyntax propertyDecl)
        {
            var lineSpan = propertyDecl.GetLocation().GetLineSpan();
            
            return new ASTProperty
            {
                Name = propertyDecl.Identifier.ValueText,
                Type = propertyDecl.Type.ToString(),
                AccessModifier = GetAccessModifier(propertyDecl.Modifiers),
                HasGetter = propertyDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,
                HasSetter = propertyDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,
                IsStatic = propertyDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                Line = lineSpan.StartLinePosition.Line + 1
            };
        }

        private List<ASTParameter> ExtractParameters(MethodDeclarationSyntax methodDecl)
        {
            var parameters = new List<ASTParameter>();
            
            for (int i = 0; i < methodDecl.ParameterList.Parameters.Count; i++)
            {
                var param = methodDecl.ParameterList.Parameters[i];
                parameters.Add(new ASTParameter
                {
                    Name = param.Identifier.ValueText,
                    Type = param.Type?.ToString() ?? "unknown",
                    IsOptional = param.Default != null,
                    DefaultValue = param.Default?.Value.ToString(),
                    Position = i
                });
            }

            return parameters;
        }

        private List<ASTImport> ExtractImports(CompilationUnitSyntax root)
        {
            var imports = new List<ASTImport>();
            
            foreach (var usingDirective in root.Usings)
            {
                var lineSpan = usingDirective.GetLocation().GetLineSpan();
                
                imports.Add(new ASTImport
                {
                    Module = usingDirective.Name?.ToString() ?? "",
                    Line = lineSpan.StartLinePosition.Line + 1,
                    IsDefaultImport = false,
                    IsNamespaceImport = true,
                    ImportedSymbols = new List<string>()
                });
            }

            return imports;
        }

        private List<ASTExport> ExtractExports(CompilationUnitSyntax root)
        {
            var exports = new List<ASTExport>();
            
            // In C#, public classes/methods are considered "exports"
            var publicClasses = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)));

            foreach (var publicClass in publicClasses)
            {
                var lineSpan = publicClass.GetLocation().GetLineSpan();
                
                exports.Add(new ASTExport
                {
                    Name = publicClass.Identifier.ValueText,
                    Type = "class",
                    IsDefault = false,
                    Line = lineSpan.StartLinePosition.Line + 1
                });
            }

            return exports;
        }

        private ASTFileMetrics CalculateFileMetrics(ASTFileAnalysis analysis, string sourceCode)
        {
            var lines = sourceCode.Split('\n');
            var codeLines = lines.Count(line => !string.IsNullOrWhiteSpace(line.Trim()) && 
                                               !line.Trim().StartsWith("//") && 
                                               !line.Trim().StartsWith("/*"));

            var complexity = analysis.Methods.Any() 
                ? analysis.Methods.Average(m => m.CyclomaticComplexity) 
                : 1.0;

            var qualityScore = CalculateQualityScore(analysis, complexity);

            return new ASTFileMetrics
            {
                LinesOfCode = codeLines,
                Statements = analysis.Statements.Count,
                Classes = analysis.Classes.Count,
                Methods = analysis.Methods.Count,
                Complexity = Math.Round(complexity, 1),
                Issues = analysis.Issues.Count,
                QualityScore = Math.Round(qualityScore, 1),
                QualityTrend = "flat"
            };
        }

        private double CalculateQualityScore(ASTFileAnalysis analysis, double complexity)
        {
            double score = 100.0;

            // Reduce score based on complexity
            if (complexity > 15) score -= (complexity - 15) * 5;
            else if (complexity > 10) score -= (complexity - 10) * 3;
            else if (complexity > 5) score -= (complexity - 5) * 1;

            // Reduce score based on issues
            foreach (var issue in analysis.Issues)
            {
                score -= issue.Severity switch
                {
                    "critical" => 25,
                    "high" => 20,
                    "medium" => 10,
                    "low" => 3,
                    _ => 5
                };
            }

            return Math.Max(0, Math.Min(100, score));
        }

        #region Helper Methods

        private string GetStatementType(StatementSyntax statement)
        {
            return statement.Kind().ToString().Replace("Statement", "").Replace("Syntax", "");
        }

        private int CalculateStatementComplexity(StatementSyntax statement)
        {
            var complexity = 1;

            // Add complexity for control structures
            if (statement is IfStatementSyntax ||
                statement is WhileStatementSyntax ||
                statement is ForStatementSyntax ||
                statement is ForEachStatementSyntax ||
                statement is SwitchStatementSyntax ||
                statement is TryStatementSyntax)
            {
                complexity++;
            }

            return complexity;
        }

        private int CalculateMethodComplexity(MethodDeclarationSyntax method)
        {
            var complexity = 1; // Base complexity

            var complexityNodes = method.DescendantNodes().Where(n =>
                n.IsKind(SyntaxKind.IfStatement) ||
                n.IsKind(SyntaxKind.WhileStatement) ||
                n.IsKind(SyntaxKind.ForStatement) ||
                n.IsKind(SyntaxKind.ForEachStatement) ||
                n.IsKind(SyntaxKind.SwitchSection) ||
                n.IsKind(SyntaxKind.CatchClause) ||
                n.IsKind(SyntaxKind.ConditionalExpression));

            return complexity + complexityNodes.Count();
        }

        private string GetAccessModifier(SyntaxTokenList modifiers)
        {
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) return "public";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) return "private";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword))) return "protected";
            if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword))) return "internal";
            return "private"; // Default in C#
        }

        private string GetMethodSignature(MethodDeclarationSyntax method)
        {
            return $"{method.ReturnType} {method.Identifier}({string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})";
        }

        private string? GetBaseClass(ClassDeclarationSyntax classDecl)
        {
            return classDecl.BaseList?.Types.FirstOrDefault()?.ToString();
        }

        private List<string> GetImplementedInterfaces(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null) return new List<string>();
            
            return classDecl.BaseList.Types.Skip(1).Select(t => t.ToString()).ToList();
        }

        private List<string> ExtractMethodCalls(MethodDeclarationSyntax method)
        {
            var methodCalls = new List<string>();
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    methodCalls.Add(memberAccess.Name.Identifier.ValueText);
                }
                else if (invocation.Expression is IdentifierNameSyntax identifier)
                {
                    methodCalls.Add(identifier.Identifier.ValueText);
                }
            }

            return methodCalls.Distinct().ToList();
        }

        private MetadataReference[] GetMetadataReferences()
        {
            var references = new List<MetadataReference>();
            
            // Add basic .NET references
            references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
            
            try
            {
                references.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location));
            }
            catch
            {
                // Ignore if reference fails
            }

            return references.ToArray();
        }

        #endregion
    }
}
