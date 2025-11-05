using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteelAxis.Directory.Models;

/// <summary>
/// Represents a feature flag for a tenant
/// Controls access to features based on subscription tier
/// Stored in the central Directory database
/// </summary>
public class FeatureFlag
{
    /// <summary>
    /// Unique identifier for the feature flag
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tenant this feature flag applies to
    /// </summary>
    [Required]
    public Guid TenantId { get; set; }

    /// <summary>
    /// Name of the feature (e.g., "EN1090Compliance", "AdvancedReporting")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FeatureName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this feature is enabled for the tenant
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Optional JSON configuration for the feature
    /// Can store feature-specific settings
    /// </summary>
    public string? ConfigJson { get; set; }

    /// <summary>
    /// When the feature flag was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the feature flag was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property: Tenant this feature flag belongs to
    /// </summary>
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }
}

/// <summary>
/// Feature name constants
/// </summary>
public static class FeatureNames
{
    // Core Features (Available in all tiers)
    public const string ProjectManagement = "ProjectManagement";
    public const string BasicInventory = "BasicInventory";
    public const string BasicReporting = "BasicReporting";

    // Professional Features
    public const string AdvancedInventory = "AdvancedInventory";
    public const string MaterialCertificates = "MaterialCertificates";
    public const string BasicEN1090 = "BasicEN1090";
    public const string CustomerPortal = "CustomerPortal";

    // Enterprise Features
    public const string FullEN1090Compliance = "FullEN1090Compliance";
    public const string AdvancedReporting = "AdvancedReporting";
    public const string APIAccess = "APIAccess";
    public const string CustomWorkflows = "CustomWorkflows";
    public const string MultiSiteManagement = "MultiSiteManagement";

    // Custom Features
    public const string CustomIntegrations = "CustomIntegrations";
    public const string DedicatedSupport = "DedicatedSupport";

    /// <summary>
    /// All available features
    /// </summary>
    public static readonly string[] AllFeatures =
    {
        ProjectManagement,
        BasicInventory,
        BasicReporting,
        AdvancedInventory,
        MaterialCertificates,
        BasicEN1090,
        CustomerPortal,
        FullEN1090Compliance,
        AdvancedReporting,
        APIAccess,
        CustomWorkflows,
        MultiSiteManagement,
        CustomIntegrations,
        DedicatedSupport
    };

    /// <summary>
    /// Get default features for a subscription tier
    /// </summary>
    public static List<string> GetDefaultFeaturesForTier(string tier)
    {
        var features = new List<string>
        {
            // Starter tier features (available to all)
            ProjectManagement,
            BasicInventory,
            BasicReporting
        };

        if (tier == SubscriptionTiers.Professional ||
            tier == SubscriptionTiers.Enterprise ||
            tier == SubscriptionTiers.Custom)
        {
            features.AddRange(new[]
            {
                AdvancedInventory,
                MaterialCertificates,
                BasicEN1090,
                CustomerPortal
            });
        }

        if (tier == SubscriptionTiers.Enterprise ||
            tier == SubscriptionTiers.Custom)
        {
            features.AddRange(new[]
            {
                FullEN1090Compliance,
                AdvancedReporting,
                APIAccess,
                CustomWorkflows,
                MultiSiteManagement
            });
        }

        if (tier == SubscriptionTiers.Custom)
        {
            features.AddRange(new[]
            {
                CustomIntegrations,
                DedicatedSupport
            });
        }

        return features;
    }
}
