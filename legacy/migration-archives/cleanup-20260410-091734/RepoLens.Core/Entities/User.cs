using Microsoft.AspNetCore.Identity;

namespace RepoLens.Core.Entities;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public int? OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }
    public string? TimeZone { get; set; }
    public UserPreferences Preferences { get; set; } = new();
    
    // Navigation properties
    public virtual Organization? Organization { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Repository> OwnedRepositories { get; set; } = new List<Repository>();
}

public class Role : IdentityRole<int>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSystemRole { get; set; } = false;
    public List<string> Permissions { get; set; } = new();
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole : IdentityUserRole<int>
{
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int? AssignedByUserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual User? AssignedByUser { get; set; }
}

public class Organization
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public OrganizationSettings Settings { get; set; } = new();
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Repository> Repositories { get; set; } = new List<Repository>();
}

public class UserPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public string Theme { get; set; } = "light"; // light, dark, auto
    public string Language { get; set; } = "en";
    public int DashboardRefreshInterval { get; set; } = 300; // seconds
    public List<string> FavoriteRepositories { get; set; } = new();
}

public class OrganizationSettings
{
    public bool AllowUserRegistration { get; set; } = false;
    public bool RequireEmailVerification { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 480; // 8 hours
    public bool EnableAuditLogging { get; set; } = true;
    public string? SsoProvider { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

// Authorization constants
public static class Permissions
{
    // Repository permissions
    public const string ViewRepositories = "repositories.view";
    public const string ManageRepositories = "repositories.manage";
    public const string DeleteRepositories = "repositories.delete";
    
    // Metrics permissions
    public const string ViewMetrics = "metrics.view";
    public const string ExportMetrics = "metrics.export";
    public const string ManageQualityGates = "metrics.quality_gates.manage";
    
    // User management permissions
    public const string ViewUsers = "users.view";
    public const string ManageUsers = "users.manage";
    public const string DeleteUsers = "users.delete";
    
    // Organization permissions
    public const string ManageOrganization = "organization.manage";
    public const string ViewAuditLogs = "organization.audit_logs.view";
    
    // System permissions
    public const string SystemAdministrator = "system.admin";
}

public static class Roles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string OrganizationAdmin = "OrganizationAdmin";
    public const string Manager = "Manager";
    public const string Developer = "Developer";
    public const string Viewer = "Viewer";
}
