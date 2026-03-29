using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using Microsoft.Extensions.Logging;

namespace RepoLens.Infrastructure.Services;

/// <summary>
/// Implementation of file analysis service for extracting code elements
/// </summary>
public class FileAnalysisService : IFileAnalysisService
{
    private readonly ILogger<FileAnalysisService> _logger;
    
    private static readonly Dictionary<string, string> ExtensionToLanguage = new()
    {
        { ".cs", "C#" },
        { ".js", "JavaScript" },
        { ".ts", "TypeScript" },
        { ".tsx", "TypeScript" },
        { ".jsx", "JavaScript" },
        { ".py", "Python" },
        { ".java", "Java" },
        { ".cpp", "C++" },
        { ".cc", "C++" },
        { ".cxx", "C++" },
        { ".c", "C" },
        { ".h", "C/C++" },
        { ".hpp", "C++" },
        { ".go", "Go" },
        { ".rs", "Rust" },
        { ".rb", "Ruby" },
        { ".php", "PHP" },
        { ".swift", "Swift" },
        { ".kt", "Kotlin" },
        { ".scala", "Scala" },
        { ".fs", "F#" },
        { ".vb", "VB.NET" },
        { ".sql", "SQL" },
        { ".r", "R" },
        { ".m", "MATLAB" },
        { ".pl", "Perl" }
    };

    public FileAnalysisService(ILogger<FileAnalysisService> logger)
    {
        _logger = logger;
    }

    public string[] SupportedLanguages => ExtensionToLanguage.Values.Distinct().ToArray();

    public bool IsSupported(string fileExtension)
    {
        return ExtensionToLanguage.ContainsKey(fileExtension.ToLowerInvariant());
    }

    public async Task<FileAnalysisResult> AnalyzeFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Starting analysis of file {FilePath}", filePath);

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var language = ExtensionToLanguage.GetValueOrDefault(extension, "Unknown");

            var result = new FileAnalysisResult
            {
                FilePath = filePath,
                Language = language,
                LineCount = CountLines(content),
                FileSize = Encoding.UTF8.GetByteCount(content),
                FileHash = ComputeFileHash(content),
                LastModified = File.Exists(filePath) ? File.GetLastWriteTime(filePath) : DateTime.UtcNow,
                Success = true
            };

            // Extract code elements based on language
            result.CodeElements = await ExtractCodeElementsAsync(content, language, cancellationToken);

