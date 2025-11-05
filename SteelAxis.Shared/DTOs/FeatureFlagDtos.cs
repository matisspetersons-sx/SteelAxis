using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.DTOs;

/// <summary>
/// Feature flag DTO
/// </summary>
public record FeatureFlagDto
{
    public Guid Id { get; init; }
    public string FeatureKey { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string SubscriptionTier { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request to create a new feature flag
/// </summary>
public record CreateFeatureRequest
{
    [Required]
    [StringLength(100)]
    public string FeatureKey { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    public bool IsEnabled { get; init; } = true;

    [Required]
    [RegularExpression("^(Free|Basic|Professional|Enterprise)$")]
    public string SubscriptionTier { get; init; } = "Free";
}

/// <summary>
/// Request to update a feature flag
/// </summary>
public record UpdateFeatureRequest
{
    [StringLength(200)]
    public string? DisplayName { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    public bool? IsEnabled { get; init; }

    [RegularExpression("^(Free|Basic|Professional|Enterprise)$")]
    public string? SubscriptionTier { get; init; }
}
