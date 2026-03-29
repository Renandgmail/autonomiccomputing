# Development Guidelines and Standards

> **CONSOLIDATED DEVELOPMENT DOCUMENTATION**
> 
> Complete development guidelines, architectural standards, and implementation instructions for maintaining consistency across the RepoLens code intelligence platform.

---

## 🏗️ **ARCHITECTURAL STANDARDS**

### **Clean Architecture Principles**

#### **Layer Separation Guidelines**
```
RepoLens.Core/
├── Entities/          # Domain models (Repository, CodeElement, etc.)
├── Services/          # Business logic interfaces
├── Repositories/      # Data access interfaces
└── Exceptions/        # Custom domain exceptions

RepoLens.Infrastructure/
├── Repositories/      # Data access implementations
├── Services/         # Business logic implementations
├── Providers/        # External service integrations
└── Migrations/       # Database schema changes

RepoLens.Api/
├── Controllers/      # REST API endpoints
├── Models/          # Request/Response DTOs
├── Services/        # API-specific services
└── Hubs/           # SignalR hubs
```

#### **Dependency Flow Rules**
- ✅ **Core** depends on nothing external
- ✅ **Infrastructure** depends on Core only
- ✅ **API** depends on Core and Infrastructure
- ❌ **Never** reverse dependencies (Core cannot depend on Infrastructure)

### **Entity Framework Patterns**

#### **Entity Design Standards**
```csharp
// ✅ FOLLOW this pattern for all entities
public class EntityName
{
    public int Id { get; set; } // Always int, never Guid
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty; // Always initialize strings
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Consistent timestamp pattern
    public DateTime? UpdatedAt { get; set; } // Nullable for optional timestamps
    
    // Navigation properties - always virtual and initialized
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
}
```

#### **DbContext Configuration Standards**
```csharp
// ✅ FOLLOW this pattern in OnModelCreating
modelBuilder.Entity<EntityName>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.Name).IsUnique(); // Add appropriate indexes
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    
    // JSON columns for PostgreSQL
    entity.Property(e => e.JsonField).HasColumnType("TEXT");
    
    // Relationships
    entity.HasOne(e => e.RelatedEntity)
          .WithMany(r => r.Entities)
          .HasForeignKey(e => e.RelatedEntityId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

### **Service Layer Patterns**

#### **Interface Naming Conventions**
```csharp
// ✅ CORRECT patterns found in RepoLens
public interface IRepositoryAnalysisService     // Feature + Service
public interface IRealMetricsCollectionService // Real + Feature + Service
public interface IFileAnalysisService          // Domain + Service

// ❌ AVOID these patterns
public interface ICodeScanner                  // Missing Service suffix
public interface IProcessor                    // Too generic
```

#### **Service Implementation Standards**
```csharp
public class ServiceName : IServiceName
{
    private readonly IDependency _dependency;
    private readonly ILogger<ServiceName> _logger;

    public ServiceName(IDependency dependency, ILogger<ServiceName> logger)
    {
        _dependency = dependency;
        _logger = logger;
    }

    public async Task<Result> MethodAsync(Parameters parameters, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting operation with {Parameter}", parameters.Value);
            
            // Business logic
            var result = await _dependency.ProcessAsync(parameters, ct);
            
            _logger.LogInformation("Operation completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed for {Parameter}", parameters.Value);
            throw;
        }
    }
}
```

---

## 🌐 **API DEVELOPMENT STANDARDS**

### **Controller Patterns**

#### **REST API Design Guidelines**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Always require authorization unless specifically public
public class EntityController : ControllerBase
{
    private readonly IEntityService _entityService;
    private readonly ILogger<EntityController> _logger;

    public EntityController(IEntityService entityService, ILogger<EntityController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all entities with optional filtering
    /// </summary>
    /// <param name="filters">Optional filtering criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of entities</returns>
    /// <response code="200">Returns the list of entities</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet]
    public async Task<IActionResult> GetEntities([FromQuery] EntityFilters? filters = null, CancellationToken ct = default)
    {
        try
        {
            var entities = await _entityService.GetEntitiesAsync(filters, ct);
            var response = entities.Select(MapToViewModel).ToList();
            
            return Ok(ApiResponse<List<EntityViewModel>>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entities");
            return StatusCode(500, ApiResponse<List<EntityViewModel>>.ErrorResult("Failed to retrieve entities"));
        }
    }
}
```

