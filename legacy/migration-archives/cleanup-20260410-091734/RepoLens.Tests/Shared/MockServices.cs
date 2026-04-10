using RepoLens.Api.Hubs;
using RepoLens.Core.Entities;

namespace RepoLens.Tests.Shared;

/// <summary>
/// Mock metrics notification service for testing
/// </summary>
public class MockMetricsNotificationService : IMetricsNotificationService
{
    public List<NotificationCall> NotificationCalls { get; } = new();
    
    public Task SendRepositoryStatusUpdateAsync(int repositoryId, RepositoryStatus status)
    {
        NotificationCalls.Add(new NotificationCall("StatusUpdate", repositoryId, status));
        return Task.CompletedTask;
    }
    
    public Task SendRepositoryStatusUpdateAsync(int repositoryId, string status, string? message = null)
    {
        NotificationCalls.Add(new NotificationCall("StatusUpdateString", repositoryId, status));
        return Task.CompletedTask;
    }
    
    public Task SendMetricsUpdateAsync(int repositoryId, object metrics)
    {
        NotificationCalls.Add(new NotificationCall("MetricsUpdate", repositoryId, metrics));
        return Task.CompletedTask;
    }
    
    public Task SendRepositorySyncProgressAsync(int repositoryId, int progressPercentage, string status)
    {
        NotificationCalls.Add(new NotificationCall("SyncProgress", repositoryId, new { progressPercentage, status }));
        return Task.CompletedTask;
    }
    
    public Task SendDashboardUpdateAsync(object dashboardData)
    {
        NotificationCalls.Add(new NotificationCall("DashboardUpdate", 0, dashboardData));
        return Task.CompletedTask;
    }
    
    public Task SendRepositoryErrorAsync(int repositoryId, string errorMessage)
    {
        NotificationCalls.Add(new NotificationCall("RepositoryError", repositoryId, errorMessage));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents a notification call for testing
/// </summary>
public class NotificationCall
{
    public string Method { get; }
    public int RepositoryId { get; }
    public object? Data { get; }
    public DateTime Timestamp { get; }

    public NotificationCall(string method, int repositoryId, object? data)
    {
        Method = method;
        RepositoryId = repositoryId;
        Data = data;
        Timestamp = DateTime.UtcNow;
    }
}
