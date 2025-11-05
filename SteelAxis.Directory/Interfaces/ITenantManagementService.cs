using SteelAxis.Shared.DTOs;
using SteelAxis.Shared.Models;

namespace SteelAxis.Directory.Interfaces;

/// <summary>
/// Service for managing tenant lifecycle (creation, updates, deletion)
/// Only used by system admins or during registration
/// All operations work with Directory database only
/// </summary>
public interface ITenantManagementService
{
    /// <summary>
    /// Create a new tenant with admin user (used during registration)
    /// Creates tenant record in Directory DB
    /// Creates dedicated tenant database
    /// Sets up initial feature flags based on subscription tier
    /// </summary>
    /// <param name="request">Tenant creation request</param>
    /// <param name="adminEntraUserId">Entra External ID user object ID for admin</param>
    /// <returns>Created tenant DTO</returns>
    Task<TenantDto> CreateTenantAsync(CreateTenantRequest request, string adminEntraUserId);

    /// <summary>
    /// Get all tenants (system admin only)
    /// </summary>
    /// <returns>List of all tenants</returns>
    Task<List<TenantDto>> GetAllTenantsAsync();

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Tenant DTO or null</returns>
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId);

    /// <summary>
    /// Update tenant information
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated tenant DTO</returns>
    Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request);

    /// <summary>
    /// Deactivate a tenant (soft delete)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    Task DeactivateTenantAsync(Guid tenantId);

    /// <summary>
    /// Update subscription tier and associated feature flags
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="newTier">New subscription tier</param>
    Task UpdateSubscriptionTierAsync(Guid tenantId, string newTier);

    /// <summary>
    /// Initialize default feature flags for a tenant based on subscription tier
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="subscriptionTier">Subscription tier</param>
    Task InitializeFeatureFlagsAsync(Guid tenantId, string subscriptionTier);

    /// <summary>
    /// Create tenant database (called during tenant creation)
    /// Uses Azure SQL Server to create dedicated database
    /// Returns connection string reference (Key Vault secret name)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="tenantName">Tenant name (used in DB naming)</param>
    /// <returns>Connection string or Key Vault reference</returns>
    Task<string> CreateTenantDatabaseAsync(Guid tenantId, string tenantName);
}
