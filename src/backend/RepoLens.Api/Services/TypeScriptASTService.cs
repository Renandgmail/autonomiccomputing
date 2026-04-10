using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using Microsoft.Extensions.Logging;

namespace RepoLens.Api.Services
{
    public class TypeScriptASTService : ITypeScriptASTService
    {
        private readonly ILogger<TypeScriptASTService> _logger;
        private static readonly string[] TypeScriptExtensions = { ".ts", ".tsx", ".js", ".jsx" };

        public TypeScriptASTService(ILogger<TypeScriptASTService> logger)
        {
            _logger = logger;
        }

        public async Task<ASTFileAnalysis> AnalyzeTypeScriptFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Analyzing TypeScript file: {FilePath}", filePath);

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
                    Language = GetLanguageFromExtension(fileInfo.Extension),
                    IsSupported = true,
                    FileSizeBytes = fileInfo.Length,
                    LineCount = sourceCode.Split('\n').Length,
                    LastModified = fileInfo.LastWriteTime
                };

                // Run TypeScript AST analysis using Node.js
                var astResult = await RunTypeScriptASTAnalysis(filePath, cancellationToken);
                
                if (astResult != null)
                {
                    // Convert Node.js AST result to our entities
                    analysis.Statements = ConvertStatements(astResult.Statements);
                    analysis.Classes = ConvertClasses(astResult.Classes);
                    analysis.Methods = ConvertMethods(astResult.Functions);
                    analysis.Imports = ConvertImports(astResult.Imports);
                    analysis.Exports = ConvertExports(astResult.Exports);

                    // Calculate metrics
                    analysis.Metrics = CalculateFileMetrics(analysis, sourceCode);

                    // Analyze code issues
                    analysis.Issues = await AnalyzeCodeIssuesAsync(filePath, analysis, cancellationToken);
                }

                _logger.LogInformation("Successfully analyzed TypeScript file: {FilePath} with {StatementCount} statements", 
                    filePath, analysis.Statements.Count);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing TypeScript file: {FilePath}", filePath);
                
