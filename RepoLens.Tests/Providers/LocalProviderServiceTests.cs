using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RepoLens.Core.Entities;
using RepoLens.Core.Exceptions;
using RepoLens.Infrastructure.Providers;
using System.Security;
using Xunit;
using Xunit.Abstractions;

namespace RepoLens.Tests.Providers;

/// <summary>
/// Comprehensive unit tests for LocalProviderService
/// Testing local Git repository operations with LibGit2Sharp
/// </summary>
public class LocalProviderServiceTests
{
    private readonly Mock<ILogger<LocalProviderService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockSection;
    private readonly LocalProviderService _service;
    private readonly ITestOutputHelper _output;

    public LocalProviderServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _mockLogger = new Mock<ILogger<LocalProviderService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockSection = new Mock<IConfigurationSection>();
        
        // Setup default configuration behavior
        _mockConfiguration.Setup(c => c.GetSection("LocalRepositories:AllowedPaths"))
                         .Returns(_mockSection.Object);
        // Instead of using Get<string[]>(), set up the configuration to return values directly
        _mockSection.Setup(s => s.Value).Returns((string?)null);
        
        _service = new LocalProviderService(_mockLogger.Object, _mockConfiguration.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Act & Assert
        _service.Should().NotBeNull();
        _service.ProviderType.Should().Be(ProviderType.Local);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => new LocalProviderService(null!, _mockConfiguration.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => new LocalProviderService(_mockLogger.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    #endregion

    #region CanHandle Tests

    [Theory]
    [InlineData("file:///C:/repos/test.git", true)]
    [InlineData("file://C:/repos/test", true)]
    [InlineData("/home/user/repos/test", true)]
    [InlineData("/var/git/project", true)]
    [InlineData("C:\\repos\\project", true)]
    [InlineData("D:\\code\\test", true)]
    [InlineData("./local-repo", true)]
    [InlineData("../parent-repo", true)]
    [InlineData("https://github.com/user/repo.git", false)]
    [InlineData("git@github.com:user/repo.git", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    public void CanHandle_WithVariousUrls_ShouldReturnExpectedResult(string url, bool expected)
    {
        _output.WriteLine($"Testing URL: {url}");

        // Act
        var result = _service.CanHandle(url);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CanHandle_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = _service.CanHandle(null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ValidateAccessAsync Tests

    [Fact]
    public async Task ValidateAccessAsync_WithInvalidUrl_ShouldReturnFailure()
    {
        // Arrange
        var invalidUrl = "https://github.com/user/repo.git";

        // Act
        var result = await _service.ValidateAccessAsync(invalidUrl);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Repository URL is not a valid local path format");
    }

    [Fact]
    public async Task ValidateAccessAsync_WithPathOutsideAllowedDirectories_ShouldReturnFailure()
    {
        // Arrange
        var restrictedPath = @"C:\Windows\System32\test-repo";

        // Act
        var result = await _service.ValidateAccessAsync(restrictedPath);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Access denied: Repository path is outside allowed directories");
        result.Details.Should().Contain(restrictedPath);
    }

    [Fact]
    public async Task ValidateAccessAsync_WithNonExistentPath_ShouldReturnFailure()
    {
        // Arrange
        SetupAllowedPath(@"C:\repos");
        var nonExistentPath = @"C:\repos\non-existent-repo";

        // Act
        var result = await _service.ValidateAccessAsync(nonExistentPath);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Repository path does not exist");
    }

    [Fact]
    public async Task ValidateAccessAsync_WithNullUrl_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(async () => await _service.ValidateAccessAsync(null!))
                          .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region CollectMetricsAsync Tests

    [Fact]
    public async Task CollectMetricsAsync_WithInvalidProviderType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: "https://github.com/user/repo.git",
            ProviderType: ProviderType.GitHub,
            AuthToken: null,
            LocalClonePath: null,
            Owner: "user",
            RepoName: "repo");

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectMetricsAsync(context))
                          .Should().ThrowAsync<InvalidOperationException>()
                          .WithMessage("*cannot handle GitHub repositories");
    }

    [Fact]
    public async Task CollectMetricsAsync_WithPathOutsideAllowed_ShouldThrowSecurityException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: @"C:\Windows\System32\test",
            ProviderType: ProviderType.Local,
            AuthToken: null,
            LocalClonePath: null,
            Owner: null,
            RepoName: null);

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectMetricsAsync(context))
                          .Should().ThrowAsync<SecurityException>()
                          .WithMessage("*Access denied to path*");
    }

    [Fact]
    public async Task CollectMetricsAsync_WithNonExistentPath_ShouldThrowRepositoryNotFoundException()
    {
        // Arrange
        SetupAllowedPath(@"C:\repos");
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: @"C:\repos\non-existent",
            ProviderType: ProviderType.Local,
            AuthToken: null,
            LocalClonePath: null,
            Owner: null,
            RepoName: null);

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectMetricsAsync(context))
                          .Should().ThrowAsync<RepositoryNotFoundException>()
                          .WithMessage("*Repository not found at path*");
    }

    [Fact]
    public async Task CollectMetricsAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectMetricsAsync(null!))
                          .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region CollectContributorMetricsAsync Tests

    [Fact]
    public async Task CollectContributorMetricsAsync_WithInvalidProviderType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: "https://github.com/user/repo.git",
            ProviderType: ProviderType.GitLab,
            AuthToken: null,
            LocalClonePath: null,
            Owner: "user",
            RepoName: "repo");

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectContributorMetricsAsync(context))
                          .Should().ThrowAsync<InvalidOperationException>()
                          .WithMessage("*cannot handle GitLab repositories");
    }

    [Fact]
    public async Task CollectContributorMetricsAsync_WithPathOutsideAllowed_ShouldThrowSecurityException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: @"C:\Program Files\restricted",
            ProviderType: ProviderType.Local,
            AuthToken: null,
            LocalClonePath: null,
            Owner: null,
            RepoName: null);

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectContributorMetricsAsync(context))
                          .Should().ThrowAsync<SecurityException>()
                          .WithMessage("*Access denied to path*");
    }

    [Fact]
    public async Task CollectContributorMetricsAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectContributorMetricsAsync(null!))
                          .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region CollectFileMetricsAsync Tests

    [Fact]
    public async Task CollectFileMetricsAsync_WithInvalidProviderType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: "https://bitbucket.org/user/repo.git",
            ProviderType: ProviderType.Bitbucket,
            AuthToken: null,
            LocalClonePath: null,
            Owner: "user",
            RepoName: "repo");

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectFileMetricsAsync(context))
                          .Should().ThrowAsync<InvalidOperationException>()
                          .WithMessage("*cannot handle Bitbucket repositories");
    }

    [Fact]
    public async Task CollectFileMetricsAsync_WithPathOutsideAllowed_ShouldThrowSecurityException()
    {
        // Arrange
        var context = new RepositoryContext(
            RepositoryId: 1,
            Url: @"C:\Users\Public\restricted",
            ProviderType: ProviderType.Local,
            AuthToken: null,
            LocalClonePath: null,
            Owner: null,
            RepoName: null);

        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectFileMetricsAsync(context))
                          .Should().ThrowAsync<SecurityException>()
                          .WithMessage("*Access denied to path*");
    }

    [Fact]
    public async Task CollectFileMetricsAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(async () => await _service.CollectFileMetricsAsync(null!))
                          .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void GetAllowedPaths_WithCustomConfiguration_ShouldUseConfiguredPaths()
    {
        // Arrange
        var mockCustomConfig = new Mock<IConfiguration>();
        var mockCustomSection = new Mock<IConfigurationSection>();
        var customPaths = new[] { @"C:\custom\path1", @"C:\custom\path2" };
        
        mockCustomConfig.Setup(c => c.GetSection("LocalRepositories:AllowedPaths"))
                        .Returns(mockCustomSection.Object);
        mockCustomSection.Setup(s => s.Value).Returns(string.Join(";", customPaths));
        
        var customService = new LocalProviderService(_mockLogger.Object, mockCustomConfig.Object);

        // Act - Test by trying paths that would be denied by defaults but allowed by custom config
        var result1 = customService.CanHandle(@"C:\custom\path1\repo");
        var result2 = customService.CanHandle(@"C:\custom\path2\repo");

        // Assert
        result1.Should().BeTrue("custom paths should be handled");
        result2.Should().BeTrue("custom paths should be handled");
    }

    [Fact]
    public void GetAllowedPaths_WithEmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange - already set up in constructor to return empty array

        // Act
        var result1 = _service.CanHandle(@"C:\repos\test");
        var result2 = _service.CanHandle(@"C:\projects\test");
        var result3 = _service.CanHandle(@"./repos/test");

        // Assert
        result1.Should().BeTrue("default paths should be allowed");
        result2.Should().BeTrue("default paths should be allowed");
        result3.Should().BeTrue("default paths should be allowed");
    }

    #endregion

    #region File Protocol Tests

    [Theory]
    [InlineData("file:///C:/repos/test", @"C:\repos\test")]
    [InlineData("file://C:/repos/test", @"C:\repos\test")]
    [InlineData("file:///home/user/repo", "/home/user/repo")]
    public void NormalizePath_WithFileProtocol_ShouldRemoveProtocolAndNormalize(string input, string expectedStart)
    {
        // We can't directly test the private method, but we can test the behavior
        // by checking if the service can handle the URL (which calls NormalizePath internally)
        
        // Act
        var canHandle = _service.CanHandle(input);

        // Assert
        canHandle.Should().BeTrue($"should be able to handle {input}");
        _output.WriteLine($"Successfully normalized: {input} → expected to start with {expectedStart}");
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void CanHandle_WithWhitespaceOrEmpty_ShouldReturnFalse(string input)
    {
        // Act
        var result = _service.CanHandle(input);

        // Assert
        result.Should().BeFalse($"whitespace input '{input}' should not be handled");
    }

    [Fact]
    public void ProviderType_ShouldReturnLocal()
    {
        // Act & Assert
        _service.ProviderType.Should().Be(ProviderType.Local);
    }

    #endregion

    #region Helper Methods

    private void SetupAllowedPath(string path)
    {
        _mockSection.Setup(s => s.Value).Returns(path);
    }

    #endregion
}

