using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteelAxis.Directory.Models;

/// <summary>
/// Represents a user profile in the SteelAxis platform
/// Stored in the central Directory database
/// Links to Microsoft Entra External ID user account
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Unique identifier for the user profile
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant this user belongs to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Microsoft Entra External ID user object ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntraUserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (from Entra ID)
    /// </summary>
    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// User's role within the tenant
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = UserRoles.ShopfloorWorker;

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the user profile was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created this profile (inviter)
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the user profile was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated this profile
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// When the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Navigation property: Tenant this user belongs to
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Full name of the user
    /// </summary>
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Display name (full name or email if name not set)
    /// </summary>
    [NotMapped]
    public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : Email;
}

/// <summary>
/// User role constants for RBAC
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Full system access, can manage tenant and users
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Strategic oversight, reporting, high-level project management
    /// </summary>
    public const string UpperManagement = "UpperManagement";

    /// <summary>
    /// Operational management, project execution, resource allocation
    /// </summary>
    public const string LowerManagement = "LowerManagement";

    /// <summary>
    /// Team leadership, task coordination, quality oversight
    /// </summary>
    public const string Supervisor = "Supervisor";

    /// <summary>
    /// Production work, fabrication tasks
    /// </summary>
    public const string ShopfloorWorker = "ShopfloorWorker";

    /// <summary>
    /// Administrative tasks, documentation, coordination
    /// </summary>
    public const string OfficeWorker = "OfficeWorker";

    /// <summary>
    /// All available roles
    /// </summary>
    public static readonly string[] AllRoles = 
    { 
        Admin, 
        UpperManagement, 
        LowerManagement, 
        Supervisor, 
        ShopfloorWorker, 
        OfficeWorker 
    };

    /// <summary>
    /// Roles that can manage users
    /// </summary>
    public static readonly string[] ManagementRoles = 
    { 
        Admin, 
        UpperManagement, 
        LowerManagement 
    };
}