                // Return basic analysis even on error
                var fileInfo = new FileInfo(filePath);
                return new ASTFileAnalysis
                {
                    FilePath = filePath,
                    Language = GetLanguageFromExtension(fileInfo.Extension),
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
                            Description = $"Failed to analyze file: {ex.Message}",
                            Recommendation = "Check file syntax and ensure it's valid TypeScript/JavaScript",
                            Line = 1,
                            RuleId = "AST001"
                        }
                    }
                };
            }
        }

        public Task<bool> IsTypeScriptFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return Task.FromResult(TypeScriptExtensions.Contains(extension));
        }

        public async Task<List<ASTIssue>> AnalyzeCodeIssuesAsync(string filePath, ASTFileAnalysis analysis, CancellationToken cancellationToken = default)
        {
            var issues = new List<ASTIssue>();

            try
            {
                // Basic static analysis rules
                var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);
                var lines = sourceCode.Split('\n');

                // Rule 1: Detect potential security issues
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // SQL injection patterns
                    if (line.Contains("SELECT") && line.Contains("+") && !line.Contains("//"))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "critical",
                            IssueType = "SQL Injection",
                            Category = "security",
                            Description = "Potential SQL injection vulnerability detected",
                            Recommendation = "Use parameterized queries or prepared statements",
                            Line = i + 1,
                            RuleId = "SEC001",
                            MoreInfoUrl = "https://owasp.org/www-community/attacks/SQL_Injection"
                        });
                    }

                    // Hardcoded passwords/keys
                    if (line.Contains("password") && line.Contains("=") && 
                        (line.Contains("\"") || line.Contains("'")) && 
                        !line.Contains("//"))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "high",
                            IssueType = "Hardcoded Credentials",
                            Category = "security",
                            Description = "Potential hardcoded password or API key detected",
                            Recommendation = "Use environment variables or secure configuration",
                            Line = i + 1,
                            RuleId = "SEC002",
                            MoreInfoUrl = "https://owasp.org/www-community/vulnerabilities/Use_of_hard-coded_password"
                        });
                    }

                    // eval() usage
                    if (line.Contains("eval("))
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "critical",
                            IssueType = "Code Injection",
                            Category = "security",
                            Description = "Use of eval() function detected - potential code injection vulnerability",
                            Recommendation = "Avoid eval() - use safer alternatives like JSON.parse() for data",
                            Line = i + 1,
                            RuleId = "SEC003"
                        });
                    }
                }

                // Rule 2: Performance issues
                foreach (var method in analysis.Methods)
                {
                    // High cyclomatic complexity
                    if (method.CyclomaticComplexity > 10)
                    {
                        issues.Add(new ASTIssue
                        {
                            MethodId = method.Id,
                            Severity = method.CyclomaticComplexity > 15 ? "high" : "medium",
                            IssueType = "High Complexity",
                            Category = "maintainability",
                            Description = $"Method '{method.Name}' has high cyclomatic complexity ({method.CyclomaticComplexity})",
                            Recommendation = "Consider breaking down into smaller methods",
                            Line = method.StartLine,
                            RuleId = "PERF001"
                        });
                    }

                    // Long methods
                    if (method.LinesOfCode > 50)
                    {
                        issues.Add(new ASTIssue
                        {
                            MethodId = method.Id,
                            Severity = "medium",
                            IssueType = "Long Method",
                            Category = "maintainability",
                            Description = $"Method '{method.Name}' is too long ({method.LinesOfCode} lines)",
                            Recommendation = "Consider extracting smaller, focused methods",
                            Line = method.StartLine,
                            RuleId = "PERF002"
                        });
                    }
                }

                // Rule 3: Code quality issues
                if (analysis.Statements.Count > 0)
                {
                    var todoComments = lines.Where((line, index) => 
                        line.ToLower().Contains("todo") || line.ToLower().Contains("fixme"))
                        .Select((line, index) => index + 1);

                    foreach (var lineNumber in todoComments)
                    {
                        issues.Add(new ASTIssue
                        {
                            Severity = "low",
                            IssueType = "TODO Comment",
                            Category = "maintainability",
                            Description = "TODO or FIXME comment found",
                            Recommendation = "Address the TODO item or create a proper issue",
                            Line = lineNumber,
                            RuleId = "QUAL001"
                        });
                    }
                }

                _logger.LogInformation("Found {IssueCount} code issues in {FilePath}", issues.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing code issues for file: {FilePath}", filePath);
            }

            return issues;
        }

        private async Task<TypeScriptASTResult?> RunTypeScriptASTAnalysis(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                // Create a temporary JavaScript file for analysis
                var tempScriptPath = Path.GetTempFileName() + ".js";
                var nodeScript = $$"""
const ts = require('typescript');
const fs = require('fs');

function analyzeFile(filePath) {
    try {
        const sourceCode = fs.readFileSync(filePath, 'utf8');
        const sourceFile = ts.createSourceFile(
            filePath,
            sourceCode,
            ts.ScriptTarget.Latest,
            true,
            filePath.endsWith('.tsx') || filePath.endsWith('.jsx') ? ts.ScriptKind.TSX : ts.ScriptKind.TS
        );

        const analysis = {
            statements: [],
            functions: [],
            classes: [],
            imports: [],
            exports: []
        };

        function visit(node, depth = 0) {
            const line = sourceFile.getLineAndCharacterOfPosition(node.getStart()).line + 1;
            const column = sourceFile.getLineAndCharacterOfPosition(node.getStart()).character + 1;
            
            // Extract statements
            if (ts.isStatement(node)) {
                analysis.statements.push({
                    type: ts.SyntaxKind[node.kind],
                    line: line,
                    column: column,
                    start: node.getStart(),
                    end: node.getEnd(),
                    text: node.getText(sourceFile).substring(0, 200),
                    complexity: calculateComplexity(node)
                });
            }

            // Extract functions
            if (ts.isFunctionDeclaration(node) || ts.isMethodDeclaration(node) || ts.isArrowFunction(node)) {
                const name = node.name ? node.name.getText(sourceFile) : '<anonymous>';
                analysis.functions.push({
                    name: name,
                    startLine: line,
                    endLine: sourceFile.getLineAndCharacterOfPosition(node.getEnd()).line + 1,
                    signature: node.getText(sourceFile).split('{')[0] || node.getText(sourceFile).substring(0, 100),
                    isAsync: !!(node.modifiers && node.modifiers.some(m => m.kind === ts.SyntaxKind.AsyncKeyword)),
                    complexity: calculateComplexity(node),
                    linesOfCode: sourceFile.getLineAndCharacterOfPosition(node.getEnd()).line - line + 1
                });
            }

            // Extract classes
            if (ts.isClassDeclaration(node)) {
                const name = node.name ? node.name.getText(sourceFile) : '<anonymous>';
                analysis.classes.push({
                    name: name,
                    startLine: line,
                    endLine: sourceFile.getLineAndCharacterOfPosition(node.getEnd()).line + 1,
                    isAbstract: !!(node.modifiers && node.modifiers.some(m => m.kind === ts.SyntaxKind.AbstractKeyword))
                });
            }

            // Extract imports
            if (ts.isImportDeclaration(node)) {
                const moduleSpecifier = node.moduleSpecifier.getText(sourceFile).replace(/['"]/g, '');
                analysis.imports.push({
                    module: moduleSpecifier,
                    line: line,
                    isDefaultImport: !!(node.importClause && node.importClause.name),
                    isNamespaceImport: !!(node.importClause && node.importClause.namedBindings && 
                        ts.isNamespaceImport(node.importClause.namedBindings))
                });
            }

            // Extract exports
            if (ts.isExportDeclaration(node) || ts.isExportAssignment(node)) {
                analysis.exports.push({
                    name: node.getText(sourceFile).substring(0, 50),
                    line: line,
                    isDefault: ts.isExportAssignment(node)
                });
            }

            ts.forEachChild(node, child => visit(child, depth + 1));
        }

        function calculateComplexity(node) {
            let complexity = 1;
            
            function countComplexity(n) {
                switch (n.kind) {
                    case ts.SyntaxKind.IfStatement:
                    case ts.SyntaxKind.WhileStatement:
                    case ts.SyntaxKind.ForStatement:
                    case ts.SyntaxKind.ForInStatement:
                    case ts.SyntaxKind.ForOfStatement:
                    case ts.SyntaxKind.DoStatement:
                    case ts.SyntaxKind.SwitchStatement:
                    case ts.SyntaxKind.CatchClause:
                    case ts.SyntaxKind.ConditionalExpression:
                        complexity++;
                        break;
                    case ts.SyntaxKind.CaseClause:
                        if (n.parent && n.parent.kind === ts.SyntaxKind.SwitchStatement) {
                            complexity++;
                        }
                        break;
                }
                
                ts.forEachChild(n, countComplexity);
            }
            
            countComplexity(node);
            return complexity;
        }

        visit(sourceFile);
        return analysis;
    } catch (error) {
        return { error: error.message };
    }
}

const filePath = process.argv[2];
const result = analyzeFile(filePath);
console.log(JSON.stringify(result, null, 2));
""";

                await File.WriteAllTextAsync(tempScriptPath, nodeScript, cancellationToken);

                try
                {
                    // Execute Node.js script with file path as argument
                    var processInfo = new ProcessStartInfo("node", $"\"{tempScriptPath}\" \"{filePath}\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processInfo);
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start Node.js process");
                    }

                    // Read result
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync(cancellationToken);

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Node.js process failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(output))
                    {
                        _logger.LogWarning("Node.js process returned empty output for file: {FilePath}", filePath);
                        return null;
                    }

                    // Parse JSON result
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    var result = JsonSerializer.Deserialize<TypeScriptASTResult>(output, options);
                    return result;
                }
                finally
                {
                    // Clean up temporary file
                    try
                    {
                        if (File.Exists(tempScriptPath))
                        {
                            File.Delete(tempScriptPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary script file: {TempPath}", tempScriptPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running TypeScript AST analysis for file: {FilePath}", filePath);
                return null;
            }
        }

        private List<ASTStatement> ConvertStatements(List<TypeScriptStatement>? statements)
        {
            if (statements == null) return new List<ASTStatement>();

            return statements.Select(s => new ASTStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                Type = s.Type ?? "unknown",
                Line = s.Line,
                Column = s.Column,
                StartPosition = s.Start,
                EndPosition = s.End,
                CodeSnippet = s.Text ?? "",
                Complexity = s.Complexity,
                Dependencies = new List<string>()
            }).ToList();
        }

        private List<ASTClass> ConvertClasses(List<TypeScriptClass>? classes)
        {
            if (classes == null) return new List<ASTClass>();

            return classes.Select(c => new ASTClass
            {
                Name = c.Name ?? "unknown",
                FullName = c.Name ?? "unknown",
                StartLine = c.StartLine,
                EndLine = c.EndLine,
                IsAbstract = c.IsAbstract,
                IsInterface = false, // TODO: Detect interfaces
                Methods = new List<ASTMethod>(),
                Properties = new List<ASTProperty>()
            }).ToList();
        }

        private List<ASTMethod> ConvertMethods(List<TypeScriptFunction>? functions)
        {
            if (functions == null) return new List<ASTMethod>();

            return functions.Select(f => new ASTMethod
            {
                Name = f.Name ?? "unknown",
                Signature = f.Signature ?? "",
                StartLine = f.StartLine,
                EndLine = f.EndLine,
                IsAsync = f.IsAsync,
                IsStatic = false, // TODO: Detect static methods
                CyclomaticComplexity = f.Complexity,
                LinesOfCode = f.LinesOfCode,
                CalledMethods = new List<string>(),
                Parameters = new List<ASTParameter>()
            }).ToList();
        }

        private List<ASTImport> ConvertImports(List<TypeScriptImport>? imports)
        {
            if (imports == null) return new List<ASTImport>();

            return imports.Select(i => new ASTImport
            {
                Module = i.Module ?? "",
                Line = i.Line,
                IsDefaultImport = i.IsDefaultImport,
                IsNamespaceImport = i.IsNamespaceImport,
                ImportedSymbols = new List<string>()
            }).ToList();
        }

        private List<ASTExport> ConvertExports(List<TypeScriptExport>? exports)
        {
            if (exports == null) return new List<ASTExport>();

            return exports.Select(e => new ASTExport
            {
                Name = e.Name ?? "",
                Type = "unknown", // TODO: Detect export type
                IsDefault = e.IsDefault,
                Line = e.Line
            }).ToList();
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
            if (complexity > 10) score -= (complexity - 10) * 5;
            else if (complexity > 5) score -= (complexity - 5) * 2;

            // Reduce score based on issues
            foreach (var issue in analysis.Issues)
            {
                score -= issue.Severity switch
                {
                    "critical" => 20,
                    "high" => 15,
                    "medium" => 10,
                    "low" => 5,
                    _ => 5
                };
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private string GetLanguageFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".ts" => "typescript",
                ".tsx" => "typescript",
                ".js" => "javascript",
                ".jsx" => "javascript",
                _ => "unknown"
            };
        }
    }

    // Internal classes for TypeScript AST result parsing
    internal class TypeScriptASTResult
    {
        public List<TypeScriptStatement>? Statements { get; set; }
        public List<TypeScriptFunction>? Functions { get; set; }
        public List<TypeScriptClass>? Classes { get; set; }
        public List<TypeScriptImport>? Imports { get; set; }
        public List<TypeScriptExport>? Exports { get; set; }
        public string? Error { get; set; }
    }

    internal class TypeScriptStatement
    {
        public string? Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string? Text { get; set; }
        public int Complexity { get; set; }
    }

    internal class TypeScriptFunction
    {
        public string? Name { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string? Signature { get; set; }
        public bool IsAsync { get; set; }
        public int Complexity { get; set; }
        public int LinesOfCode { get; set; }
    }

    internal class TypeScriptClass
    {
        public string? Name { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public bool IsAbstract { get; set; }
    }

    internal class TypeScriptImport
    {
        public string? Module { get; set; }
        public int Line { get; set; }
        public bool IsDefaultImport { get; set; }
        public bool IsNamespaceImport { get; set; }
    }

    internal class TypeScriptExport
    {
        public string? Name { get; set; }
        public int Line { get; set; }
        public bool IsDefault { get; set; }
    }
}
