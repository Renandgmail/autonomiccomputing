using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RepoLens.Api.Controllers;
using RepoLens.Api.Models;
using RepoLens.Core.Services;
using RepoLens.Core.Entities;
using Xunit;

namespace RepoLens.Tests.Controllers;

public class SearchControllerTests
{
    private readonly Mock<IQueryProcessingService> _mockQueryProcessingService;
    private readonly Mock<ILogger<SearchController>> _mockLogger;
    private readonly SearchController _controller;

    public SearchControllerTests()
    {
        _mockQueryProcessingService = new Mock<IQueryProcessingService>();
        _mockLogger = new Mock<ILogger<SearchController>>();
        _controller = new SearchController(_mockQueryProcessingService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessQuery_WithEmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var request = new NaturalLanguageQueryRequest
        {
            Query = "",
            RepositoryId = 1
        };

        // Act
        var result = await _controller.ProcessQuery(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Query cannot be empty", response.ErrorMessage);
    }

    [Fact]
    public async Task GetSuggestions_WithValidQuery_ReturnsOkResult()
    {
        // Arrange
        var partialQuery = "find auth";
        var repositoryId = 1;
        var suggestions = new[] { "find authentication", "find authorization", "find auth methods" };

        _mockQueryProcessingService
            .Setup(x => x.GetSuggestionsAsync(partialQuery, repositoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestions);

        // Act
        var result = await _controller.GetSuggestions(partialQuery, repositoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);

        _mockQueryProcessingService.Verify(
            x => x.GetSuggestionsAsync(partialQuery, repositoryId, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task GetSuggestions_WithEmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetSuggestions("", 1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Partial query cannot be empty", response.ErrorMessage);
    }

    [Fact]
    public void AnalyzeIntent_WithEmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var request = new QueryIntentRequest
        {
            Query = ""
        };

        // Act
        var result = _controller.AnalyzeIntent(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Query cannot be empty", response.ErrorMessage);
    }

    [Fact]
    public void GetExampleQueries_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetExampleQueries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
    }
}
