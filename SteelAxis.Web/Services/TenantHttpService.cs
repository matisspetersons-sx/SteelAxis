using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for tenant operations
/// Calls API endpoints in SteelAxis.Api
/// </summary>
public interface ITenantHttpService
{
    /// <summary>
    /// Get current tenant information
    /// </summary>
    Task<TenantDto?> GetCurrentTenantAsync();

    /// <summary>
    /// Get current tenant with feature flags
    /// </summary>
    Task<TenantWithFeaturesDto?> GetCurrentTenantWithFeaturesAsync();

    /// <summary>
    /// Get all tenants (System Admin only)
    /// </summary>
    Task<List<TenantDto>> GetAllTenantsAsync();

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    Task<TenantDto?> GetTenantByIdAsync(Guid id);

    /// <summary>
    /// Update tenant information
    /// </summary>
    Task<TenantDto?> UpdateTenantAsync(Guid id, UpdateTenantRequest request);

    /// <summary>
    /// Update subscription tier
    /// </summary>
    Task<bool> UpdateSubscriptionTierAsync(Guid id, string tier);

    /// <summary>
    /// Deactivate tenant
    /// </summary>
    Task<bool> DeactivateTenantAsync(Guid id);
}

/// <summary>
/// Implementation of tenant HTTP service
/// </summary>
public class TenantHttpService : ITenantHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantHttpService> _logger;

    public TenantHttpService(HttpClient httpClient, ILogger<TenantHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TenantDto?> GetCurrentTenantAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/tenants/current");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get current tenant. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current tenant");
            return null;
        }
    }

    public async Task<TenantWithFeaturesDto?> GetCurrentTenantWithFeaturesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/tenants/current/features");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get tenant with features. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantWithFeaturesDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant with features");
            return null;
        }
    }

    public async Task<List<TenantDto>> GetAllTenantsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/tenants");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<TenantDto>>() ?? new List<TenantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tenants");
            return new List<TenantDto>();
        }
    }

    public async Task<TenantDto?> GetTenantByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/tenants/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", id);
            return null;
        }
    }

    public async Task<TenantDto?> UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tenants/{id}", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update tenant {TenantId}. Status: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            return null;
        }
    }

    public async Task<bool> UpdateSubscriptionTierAsync(Guid id, string tier)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tenants/{id}/subscription", tier);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription tier for tenant {TenantId}", id);
            return false;
        }
    }

    public async Task<bool> DeactivateTenantAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/tenants/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tenant {TenantId}", id);
            return false;
        }
    }
}
