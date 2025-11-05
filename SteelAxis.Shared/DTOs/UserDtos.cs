using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.DTOs;

/// <summary>
/// Data transfer object for user profile information
/// </summary>
public record UserProfileDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>
/// Request to invite a new user to the tenant
/// </summary>
public record InviteUserRequest
{
    /// <summary>
    /// Email address of the user to invite
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256, ErrorMessage = "Email must be less than 256 characters")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// First name of the user
    /// </summary>
    [StringLength(100, ErrorMessage = "First name must be less than 100 characters")]
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    [StringLength(100, ErrorMessage = "Last name must be less than 100 characters")]
    public string? LastName { get; init; }

    /// <summary>
    /// Role to assign to the user
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; init; } = "ShopfloorWorker";

    /// <summary>
    /// Optional message to include in invitation email
    /// </summary>
    [StringLength(500, ErrorMessage = "Message must be less than 500 characters")]
    public string? Message { get; init; }
}

/// <summary>
/// Request to update user profile
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// First name
    /// </summary>
    [StringLength(100, ErrorMessage = "First name must be less than 100 characters")]
    public string? FirstName { get; init; }

    /// <summary>
    /// Last name
    /// </summary>
    [StringLength(100, ErrorMessage = "Last name must be less than 100 characters")]
    public string? LastName { get; init; }

    /// <summary>
    /// Role
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool? IsActive { get; init; }
}

/// <summary>
/// Response for current user with tenant information
/// </summary>
public record CurrentUserDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string SubscriptionTier { get; init; } = string.Empty;
    public bool IsTrial { get; init; }
    public Dictionary<string, bool> Features { get; init; } = new();
}