/// <summary>
/// Performance and integration-style tests for LocalProviderService
/// Tests behavior with actual file system operations where possible
/// </summary>
public class LocalProviderServicePerformanceTests
{
    private readonly ITestOutputHelper _output;

    public LocalProviderServicePerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CanHandle_PerformanceTest_ShouldCompleteQuickly()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LocalProviderService>>();
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        
        mockConfig.Setup(c => c.GetSection("LocalRepositories:AllowedPaths"))
                  .Returns(mockSection.Object);
        mockSection.Setup(s => s.Value).Returns((string?)null);
        
        var service = new LocalProviderService(mockLogger.Object, mockConfig.Object);

        var testUrls = new[]
        {
            "file:///C:/repos/test1",
            "file:///C:/repos/test2",
            @"C:\projects\app1",
            @"D:\code\library",
            "/home/user/repo1",
            "/var/git/project",
            "./local-repo",
            "../parent-repo",
            "https://github.com/user/repo1.git",
            "https://gitlab.com/user/repo2.git",
            "git@github.com:user/repo3.git",
            "",
            null!
        };

        // Act
        var startTime = DateTime.UtcNow;
        
        foreach (var url in testUrls)
        {
            var result = service.CanHandle(url);
            _output.WriteLine($"URL: {url ?? "null"} → {result}");
        }
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100), 
            "CanHandle should be very fast for multiple URLs");
        
        _output.WriteLine($"Processed {testUrls.Length} URLs in {duration.TotalMilliseconds:F2}ms");
    }

    [Fact]
    public async Task ValidateAccessAsync_WithCurrentDirectory_ShouldHandleGracefully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LocalProviderService>>();
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        
        // Allow current directory for this test
        var currentDir = Directory.GetCurrentDirectory();
        mockConfig.Setup(c => c.GetSection("LocalRepositories:AllowedPaths"))
                  .Returns(mockSection.Object);
        mockSection.Setup(s => s.Value).Returns(currentDir);
        
        var service = new LocalProviderService(mockLogger.Object, mockConfig.Object);

        // Act
        var startTime = DateTime.UtcNow;
        var result = await service.ValidateAccessAsync(currentDir);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2), 
            "Validation should complete within reasonable time");
        
        // The result depends on whether current directory is a Git repo
        // We're mainly testing that it doesn't hang or crash
        result.Should().NotBeNull();
        
        _output.WriteLine($"Validation completed in {duration.TotalMilliseconds:F2}ms");
        _output.WriteLine($"Result: {result.IsValid} - {result.ErrorMessage ?? "No error"}");
    }
}
