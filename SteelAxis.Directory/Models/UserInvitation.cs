using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteelAxis.Directory.Models;

/// <summary>
/// Represents a user invitation to join a tenant
/// Stored in the central Directory database
/// </summary>
public class UserInvitation
{
    /// <summary>
    /// Unique identifier for the invitation
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant the user is being invited to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Email address of the invited user
    /// </summary>
    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name of the invited user
    /// </summary>
    [MaxLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the invited user
    /// </summary>
    [MaxLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Role the user will have when they accept
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = UserRoles.ShopfloorWorker;

    /// <summary>
    /// User ID who sent the invitation
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InvitedBy { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user who sent the invitation
    /// </summary>
    [MaxLength(200)]
    public string? InvitedByName { get; set; }

    /// <summary>
    /// Current status of the invitation
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// Unique token for invitation acceptance
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InvitationToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// When the invitation was created/sent
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the invitation expires
    /// Default: 7 days from creation
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>
    /// When the invitation was accepted
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// User ID who accepted the invitation
    /// </summary>
    [MaxLength(100)]
    public string? AcceptedBy { get; set; }

    /// <summary>
    /// When the invitation was cancelled
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// User ID who cancelled the invitation
    /// </summary>
    [MaxLength(100)]
    public string? CancelledBy { get; set; }

    /// <summary>
    /// Optional message to include in the invitation email
    /// </summary>
    [MaxLength(500)]
    public string? Message { get; set; }

    /// <summary>
    /// Navigation property: Tenant this invitation belongs to
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// Whether the invitation is still valid
    /// </summary>
    [NotMapped]
    public bool IsValid => Status == InvitationStatus.Pending
                           && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Whether the invitation has expired
    /// </summary>
    [NotMapped]
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
}

/// <summary>
/// Invitation status constants
/// </summary>
public static class InvitationStatus
{
    /// <summary>
    /// Invitation sent, awaiting acceptance
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// Invitation accepted, user account created
    /// </summary>
    public const string Accepted = "Accepted";

    /// <summary>
    /// Invitation expired (not accepted within time limit)
    /// </summary>
    public const string Expired = "Expired";

    /// <summary>
    /// Invitation cancelled by admin
    /// </summary>
    public const string Cancelled = "Cancelled";

    /// <summary>
    /// All available statuses
    /// </summary>
    public static readonly string[] AllStatuses =
    {
        Pending,
        Accepted,
        Expired,
        Cancelled
    };
}
