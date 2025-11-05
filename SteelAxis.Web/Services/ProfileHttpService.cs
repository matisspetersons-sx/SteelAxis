using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for current user profile operations
/// </summary>
public interface IProfileHttpService
{
    /// <summary>
    /// Get current authenticated user's profile
    /// </summary>
    Task<CurrentUserDto?> GetCurrentUserAsync();
}

/// <summary>
/// Implementation of profile HTTP service
/// </summary>
public class ProfileHttpService : IProfileHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProfileHttpService> _logger;

    public ProfileHttpService(HttpClient httpClient, ILogger<ProfileHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/profile/me");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get current user profile. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CurrentUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return null;
        }
    }
}
