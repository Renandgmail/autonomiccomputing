using Moq;
using RepoLens.Core.Entities;
using RepoLens.Core.Services;
using RepoLens.Infrastructure.Providers;
using Xunit;

namespace RepoLens.Tests.Services;

public class GitProviderFactoryTests
{
    private readonly Mock<IGitProviderService> _mockGitHubProvider;
    private readonly Mock<IGitProviderService> _mockGitLabProvider;
    private readonly Mock<IGitProviderService> _mockLocalProvider;
    private readonly GitProviderFactory _factory;

    public GitProviderFactoryTests()
    {
        // Setup mock providers
        _mockGitHubProvider = new Mock<IGitProviderService>();
        _mockGitHubProvider.Setup(p => p.ProviderType).Returns(ProviderType.GitHub);
        _mockGitHubProvider.Setup(p => p.CanHandle("https://github.com/owner/repo")).Returns(true);
        _mockGitHubProvider.Setup(p => p.CanHandle("git@github.com:owner/repo.git")).Returns(true);

        _mockGitLabProvider = new Mock<IGitProviderService>();
        _mockGitLabProvider.Setup(p => p.ProviderType).Returns(ProviderType.GitLab);
        _mockGitLabProvider.Setup(p => p.CanHandle("https://gitlab.com/owner/repo")).Returns(true);
        _mockGitLabProvider.Setup(p => p.CanHandle("git@gitlab.com:owner/repo.git")).Returns(true);

        _mockLocalProvider = new Mock<IGitProviderService>();
        _mockLocalProvider.Setup(p => p.ProviderType).Returns(ProviderType.Local);
        _mockLocalProvider.Setup(p => p.CanHandle("/path/to/repo")).Returns(true);
        _mockLocalProvider.Setup(p => p.CanHandle("C:\\path\\to\\repo")).Returns(true);
        _mockLocalProvider.Setup(p => p.CanHandle("file:///path/to/repo")).Returns(true);

        var providers = new List<IGitProviderService>
        {
            _mockGitHubProvider.Object,
            _mockGitLabProvider.Object,
            _mockLocalProvider.Object
        };

        _factory = new GitProviderFactory(providers);
    }

