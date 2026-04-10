using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RepoLens.Api.Hubs;

[Authorize]
public class MetricsHub : Hub
{
    private readonly ILogger<MetricsHub> _logger;

    public MetricsHub(ILogger<MetricsHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        
        _logger.LogInformation("User {UserEmail} connected to MetricsHub with connection {ConnectionId}", 
            userEmail ?? "Unknown", Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
        
        _logger.LogInformation("User {UserEmail} disconnected from MetricsHub with connection {ConnectionId}", 
            userEmail ?? "Unknown", Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific repository group to receive updates for that repository
    /// </summary>
    /// <param name="repositoryId">The repository ID to join</param>
    public async Task JoinRepositoryGroup(string repositoryId)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("User {UserEmail} joined repository group {GroupName}", 
                userEmail ?? "Unknown", groupName);
                
            await Clients.Caller.SendAsync("JoinedRepositoryGroup", repositoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining repository group {RepositoryId}", repositoryId);
            await Clients.Caller.SendAsync("Error", "Failed to join repository group");
        }
    }

    /// <summary>
    /// Leave a specific repository group
    /// </summary>
    /// <param name="repositoryId">The repository ID to leave</param>
    public async Task LeaveRepositoryGroup(string repositoryId)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("User {UserEmail} left repository group {GroupName}", 
                userEmail ?? "Unknown", groupName);
                
            await Clients.Caller.SendAsync("LeftRepositoryGroup", repositoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving repository group {RepositoryId}", repositoryId);
            await Clients.Caller.SendAsync("Error", "Failed to leave repository group");
        }
    }

    /// <summary>
    /// Join the general dashboard group to receive overall system updates
    /// </summary>
    public async Task JoinDashboardGroup()
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
            
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("User {UserEmail} joined dashboard group", userEmail ?? "Unknown");
            
            await Clients.Caller.SendAsync("JoinedDashboardGroup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining dashboard group");
            await Clients.Caller.SendAsync("Error", "Failed to join dashboard group");
        }
    }

    /// <summary>
    /// Leave the dashboard group
    /// </summary>
    public async Task LeaveDashboardGroup()
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
            
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("User {UserEmail} left dashboard group", userEmail ?? "Unknown");
            
            await Clients.Caller.SendAsync("LeftDashboardGroup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving dashboard group");
            await Clients.Caller.SendAsync("Error", "Failed to leave dashboard group");
        }
    }
}

/// <summary>
/// Service for sending real-time updates via SignalR
/// </summary>
public interface IMetricsNotificationService
{
    Task SendRepositoryStatusUpdateAsync(int repositoryId, string status, string? message = null);
    Task SendMetricsUpdateAsync(int repositoryId, object metrics);
    Task SendRepositorySyncProgressAsync(int repositoryId, int percentage, string currentStep);
    Task SendDashboardUpdateAsync(object dashboardData);
    Task SendRepositoryErrorAsync(int repositoryId, string errorMessage);
}

/// <summary>
/// Implementation of the metrics notification service using SignalR
/// </summary>
public class MetricsNotificationService : IMetricsNotificationService
{
    private readonly IHubContext<MetricsHub> _hubContext;
    private readonly ILogger<MetricsNotificationService> _logger;

    public MetricsNotificationService(
        IHubContext<MetricsHub> hubContext,
        ILogger<MetricsNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendRepositoryStatusUpdateAsync(int repositoryId, string status, string? message = null)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            var update = new
            {
                repositoryId,
                status,
                message,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync("RepositoryStatusUpdate", update);
            
            _logger.LogDebug("Sent repository status update for repository {RepositoryId}: {Status}", 
                repositoryId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending repository status update for repository {RepositoryId}", repositoryId);
        }
    }

    public async Task SendMetricsUpdateAsync(int repositoryId, object metrics)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            var update = new
            {
                repositoryId,
                metrics,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync("MetricsUpdate", update);
            
            _logger.LogDebug("Sent metrics update for repository {RepositoryId}", repositoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending metrics update for repository {RepositoryId}", repositoryId);
        }
    }

    public async Task SendRepositorySyncProgressAsync(int repositoryId, int percentage, string currentStep)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            var progress = new
            {
                repositoryId,
                percentage,
                currentStep,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync("SyncProgress", progress);
            
            _logger.LogDebug("Sent sync progress for repository {RepositoryId}: {Percentage}% - {Step}", 
                repositoryId, percentage, currentStep);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending sync progress for repository {RepositoryId}", repositoryId);
        }
    }

    public async Task SendDashboardUpdateAsync(object dashboardData)
    {
        try
        {
            var update = new
            {
                data = dashboardData,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("dashboard").SendAsync("DashboardUpdate", update);
            
            _logger.LogDebug("Sent dashboard update to all dashboard subscribers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending dashboard update");
        }
    }

    public async Task SendRepositoryErrorAsync(int repositoryId, string errorMessage)
    {
        try
        {
            var groupName = $"repository_{repositoryId}";
            var error = new
            {
                repositoryId,
                error = errorMessage,
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group(groupName).SendAsync("RepositoryError", error);
            
            _logger.LogDebug("Sent repository error for repository {RepositoryId}: {ErrorMessage}", 
                repositoryId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending repository error for repository {RepositoryId}", repositoryId);
        }
    }
}
