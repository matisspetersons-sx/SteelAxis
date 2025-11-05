using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for new tenant registration (admin signup)
/// Public endpoint - no authentication required
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(
        ITenantManagementService tenantManagementService,
        ILogger<RegistrationController> logger)
    {
        _tenantManagementService = tenantManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new tenant with admin user
    /// This is called AFTER the user has authenticated with Entra External ID
    /// Creates tenant, provisions database, sets up feature flags, creates admin profile
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <returns>Created tenant</returns>
    [HttpPost("admin")]
    [Authorize] // User must be authenticated with Entra External ID first
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantDto>> RegisterAdmin([FromBody] CreateTenantRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get authenticated user's Entra External ID
            var entraUserId = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(entraUserId))
            {
                _logger.LogError("No Entra user ID found in claims for registration");
                return Unauthorized("User authentication is invalid");
            }

            _logger.LogInformation("Starting admin registration for user {EntraUserId}, company {CompanyName}", 
                entraUserId, request.Name);

            // Create tenant with admin user
            var tenant = await _tenantManagementService.CreateTenantAsync(request, entraUserId);

            _logger.LogInformation("Tenant {TenantId} created successfully with admin {EntraUserId}", 
                tenant.Id, entraUserId);

            return CreatedAtAction(
                nameof(TenantsController.GetTenantById),
                "Tenants",
                new { id = tenant.Id },
                tenant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid registration request for {CompanyName}", request.Name);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin registration for {CompanyName}", request.Name);
            return StatusCode(500, "An error occurred during registration. Please try again.");
        }
    }

    /// <summary>
    /// Check if a domain is available
    /// Public endpoint - no authentication required
    /// </summary>
    /// <param name="domain">Domain to check</param>
    /// <returns>Availability status</returns>
    [HttpGet("check-domain/{domain}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> CheckDomainAvailability(string domain)
    {
        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(domain) || domain.Length < 3)
            {
                return BadRequest(new { available = false, message = "Domain must be at least 3 characters" });
            }

            // Check format
            if (!System.Text.RegularExpressions.Regex.IsMatch(domain, @"^[a-z0-9-]+$"))
            {
                return BadRequest(new { available = false, message = "Domain can only contain lowercase letters, numbers, and hyphens" });
            }

            // Check availability (this would query the database)
            // TODO: Implement actual check against DirectoryDbContext
            var isAvailable = true; // Placeholder

            return Ok(new 
            { 
                available = isAvailable,
                domain = domain,
                message = isAvailable ? "Domain is available" : "Domain is already taken"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking domain availability for {Domain}", domain);
            return StatusCode(500, "An error occurred while checking domain availability");
        }
    }
}
