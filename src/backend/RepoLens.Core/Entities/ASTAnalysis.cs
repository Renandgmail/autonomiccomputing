using System.ComponentModel.DataAnnotations;

namespace RepoLens.Core.Entities
{
    public class ASTRepositoryAnalysis
    {
        [Key]
        public int Id { get; set; }
        public int RepositoryId { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public string Version { get; set; } = "1.0";
        public ASTMetrics Metrics { get; set; } = new();
        public List<ASTFileAnalysis> Files { get; set; } = new();
        public List<ASTDependency> Dependencies { get; set; } = new();
        public List<DuplicateCodeBlock> DuplicateBlocks { get; set; } = new();
        
        // Navigation properties
        public Repository Repository { get; set; } = null!;
    }

    public class ASTFileAnalysis
    {
        [Key]
        public int Id { get; set; }
        public int RepositoryAnalysisId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FileExtension => Path.GetExtension(FilePath);
        public string Language { get; set; } = string.Empty;
        public bool IsSupported { get; set; } = true;
        public long FileSizeBytes { get; set; }
        public int LineCount { get; set; }
        public DateTime LastModified { get; set; }
        
        // AST Analysis Results
        public ASTFileMetrics Metrics { get; set; } = new();
        public List<ASTStatement> Statements { get; set; } = new();
        public List<ASTClass> Classes { get; set; } = new();
        public List<ASTMethod> Methods { get; set; } = new();
        public List<ASTImport> Imports { get; set; } = new();
        public List<ASTExport> Exports { get; set; } = new();
        public List<ASTIssue> Issues { get; set; } = new();
        
        // Navigation properties
        public ASTRepositoryAnalysis RepositoryAnalysis { get; set; } = null!;
    }

    public class ASTStatement
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public string StatementId { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty; // 'assignment', 'condition', 'loop', 'call', 'return', etc.
        public int Line { get; set; }
        public int Column { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string CodeSnippet { get; set; } = string.Empty;
        public int Complexity { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public List<ASTIssue> Issues { get; set; } = new();
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }

    public class ASTClass
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string AccessModifier { get; set; } = string.Empty;
        public bool IsAbstract { get; set; }
        public bool IsInterface { get; set; }
        public string? BaseClass { get; set; }
        public List<string> Interfaces { get; set; } = new();
        public List<ASTMethod> Methods { get; set; } = new();
        public List<ASTProperty> Properties { get; set; } = new();
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }

    public class ASTMethod
    {
        [Key]
        public int Id { get; set; }
        public int? ClassId { get; set; }
        public int FileAnalysisId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string AccessModifier { get; set; } = string.Empty;
        public bool IsStatic { get; set; }
        public bool IsAsync { get; set; }
        public string ReturnType { get; set; } = string.Empty;
        public List<ASTParameter> Parameters { get; set; } = new();
        public int CyclomaticComplexity { get; set; }
        public int LinesOfCode { get; set; }
        public List<string> CalledMethods { get; set; } = new();
        public List<ASTIssue> Issues { get; set; } = new();
        
        // Navigation properties
        public ASTClass? Class { get; set; }
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }

    public class ASTProperty
    {
        [Key]
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string AccessModifier { get; set; } = string.Empty;
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
        public bool IsStatic { get; set; }
        public int Line { get; set; }
        
        // Navigation properties
        public ASTClass Class { get; set; } = null!;
    }

    public class ASTParameter
    {
        [Key]
        public int Id { get; set; }
        public int MethodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsOptional { get; set; }
        public string? DefaultValue { get; set; }
        public int Position { get; set; }
        
        // Navigation properties
        public ASTMethod Method { get; set; } = null!;
    }

    public class ASTImport
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public List<string> ImportedSymbols { get; set; } = new();
        public bool IsDefaultImport { get; set; }
        public bool IsNamespaceImport { get; set; }
        public int Line { get; set; }
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }

    public class ASTExport
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 'function', 'class', 'variable', 'type'
        public bool IsDefault { get; set; }
        public int Line { get; set; }
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }

    public class ASTDependency
    {
        [Key]
        public int Id { get; set; }
        public int RepositoryAnalysisId { get; set; }
        public string SourceFile { get; set; } = string.Empty;
        public string TargetFile { get; set; } = string.Empty;
        public string DependencyType { get; set; } = string.Empty; // 'import', 'call', 'inheritance', 'composition'
        public string? SourceElement { get; set; }
        public string? TargetElement { get; set; }
        public int Weight { get; set; } = 1; // Number of dependencies
        public bool IsCircular { get; set; }
        
        // Navigation properties
        public ASTRepositoryAnalysis RepositoryAnalysis { get; set; } = null!;
    }

    public class DuplicateCodeBlock
    {
        [Key]
        public int Id { get; set; }
        public int RepositoryAnalysisId { get; set; }
        public string GroupId { get; set; } = Guid.NewGuid().ToString();
        public string FilePath { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string CodeBlock { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty; // For matching duplicates
        public int SimilarityScore { get; set; } // 0-100 percentage
        public int LinesOfCode { get; set; }
        public string DuplicateType { get; set; } = string.Empty; // 'exact', 'similar', 'semantic'
        
        // Navigation properties
        public ASTRepositoryAnalysis RepositoryAnalysis { get; set; } = null!;
    }

    public class ASTIssue
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public int? StatementId { get; set; }
        public int? MethodId { get; set; }
        public string Severity { get; set; } = string.Empty; // 'critical', 'high', 'medium', 'low'
        public string IssueType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // 'security', 'performance', 'maintainability', 'reliability'
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public int Line { get; set; }
        public int? Column { get; set; }
        public string RuleId { get; set; } = string.Empty;
        public string? MoreInfoUrl { get; set; }
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
        public ASTStatement? Statement { get; set; }
        public ASTMethod? Method { get; set; }
    }

    public class ASTMetrics
    {
        [Key]
        public int Id { get; set; }
        public int RepositoryAnalysisId { get; set; }
        public int TotalFiles { get; set; }
        public int TotalLinesOfCode { get; set; }
        public int TotalStatements { get; set; }
        public int TotalClasses { get; set; }
        public int TotalMethods { get; set; }
        public double AverageComplexity { get; set; }
        public int MaxComplexity { get; set; }
        public int TotalIssues { get; set; }
        public int CriticalIssues { get; set; }
        public int HighIssues { get; set; }
        public int MediumIssues { get; set; }
        public int LowIssues { get; set; }
        public double CodeDuplicationPercentage { get; set; }
        public int CircularDependencies { get; set; }
        public double TechnicalDebtHours { get; set; }
        
        // Navigation properties
        public ASTRepositoryAnalysis RepositoryAnalysis { get; set; } = null!;
    }

    public class ASTFileMetrics
    {
        [Key]
        public int Id { get; set; }
        public int FileAnalysisId { get; set; }
        public int LinesOfCode { get; set; }
        public int Statements { get; set; }
        public int Classes { get; set; }
        public int Methods { get; set; }
        public double Complexity { get; set; }
        public int Issues { get; set; }
        public double QualityScore { get; set; } // 0-100
        public string QualityTrend { get; set; } = "flat"; // 'up', 'down', 'flat'
        
        // Navigation properties
        public ASTFileAnalysis FileAnalysis { get; set; } = null!;
    }
}
