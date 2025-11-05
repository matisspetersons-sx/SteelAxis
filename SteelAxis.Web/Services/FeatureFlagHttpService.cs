using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for feature flag operations
/// </summary>
public interface IFeatureFlagHttpService
{
    /// <summary>
    /// Get all feature flags for current tenant
    /// </summary>
    Task<List<FeatureFlagDto>> GetFeaturesAsync();

    /// <summary>
    /// Check if specific feature is enabled for current tenant
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(string featureKey);

    /// <summary>
    /// Get all available features (System Admin only)
    /// </summary>
    Task<List<FeatureFlagDto>> GetAllFeaturesAsync();

    /// <summary>
    /// Create new feature flag (System Admin only)
    /// </summary>
    Task<FeatureFlagDto?> CreateFeatureAsync(CreateFeatureRequest request);

    /// <summary>
    /// Update feature flag (System Admin only)
    /// </summary>
    Task<FeatureFlagDto?> UpdateFeatureAsync(Guid id, UpdateFeatureRequest request);

    /// <summary>
    /// Delete feature flag (System Admin only)
    /// </summary>
    Task<bool> DeleteFeatureAsync(Guid id);
}

/// <summary>
/// Implementation of feature flag HTTP service
/// </summary>
public class FeatureFlagHttpService : IFeatureFlagHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeatureFlagHttpService> _logger;

    public FeatureFlagHttpService(HttpClient httpClient, ILogger<FeatureFlagHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<FeatureFlagDto>> GetFeaturesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/features");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<FeatureFlagDto>>() ?? new List<FeatureFlagDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting features");
            return new List<FeatureFlagDto>();
        }
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/features/{featureKey}/enabled");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature {FeatureKey}", featureKey);
            return false;
        }
    }

    public async Task<List<FeatureFlagDto>> GetAllFeaturesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/features/all");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<FeatureFlagDto>>() ?? new List<FeatureFlagDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all features");
            return new List<FeatureFlagDto>();
        }
    }

    public async Task<FeatureFlagDto?> CreateFeatureAsync(CreateFeatureRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/features", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create feature. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FeatureFlagDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature");
            return null;
        }
    }

    public async Task<FeatureFlagDto?> UpdateFeatureAsync(Guid id, UpdateFeatureRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/features/{id}", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update feature {FeatureId}. Status: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FeatureFlagDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature {FeatureId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteFeatureAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/features/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature {FeatureId}", id);
            return false;
        }
    }
}
