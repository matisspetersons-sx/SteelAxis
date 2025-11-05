using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for tenant management operations
/// Requires authentication with JWT Bearer token
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        ITenantManagementService tenantManagementService,
        ITenantService tenantService,
        ILogger<TenantsController> logger)
    {
        _tenantManagementService = tenantManagementService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get current tenant information
    /// </summary>
    /// <returns>Current tenant details</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetCurrentTenant()
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var tenant = await _tenantManagementService.GetTenantByIdAsync(tenantId);

            if (tenant == null)
            {
                return NotFound("Tenant not found");
            }

            return Ok(tenant);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to current tenant");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current tenant");
            return StatusCode(500, "An error occurred while retrieving tenant information");
        }
    }

    /// <summary>
    /// Get current tenant with feature flags
    /// </summary>
    /// <returns>Current tenant with enabled features</returns>
    [HttpGet("current/features")]
    [ProducesResponseType(typeof(TenantWithFeaturesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantWithFeaturesDto>> GetCurrentTenantWithFeatures()
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var tenant = await _tenantManagementService.GetTenantByIdAsync(tenantId);

            if (tenant == null)
            {
                return NotFound("Tenant not found");
            }

            var features = await _tenantService.GetFeaturesAsync();

            var tenantWithFeatures = new TenantWithFeaturesDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Domain = tenant.Domain,
                SubscriptionTier = tenant.SubscriptionTier,
                IsActive = tenant.IsActive,
                IsTrial = tenant.IsTrial,
                TrialEndsAt = tenant.TrialEndsAt,
                Features = features
            };

            return Ok(tenantWithFeatures);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to tenant features");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant features");
            return StatusCode(500, "An error occurred while retrieving tenant features");
        }
    }

    /// <summary>
    /// Get all tenants (System Admin only)
    /// </summary>
    /// <returns>List of all tenants</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<TenantDto>>> GetAllTenants()
    {
        try
        {
            // TODO: Add system admin role check
            var tenants = await _tenantManagementService.GetAllTenantsAsync();
            return Ok(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            return StatusCode(500, "An error occurred while retrieving tenants");
        }
    }

    /// <summary>
    /// Get tenant by ID (System Admin only)
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Tenant details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantDto>> GetTenantById(Guid id)
    {
        try
        {
            var tenant = await _tenantManagementService.GetTenantByIdAsync(id);

            if (tenant == null)
            {
                return NotFound($"Tenant {id} not found");
            }

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while retrieving tenant");
        }
    }

    /// <summary>
    /// Update tenant information
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated tenant</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // TODO: Check if user has permission to update tenant (Admin only)
            var currentTenantId = _tenantService.GetCurrentTenantId();
            if (id != currentTenantId)
            {
                // Only allow updating own tenant unless system admin
                return Forbid();
            }

            var tenant = await _tenantManagementService.UpdateTenantAsync(id, request);
            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant {TenantId} not found for update", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while updating tenant");
        }
    }

    /// <summary>
    /// Update subscription tier
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <param name="tier">New subscription tier</param>
    /// <returns>Success response</returns>
    [HttpPut("{id:guid}/subscription")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSubscriptionTier(Guid id, [FromBody] string tier)
    {
        try
        {
            await _tenantManagementService.UpdateSubscriptionTierAsync(id, tier);
            return Ok(new { message = "Subscription tier updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid subscription tier: {Tier}", tier);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription tier for tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while updating subscription tier");
        }
    }

    /// <summary>
    /// Deactivate tenant (Soft delete)
    /// </summary>
    /// <param name="id">Tenant ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateTenant(Guid id)
    {
        try
        {
            await _tenantManagementService.DeactivateTenantAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant {TenantId} not found for deactivation", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tenant {TenantId}", id);
            return StatusCode(500, "An error occurred while deactivating tenant");
        }
    }
}