#### **Response Wrapper Pattern**
```csharp
// ✅ ALWAYS use ApiResponse<T> wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> ErrorResult(string error) => new() { Success = false, Error = error };
}

// Usage in controllers
return Ok(ApiResponse<EntityViewModel>.SuccessResult(entity));
return BadRequest(ApiResponse<EntityViewModel>.ErrorResult("Invalid input"));
```

### **Request/Response Models**

#### **DTO Naming Conventions**
```csharp
// Requests
public class CreateEntityRequest { }
public class UpdateEntityRequest { }
public class EntityFilters { }

// Responses  
public class EntityViewModel { }
public class EntitySummaryViewModel { }
public class EntityDetailsViewModel { }
```

---

## 🎨 **FRONTEND DEVELOPMENT STANDARDS**

### **React Component Patterns**

#### **Component Structure Standards**
```tsx
import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  CircularProgress
} from '@mui/material';
import { IconName } from '@mui/icons-material';

interface ComponentProps {
  requiredProp: string;
  optionalProp?: number;
}

const ComponentName: React.FC<ComponentProps> = ({ requiredProp, optionalProp = 0 }) => {
  const [data, setData] = useState<DataType | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadData();
  }, [requiredProp]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // API call
      const response = await fetch(`/api/endpoint/${requiredProp}`);
      if (!response.ok) throw new Error('Failed to load data');
      
      const result = await response.json();
      setData(result.data);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAction = async () => {
    try {
      setIsLoading(true);
      // Action implementation
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Action failed');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Card>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconName sx={{ mr: 2, color: 'primary.main' }} />
          <Typography variant="h6">Title</Typography>
          <Box sx={{ flexGrow: 1 }} />
          <Button variant="contained" onClick={handleAction} disabled={isLoading}>
            Action
          </Button>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {data && (
          <Box>
            {/* Content rendering */}
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default ComponentName;
```

#### **Material-UI Theme Integration**
```tsx
// ✅ FOLLOW existing theme patterns
const theme = {
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    h6: {
      fontWeight: 600,
    },
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        },
      },
    },
  },
};
```

### **State Management Standards**

#### **useState Patterns**
```tsx
// ✅ CORRECT patterns
const [data, setData] = useState<DataType | null>(null);
const [isLoading, setIsLoading] = useState(false);
const [errors, setErrors] = useState<string[]>([]);

// ❌ AVOID these patterns
const [state, setState] = useState({}); // Too generic
const [loading, setLoading] = useState(true); // Should start false
```

---

## 🧪 **TESTING STANDARDS**

### **Unit Test Patterns**

#### **Service Test Standards**
```csharp
[TestClass]
public class ServiceNameTests
{
    private Mock<IDependency> _mockDependency;
    private Mock<ILogger<ServiceName>> _mockLogger;
    private ServiceName _service;

    [TestInitialize]
    public void Setup()
    {
        _mockDependency = new Mock<IDependency>();
        _mockLogger = new Mock<ILogger<ServiceName>>();
        _service = new ServiceName(_mockDependency.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task MethodName_WithValidInput_ShouldReturnExpectedResult()
    {
        // Arrange
        var input = new InputType { Value = "test" };
        var expectedResult = new ResultType { Success = true };
        
        _mockDependency
            .Setup(x => x.ProcessAsync(It.IsAny<InputType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.MethodAsync(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedResult.Success, result.Success);
        _mockDependency.Verify(x => x.ProcessAsync(input, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task MethodName_WithInvalidInput_ShouldThrowException()
    {
        // Arrange
        var invalidInput = new InputType { Value = null };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
            () => _service.MethodAsync(invalidInput)
        );
    }
}
```

#### **Controller Test Standards**
```csharp
[TestClass]
public class ControllerNameTests
{
    private Mock<IService> _mockService;
    private Mock<ILogger<ControllerName>> _mockLogger;
    private ControllerName _controller;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<IService>();
        _mockLogger = new Mock<ILogger<ControllerName>>();
        _controller = new ControllerName(_mockService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetEntity_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var entityId = 1;
        var entity = new Entity { Id = entityId, Name = "Test" };
        
        _mockService
            .Setup(x => x.GetEntityAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _controller.GetEntity(entityId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        var response = okResult.Value as ApiResponse<EntityViewModel>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(entityId, response.Data.Id);
    }
}
```

