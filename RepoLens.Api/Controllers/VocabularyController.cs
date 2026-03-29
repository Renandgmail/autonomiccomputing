using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;

namespace RepoLens.Api.Controllers;

/// <summary>
/// Controller for vocabulary extraction and domain intelligence operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyExtractionService _vocabularyService;
    private readonly ILogger<VocabularyController> _logger;

    public VocabularyController(
        IVocabularyExtractionService vocabularyService,
        ILogger<VocabularyController> logger)
    {
        _vocabularyService = vocabularyService;
        _logger = logger;
    }

    /// <summary>
    /// Extract domain-specific vocabulary from a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vocabulary extraction results with business and technical terms</returns>
    [HttpPost("extract/{repositoryId:int}")]
    public async Task<IActionResult> ExtractVocabulary(int repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting vocabulary extraction for repository {RepositoryId}", repositoryId);
            
            var result = await _vocabularyService.ExtractVocabularyAsync(repositoryId, cancellationToken);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                _logger.LogError("Vocabulary extraction failed for repository {RepositoryId}: {Error}", 
                    repositoryId, result.ErrorMessage);
                return BadRequest(new { error = result.ErrorMessage });
            }

            _logger.LogInformation("Vocabulary extraction completed for repository {RepositoryId}. " +
                                 "Extracted {TotalTerms} terms in {Duration}ms", 
                repositoryId, result.TotalTermsExtracted, result.ProcessingTime.TotalMilliseconds);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Repository {RepositoryId} not found: {Message}", repositoryId, ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during vocabulary extraction for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "An unexpected error occurred during vocabulary extraction" });
        }
    }

    /// <summary>
    /// Get extracted vocabulary terms for a repository with filtering
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="termType">Filter by term type (optional)</param>
    /// <param name="source">Filter by source (optional)</param>
    /// <param name="domain">Filter by domain (optional)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50, max: 200)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated vocabulary terms</returns>
    [HttpGet("{repositoryId:int}/terms")]
    public async Task<IActionResult> GetVocabularyTerms(
        int repositoryId,
        [FromQuery] string? termType = null,
        [FromQuery] string? source = null,
        [FromQuery] string? domain = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 50;

            var filter = new VocabularyFilter
            {
                Page = page,
                PageSize = pageSize
            };

            // Parse optional filters
            if (!string.IsNullOrEmpty(termType) && Enum.TryParse<VocabularyTermType>(termType, true, out var termTypeEnum))
                filter.TermType = termTypeEnum;

            if (!string.IsNullOrEmpty(source) && Enum.TryParse<VocabularySource>(source, true, out var sourceEnum))
                filter.Source = sourceEnum;

            if (!string.IsNullOrEmpty(domain))
                filter.Domains = new List<string> { domain };

            var result = await _vocabularyService.GetVocabularyAsync(repositoryId, filter, cancellationToken);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Vocabulary query functionality will be available in the next release" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vocabulary for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "An error occurred while retrieving vocabulary terms" });
        }
    }

    /// <summary>
    /// Get business-technical term mapping for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business to technical term mappings</returns>
    [HttpGet("{repositoryId:int}/business-mapping")]
    public async Task<IActionResult> GetBusinessTermMapping(int repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vocabularyService.GetBusinessTermMappingAsync(repositoryId, cancellationToken);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Business term mapping functionality will be available in the next release" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business term mapping for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "An error occurred while retrieving business term mappings" });
        }
    }

    /// <summary>
    /// Get concept relationships for a specific term
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="termId">Term ID to analyze relationships for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Concept relationship graph</returns>
    [HttpGet("{repositoryId:int}/terms/{termId:int}/relationships")]
    public async Task<IActionResult> GetConceptRelationships(int repositoryId, int termId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vocabularyService.GetConceptRelationshipsAsync(repositoryId, termId, cancellationToken);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Concept relationship analysis will be available in the next release" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving concept relationships for term {TermId} in repository {RepositoryId}", 
                termId, repositoryId);
            return StatusCode(500, new { error = "An error occurred while retrieving concept relationships" });
        }
    }

    /// <summary>
    /// Search for similar vocabulary terms
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="searchTerm">Term to search for</param>
    /// <param name="searchType">Type of similarity search (default: SimilarMeaning)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar terms</returns>
    [HttpGet("{repositoryId:int}/search")]
    public async Task<IActionResult> SearchSimilarTerms(
        int repositoryId,
        [FromQuery] string searchTerm,
        [FromQuery] string searchType = "SimilarMeaning",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            if (!Enum.TryParse<VocabularySearchType>(searchType, true, out var searchTypeEnum))
            {
                searchTypeEnum = VocabularySearchType.SimilarMeaning;
            }

            var result = await _vocabularyService.SearchSimilarTermsAsync(repositoryId, searchTerm, searchTypeEnum, cancellationToken);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Vocabulary search functionality will be available in the next release" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for similar terms to '{SearchTerm}' in repository {RepositoryId}", 
                searchTerm, repositoryId);
            return StatusCode(500, new { error = "An error occurred while searching for similar terms" });
        }
    }

    /// <summary>
    /// Update vocabulary based on file changes
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="request">Update request with changed files</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vocabulary update result</returns>
    [HttpPut("{repositoryId:int}/update")]
    public async Task<IActionResult> UpdateVocabulary(
        int repositoryId,
        [FromBody] VocabularyUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request?.ChangedFiles == null || !request.ChangedFiles.Any())
            {
                return BadRequest(new { error = "List of changed files is required" });
            }

            var result = await _vocabularyService.UpdateVocabularyAsync(repositoryId, request.ChangedFiles, cancellationToken);
            return Ok(result);
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { error = "Incremental vocabulary update functionality will be available in the next release" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vocabulary for repository {RepositoryId}", repositoryId);
            return StatusCode(500, new { error = "An error occurred while updating vocabulary" });
        }
    }
}

/// <summary>
/// Request model for vocabulary updates
/// </summary>
public class VocabularyUpdateRequest
{
    /// <summary>
    /// List of file paths that have changed
    /// </summary>
    public List<string> ChangedFiles { get; set; } = new();
}
