using RepoLens.Core.Entities;

namespace RepoLens.Core.Services;

/// <summary>
/// Service interface for extracting and managing domain-specific vocabulary from codebases
/// </summary>
public interface IVocabularyExtractionService
{
    /// <summary>
    /// Extract vocabulary from a repository's codebase
    /// </summary>
    /// <param name="repositoryId">Repository to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vocabulary extraction result</returns>
    Task<VocabularyExtractionResult> ExtractVocabularyAsync(int repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vocabulary terms for a repository with filtering and pagination
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="filter">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered vocabulary terms</returns>
    Task<VocabularyQueryResult> GetVocabularyAsync(int repositoryId, VocabularyFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get concept relationships and semantic mappings
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="termId">Central term ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Concept relationship graph</returns>
    Task<ConceptRelationshipGraph> GetConceptRelationshipsAsync(int repositoryId, int termId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update vocabulary based on code changes
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="changedFiles">List of files that changed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vocabulary update result</returns>
    Task<VocabularyUpdateResult> UpdateVocabularyAsync(int repositoryId, List<string> changedFiles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business-technical term mapping for domain understanding
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business term mappings</returns>
    Task<BusinessTermMapping> GetBusinessTermMappingAsync(int repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search vocabulary terms by similarity or pattern
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="searchTerm">Term to search for</param>
    /// <param name="searchType">Type of search to perform</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Similar vocabulary terms</returns>
    Task<List<VocabularyTerm>> SearchSimilarTermsAsync(int repositoryId, string searchTerm, VocabularySearchType searchType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of vocabulary extraction operation
/// </summary>
public class VocabularyExtractionResult
{
    public int RepositoryId { get; set; }
    public int TotalTermsExtracted { get; set; }
    public int BusinessTermsIdentified { get; set; }
    public int TechnicalTermsIdentified { get; set; }
    public int ConceptRelationshipsFound { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    public double RelevanceScore { get; set; }
    public List<string> DominantDomains { get; set; } = new();
    public List<VocabularyTerm> HighValueTerms { get; set; } = new();
    public string? ErrorMessage { get; set; }
    
    // Properties for backward compatibility with tests
    public int TotalTerms { get; set; }
    public int BusinessTermsCount { get; set; }
    public int TechnicalTermsCount { get; set; }
    public int DomainSpecificCount { get; set; }
}

/// <summary>
/// Filter criteria for vocabulary queries
/// </summary>
public class VocabularyFilter
{
    public VocabularyTermType? TermType { get; set; }
    public VocabularySource? Source { get; set; }
    public double? MinimumRelevanceScore { get; set; }
    public int? MinimumFrequency { get; set; }
    public List<string>? Domains { get; set; }
    public List<string>? Languages { get; set; }
    public string? SearchText { get; set; }
    public VocabularySortOrder SortBy { get; set; } = VocabularySortOrder.Relevance;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Result of vocabulary query with pagination
/// </summary>
public class VocabularyQueryResult
{
    public List<VocabularyTerm> Terms { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }
    public VocabularyStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Concept relationship graph for semantic understanding
/// </summary>
public class ConceptRelationshipGraph
{
    public VocabularyTerm CentralTerm { get; set; } = null!;
    public List<ConceptRelationship> Relationships { get; set; } = new();
    public List<VocabularyTerm> RelatedTerms { get; set; } = new();
    public Dictionary<string, List<string>> ConceptClusters { get; set; } = new();
    public double GraphDensity { get; set; }
    public int MaxDepth { get; set; }
}

/// <summary>
/// Represents a relationship between two concepts
/// </summary>
public class ConceptRelationship
{
    public int FromTermId { get; set; }
    public int ToTermId { get; set; }
    public ConceptRelationshipType RelationshipType { get; set; }
    public double Strength { get; set; }
    public int Frequency { get; set; }
    public List<string> ContextExamples { get; set; } = new();
}

/// <summary>
/// Business-technical term mapping for domain understanding
/// </summary>
public class BusinessTermMapping
{
    public Dictionary<string, List<string>> BusinessToTechnical { get; set; } = new();
    public Dictionary<string, List<string>> TechnicalToBusiness { get; set; } = new();
    public List<DomainConcept> DomainConcepts { get; set; } = new();
    public List<BusinessProcess> IdentifiedProcesses { get; set; } = new();
    public double MappingConfidence { get; set; }
}

/// <summary>
/// Represents a business domain concept
/// </summary>
public class DomainConcept
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> TechnicalImplementations { get; set; } = new();
    public List<string> BusinessPurposes { get; set; } = new();
    public double Confidence { get; set; }
}

/// <summary>
/// Represents an identified business process
/// </summary>
public class BusinessProcess
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public List<VocabularyTerm> RelatedTerms { get; set; } = new();
    public List<string> CodeAreas { get; set; } = new();
}

/// <summary>
/// Result of vocabulary update operation
/// </summary>
public class VocabularyUpdateResult
{
    public int RepositoryId { get; set; }
    public int FilesProcessed { get; set; }
    public int NewTermsAdded { get; set; }
    public int TermsUpdated { get; set; }
    public int TermsRemoved { get; set; }
    public List<VocabularyTerm> NewHighValueTerms { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Statistics about vocabulary in a repository
/// </summary>
public class VocabularyStatistics
{
    public int TotalTerms { get; set; }
    public int BusinessTerms { get; set; }
    public int TechnicalTerms { get; set; }
    public int DomainSpecificTerms { get; set; }
    public Dictionary<string, int> TermsByLanguage { get; set; } = new();
    public Dictionary<string, int> TermsByDomain { get; set; } = new();
    public Dictionary<VocabularySource, int> TermsBySource { get; set; } = new();
    public double AverageRelevanceScore { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Types of concept relationships
/// </summary>
public enum ConceptRelationshipType
{
    SimilarTo = 1,
    PartOf = 2,
    ImplementedBy = 3,
    DependsOn = 4,
    RelatedTo = 5,
    OppositeOf = 6,
    IsA = 7,
    UsedWith = 8,
    ReplacedBy = 9,
    EvolutionOf = 10
}

/// <summary>
/// Types of vocabulary search
/// </summary>
public enum VocabularySearchType
{
    ExactMatch = 1,
    SimilarMeaning = 2,
    SimilarSpelling = 3,
    SameContext = 4,
    RelatedConcept = 5,
    SameDomain = 6
}

/// <summary>
/// Sort orders for vocabulary results
/// </summary>
public enum VocabularySortOrder
{
    Relevance = 1,
    Frequency = 2,
    Alphabetical = 3,
    RecentlyAdded = 4,
    Domain = 5,
    TermType = 6
}
