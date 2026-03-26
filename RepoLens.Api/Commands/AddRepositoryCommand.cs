using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Api.Services;
using RepoLens.Api.Models;

namespace RepoLens.Api.Commands
{
    public class AddRepositoryCommand
    {
        private readonly IRepositoryRepository _repositoryRepository;
        private readonly IRepositoryValidationService _validationService;
        private readonly ILogger<AddRepositoryCommand> _logger;

        public AddRepositoryCommand(
            IRepositoryRepository repositoryRepository,
            IRepositoryValidationService validationService,
            ILogger<AddRepositoryCommand> logger)
        {
            _repositoryRepository = repositoryRepository;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<AddRepositoryResult> ExecuteAsync(AddRepositoryRequest request)
        {
            try
            {
                _logger.LogInformation("Adding repository: {Url}", request.Url);

                // Validate URL format
                var urlValidation = _validationService.ValidateUrlFormat(request.Url);
                if (!urlValidation.IsValid)
                {
                    _logger.LogWarning("Invalid URL format: {Url}, Error: {Error}", request.Url, urlValidation.ErrorMessage);
                    return AddRepositoryResult.Failure(urlValidation.ErrorMessage);
                }

                // Check if repository already exists
                var existingRepo = await _repositoryRepository.GetByUrlAsync(request.Url);
                if (existingRepo != null)
                {
                    _logger.LogWarning("Repository already exists: {Url}", request.Url);
                    return AddRepositoryResult.Failure("Repository with this URL already exists");
                }

                // Validate repository access
                var hasAccess = await _validationService.ValidateRepositoryAccessAsync(request.Url);
                if (!hasAccess)
                {
                    _logger.LogWarning("Cannot access repository: {Url}", request.Url);
                    return AddRepositoryResult.Failure("Unable to access repository. Please check the URL and your permissions.");
                }

                // Detect provider type
                var providerType = _validationService.DetectProviderType(request.Url);
                
                // Create repository entity
                var repository = new Repository
                {
                    Url = request.Url,
                    Name = string.IsNullOrEmpty(request.Name) 
                        ? _validationService.ExtractRepositoryNameFromUrl(request.Url)
                        : request.Name,
                    ProviderType = providerType,
                    CreatedAt = DateTime.UtcNow,
                    LastSyncCommit = string.Empty
                };

                // Add to database
                var addedRepository = await _repositoryRepository.AddAsync(repository);
                
                _logger.LogInformation("Repository added successfully: {Id} - {Name}", addedRepository.Id, addedRepository.Name);
                
                return AddRepositoryResult.CreateSuccess(addedRepository);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add repository: {Url}", request.Url);
                return AddRepositoryResult.Failure($"Failed to add repository: {ex.Message}");
            }
        }
    }

    public class AddRepositoryRequest
    {
        public string Url { get; }
        public string? Name { get; }

        public AddRepositoryRequest(string url, string? name = null)
        {
            Url = url;
            Name = name;
        }
    }

    public class AddRepositoryResult
    {
        public bool Success { get; private set; }
        public Repository? Repository { get; private set; }
        public string? ErrorMessage { get; private set; }

        private AddRepositoryResult() { }

        public static AddRepositoryResult CreateSuccess(Repository repository)
        {
            return new AddRepositoryResult
            {
                Success = true,
                Repository = repository
            };
        }

        public static AddRepositoryResult Failure(string errorMessage)
        {
            return new AddRepositoryResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
