using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.DTOs;

/// <summary>
/// Data transfer object for tenant information
/// </summary>
public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Domain { get; init; }
    public string SubscriptionTier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsTrial { get; init; }
    public DateTime? TrialEndsAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public int UserCount { get; init; }
    public int ActiveUserCount { get; init; }
}

/// <summary>
/// Request to create a new tenant (admin registration)
/// </summary>
public record CreateTenantRequest
{
    /// <summary>
    /// Company/organization name
    /// </summary>
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 200 characters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional domain identifier (e.g., "acmesteel")
    /// </summary>
    [StringLength(100, ErrorMessage = "Domain must be less than 100 characters")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Domain can only contain lowercase letters, numbers, and hyphens")]
    public string? Domain { get; init; }

    /// <summary>
    /// Subscription tier
    /// </summary>
    [Required(ErrorMessage = "Subscription tier is required")]
    public string SubscriptionTier { get; init; } = "Starter";

    /// <summary>
    /// Whether this is a trial account
    /// </summary>
    public bool IsTrial { get; init; } = false;

    /// <summary>
    /// Trial duration in days (default: 30)
    /// </summary>
    public int TrialDays { get; init; } = 30;

    /// <summary>
    /// Admin user email
    /// </summary>
    [Required(ErrorMessage = "Admin email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string AdminEmail { get; init; } = string.Empty;

    /// <summary>
    /// Admin user first name
    /// </summary>
    [StringLength(100, ErrorMessage = "First name must be less than 100 characters")]
    public string? AdminFirstName { get; init; }

    /// <summary>
    /// Admin user last name
    /// </summary>
    [StringLength(100, ErrorMessage = "Last name must be less than 100 characters")]
    public string? AdminLastName { get; init; }
}

/// <summary>
/// Request to update tenant information
/// </summary>
public record UpdateTenantRequest
{
    /// <summary>
    /// Company/organization name
    /// </summary>
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 200 characters")]
    public string? Name { get; init; }

    /// <summary>
    /// Subscription tier
    /// </summary>
    public string? SubscriptionTier { get; init; }

    /// <summary>
    /// Whether the tenant is active
    /// </summary>
    public bool? IsActive { get; init; }
}

/// <summary>
/// Response for tenant with feature flags
/// </summary>
public record TenantWithFeaturesDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Domain { get; init; }
    public string SubscriptionTier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsTrial { get; init; }
    public DateTime? TrialEndsAt { get; init; }
    public Dictionary<string, bool> Features { get; init; } = new();
}