### **Integration Test Patterns**

#### **API Integration Tests**
```csharp
[TestClass]
public class EntityIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task CreateEntity_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new CreateEntityRequest
        {
            Name = "Test Entity",
            Description = "Test Description"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/entities", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<EntityViewModel>>(content);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(request.Name, result.Data.Name);
    }
}
```

---

## 🚀 **DEPLOYMENT STANDARDS**

### **Docker Configuration Standards**

#### **Dockerfile Best Practices**
```dockerfile
# Use specific version tags, not 'latest'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install dependencies in order of change frequency (least to most)
RUN apt-get update && apt-get install -y \
    git \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy application files
COPY --from=build /app/publish .

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
RUN chown -R appuser:appgroup /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

EXPOSE 80
ENTRYPOINT ["dotnet", "RepoLens.Api.dll"]
```

#### **docker-compose Standards**
```yaml
version: '3.8'

services:
  repolens-api:
    build: 
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT:-Development}
      - ConnectionStrings__DefaultConnection=${CONNECTION_STRING}
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:
    driver: local
```

---

## 📊 **MONITORING AND LOGGING STANDARDS**

### **Structured Logging Patterns**

#### **Log Level Guidelines**
```csharp
// ✅ CORRECT usage
_logger.LogInformation("User {UserId} accessed repository {RepositoryId}", userId, repositoryId);
_logger.LogWarning("Repository scan took longer than expected: {Duration}ms", duration);
_logger.LogError(ex, "Failed to process repository {RepositoryId}", repositoryId);

// ❌ AVOID these patterns
_logger.LogInformation($"User {userId} accessed repository"); // String interpolation
_logger.LogError("An error occurred"); // No context
_logger.LogDebug("Debug info"); // In production code
```

#### **Performance Monitoring**
```csharp
public async Task<Result> MethodAsync(Parameters parameters)
{
    using var activity = ActivitySource.StartActivity("MethodName");
    activity?.SetTag("parameter.value", parameters.Value);
    
    var stopwatch = Stopwatch.StartNew();
    try
    {
        var result = await ProcessAsync(parameters);
        
        _logger.LogInformation("Method completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Method failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
        throw;
    }
}
```

---

## 🔒 **SECURITY STANDARDS**

### **Authentication and Authorization**

#### **JWT Configuration**
```csharp
// Program.cs configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
        };
    });
```

#### **Authorization Policies**
```csharp
// Define policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireRepositoryAccess", policy => 
        policy.Requirements.Add(new RepositoryAccessRequirement()));
});

// Use in controllers
[Authorize(Policy = "RequireRepositoryAccess")]
[HttpGet("{id}")]
public async Task<IActionResult> GetRepository(int id) { }
```

### **Input Validation Standards**

#### **Model Validation**
```csharp
public class CreateEntityRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Url]
    public string? WebsiteUrl { get; set; }

    [Range(1, 100)]
    public int Priority { get; set; }
}
```

---

## 📚 **DOCUMENTATION STANDARDS**

### **API Documentation Requirements**

#### **XML Documentation**
```csharp
/// <summary>
/// Creates a new repository and starts initial analysis
/// </summary>
/// <param name="request">Repository creation details</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Created repository with initial metadata</returns>
/// <response code="201">Repository created successfully</response>
/// <response code="400">Invalid repository URL or validation failed</response>
/// <response code="409">Repository already exists</response>
/// <response code="401">If the user is not authenticated</response>
[HttpPost]
[ProducesResponseType(typeof(ApiResponse<RepositoryViewModel>), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryRequest request, CancellationToken ct = default)
```

### **README Standards**

#### **Component Documentation Template**
```markdown
# Component Name

## Overview
Brief description of what this component does and its purpose in the system.

## Features
- Feature 1: Description
- Feature 2: Description

## Usage
```typescript
import ComponentName from './ComponentName';

<ComponentName 
  requiredProp="value"
  optionalProp={123}
/>
```

## Props
| Prop | Type | Required | Description |
|------|------|----------|-------------|
| requiredProp | string | Yes | Description of required prop |
| optionalProp | number | No | Description with default value |

## Examples
Practical usage examples with code snippets.
```

These development guidelines and standards ensure consistency, maintainability, and quality across the entire RepoLens codebase while providing clear patterns for all team members to follow.
