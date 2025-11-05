using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Directory.Models;

/// <summary>
/// Represents a tenant (company/organization) in the SteelAxis platform
/// Stored in the central Directory database
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant company/organization name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional domain for tenant identification (e.g., "acmesteel")
    /// </summary>
    [MaxLength(100)]
    public string? Domain { get; set; }

    /// <summary>
    /// Subscription tier: Starter, Professional, Enterprise, Custom
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SubscriptionTier { get; set; } = "Starter";

    /// <summary>
    /// Connection string to tenant's dedicated database
    /// Stored encrypted in production
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Whether the tenant account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a demo/trial tenant
    /// </summary>
    public bool IsTrial { get; set; } = false;

    /// <summary>
    /// When the trial period ends (null if not a trial)
    /// </summary>
    public DateTime? TrialEndsAt { get; set; }

    /// <summary>
    /// When the tenant account was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who created the tenant (first admin)
    /// </summary>
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// When the tenant was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last updated the tenant
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Navigation property: Users belonging to this tenant
    /// </summary>
    public virtual ICollection<UserProfile> Users { get; set; } = new List<UserProfile>();

    /// <summary>
    /// Navigation property: Feature flags for this tenant
    /// </summary>
    public virtual ICollection<FeatureFlag> FeatureFlags { get; set; } = new List<FeatureFlag>();

    /// <summary>
    /// Navigation property: User invitations for this tenant
    /// </summary>
    public virtual ICollection<UserInvitation> Invitations { get; set; } = new List<UserInvitation>();
}

/// <summary>
/// Subscription tier constants
/// </summary>
public static class SubscriptionTiers
{
    public const string Starter = "Starter";
    public const string Professional = "Professional";
    public const string Enterprise = "Enterprise";
    public const string Custom = "Custom";

    public static readonly string[] AllTiers = { Starter, Professional, Enterprise, Custom };
}
