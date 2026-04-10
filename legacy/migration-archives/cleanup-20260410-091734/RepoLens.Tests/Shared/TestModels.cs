using RepoLens.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace RepoLens.Tests.Shared;

/// <summary>
/// Model representing a real repository file for testing
/// </summary>
public class RealRepositoryFile
{
    public string RelativePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public int LineCount { get; set; }
    public DateTime LastModified { get; set; }
}


/// <summary>
/// Repository analysis model for testing
/// </summary>
public class RepositoryAnalysis
{
    public int RepositoryId { get; set; }
    public int TotalFiles { get; set; }
    public int TotalLines { get; set; }
    public Dictionary<string, LanguageStats> Languages { get; set; } = new();
    public Dictionary<string, int> FileTypes { get; set; } = new();
}

/// <summary>
/// Language statistics for testing
/// </summary>
public class LanguageStats
{
    public string Name { get; set; } = "";
    public int FileCount { get; set; }
    public int LineCount { get; set; }
}

/// <summary>
/// Access modifier enum for testing (missing from RepoLens.Core)
/// </summary>
public enum AccessModifier
{
    Public, 
    Private, 
    Protected, 
    Internal, 
    ProtectedInternal, 
    PrivateProtected
}
