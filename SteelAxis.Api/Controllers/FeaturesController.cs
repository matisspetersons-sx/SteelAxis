using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for feature flag management
/// Returns enabled features for current tenant
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeaturesController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(
        ITenantService tenantService,
        ILogger<FeaturesController> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all feature flags for current tenant
    /// </summary>
    /// <returns>Dictionary of feature names and enabled status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Dictionary<string, bool>>> GetFeatures()
    {
        try
        {
            var features = await _tenantService.GetFeaturesAsync();
            return Ok(features);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to features");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving features");
            return StatusCode(500, "An error occurred while retrieving features");
        }
    }

    /// <summary>
    /// Check if a specific feature is enabled
    /// </summary>
    /// <param name="featureName">Feature name to check</param>
    /// <returns>Enabled status</returns>
    [HttpGet("{featureName}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> IsFeatureEnabled(string featureName)
    {
        try
        {
            var isEnabled = await _tenantService.IsFeatureEnabledAsync(featureName);
            return Ok(new { featureName, isEnabled });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to feature check");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature {FeatureName}", featureName);
            return StatusCode(500, "An error occurred while checking feature");
        }
    }
}
