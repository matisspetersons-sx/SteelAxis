using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for user management operations
/// </summary>
public interface IUserHttpService
{
    /// <summary>
    /// Get all users in current tenant
    /// </summary>
    Task<List<UserProfileDto>> GetUsersAsync();

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserProfileDto?> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Invite new user to tenant
    /// </summary>
    Task<InvitationDto?> InviteUserAsync(InviteUserRequest request);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<UserProfileDto?> UpdateUserAsync(Guid id, UpdateUserRequest request);

    /// <summary>
    /// Deactivate user
    /// </summary>
    Task<bool> DeactivateUserAsync(Guid id);
}

/// <summary>
/// Implementation of user HTTP service
/// </summary>
public class UserHttpService : IUserHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserHttpService> _logger;

    public UserHttpService(HttpClient httpClient, ILogger<UserHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<UserProfileDto>> GetUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/users");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UserProfileDto>>() ?? new List<UserProfileDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new List<UserProfileDto>();
        }
    }

    public async Task<UserProfileDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            return null;
        }
    }

    public async Task<InvitationDto?> InviteUserAsync(InviteUserRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/invite", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to invite user. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<InvitationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user");
            return null;
        }
    }

    public async Task<UserProfileDto?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update user {UserId}. Status: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return null;
        }
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return false;
        }
    }
}
