using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;
using RepoLens.Infrastructure;

namespace RepoLens.Infrastructure.Services;

/// <summary>
/// Advanced vocabulary extraction service with domain-specific intelligence
/// </summary>
public class VocabularyExtractionService : IVocabularyExtractionService
{
    private readonly RepoLensDbContext _context;
    private readonly ILogger<VocabularyExtractionService> _logger;

    // Domain-specific term patterns and classifications
    private static readonly Dictionary<string, VocabularyTermType> DomainPatterns = new()
    {
        // Business terms
        { @"\b(customer|client|user|account|order|payment|invoice|subscription|billing)\b", VocabularyTermType.BusinessTerm },
        { @"\b(product|service|feature|requirement|business|domain|process|workflow)\b", VocabularyTermType.BusinessTerm },
        { @"\b(policy|rule|regulation|compliance|audit|governance|strategy)\b", VocabularyTermType.BusinessTerm },
        { @"\b(revenue|profit|cost|budget|pricing|discount|refund|commission)\b", VocabularyTermType.BusinessTerm },
        { @"\b(marketing|sales|support|operations|finance|hr|legal|procurement)\b", VocabularyTermType.BusinessTerm },
        
        // Technical terms
        { @"\b(api|service|controller|repository|factory|builder|adapter|proxy)\b", VocabularyTermType.TechnicalTerm },
        { @"\b(database|cache|queue|stream|pipeline|endpoint|middleware|auth)\b", VocabularyTermType.TechnicalTerm },
        { @"\b(algorithm|pattern|framework|library|package|module|component)\b", VocabularyTermType.TechnicalTerm },
        { @"\b(performance|scalability|security|reliability|availability|monitoring)\b", VocabularyTermType.TechnicalTerm },
        { @"\b(deployment|configuration|environment|infrastructure|devops|cicd)\b", VocabularyTermType.TechnicalTerm },
        
        // Domain-specific patterns
        { @"\b(healthcare|patient|medical|diagnosis|treatment|prescription|clinic)\b", VocabularyTermType.DomainSpecific },
        { @"\b(financial|banking|credit|debit|loan|mortgage|investment|trading)\b", VocabularyTermType.DomainSpecific },
        { @"\b(ecommerce|shopping|cart|checkout|shipping|inventory|catalog)\b", VocabularyTermType.DomainSpecific },
        { @"\b(education|student|course|curriculum|assessment|grade|learning)\b", VocabularyTermType.DomainSpecific },
        { @"\b(manufacturing|supply|logistics|warehouse|production|quality)\b", VocabularyTermType.DomainSpecific }
    };

