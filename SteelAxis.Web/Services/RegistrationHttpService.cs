using SteelAxis.Shared.DTOs;

namespace SteelAxis.Web.Services;

/// <summary>
/// HTTP service for tenant registration operations
/// </summary>
public interface IRegistrationHttpService
{
    /// <summary>
    /// Register new tenant with admin user (public endpoint)
    /// </summary>
    Task<TenantDto?> RegisterTenantAsync(CreateTenantRequest request);
}

/// <summary>
/// Implementation of registration HTTP service
/// </summary>
public class RegistrationHttpService : IRegistrationHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistrationHttpService> _logger;

    public RegistrationHttpService(HttpClient httpClient, ILogger<RegistrationHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TenantDto?> RegisterTenantAsync(CreateTenantRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/registration/register", request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to register tenant. Status: {StatusCode}", response.StatusCode);
                
                // Try to read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Registration error details: {ErrorContent}", errorContent);
                
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering tenant");
            return null;
        }
    }
}
