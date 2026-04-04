using System.ComponentModel.DataAnnotations;

namespace RepoLens.Core.Entities;

public class CodeElement
{
    public int Id { get; set; }
    public int FileId { get; set; } // References RepositoryFile.Id
    
    public CodeElementType ElementType { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? FullyQualifiedName { get; set; }
    
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    
    public string? Signature { get; set; }
    
    [StringLength(20)]
    public string? AccessModifier { get; set; }
    
    public bool IsStatic { get; set; } = false;
    public bool IsAsync { get; set; } = false;
    
    [StringLength(200)]
    public string? ReturnType { get; set; }
    
    public string? Parameters { get; set; } // JSON as TEXT for PostgreSQL
    public string? Documentation { get; set; }
    public int? Complexity { get; set; }
    
    // Additional properties for search and analysis
    public string? File { get; set; } // File path for search results
    public string? FullContent { get; set; } // Full content for analysis
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual RepositoryFile RepositoryFile { get; set; } = null!;
}

// Follow existing enum pattern from RepositoryStatus
public enum CodeElementType
{
    Namespace = 1,
    Class = 2,
    Interface = 3,
    Struct = 4,
    Enum = 5,
    Method = 6,
    Property = 7,
    Field = 8,
    Constructor = 9,
    Event = 10,
    Delegate = 11,
    Variable = 12,
    Function = 13, // For non-OOP languages
    Module = 14,   // For languages like Python
    Component = 15, // For frontend frameworks
    Hook = 16      // For React hooks
}