    // Business process indicators
    private static readonly HashSet<string> BusinessProcessIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "create", "update", "delete", "process", "handle", "manage", "execute", "perform",
        "validate", "verify", "approve", "reject", "submit", "cancel", "complete", "initialize",
        "calculate", "compute", "generate", "transform", "convert", "migrate", "import", "export",
        "notify", "alert", "send", "receive", "publish", "subscribe", "schedule", "trigger"
    };

    // Technical implementation indicators
    private static readonly HashSet<string> TechnicalIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "impl", "implementation", "service", "provider", "factory", "builder", "helper", "utility",
        "manager", "handler", "processor", "controller", "repository", "dao", "dto", "entity",
        "model", "view", "component", "module", "package", "namespace", "interface", "abstract",
        "base", "core", "common", "shared", "generic", "custom", "extension", "plugin"
    };

    public VocabularyExtractionService(RepoLensDbContext context, ILogger<VocabularyExtractionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VocabularyExtractionResult> ExtractVocabularyAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting vocabulary extraction for repository {RepositoryId}", repositoryId);

        try
        {
            var repository = await _context.Repositories
                .Include(r => r.RepositoryFiles)
                    .ThenInclude(f => f.CodeElements)
                .FirstOrDefaultAsync(r => r.Id == repositoryId, cancellationToken);

            if (repository == null)
            {
                throw new ArgumentException($"Repository with ID {repositoryId} not found");
            }

            // Clear existing vocabulary for this repository
            var existingTerms = await _context.VocabularyTerms
                .Where(vt => vt.RepositoryId == repositoryId)
                .ToListAsync(cancellationToken);
            _context.VocabularyTerms.RemoveRange(existingTerms);

            // Extract vocabulary from all sources
            var extractedTerms = new List<VocabularyTerm>();
            var termFrequency = new Dictionary<string, int>();
            var termContexts = new Dictionary<string, List<VocabularyLocation>>();

            // Process code elements (classes, methods, properties) from repository files
            foreach (var file in repository.RepositoryFiles)
            {
                // Extract from file name and path
                var fileNameTerms = ExtractFromIdentifier(file.FileName, file.FilePath, 1, VocabularySource.SourceCode);
                extractedTerms.AddRange(fileNameTerms);

                foreach (var element in file.CodeElements)
                {
                    var elementTerms = ExtractFromCodeElement(element, file.FilePath, termFrequency, termContexts);
                    extractedTerms.AddRange(elementTerms);
                }
            }

            // Normalize and score terms
            var normalizedTerms = await NormalizeAndScoreTermsAsync(extractedTerms, termFrequency, repositoryId);

            // Build relationships between terms
            var relationships = BuildTermRelationships(normalizedTerms);

            // Identify business concepts
            var businessConcepts = IdentifyBusinessConcepts(normalizedTerms);

            // Save to database
            await _context.VocabularyTerms.AddRangeAsync(normalizedTerms, cancellationToken);
            await _context.VocabularyTermRelationships.AddRangeAsync(relationships, cancellationToken);
            await _context.BusinessConcepts.AddRangeAsync(businessConcepts, cancellationToken);

            // Update repository vocabulary stats
            var stats = CalculateVocabularyStats(normalizedTerms, businessConcepts, repositoryId);
            await _context.VocabularyStats.AddAsync(stats, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var processingTime = DateTime.UtcNow - startTime;
            var businessTerms = normalizedTerms.Count(t => t.TermType == VocabularyTermType.BusinessTerm || 
                                                          t.TermType == VocabularyTermType.BusinessProcess || 
                                                          t.TermType == VocabularyTermType.BusinessEntity);
            var technicalTerms = normalizedTerms.Count(t => t.TermType == VocabularyTermType.TechnicalTerm);

            var result = new VocabularyExtractionResult
            {
                RepositoryId = repositoryId,
                TotalTermsExtracted = normalizedTerms.Count,
                BusinessTermsIdentified = businessTerms,
                TechnicalTermsIdentified = technicalTerms,
                ConceptRelationshipsFound = relationships.Count,
                ProcessingTime = processingTime,
                RelevanceScore = normalizedTerms.Count > 0 ? normalizedTerms.Average(t => t.RelevanceScore) : 0,
                DominantDomains = stats.TopDomains.Take(5).ToList(),
                HighValueTerms = normalizedTerms
                    .Where(t => t.RelevanceScore > 0.8)
                    .OrderByDescending(t => t.RelevanceScore)
                    .Take(20)
                    .ToList()
            };

            _logger.LogInformation("Completed vocabulary extraction for repository {RepositoryId} in {Duration}ms. " +
                                 "Extracted {TotalTerms} terms ({BusinessTerms} business, {TechnicalTerms} technical)",
                repositoryId, processingTime.TotalMilliseconds, result.TotalTermsExtracted,
                result.BusinessTermsIdentified, result.TechnicalTermsIdentified);

            return result;
        }
        catch (Exception ex)
        {
            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error extracting vocabulary for repository {RepositoryId}", repositoryId);
            
            return new VocabularyExtractionResult
            {
                RepositoryId = repositoryId,
                ProcessingTime = processingTime,
                ErrorMessage = ex.Message
            };
        }
    }

    // Note: RepositoryFile doesn't contain Content, so we extract from metadata and code elements only
    private async Task<List<VocabularyTerm>> ExtractFromFileAsync(
        RepositoryFile file, 
        Dictionary<string, int> termFrequency, 
        Dictionary<string, List<VocabularyLocation>> termContexts,
        CancellationToken cancellationToken)
    {
        var terms = new List<VocabularyTerm>();

        // Extract from file path and language information
        var fileNameTerms = ExtractFromIdentifier(file.FileName, file.FilePath, 1, VocabularySource.SourceCode);
        terms.AddRange(fileNameTerms);

        // Extract language as a domain term
        if (!string.IsNullOrEmpty(file.Language))
        {
            var languageTerm = new VocabularyTerm
            {
                Term = file.Language,
                NormalizedTerm = NormalizeTerm(file.Language),
                TermType = VocabularyTermType.TechnicalTerm,
                Source = VocabularySource.SourceCode,
                Language = file.Language,
                Frequency = 1,
                RelevanceScore = 0.6,
                BusinessRelevance = 0.1,
                TechnicalRelevance = 0.9,
                Context = $"Programming language: {file.Language}",
                Domain = "Technology",
                Locations = new List<VocabularyLocation>
                {
                    new VocabularyLocation
                    {
                        FilePath = file.FilePath,
                        StartLine = 1,
                        EndLine = 1,
                        ContextType = VocabularyLocationContext.Configuration,
                        ContextDescription = $"File language: {file.Language}",
                        SurroundingCode = file.Language
                    }
                }
            };
            terms.Add(languageTerm);
        }

        // Update frequency tracking
        foreach (var term in terms)
        {
            var normalizedTerm = term.NormalizedTerm;
            termFrequency[normalizedTerm] = termFrequency.GetValueOrDefault(normalizedTerm, 0) + 1;
            
            if (!termContexts.ContainsKey(normalizedTerm))
                termContexts[normalizedTerm] = new List<VocabularyLocation>();
            
            termContexts[normalizedTerm].AddRange(term.Locations);
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractFromComments(string content, string filePath)
    {
        var terms = new List<VocabularyTerm>();
        var lines = content.Split('\n');

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            
            // Single line comments
            var singleLineMatch = Regex.Match(line, @"//\s*(.+)");
            if (singleLineMatch.Success)
            {
                var commentText = singleLineMatch.Groups[1].Value.Trim();
                var commentTerms = ExtractTermsFromText(commentText, filePath, lineIndex + 1, VocabularySource.Comments);
                terms.AddRange(commentTerms);
            }
        }

        // Multi-line comments
        var multiLinePattern = @"/\*\*(.*?)\*/";
        var multiLineMatches = Regex.Matches(content, multiLinePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        foreach (Match match in multiLineMatches)
        {
            var commentText = match.Groups[1].Value;
            var cleanComment = Regex.Replace(commentText, @"[\*\s]+", " ").Trim();
            var commentTerms = ExtractTermsFromText(cleanComment, filePath, 0, VocabularySource.Documentation);
            terms.AddRange(commentTerms);
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractFromStringLiterals(string content, string filePath)
    {
        var terms = new List<VocabularyTerm>();
        var stringPattern = @"""([^""\\]*(\\.[^""\\]*)*)""";
        var matches = Regex.Matches(content, stringPattern);

        foreach (Match match in matches)
        {
            var stringValue = match.Groups[1].Value;
            if (stringValue.Length > 3 && !IsCodeArtifact(stringValue))
            {
                var stringTerms = ExtractTermsFromText(stringValue, filePath, 0, VocabularySource.SourceCode);
                terms.AddRange(stringTerms);
            }
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractFromCodeElement(
        CodeElement element, 
        string filePath, 
        Dictionary<string, int> termFrequency, 
        Dictionary<string, List<VocabularyLocation>> termContexts)
    {
        var terms = new List<VocabularyTerm>();

        // Extract from element name
        var nameTerms = ExtractFromIdentifier(element.Name, filePath, element.StartLine, GetSourceFromElementType(element.ElementType));
        terms.AddRange(nameTerms);

        // Extract from parameters
        if (!string.IsNullOrEmpty(element.Parameters))
        {
            var parameterTerms = ExtractParameterTerms(element.Parameters, filePath, element.StartLine);
            terms.AddRange(parameterTerms);
        }

        // Update frequency tracking
        foreach (var term in terms)
        {
            var normalizedTerm = term.NormalizedTerm;
            termFrequency[normalizedTerm] = termFrequency.GetValueOrDefault(normalizedTerm, 0) + 1;
            
            if (!termContexts.ContainsKey(normalizedTerm))
                termContexts[normalizedTerm] = new List<VocabularyLocation>();
            
            termContexts[normalizedTerm].AddRange(term.Locations);
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractTermsFromText(string text, string filePath, int lineNumber, VocabularySource source)
    {
        var terms = new List<VocabularyTerm>();
        
        // Clean and tokenize text
        var cleanText = CleanText(text);
        var words = TokenizeText(cleanText);

        foreach (var word in words)
        {
            if (IsValidTerm(word))
            {
                var term = new VocabularyTerm
                {
                    Term = word,
                    NormalizedTerm = NormalizeTerm(word),
                    TermType = ClassifyTerm(word),
                    Source = source,
                    Language = "Unknown",
                    Frequency = 1,
                    RelevanceScore = CalculateRelevanceScore(word, cleanText),
                    BusinessRelevance = CalculateBusinessRelevance(word),
                    TechnicalRelevance = CalculateTechnicalRelevance(word),
                    Context = text,
                    Domain = IdentifyDomain(word, text),
                    Locations = new List<VocabularyLocation>
                    {
                        new VocabularyLocation
                        {
                            FilePath = filePath,
                            StartLine = lineNumber,
                            EndLine = lineNumber,
                            ContextType = GetContextFromSource(source),
                            ContextDescription = text.Length > 100 ? text.Substring(0, 100) + "..." : text,
                            SurroundingCode = text
                        }
                    }
                };

                terms.Add(term);
            }
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractFromIdentifier(string identifier, string filePath, int lineNumber, VocabularySource source)
    {
        var terms = new List<VocabularyTerm>();
        
        if (string.IsNullOrEmpty(identifier) || IsCodeArtifact(identifier))
            return terms;

        // Split camelCase and PascalCase
        var words = SplitIdentifier(identifier);
        
        foreach (var word in words)
        {
            if (IsValidTerm(word))
            {
                var term = new VocabularyTerm
                {
                    Term = word,
                    NormalizedTerm = NormalizeTerm(word),
                    TermType = GetTermTypeFromSource(source),
                    Source = source,
                    Language = "Unknown",
                    Frequency = 1,
                    RelevanceScore = CalculateRelevanceScore(word, identifier),
                    BusinessRelevance = CalculateBusinessRelevance(word),
                    TechnicalRelevance = CalculateTechnicalRelevance(word),
                    Context = identifier,
                    Domain = IdentifyDomain(word, identifier),
                    Locations = new List<VocabularyLocation>
                    {
                        new VocabularyLocation
                        {
                            FilePath = filePath,
                            StartLine = lineNumber,
                            EndLine = lineNumber,
                            ContextType = GetContextFromSource(source),
                            ContextDescription = $"Identifier: {identifier}",
                            SurroundingCode = identifier
                        }
                    }
                };

                terms.Add(term);
            }
        }

        return terms;
    }

    private List<VocabularyTerm> ExtractParameterTerms(string parameters, string filePath, int lineNumber)
    {
        var terms = new List<VocabularyTerm>();
        var paramPattern = @"\b(\w+)\s+(\w+)";
        var matches = Regex.Matches(parameters, paramPattern);

        foreach (Match match in matches)
        {
            var paramName = match.Groups[2].Value;
            var paramTerms = ExtractFromIdentifier(paramName, filePath, lineNumber, VocabularySource.ParameterNames);
            terms.AddRange(paramTerms);
        }

        return terms;
    }

    private async Task<List<VocabularyTerm>> NormalizeAndScoreTermsAsync(
        List<VocabularyTerm> terms, 
        Dictionary<string, int> termFrequency, 
        int repositoryId)
    {
        var termGroups = terms.GroupBy(t => t.NormalizedTerm).ToList();
        var normalizedTerms = new List<VocabularyTerm>();

        foreach (var group in termGroups)
        {
            var normalizedTerm = group.Key;
            var termList = group.ToList();
            var frequency = termFrequency.GetValueOrDefault(normalizedTerm, 1);

            // Merge similar terms
            var mergedTerm = new VocabularyTerm
            {
                RepositoryId = repositoryId,
                Term = termList.First().Term,
                NormalizedTerm = normalizedTerm,
                TermType = DetermineConsensusTermType(termList),
                Source = DetermineConsensusSource(termList),
                Language = DetermineConsensusLanguage(termList),
                Frequency = frequency,
                RelevanceScore = CalculateFinalRelevanceScore(termList, frequency),
                BusinessRelevance = termList.Max(t => t.BusinessRelevance),
                TechnicalRelevance = termList.Max(t => t.TechnicalRelevance),
                Context = string.Join("; ", termList.Select(t => t.Context).Distinct()),
                Domain = DetermineConsensusDomain(termList),
                Synonyms = new List<string>(),
                RelatedTerms = new List<string>(),
                UsageExamples = termList.Select(t => t.Context).Distinct().Take(5).ToList(),
                Locations = termList.SelectMany(t => t.Locations).ToList(),
                Metadata = new Dictionary<string, object>
                {
                    { "SourceCount", termList.Count },
                    { "UniqueLocations", termList.SelectMany(t => t.Locations).Count() },
                    { "DominantSource", DetermineConsensusSource(termList).ToString() }
                }
            };

            normalizedTerms.Add(mergedTerm);
        }

        return normalizedTerms;
    }

    private List<VocabularyTermRelationship> BuildTermRelationships(List<VocabularyTerm> terms)
    {
        var relationships = new List<VocabularyTermRelationship>();
        var termDict = terms.ToDictionary(t => t.NormalizedTerm, t => t);

        foreach (var term in terms)
        {
            // Find similar terms (edit distance, common roots, etc.)
            var similarTerms = terms.Where(t => t.Id != term.Id && 
                                                CalculateSimilarity(term.NormalizedTerm, t.NormalizedTerm) > 0.7)
                                   .ToList();

            foreach (var similarTerm in similarTerms)
            {
                relationships.Add(new VocabularyTermRelationship
                {
                    FromTermId = term.Id,
                    ToTermId = similarTerm.Id,
                    RelationshipType = VocabularyRelationshipType.SimilarTo,
                    Strength = CalculateSimilarity(term.NormalizedTerm, similarTerm.NormalizedTerm),
                    CoOccurrenceCount = CalculateCoOccurrence(term, similarTerm),
                    Context = $"Similar terms found in {term.Locations.Count} locations"
                });
            }

            // Find domain relationships
            var domainRelated = terms.Where(t => t.Id != term.Id && 
                                               t.Domain == term.Domain && 
                                               !string.IsNullOrEmpty(term.Domain))
                                    .ToList();

            foreach (var relatedTerm in domainRelated.Take(10))
            {
                relationships.Add(new VocabularyTermRelationship
                {
                    FromTermId = term.Id,
                    ToTermId = relatedTerm.Id,
                    RelationshipType = VocabularyRelationshipType.RelatedTo,
                    Strength = 0.6,
                    CoOccurrenceCount = 1,
                    Context = $"Same domain: {term.Domain}"
                });
            }
        }

        return relationships;
    }

    private List<BusinessConcept> IdentifyBusinessConcepts(List<VocabularyTerm> terms)
    {
        var concepts = new List<BusinessConcept>();
        var businessTerms = terms.Where(t => t.BusinessRelevance > 0.6).ToList();

        // Group by domain
        var domainGroups = businessTerms.Where(t => !string.IsNullOrEmpty(t.Domain))
                                      .GroupBy(t => t.Domain)
                                      .Where(g => g.Count() >= 3);

        foreach (var domainGroup in domainGroups)
        {
            var concept = new BusinessConcept
            {
                RepositoryId = businessTerms.First().RepositoryId,
                Name = domainGroup.Key,
                Description = $"Business domain with {domainGroup.Count()} related terms",
                Domain = domainGroup.Key,
                ConceptType = BusinessConceptType.Domain,
                Confidence = Math.Min(1.0, domainGroup.Count() / 10.0),
                Keywords = domainGroup.Select(t => t.Term).Distinct().ToList(),
                TechnicalMappings = domainGroup.SelectMany(t => t.Locations)
                                              .Select(l => l.FilePath)
                                              .Distinct()
                                              .ToList(),
                RelatedTermIds = domainGroup.Select(t => t.Id).ToList()
            };

            concepts.Add(concept);
        }

        // Identify processes
        var processTerms = businessTerms.Where(t => BusinessProcessIndicators.Any(p => 
            t.NormalizedTerm.Contains(p, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (processTerms.Any())
        {
            concepts.Add(new BusinessConcept
            {
                RepositoryId = businessTerms.First().RepositoryId,
                Name = "Business Processes",
                Description = "Identified business process terms",
                Domain = "Process",
                ConceptType = BusinessConceptType.Process,
                Confidence = 0.8,
                Keywords = processTerms.Select(t => t.Term).ToList(),
                RelatedTermIds = processTerms.Select(t => t.Id).ToList()
            });
        }

        return concepts;
    }

    private VocabularyStats CalculateVocabularyStats(List<VocabularyTerm> terms, List<BusinessConcept> concepts, int repositoryId)
    {
        return new VocabularyStats
        {
            RepositoryId = repositoryId,
            TotalTerms = terms.Count,
            UniqueTerms = terms.Select(t => t.NormalizedTerm).Distinct().Count(),
            BusinessTerms = terms.Count(t => t.BusinessRelevance > 0.5),
            TechnicalTerms = terms.Count(t => t.TechnicalRelevance > 0.5),
            DomainSpecificTerms = terms.Count(t => t.TermType == VocabularyTermType.DomainSpecific),
            AverageRelevanceScore = terms.Any() ? terms.Average(t => t.RelevanceScore) : 0,
            VocabularyDensity = CalculateVocabularyDensity(terms),
            BusinessTechnicalRatio = CalculateBusinessTechnicalRatio(terms),
            LanguageDistribution = terms.GroupBy(t => t.Language).ToDictionary(g => g.Key, g => g.Count()),
            DomainDistribution = terms.Where(t => !string.IsNullOrEmpty(t.Domain))
                                    .GroupBy(t => t.Domain!)
                                    .ToDictionary(g => g.Key, g => g.Count()),
            SourceDistribution = terms.GroupBy(t => t.Source).ToDictionary(g => g.Key, g => g.Count()),
            TopDomains = terms.Where(t => !string.IsNullOrEmpty(t.Domain))
                             .GroupBy(t => t.Domain!)
                             .OrderByDescending(g => g.Count())
                             .Take(10)
                             .Select(g => g.Key)
                             .ToList(),
            EmergingTerms = terms.Where(t => t.RelevanceScore > 0.8 && t.Frequency < 3)
                                 .Select(t => t.Term)
                                 .ToList()
        };
    }

    #region Helper Methods

    private string CleanText(string text)
    {
        // Remove special characters, normalize whitespace
        var cleaned = Regex.Replace(text, @"[^\w\s]", " ");
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }

    private List<string> TokenizeText(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                  .Where(word => word.Length > 2)
                  .ToList();
    }

    private List<string> SplitIdentifier(string identifier)
    {
        // Split camelCase and PascalCase
        var words = new List<string>();
        var current = new StringBuilder();

        for (int i = 0; i < identifier.Length; i++)
        {
            var c = identifier[i];
            
            if (i > 0 && char.IsUpper(c) && char.IsLower(identifier[i - 1]))
            {
                if (current.Length > 0)
                {
                    words.Add(current.ToString());
                    current.Clear();
                }
            }
            else if (i > 0 && char.IsLower(c) && char.IsUpper(identifier[i - 1]) && current.Length > 1)
            {
                var lastChar = current[current.Length - 1];
                current.Remove(current.Length - 1, 1);
                if (current.Length > 0)
                {
                    words.Add(current.ToString());
                    current.Clear();
                }
                current.Append(lastChar);
            }
            
            current.Append(c);
        }

        if (current.Length > 0)
        {
            words.Add(current.ToString());
        }

        // Also split on underscores and numbers
        var finalWords = new List<string>();
        foreach (var word in words)
        {
            var parts = word.Split('_', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var cleanPart = Regex.Replace(part, @"\d+", "").Trim();
                if (cleanPart.Length > 2)
                {
                    finalWords.Add(cleanPart);
                }
            }
        }

        return finalWords.Distinct().ToList();
    }

    private bool IsValidTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 3 || term.Length > 50)
            return false;

        // Filter out common programming keywords
        var programmingKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "var", "int", "string", "bool", "void", "public", "private", "protected", "static",
            "class", "interface", "namespace", "using", "return", "if", "else", "for", "while",
            "foreach", "try", "catch", "finally", "throw", "new", "null", "true", "false",
            "this", "base", "override", "virtual", "abstract", "sealed", "const", "readonly"
        };

        return !programmingKeywords.Contains(term) && 
               !Regex.IsMatch(term, @"^\d+$") && // Not just numbers
               Regex.IsMatch(term, @"^[a-zA-Z][a-zA-Z0-9]*$"); // Valid identifier pattern
    }

    private bool IsCodeArtifact(string text)
    {
        // Check if text looks like code (lots of special characters, etc.)
        var specialCharCount = text.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        return specialCharCount > text.Length * 0.3;
    }

    private string NormalizeTerm(string term)
    {
        return term.ToLowerInvariant();
    }

    private VocabularyTermType ClassifyTerm(string term)
    {
        var lowerTerm = term.ToLowerInvariant();

        // Check against domain patterns
        foreach (var pattern in DomainPatterns)
        {
            if (Regex.IsMatch(lowerTerm, pattern.Key, RegexOptions.IgnoreCase))
            {
                return pattern.Value;
            }
        }

        // Check business process indicators
        if (BusinessProcessIndicators.Contains(term))
        {
            return VocabularyTermType.BusinessProcess;
        }

        // Check technical indicators
        if (TechnicalIndicators.Contains(term))
        {
            return VocabularyTermType.TechnicalTerm;
        }

        // Default classification
        return VocabularyTermType.Unknown;
    }

    private double CalculateRelevanceScore(string term, string context)
    {
        var score = 0.5; // Base score

        // Increase score for longer, more descriptive terms
        score += Math.Min(0.2, term.Length / 50.0);

        // Increase score for terms that appear to be domain-specific
        if (ClassifyTerm(term) != VocabularyTermType.Unknown)
        {
            score += 0.3;
        }

        // Increase score based on context richness
        if (!string.IsNullOrEmpty(context) && context.Length > 20)
        {
            score += 0.1;
        }

        return Math.Min(1.0, score);
    }

    private double CalculateBusinessRelevance(string term)
    {
        var lowerTerm = term.ToLowerInvariant();
        
        foreach (var pattern in DomainPatterns)
        {
            if (pattern.Value == VocabularyTermType.BusinessTerm && 
                Regex.IsMatch(lowerTerm, pattern.Key, RegexOptions.IgnoreCase))
            {
                return 0.9;
            }
        }

        if (BusinessProcessIndicators.Contains(term))
        {
            return 0.8;
        }

        return 0.1;
    }

    private double CalculateTechnicalRelevance(string term)
    {
        var lowerTerm = term.ToLowerInvariant();
        
        foreach (var pattern in DomainPatterns)
        {
            if (pattern.Value == VocabularyTermType.TechnicalTerm && 
                Regex.IsMatch(lowerTerm, pattern.Key, RegexOptions.IgnoreCase))
            {
                return 0.9;
            }
        }

        if (TechnicalIndicators.Contains(term))
        {
            return 0.8;
        }

        return 0.1;
    }

    private string? IdentifyDomain(string term, string context)
    {
        var lowerTerm = term.ToLowerInvariant();
        var lowerContext = context.ToLowerInvariant();

        // Domain identification patterns
        var domains = new Dictionary<string, string[]>
        {
            { "Financial", new[] { "payment", "billing", "invoice", "credit", "debit", "banking", "finance" } },
            { "Healthcare", new[] { "patient", "medical", "diagnosis", "treatment", "clinic", "hospital" } },
            { "Ecommerce", new[] { "product", "cart", "checkout", "shipping", "inventory", "catalog" } },
            { "Education", new[] { "student", "course", "grade", "curriculum", "learning", "assessment" } },
            { "Manufacturing", new[] { "production", "quality", "supply", "logistics", "warehouse" } },
            { "Security", new[] { "auth", "login", "password", "token", "security", "permission" } },
            { "Analytics", new[] { "metric", "report", "dashboard", "analysis", "data", "statistics" } }
        };

        foreach (var domain in domains)
        {
            if (domain.Value.Any(keyword => lowerTerm.Contains(keyword) || lowerContext.Contains(keyword)))
            {
                return domain.Key;
            }
        }

        return null;
    }

    private VocabularySource GetSourceFromElementType(CodeElementType elementType)
    {
        return elementType switch
        {
            CodeElementType.Class => VocabularySource.ClassNames,
            CodeElementType.Method => VocabularySource.MethodNames,
            CodeElementType.Function => VocabularySource.MethodNames,
            CodeElementType.Constructor => VocabularySource.MethodNames,
            CodeElementType.Property => VocabularySource.PropertyNames,
            CodeElementType.Field => VocabularySource.PropertyNames,
            CodeElementType.Enum => VocabularySource.EnumNames,
            CodeElementType.Interface => VocabularySource.InterfaceNames,
            CodeElementType.Namespace => VocabularySource.NamespaceNames,
            CodeElementType.Variable => VocabularySource.VariableNames,
            CodeElementType.Event => VocabularySource.PropertyNames,
            CodeElementType.Delegate => VocabularySource.ClassNames,
            CodeElementType.Struct => VocabularySource.ClassNames,
            CodeElementType.Module => VocabularySource.ClassNames,
            CodeElementType.Component => VocabularySource.ClassNames,
            CodeElementType.Hook => VocabularySource.MethodNames,
            _ => VocabularySource.SourceCode
        };
    }

    private VocabularyTermType GetTermTypeFromSource(VocabularySource source)
    {
        return source switch
        {
            VocabularySource.ClassNames => VocabularyTermType.ClassName,
            VocabularySource.MethodNames => VocabularyTermType.MethodName,
            VocabularySource.PropertyNames => VocabularyTermType.PropertyName,
            VocabularySource.ParameterNames => VocabularyTermType.ParameterName,
            VocabularySource.ConstantNames => VocabularyTermType.ConstantName,
            VocabularySource.EnumNames => VocabularyTermType.EnumValue,
            VocabularySource.InterfaceNames => VocabularyTermType.InterfaceName,
            VocabularySource.NamespaceNames => VocabularyTermType.NamespaceName,
            VocabularySource.Comments => VocabularyTermType.CommentKeyword,
            VocabularySource.Documentation => VocabularyTermType.DocumentationTerm,
            _ => VocabularyTermType.Unknown
        };
    }

    private VocabularyLocationContext GetContextFromSource(VocabularySource source)
    {
        return source switch
        {
            VocabularySource.ClassNames => VocabularyLocationContext.ClassDeclaration,
            VocabularySource.MethodNames => VocabularyLocationContext.MethodDeclaration,
            VocabularySource.PropertyNames => VocabularyLocationContext.PropertyDeclaration,
            VocabularySource.ParameterNames => VocabularyLocationContext.ParameterDeclaration,
            VocabularySource.VariableNames => VocabularyLocationContext.VariableDeclaration,
            VocabularySource.Comments => VocabularyLocationContext.Comment,
            VocabularySource.Documentation => VocabularyLocationContext.Documentation,
            _ => VocabularyLocationContext.Unknown
        };
    }

    private VocabularyTermType DetermineConsensusTermType(List<VocabularyTerm> terms)
    {
        var typeCounts = terms.GroupBy(t => t.TermType)
                             .OrderByDescending(g => g.Count())
                             .First();
        return typeCounts.Key;
    }

    private VocabularySource DetermineConsensusSource(List<VocabularyTerm> terms)
    {
        var sourceCounts = terms.GroupBy(t => t.Source)
                               .OrderByDescending(g => g.Count())
                               .First();
        return sourceCounts.Key;
    }

    private string DetermineConsensusLanguage(List<VocabularyTerm> terms)
    {
        var languageCounts = terms.GroupBy(t => t.Language)
                                 .OrderByDescending(g => g.Count())
                                 .First();
        return languageCounts.Key;
    }

    private string? DetermineConsensusDomain(List<VocabularyTerm> terms)
    {
        var domainCounts = terms.Where(t => !string.IsNullOrEmpty(t.Domain))
                               .GroupBy(t => t.Domain)
                               .OrderByDescending(g => g.Count())
                               .FirstOrDefault();
        return domainCounts?.Key;
    }

    private double CalculateFinalRelevanceScore(List<VocabularyTerm> terms, int frequency)
    {
        var baseScore = terms.Average(t => t.RelevanceScore);
        
        // Boost score for frequently used terms
        var frequencyBoost = Math.Min(0.2, frequency / 100.0);
        
        // Boost score for terms found in multiple contexts
        var contextBoost = Math.Min(0.1, terms.Count / 10.0);
        
        return Math.Min(1.0, baseScore + frequencyBoost + contextBoost);
    }

    private double CalculateSimilarity(string term1, string term2)
    {
        // Simple edit distance-based similarity
        var distance = CalculateLevenshteinDistance(term1, term2);
        var maxLength = Math.Max(term1.Length, term2.Length);
        return 1.0 - (double)distance / maxLength;
    }

    private int CalculateLevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        var d = new int[s.Length + 1, t.Length + 1];

        for (var i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= t.Length; j++) d[0, j] = j;

        for (var i = 1; i <= s.Length; i++)
        {
            for (var j = 1; j <= t.Length; j++)
            {
                var cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[s.Length, t.Length];
    }

    private int CalculateCoOccurrence(VocabularyTerm term1, VocabularyTerm term2)
    {
        // Count how many files both terms appear in
        var term1Files = term1.Locations.Select(l => l.FilePath).Distinct().ToHashSet();
        var term2Files = term2.Locations.Select(l => l.FilePath).Distinct().ToHashSet();
        
        return term1Files.Intersect(term2Files).Count();
    }

    private double CalculateVocabularyDensity(List<VocabularyTerm> terms)
    {
        if (!terms.Any()) return 0;
        
        var totalFiles = terms.SelectMany(t => t.Locations)
                             .Select(l => l.FilePath)
                             .Distinct()
                             .Count();
        
        return totalFiles > 0 ? (double)terms.Count / totalFiles : 0;
    }

    private double CalculateBusinessTechnicalRatio(List<VocabularyTerm> terms)
    {
        var businessCount = terms.Count(t => t.BusinessRelevance > 0.5);
        var technicalCount = terms.Count(t => t.TechnicalRelevance > 0.5);
        
        return technicalCount > 0 ? (double)businessCount / technicalCount : 0;
    }

    #endregion

    // Implement other interface methods with basic implementations for now
    public Task<VocabularyQueryResult> GetVocabularyAsync(int repositoryId, VocabularyFilter filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will implement in next iteration");
    }

    public Task<ConceptRelationshipGraph> GetConceptRelationshipsAsync(int repositoryId, int termId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will implement in next iteration");
    }

    public Task<VocabularyUpdateResult> UpdateVocabularyAsync(int repositoryId, List<string> changedFiles, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will implement in next iteration");
    }

    public Task<BusinessTermMapping> GetBusinessTermMappingAsync(int repositoryId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will implement in next iteration");
    }

    public Task<List<VocabularyTerm>> SearchSimilarTermsAsync(int repositoryId, string searchTerm, VocabularySearchType searchType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will implement in next iteration");
    }
}
