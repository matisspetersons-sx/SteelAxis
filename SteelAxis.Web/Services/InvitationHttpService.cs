using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for user invitation operations
/// </summary>
public interface IInvitationHttpService
{
    /// <summary>
    /// Get all invitations for current tenant
    /// </summary>
    Task<List<InvitationDto>> GetInvitationsAsync();

    /// <summary>
    /// Get invitation by ID
    /// </summary>
    Task<InvitationDto?> GetInvitationByIdAsync(Guid id);

    /// <summary>
    /// Get invitation by token (public endpoint)
    /// </summary>
    Task<InvitationDto?> GetInvitationByTokenAsync(string token);

    /// <summary>
    /// Accept invitation (public endpoint)
    /// </summary>
    Task<AcceptInvitationResponse?> AcceptInvitationAsync(AcceptInvitationRequest request);

    /// <summary>
    /// Resend invitation
    /// </summary>
    Task<bool> ResendInvitationAsync(Guid id);

    /// <summary>
    /// Revoke invitation
    /// </summary>
    Task<bool> RevokeInvitationAsync(Guid id);
}

/// <summary>
/// Implementation of invitation HTTP service
/// </summary>
public class InvitationHttpService : IInvitationHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InvitationHttpService> _logger;

    public InvitationHttpService(HttpClient httpClient, ILogger<InvitationHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<InvitationDto>> GetInvitationsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/invitations");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<InvitationDto>>() ?? new List<InvitationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitations");
            return new List<InvitationDto>();
        }
    }

    public async Task<InvitationDto?> GetInvitationByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/invitations/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<InvitationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation {InvitationId}", id);
            return null;
        }
    }

    public async Task<InvitationDto?> GetInvitationByTokenAsync(string token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/invitations/token/{token}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<InvitationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation by token");
            return null;
        }
    }

    public async Task<AcceptInvitationResponse?> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/invitations/accept", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to accept invitation. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AcceptInvitationResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return null;
        }
    }

    public async Task<bool> ResendInvitationAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/invitations/{id}/resend", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {InvitationId}", id);
            return false;
        }
    }

    public async Task<bool> RevokeInvitationAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/invitations/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking invitation {InvitationId}", id);
            return false;
        }
    }
}
