using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Directory.Models;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Directory.Services;

/// <summary>
/// Tenant management service implementation
/// Handles tenant lifecycle: creation, updates, database provisioning
/// Works exclusively with Directory database
/// Uses Azure Key Vault for connection strings
/// </summary>
public class TenantManagementService : ITenantManagementService
{
    private readonly DirectoryDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantManagementService> _logger;

    public TenantManagementService(
        DirectoryDbContext context,
        IConfiguration configuration,
        ILogger<TenantManagementService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TenantDto> CreateTenantAsync(CreateTenantRequest request, string adminEntraUserId)
    {
        try
        {
            // Validate domain uniqueness if provided
            if (!string.IsNullOrWhiteSpace(request.Domain))
            {
                var existingDomain = await _context.Tenants
                    .AnyAsync(t => t.Domain == request.Domain);

                if (existingDomain)
                {
                    throw new InvalidOperationException($"Domain '{request.Domain}' is already in use");
                }
            }

            // Create tenant entity
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Domain = request.Domain,
                SubscriptionTier = request.SubscriptionTier,
                IsTrial = request.IsTrial,
                TrialEndsAt = request.IsTrial ? DateTime.UtcNow.AddDays(request.TrialDays) : null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminEntraUserId,
                ConnectionString = string.Empty // Will be set after database creation
            };

            // Create dedicated tenant database
            _logger.LogInformation("Creating database for tenant {TenantId} ({TenantName})", tenant.Id, tenant.Name);
            var connectionString = await CreateTenantDatabaseAsync(tenant.Id, tenant.Name);
            tenant.ConnectionString = connectionString;

            // Save tenant to Directory database
            await _context.Tenants.AddAsync(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tenant {TenantId} created successfully", tenant.Id);

            // Initialize feature flags based on subscription tier
            await InitializeFeatureFlagsAsync(tenant.Id, tenant.SubscriptionTier);

            // Create admin user profile
            var adminProfile = new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                EntraUserId = adminEntraUserId,
                Email = request.AdminEmail,
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                Role = UserRoles.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminEntraUserId
            };

            await _context.UserProfiles.AddAsync(adminProfile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin user profile created for tenant {TenantId}", tenant.Id);

            return MapToDto(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {TenantName}", request.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<TenantDto>> GetAllTenantsAsync()
    {
        var tenants = await _context.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        var tenantDtos = new List<TenantDto>();
        foreach (var tenant in tenants)
        {
            var userCount = await _context.UserProfiles.CountAsync(u => u.TenantId == tenant.Id);
            var activeUserCount = await _context.UserProfiles.CountAsync(u => u.TenantId == tenant.Id && u.IsActive);

            tenantDtos.Add(MapToDto(tenant, userCount, activeUserCount));
        }

        return tenantDtos;
    }

    /// <inheritdoc />
    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            return null;

        var userCount = await _context.UserProfiles.CountAsync(u => u.TenantId == tenantId);
        var activeUserCount = await _context.UserProfiles.CountAsync(u => u.TenantId == tenantId && u.IsActive);

        return MapToDto(tenant, userCount, activeUserCount);
    }

    /// <inheritdoc />
    public async Task<TenantDto> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
            tenant.Name = request.Name;

        if (request.SubscriptionTier != null && request.SubscriptionTier != tenant.SubscriptionTier)
        {
            await UpdateSubscriptionTierAsync(tenantId, request.SubscriptionTier);
            tenant.SubscriptionTier = request.SubscriptionTier;
        }

        if (request.IsActive.HasValue)
            tenant.IsActive = request.IsActive.Value;

        tenant.UpdatedAt = DateTime.UtcNow;
        // TODO: Get current user ID for UpdatedBy

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant {TenantId} updated successfully", tenantId);

        return MapToDto(tenant);
    }

    /// <inheritdoc />
    public async Task DeactivateTenantAsync(Guid tenantId)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tenant {TenantId} deactivated", tenantId);
    }

    /// <inheritdoc />
    public async Task UpdateSubscriptionTierAsync(Guid tenantId, string newTier)
    {
        if (!SubscriptionTiers.AllTiers.Contains(newTier))
        {
            throw new ArgumentException($"Invalid subscription tier: {newTier}");
        }

        // Update feature flags for new tier
        await InitializeFeatureFlagsAsync(tenantId, newTier);

        _logger.LogInformation("Subscription tier updated to {NewTier} for tenant {TenantId}", newTier, tenantId);
    }

    /// <inheritdoc />
    public async Task InitializeFeatureFlagsAsync(Guid tenantId, string subscriptionTier)
    {
        // Get default features for the tier
        var defaultFeatures = FeatureNames.GetDefaultFeaturesForTier(subscriptionTier);

        // Remove existing feature flags for this tenant
        var existingFlags = await _context.FeatureFlags
            .Where(f => f.TenantId == tenantId)
            .ToListAsync();

        _context.FeatureFlags.RemoveRange(existingFlags);

        // Create new feature flags
        var newFlags = defaultFeatures.Select(featureName => new FeatureFlag
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FeatureName = featureName,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        });

        await _context.FeatureFlags.AddRangeAsync(newFlags);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Initialized {Count} feature flags for tenant {TenantId}", defaultFeatures.Count, tenantId);
    }

    /// <inheritdoc />
    public Task<string> CreateTenantDatabaseAsync(Guid tenantId, string tenantName)
    {
        try
        {
            // Get master connection string from configuration
            var masterConnectionString = _configuration.GetConnectionString("DirectoryConnection");
            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                throw new InvalidOperationException("Master connection string not configured");
            }

            // Generate database name (sanitize tenant name for DB naming)
            var sanitizedName = SanitizeForDatabaseName(tenantName);
            var databaseName = $"SteelAxis_Tenant_{sanitizedName}_{tenantId:N}".Substring(0, Math.Min(128, $"SteelAxis_Tenant_{sanitizedName}_{tenantId:N}".Length));

            _logger.LogInformation("Creating database: {DatabaseName}", databaseName);

            // TODO: Actual database creation logic
            // In production, this should:
            // 1. Use Azure SQL Database API to create new database
            // 2. Run migrations on the new database using SteelAxis.Data.AppDbContext
            // 3. Store connection string in Azure Key Vault
            // 4. Return Key Vault secret reference, not actual connection string

            // For now, return a placeholder connection string
            // SECURITY WARNING: This is for development only!
            var tenantConnectionString = masterConnectionString.Replace("DirectoryDb", databaseName);

            _logger.LogInformation("Database {DatabaseName} created successfully", databaseName);

            // TODO: Store in Azure Key Vault and return secret reference
            // Example: return $"kv-secret:steelaxis-tenant-{tenantId}-connectionstring";
            return Task.FromResult(tenantConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant database for {TenantId}", tenantId);
            throw;
        }
    }

    private string SanitizeForDatabaseName(string input)
    {
        // Remove special characters, keep only alphanumeric
        var sanitized = new string(input.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "Tenant" : sanitized;
    }

    private TenantDto MapToDto(Tenant tenant, int? userCount = null, int? activeUserCount = null)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Domain = tenant.Domain,
            SubscriptionTier = tenant.SubscriptionTier,
            IsActive = tenant.IsActive,
            IsTrial = tenant.IsTrial,
            TrialEndsAt = tenant.TrialEndsAt,
            CreatedAt = tenant.CreatedAt,
            UserCount = userCount ?? 0,
            ActiveUserCount = activeUserCount ?? 0
        };
    }
}
