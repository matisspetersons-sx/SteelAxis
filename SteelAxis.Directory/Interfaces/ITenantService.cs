using SteelAxis.Directory.Models;

namespace SteelAxis.Directory.Interfaces;

/// <summary>
/// Service for resolving and retrieving current tenant information
/// Uses Azure CIAM (Entra External ID) claims to identify tenant
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Get the current tenant ID from the authenticated user's claims
    /// </summary>
    /// <returns>Tenant ID</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not authenticated or tenant claim is missing</exception>
    Guid GetCurrentTenantId();

    /// <summary>
    /// Get the current user's Entra External ID (object ID)
    /// </summary>
    /// <returns>Entra user object ID</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not authenticated</exception>
    string GetCurrentUserId();

    /// <summary>
    /// Get full tenant information from Directory database
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Tenant entity or null if not found</returns>
    Task<Tenant?> GetTenantAsync(Guid tenantId);

    /// <summary>
    /// Get the current tenant's information
    /// </summary>
    /// <returns>Current tenant entity</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not authenticated</exception>
    /// <exception cref="InvalidOperationException">If tenant not found</exception>
    Task<Tenant> GetCurrentTenantAsync();

    /// <summary>
    /// Get tenant database connection string from Azure Key Vault
    /// NEVER returns the actual connection string - only a reference
    /// The actual connection is retrieved from Key Vault at runtime
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Connection string or Key Vault reference</returns>
    Task<string> GetTenantConnectionStringAsync(Guid tenantId);

    /// <summary>
    /// Check if a feature is enabled for the current tenant
    /// </summary>
    /// <param name="featureName">Feature name to check</param>
    /// <returns>True if feature is enabled</returns>
    Task<bool> IsFeatureEnabledAsync(string featureName);

    /// <summary>
    /// Get all enabled features for the current tenant
    /// </summary>
    /// <returns>Dictionary of feature names and their enabled status</returns>
    Task<Dictionary<string, bool>> GetFeaturesAsync();
}
