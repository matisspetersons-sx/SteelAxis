using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Directory.Models;

namespace SteelAxis.Directory.Services;

/// <summary>
/// Tenant service implementation using Azure CIAM (Entra External ID) claims
/// Retrieves tenant information from Directory database
/// Uses Azure Key Vault for connection strings (no secrets in DB)
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DirectoryDbContext _directoryContext;
    private readonly ILogger<TenantService> _logger;

    // Azure CIAM claim types
    private const string TenantIdClaimType = "extension_TenantId";
    private const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        DirectoryDbContext directoryContext,
        ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _directoryContext = directoryContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public Guid GetCurrentTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Attempted to get tenant ID for unauthenticated user");
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        // Get tenant ID from custom claim (set during invitation acceptance)
        var tenantIdClaim = user.FindFirst(TenantIdClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            _logger.LogError("Authenticated user {UserId} has no tenant ID claim", GetCurrentUserId());
            throw new UnauthorizedAccessException("User has no tenant association");
        }

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            _logger.LogError("Invalid tenant ID format in claims: {TenantIdClaim}", tenantIdClaim);
            throw new InvalidOperationException("Invalid tenant ID format");
        }

        return tenantId;
    }

    /// <inheritdoc />
    public string GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        // Get Entra External ID object identifier
        var userId = user.FindFirst(ObjectIdClaimType)?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogError("Authenticated user has no object identifier claim");
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetTenantAsync(Guid tenantId)
    {
        try
        {
            return await _directoryContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Tenant> GetCurrentTenantAsync()
    {
        var tenantId = GetCurrentTenantId();
        var tenant = await GetTenantAsync(tenantId);

        if (tenant == null)
        {
            _logger.LogError("Tenant {TenantId} not found or inactive", tenantId);
            throw new InvalidOperationException($"Tenant {tenantId} not found or inactive");
        }

        return tenant;
    }

    /// <inheritdoc />
    public async Task<string> GetTenantConnectionStringAsync(Guid tenantId)
    {
        var tenant = await GetTenantAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        // TODO: In production, this should retrieve from Azure Key Vault
        // The ConnectionString field in DB should store the Key Vault secret name, not the actual connection string
        // Example: "kv-secret:steelaxis-tenant-{tenantId}-connectionstring"

        // For now, return the stored value (will be updated when Key Vault integration is added)
        // SECURITY NOTE: This is temporary - production MUST use Key Vault
        return tenant.ConnectionString;
    }

    /// <inheritdoc />
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        var tenantId = GetCurrentTenantId();

        var featureFlag = await _directoryContext.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.FeatureName == featureName);

        return featureFlag?.IsEnabled ?? false;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, bool>> GetFeaturesAsync()
    {
        var tenantId = GetCurrentTenantId();

        var features = await _directoryContext.FeatureFlags
            .AsNoTracking()
            .Where(f => f.TenantId == tenantId)
            .ToDictionaryAsync(f => f.FeatureName, f => f.IsEnabled);

        return features;
    }
}
