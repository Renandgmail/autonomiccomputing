using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepoLens.Core.Entities;
using RepoLens.Core.Repositories;
using RepoLens.Api.Commands;
using RepoLens.Api.Models;
using RepoLens.Api.Services;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Commands;

/// <summary>
/// Comprehensive unit tests for AddRepositoryCommand
/// Testing command pattern implementation and business logic
/// </summary>
public class AddRepositoryCommandTests
{
    private readonly Mock<IRepositoryRepository> _mockRepositoryRepository;
    private readonly Mock<IRepositoryValidationService> _mockValidationService;
    private readonly Mock<ILogger<AddRepositoryCommand>> _mockLogger;
    private readonly AddRepositoryCommand _command;
    private readonly ITestOutputHelper _output;

    public AddRepositoryCommandTests(ITestOutputHelper output)
    {
        _output = output;
        _mockRepositoryRepository = new Mock<IRepositoryRepository>();
        _mockValidationService = new Mock<IRepositoryValidationService>();
        _mockLogger = new Mock<ILogger<AddRepositoryCommand>>();
        
        _command = new AddRepositoryCommand(
            _mockRepositoryRepository.Object,
            _mockValidationService.Object,
            _mockLogger.Object);
    }

    #region Successful Scenarios

    [Fact]
    public async Task ExecuteAsync_WithValidRepository_ShouldSucceed()
    {
        // Arrange
        var request = new AddRepositoryRequest("https://github.com/user/repo.git", "Test Repo");
        
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositoryCreation();

        _output.WriteLine($"Testing repository addition: {request.Url}");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Repository.Should().NotBeNull();
        result.Repository!.Name.Should().Be("Test Repo");
        result.Repository.Url.Should().Be(request.Url);
        result.ErrorMessage.Should().BeNull();

        VerifyRepositoryAdded();
        VerifyLogging(LogLevel.Information, Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithoutProvidedName_ShouldExtractFromUrl()
    {
        // Arrange
        var request = new AddRepositoryRequest("https://github.com/user/awesome-project.git");
        
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositoryCreation();
        
        _mockValidationService
            .Setup(v => v.ExtractRepositoryNameFromUrl(request.Url))
            .Returns("awesome-project");

        _output.WriteLine("Testing automatic name extraction from URL");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Repository!.Name.Should().Be("awesome-project");
        
        _mockValidationService.Verify(
            v => v.ExtractRepositoryNameFromUrl(request.Url), 
            Times.Once);
    }

    [Theory]
    [InlineData("https://github.com/user/repo.git", ProviderType.GitHub)]
    [InlineData("https://gitlab.com/user/repo.git", ProviderType.GitLab)]
    [InlineData("https://bitbucket.org/user/repo.git", ProviderType.Bitbucket)]
    [InlineData("/local/path/to/repo", ProviderType.Local)]
    [InlineData("https://dev.azure.com/org/proj/_git/repo", ProviderType.AzureDevOps)]
    public async Task ExecuteAsync_ShouldDetectAndSetCorrectProviderType(string url, ProviderType expectedProviderType)
    {
        // Arrange
        var request = new AddRepositoryRequest(url, "Test Repo");
        
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        SetupRepositoryCreation();
        
        _mockValidationService
            .Setup(v => v.DetectProviderType(url))
            .Returns(expectedProviderType);

        _output.WriteLine($"Testing provider type detection: {url} → {expectedProviderType}");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Repository!.ProviderType.Should().Be(expectedProviderType);
        
        _mockValidationService.Verify(
            v => v.DetectProviderType(url), 
            Times.Once);
    }

    #endregion

    #region Validation Failure Scenarios

    [Fact]
    public async Task ExecuteAsync_WithInvalidUrlFormat_ShouldReturnError()
    {
        // Arrange
        var request = new AddRepositoryRequest("invalid-url", "Test Repo");
        
        SetupValidationFailure("Invalid URL format");

        _output.WriteLine("Testing invalid URL format handling");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid URL format");
        result.Repository.Should().BeNull();

        _mockRepositoryRepository.Verify(
            r => r.AddAsync(It.IsAny<Repository>()), 
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInaccessibleRepository_ShouldReturnError()
    {
        // Arrange
        var request = new AddRepositoryRequest("https://github.com/user/private-repo.git");
        
        SetupValidationFailure("Unable to access repository. Please check the URL and your permissions.");

        _output.WriteLine("Testing inaccessible repository handling");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unable to access repository");
        
        _mockRepositoryRepository.Verify(
            r => r.AddAsync(It.IsAny<Repository>()), 
            Times.Never);
    }

    #endregion

    #region Duplicate Repository Scenarios

    [Fact]
    public async Task ExecuteAsync_WithExistingRepository_ShouldReturnError()
    {
        // Arrange
        var request = new AddRepositoryRequest("https://github.com/user/existing-repo.git");
        var existingRepo = new Repository { Id = 1, Url = request.Url, Name = "Existing Repo" };
        
        SetupValidationSuccess();
        _mockRepositoryRepository
            .Setup(r => r.GetByUrlAsync(request.Url))
            .ReturnsAsync(existingRepo);

        _output.WriteLine("Testing duplicate repository detection");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Repository with this URL already exists");
        
        VerifyLogging(LogLevel.Warning, Times.Once());
        _mockRepositoryRepository.Verify(
            r => r.AddAsync(It.IsAny<Repository>()), 
            Times.Never);
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task ExecuteAsync_WithRepositoryException_ShouldReturnError()
    {
        // Arrange
        var request = new AddRepositoryRequest("https://github.com/user/repo.git");
        var exception = new Exception("Database connection failed");
        
        SetupValidationSuccess();
        SetupRepositoryNotExists();
        
        _mockRepositoryRepository
            .Setup(r => r.AddAsync(It.IsAny<Repository>()))
            .ThrowsAsync(exception);

        _output.WriteLine("Testing exception handling during repository creation");

        // Act
        var result = await _command.ExecuteAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().StartWith("Failed to add repository:");
        result.ErrorMessage.Should().Contain("Database connection failed");
        
        VerifyLogging(LogLevel.Error, Times.Once());
    }

    #endregion

    #region Test Helpers

    private void SetupValidationSuccess()
    {
        var validationResult = ValidationResult.Success();
        
        _mockValidationService
            .Setup(v => v.ValidateUrlFormat(It.IsAny<string>()))
            .Returns(validationResult);
            
        _mockValidationService
            .Setup(v => v.ValidateRepositoryAccessAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
            
        _mockValidationService
            .Setup(v => v.DetectProviderType(It.IsAny<string>()))
            .Returns(ProviderType.GitHub); // Default to GitHub for general tests
    }

    private void SetupValidationFailure(string errorMessage)
    {
        var validationResult = ValidationResult.Failure(errorMessage);
        
        _mockValidationService
            .Setup(v => v.ValidateUrlFormat(It.IsAny<string>()))
            .Returns(validationResult);
    }

    private void SetupRepositoryNotExists()
    {
        _mockRepositoryRepository
            .Setup(r => r.GetByUrlAsync(It.IsAny<string>()))
            .ReturnsAsync((Repository?)null);
    }

    private void SetupRepositoryCreation()
    {
        _mockRepositoryRepository
            .Setup(r => r.AddAsync(It.IsAny<Repository>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Repository repo, CancellationToken ct) =>
            {
                repo.Id = 123; // Simulate database assignment
                return repo;
            });
    }

    private void VerifyRepositoryAdded()
    {
        _mockRepositoryRepository.Verify(
            r => r.AddAsync(It.Is<Repository>(repo => 
                repo.Url == "https://github.com/user/repo.git" &&
                repo.LastSyncCommit == string.Empty &&
                repo.CreatedAt != default)), 
            Times.Once);
    }

    private void VerifyLogging(LogLevel expectedLevel, Times expectedTimes)
    {
        _mockLogger.Verify(
            logger => logger.Log(
                expectedLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            expectedTimes);
    }

    #endregion
}

/// <summary>
/// Performance tests for AddRepositoryCommand
/// </summary>
public class AddRepositoryCommandPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public AddRepositoryCommandPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var mockRepo = new Mock<IRepositoryRepository>();
        var mockValidation = new Mock<IRepositoryValidationService>();
        var mockLogger = new Mock<ILogger<AddRepositoryCommand>>();
        
        var command = new AddRepositoryCommand(mockRepo.Object, mockValidation.Object, mockLogger.Object);
        var request = new AddRepositoryRequest("https://github.com/user/repo.git");

        // Setup mocks for successful execution
        mockValidation.Setup(v => v.ValidateUrlFormat(It.IsAny<string>()))
                     .Returns(ValidationResult.Success());
        mockValidation.Setup(v => v.ValidateRepositoryAccessAsync(It.IsAny<string>()))
                     .ReturnsAsync(true);
        mockValidation.Setup(v => v.DetectProviderType(It.IsAny<string>()))
                     .Returns(ProviderType.GitHub);
        mockRepo.Setup(r => r.GetByUrlAsync(It.IsAny<string>()))
                .ReturnsAsync((Repository?)null);
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Repository>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Repository repo, CancellationToken ct) => repo);

        // Act
        var startTime = DateTime.UtcNow;
        var result = await command.ExecuteAsync(request);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5), 
            "Command should complete within reasonable time");
        result.Success.Should().BeTrue();
        
        _output.WriteLine($"Command execution time: {duration.TotalMilliseconds:F2}ms");
    }
}
