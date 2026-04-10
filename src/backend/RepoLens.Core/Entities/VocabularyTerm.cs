namespace RepoLens.Core.Entities;

/// <summary>
/// Represents a domain-specific vocabulary term extracted from code
/// </summary>
public class VocabularyTerm
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string Term { get; set; } = string.Empty;
    public string NormalizedTerm { get; set; } = string.Empty;
    public VocabularyTermType TermType { get; set; }
    public VocabularySource Source { get; set; }
    public string Language { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double RelevanceScore { get; set; }
    public double BusinessRelevance { get; set; }
    public double TechnicalRelevance { get; set; }
    public string? Context { get; set; }
    public string? Definition { get; set; }
    public string? Domain { get; set; }
    public List<string> Synonyms { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public List<string> UsageExamples { get; set; } = new();
    public List<VocabularyLocation> Locations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Repository Repository { get; set; } = null!;
    public List<VocabularyTermRelationship> FromRelationships { get; set; } = new();
    public List<VocabularyTermRelationship> ToRelationships { get; set; } = new();
}

/// <summary>
/// Represents the location where a vocabulary term was found
/// </summary>
public class VocabularyLocation
{
    public int Id { get; set; }
    public int VocabularyTermId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int StartColumn { get; set; }
    public int EndColumn { get; set; }
    public VocabularyLocationContext ContextType { get; set; }
    public string ContextDescription { get; set; } = string.Empty;
    public string? SurroundingCode { get; set; }
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public VocabularyTerm VocabularyTerm { get; set; } = null!;
}

/// <summary>
/// Represents relationships between vocabulary terms
/// </summary>
public class VocabularyTermRelationship
{
    public int Id { get; set; }
    public int FromTermId { get; set; }
    public int ToTermId { get; set; }
    public VocabularyRelationshipType RelationshipType { get; set; }
    public double Strength { get; set; }
    public int CoOccurrenceCount { get; set; }
    public string? Context { get; set; }
    public List<string> Evidence { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public VocabularyTerm FromTerm { get; set; } = null!;
    public VocabularyTerm ToTerm { get; set; } = null!;
}

/// <summary>
/// Represents a business concept extracted from code
/// </summary>
public class BusinessConcept
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public BusinessConceptType ConceptType { get; set; }
    public double Confidence { get; set; }
    public List<string> Keywords { get; set; } = new();
    public List<string> TechnicalMappings { get; set; } = new();
    public List<string> BusinessPurposes { get; set; } = new();
    public List<int> RelatedTermIds { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Repository Repository { get; set; } = null!;
}

/// <summary>
/// Represents domain-specific vocabulary statistics
/// </summary>
public class VocabularyStats
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public int TotalTerms { get; set; }
    public int UniqueTerms { get; set; }
    public int BusinessTerms { get; set; }
    public int TechnicalTerms { get; set; }
    public int DomainSpecificTerms { get; set; }
    public double AverageRelevanceScore { get; set; }
    public double VocabularyDensity { get; set; }
    public double BusinessTechnicalRatio { get; set; }
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public Dictionary<string, int> DomainDistribution { get; set; } = new();
    public Dictionary<VocabularySource, int> SourceDistribution { get; set; } = new();
    public List<string> TopDomains { get; set; } = new();
    public List<string> EmergingTerms { get; set; } = new();
    public List<string> DeprecatedTerms { get; set; } = new();

    // Navigation properties
    public Repository Repository { get; set; } = null!;
}

/// <summary>
/// Types of vocabulary terms
/// </summary>
public enum VocabularyTermType
{
    Unknown = 0,
    BusinessTerm = 1,
    TechnicalTerm = 2,
    DomainSpecific = 3,
    MethodName = 4,
    ClassName = 5,
    VariableName = 6,
    PropertyName = 7,
    ParameterName = 8,
    ConstantName = 9,
    EnumValue = 10,
    InterfaceName = 11,
    NamespaceName = 12,
    CommentKeyword = 13,
    DocumentationTerm = 14,
    ConfigurationKey = 15,
    DatabaseField = 16,
    ApiEndpoint = 17,
    BusinessProcess = 18,
    BusinessEntity = 19,
    BusinessRule = 20
}

/// <summary>
/// Sources where vocabulary terms can be found
/// </summary>
public enum VocabularySource
{
    Unknown = 0,
    SourceCode = 1,
    Comments = 2,
    Documentation = 3,
    VariableNames = 4,
    MethodNames = 5,
    ClassNames = 6,
    PropertyNames = 7,
    ParameterNames = 8,
    ConstantNames = 9,
    EnumNames = 10,
    InterfaceNames = 11,
    NamespaceNames = 12,
    ConfigurationFiles = 13,
    DatabaseSchema = 14,
    ApiDocumentation = 15,
    TestNames = 16,
    LogMessages = 17,
    ErrorMessages = 18,
    UserInterface = 19,
    Requirements = 20
}

/// <summary>
/// Context types for vocabulary locations
/// </summary>
public enum VocabularyLocationContext
{
    Unknown = 0,
    ClassDeclaration = 1,
    MethodDeclaration = 2,
    PropertyDeclaration = 3,
    VariableDeclaration = 4,
    ParameterDeclaration = 5,
    Comment = 6,
    Documentation = 7,
    StringLiteral = 8,
    Annotation = 9,
    Configuration = 10,
    DatabaseQuery = 11,
    ApiCall = 12,
    TestMethod = 13,
    ErrorHandling = 14,
    LoggingStatement = 15,
    BusinessLogic = 16,
    DataAccess = 17,
    UserInterface = 18,
    Validation = 19,
    Security = 20
}

/// <summary>
/// Types of relationships between vocabulary terms
/// </summary>
public enum VocabularyRelationshipType
{
    Unknown = 0,
    Synonym = 1,
    Antonym = 2,
    HypernymOf = 3,     // Is a broader term for
    HyponymOf = 4,      // Is a more specific term for
    MeronymOf = 5,      // Is part of
    HolonymOf = 6,      // Contains
    SimilarTo = 7,
    RelatedTo = 8,
    DependsOn = 9,
    ImplementedBy = 10,
    ConfiguredBy = 11,
    UsedWith = 12,
    ReplacedBy = 13,
    EvolutionOf = 14,
    OppositeOf = 15,
    CausedBy = 16,
    ResultsIn = 17,
    CoOccursWith = 18,
    DefinedAs = 19,
    ExampleOf = 20
}

/// <summary>
/// Types of business concepts
/// </summary>
public enum BusinessConceptType
{
    Unknown = 0,
    Entity = 1,
    Process = 2,
    Rule = 3,
    Event = 4,
    Service = 5,
    Workflow = 6,
    Policy = 7,
    Constraint = 8,
    Requirement = 9,
    Feature = 10,
    Domain = 11,
    Subdomain = 12,
    Capability = 13,
    Resource = 14,
    Actor = 15,
    Goal = 16,
    Objective = 17,
    Metric = 18,
    KPI = 19,
    Value = 20
}