            stopwatch.Stop();
            result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogDebug("Completed analysis of {FilePath} in {ElapsedMs}ms. Found {ElementCount} code elements", 
                filePath, stopwatch.ElapsedMilliseconds, result.CodeElements.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file {FilePath}", filePath);
            
            stopwatch.Stop();
            return new FileAnalysisResult
            {
                FilePath = filePath,
                Language = "Unknown",
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    private static int CountLines(string content)
    {
        if (string.IsNullOrEmpty(content)) return 0;
        return content.Count(c => c == '\n') + 1;
    }

    private static string ComputeFileHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private async Task<List<CodeElement>> ExtractCodeElementsAsync(string content, string language, CancellationToken cancellationToken)
    {
        var elements = new List<CodeElement>();

        try
        {
            switch (language.ToLowerInvariant())
            {
                case "c#":
                    elements.AddRange(ExtractCSharpElements(content));
                    break;
                case "javascript":
                case "typescript":
                    elements.AddRange(ExtractJavaScriptElements(content));
                    break;
                case "python":
                    elements.AddRange(ExtractPythonElements(content));
                    break;
                case "java":
                    elements.AddRange(ExtractJavaElements(content));
                    break;
                case "c++":
                case "c":
                case "c/c++":
                    elements.AddRange(ExtractCppElements(content));
                    break;
                default:
                    // Generic extraction for other languages
                    elements.AddRange(ExtractGenericElements(content, language));
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting elements for language {Language}", language);
            // Continue with generic extraction as fallback
            elements.AddRange(ExtractGenericElements(content, language));
        }

        return elements;
    }

    private List<CodeElement> ExtractCSharpElements(string content)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Namespace extraction
            var namespaceMatch = Regex.Match(line, @"namespace\s+([^\s{;]+)");
            if (namespaceMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Namespace,
                    Name = namespaceMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = "public"
                });
            }

            // Class extraction
            var classMatch = Regex.Match(line, @"(public|private|internal|protected)?\s*(static)?\s*(abstract|sealed)?\s*class\s+([^\s<:{]+)");
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Class,
                    Name = classMatch.Groups[4].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = string.IsNullOrEmpty(classMatch.Groups[1].Value) ? "internal" : classMatch.Groups[1].Value,
                    IsStatic = classMatch.Groups[2].Success
                });
            }

            // Interface extraction
            var interfaceMatch = Regex.Match(line, @"(public|private|internal|protected)?\s*interface\s+([^\s<:{]+)");
            if (interfaceMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Interface,
                    Name = interfaceMatch.Groups[2].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = string.IsNullOrEmpty(interfaceMatch.Groups[1].Value) ? "internal" : interfaceMatch.Groups[1].Value
                });
            }

            // Method extraction
            var methodMatch = Regex.Match(line, @"(public|private|internal|protected)?\s*(static)?\s*(async)?\s*(virtual|override|abstract)?\s*([^\s\(]+)\s+([^\s\(]+)\s*\(([^)]*)\)");
            if (methodMatch.Success && !line.Contains("class") && !line.Contains("interface"))
            {
                var methodName = methodMatch.Groups[6].Value;
                var returnType = methodMatch.Groups[5].Value;
                var parameters = methodMatch.Groups[7].Value;

                // Skip properties (they don't have parameters like methods)
                if (!line.Contains("get;") && !line.Contains("set;"))
                {
                    elements.Add(new CodeElement
                    {
                        ElementType = CodeElementType.Method,
                        Name = methodName,
                        StartLine = lineNumber,
                        EndLine = FindMethodEnd(lines, i),
                        AccessModifier = string.IsNullOrEmpty(methodMatch.Groups[1].Value) ? "private" : methodMatch.Groups[1].Value,
                        IsStatic = methodMatch.Groups[2].Success,
                        IsAsync = methodMatch.Groups[3].Success,
                        ReturnType = returnType,
                        Parameters = parameters
                    });
                }
            }

            // Property extraction
            var propertyMatch = Regex.Match(line, @"(public|private|internal|protected)?\s*(static)?\s*([^\s]+)\s+([^\s{]+)\s*\{\s*(get;?\s*set;?|get;|set;)");
            if (propertyMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Property,
                    Name = propertyMatch.Groups[4].Value,
                    StartLine = lineNumber,
                    EndLine = lineNumber,
                    AccessModifier = string.IsNullOrEmpty(propertyMatch.Groups[1].Value) ? "private" : propertyMatch.Groups[1].Value,
                    IsStatic = propertyMatch.Groups[2].Success,
                    ReturnType = propertyMatch.Groups[3].Value
                });
            }
        }

        return elements;
    }

    private List<CodeElement> ExtractJavaScriptElements(string content)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Function declarations
            var funcMatch = Regex.Match(line, @"function\s+([^\s\(]+)\s*\(([^)]*)\)");
            if (funcMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Function,
                    Name = funcMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = FindJavaScriptFunctionEnd(lines, i),
                    Parameters = funcMatch.Groups[2].Value,
                    AccessModifier = "public"
                });
            }

            // Arrow functions
            var arrowMatch = Regex.Match(line, @"const\s+([^\s=]+)\s*=\s*\([^)]*\)\s*=>");
            if (arrowMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Function,
                    Name = arrowMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = lineNumber,
                    AccessModifier = "public"
                });
            }

            // Class declarations
            var classMatch = Regex.Match(line, @"class\s+([^\s{]+)");
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Class,
                    Name = classMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = "public"
                });
            }
        }

        return elements;
    }

    private List<CodeElement> ExtractPythonElements(string content)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;
            var trimmedLine = line.Trim();

            // Class definitions
            var classMatch = Regex.Match(trimmedLine, @"class\s+([^\s\(:\)]+)");
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Class,
                    Name = classMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = FindPythonBlockEnd(lines, i),
                    AccessModifier = "public"
                });
            }

            // Function definitions
            var funcMatch = Regex.Match(trimmedLine, @"def\s+([^\s\(]+)\s*\(([^)]*)\)");
            if (funcMatch.Success)
            {
                var functionName = funcMatch.Groups[1].Value;
                var isPrivate = functionName.StartsWith("_");
                
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Method,
                    Name = functionName,
                    StartLine = lineNumber,
                    EndLine = FindPythonBlockEnd(lines, i),
                    Parameters = funcMatch.Groups[2].Value,
                    AccessModifier = isPrivate ? "private" : "public"
                });
            }
        }

        return elements;
    }

    private List<CodeElement> ExtractJavaElements(string content)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Class declarations
            var classMatch = Regex.Match(line, @"(public|private|protected)?\s*(static)?\s*(final|abstract)?\s*class\s+([^\s<{]+)");
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Class,
                    Name = classMatch.Groups[4].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = string.IsNullOrEmpty(classMatch.Groups[1].Value) ? "package" : classMatch.Groups[1].Value,
                    IsStatic = classMatch.Groups[2].Success
                });
            }

            // Method declarations
            var methodMatch = Regex.Match(line, @"(public|private|protected)?\s*(static)?\s*(final|abstract|synchronized)?\s*([^\s\(]+)\s+([^\s\(]+)\s*\(([^)]*)\)");
            if (methodMatch.Success && !line.Contains("class") && !line.Contains("interface"))
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Method,
                    Name = methodMatch.Groups[5].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = string.IsNullOrEmpty(methodMatch.Groups[1].Value) ? "package" : methodMatch.Groups[1].Value,
                    IsStatic = methodMatch.Groups[2].Success,
                    ReturnType = methodMatch.Groups[4].Value,
                    Parameters = methodMatch.Groups[6].Value
                });
            }
        }

        return elements;
    }

    private List<CodeElement> ExtractCppElements(string content)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Class declarations
            var classMatch = Regex.Match(line, @"class\s+([^\s{;:]+)");
            if (classMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Class,
                    Name = classMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = FindMatchingBrace(lines, i),
                    AccessModifier = "public"
                });
            }

            // Function declarations (simplified)
            var funcMatch = Regex.Match(line, @"([^\s\(]*)\s+([^\s\(]+)\s*\(([^)]*)\)\s*[{;]");
            if (funcMatch.Success && !line.Contains("class") && !line.Contains("struct") && !line.Contains("#"))
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Function,
                    Name = funcMatch.Groups[2].Value,
                    StartLine = lineNumber,
                    EndLine = line.Contains(";") ? lineNumber : FindMatchingBrace(lines, i),
                    ReturnType = funcMatch.Groups[1].Value,
                    Parameters = funcMatch.Groups[3].Value,
                    AccessModifier = "public"
                });
            }
        }

        return elements;
    }

    private List<CodeElement> ExtractGenericElements(string content, string language)
    {
        var elements = new List<CodeElement>();
        var lines = content.Split('\n');

        // Generic function/method extraction - looks for common patterns
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#") || line.StartsWith("*"))
                continue;

            // Look for function-like patterns
            var funcPattern = @"(\w+)\s*\([^)]*\)\s*[{:]";
            var funcMatch = Regex.Match(line, funcPattern);
            if (funcMatch.Success)
            {
                elements.Add(new CodeElement
                {
                    ElementType = CodeElementType.Function,
                    Name = funcMatch.Groups[1].Value,
                    StartLine = lineNumber,
                    EndLine = lineNumber,
                    AccessModifier = "public"
                });
            }
        }

        return elements;
    }

    private int FindMatchingBrace(string[] lines, int startIndex)
    {
        var openBraces = 0;
        var foundOpenBrace = false;

        for (int i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i];
            foreach (var char_ in line)
            {
                if (char_ == '{')
                {
                    openBraces++;
                    foundOpenBrace = true;
                }
                else if (char_ == '}')
                {
                    openBraces--;
                    if (foundOpenBrace && openBraces == 0)
                    {
                        return i + 1;
                    }
                }
            }
        }

        return startIndex + 1; // Fallback
    }

    private int FindMethodEnd(string[] lines, int startIndex)
    {
        // For methods, look for matching brace or semicolon
        for (int i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.EndsWith(";"))
            {
                return i + 1; // Abstract method or interface method
            }
        }
        return FindMatchingBrace(lines, startIndex);
    }

    private int FindJavaScriptFunctionEnd(string[] lines, int startIndex)
    {
        return FindMatchingBrace(lines, startIndex);
    }

    private int FindPythonBlockEnd(string[] lines, int startIndex)
    {
        if (startIndex >= lines.Length) return startIndex + 1;

        var initialIndentation = GetIndentationLevel(lines[startIndex]);
        
        for (int i = startIndex + 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var currentIndentation = GetIndentationLevel(line);
            if (currentIndentation <= initialIndentation)
            {
                return i;
            }
        }

        return lines.Length;
    }

    private int GetIndentationLevel(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == ' ')
                count++;
            else if (c == '\t')
                count += 4; // Assume tab = 4 spaces
            else
                break;
        }
        return count;
    }
}