    [Fact]
    public void Constructor_WithNullProviders_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GitProviderFactory(null!));
    }

    [Theory]
    [InlineData("https://github.com/owner/repo")]
    [InlineData("git@github.com:owner/repo.git")]
    public void GetProvider_WithKnownGitHubUrl_ShouldReturnGitHubProvider(string url)
    {
        // Act
        var provider = _factory.GetProvider(url);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.GitHub, provider.ProviderType);
        Assert.Same(_mockGitHubProvider.Object, provider);
    }

    [Theory]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("git@gitlab.com:owner/repo.git")]
    public void GetProvider_WithKnownGitLabUrl_ShouldReturnGitLabProvider(string url)
    {
        // Act
        var provider = _factory.GetProvider(url);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.GitLab, provider.ProviderType);
        Assert.Same(_mockGitLabProvider.Object, provider);
    }

    [Theory]
    [InlineData("/path/to/repo")]
    [InlineData("C:\\path\\to\\repo")]
    [InlineData("file:///path/to/repo")]
    public void GetProvider_WithKnownLocalPath_ShouldReturnLocalProvider(string url)
    {
        // Act
        var provider = _factory.GetProvider(url);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.Local, provider.ProviderType);
        Assert.Same(_mockLocalProvider.Object, provider);
    }

    [Theory]
    [InlineData("https://unknown-provider.com/owner/repo")]
    [InlineData("https://example.com/not-a-repo")]
    [InlineData("ftp://server.com/repo")]
    public void GetProvider_WithUnknownUrl_ShouldThrowNotSupportedException(string url)
    {
        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => _factory.GetProvider(url));
        Assert.Contains("No provider found that can handle the URL", exception.Message);
        Assert.Contains(url, exception.Message);
        Assert.Contains("Supported provider types:", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetProvider_WithNullOrEmptyUrl_ShouldThrowArgumentNullException(string? url)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.GetProvider(url!));
    }

    [Fact]
    public void GetProvider_WithGitHubProviderType_ShouldReturnGitHubProvider()
    {
        // Act
        var provider = _factory.GetProvider(ProviderType.GitHub);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.GitHub, provider.ProviderType);
        Assert.Same(_mockGitHubProvider.Object, provider);
    }

    [Fact]
    public void GetProvider_WithGitLabProviderType_ShouldReturnGitLabProvider()
    {
        // Act
        var provider = _factory.GetProvider(ProviderType.GitLab);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.GitLab, provider.ProviderType);
        Assert.Same(_mockGitLabProvider.Object, provider);
    }

    [Fact]
    public void GetProvider_WithLocalProviderType_ShouldReturnLocalProvider()
    {
        // Act
        var provider = _factory.GetProvider(ProviderType.Local);

        // Assert
        Assert.NotNull(provider);
        Assert.Equal(ProviderType.Local, provider.ProviderType);
        Assert.Same(_mockLocalProvider.Object, provider);
    }

    [Fact]
    public void GetProvider_WithUnknownProviderType_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => _factory.GetProvider(ProviderType.Unknown));
        Assert.Contains("Cannot create provider for Unknown provider type", exception.Message);
    }

    [Fact]
    public void GetProvider_WithUnsupportedProviderType_ShouldThrowNotSupportedException()
    {
        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => _factory.GetProvider(ProviderType.Bitbucket));
        Assert.Contains("No provider found for type 'Bitbucket'", exception.Message);
        Assert.Contains("Supported provider types:", exception.Message);
    }

    [Fact]
    public void SupportedProviders_ShouldReturnAllRegisteredProviderTypes()
    {
        // Act
        var supportedProviders = _factory.SupportedProviders.ToList();

        // Assert
        Assert.Contains(ProviderType.GitHub, supportedProviders);
        Assert.Contains(ProviderType.GitLab, supportedProviders);
        Assert.Contains(ProviderType.Local, supportedProviders);
        Assert.DoesNotContain(ProviderType.Unknown, supportedProviders);
        Assert.Equal(3, supportedProviders.Count);
    }

    [Fact]
    public void SupportedProviders_WithEmptyProvidersList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyFactory = new GitProviderFactory(new List<IGitProviderService>());

        // Act
        var supportedProviders = emptyFactory.SupportedProviders.ToList();

        // Assert
        Assert.Empty(supportedProviders);
    }

    [Fact]
    public void SupportedProviders_ShouldExcludeUnknownProviderType()
    {
        // Arrange
        var mockUnknownProvider = new Mock<IGitProviderService>();
        mockUnknownProvider.Setup(p => p.ProviderType).Returns(ProviderType.Unknown);

        var providers = new List<IGitProviderService>
        {
            _mockGitHubProvider.Object,
            mockUnknownProvider.Object
        };

        var factory = new GitProviderFactory(providers);

        // Act
        var supportedProviders = factory.SupportedProviders.ToList();

        // Assert
        Assert.Contains(ProviderType.GitHub, supportedProviders);
        Assert.DoesNotContain(ProviderType.Unknown, supportedProviders);
        Assert.Single(supportedProviders);
    }

    [Fact]
    public void SupportedProviders_ShouldReturnDistinctProviderTypes()
    {
        // Arrange - Create two instances of the same provider type
        var mockGitHubProvider2 = new Mock<IGitProviderService>();
        mockGitHubProvider2.Setup(p => p.ProviderType).Returns(ProviderType.GitHub);

        var providers = new List<IGitProviderService>
        {
            _mockGitHubProvider.Object,
            mockGitHubProvider2.Object,
            _mockGitLabProvider.Object
        };

        var factory = new GitProviderFactory(providers);

        // Act
        var supportedProviders = factory.SupportedProviders.ToList();

        // Assert
        Assert.Contains(ProviderType.GitHub, supportedProviders);
        Assert.Contains(ProviderType.GitLab, supportedProviders);
        Assert.Equal(2, supportedProviders.Count); // Should be distinct
    }
}
