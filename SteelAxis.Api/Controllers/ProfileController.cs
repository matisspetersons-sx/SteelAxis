using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for current user profile and tenant information
/// Returns combined user + tenant context
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ITenantManagementService _tenantManagementService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IUserManagementService userManagementService,
        ITenantManagementService tenantManagementService,
        ITenantService tenantService,
        ILogger<ProfileController> logger)
    {
        _userManagementService = userManagementService;
        _tenantManagementService = tenantManagementService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's profile with tenant information and features
    /// </summary>
    /// <returns>Current user profile with context</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentProfile()
    {
        try
        {
            var entraUserId = _tenantService.GetCurrentUserId();
            var tenantId = _tenantService.GetCurrentTenantId();

            // Get user profile
            var userProfile = await _userManagementService.GetUserByEntraIdAsync(entraUserId);
            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for Entra user {EntraUserId}", entraUserId);
                return NotFound("User profile not found");
            }

            // Get tenant info
            var tenant = await _tenantManagementService.GetTenantByIdAsync(tenantId);
            if (tenant == null)
            {
                _logger.LogWarning("Tenant {TenantId} not found", tenantId);
                return NotFound("Tenant not found");
            }

            // Get features
            var features = await _tenantService.GetFeaturesAsync();

            // Update last login
            await _userManagementService.UpdateLastLoginAsync(entraUserId);

            var currentUser = new CurrentUserDto
            {
                UserId = userProfile.Id,
                Email = userProfile.Email,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                FullName = userProfile.FullName,
                DisplayName = userProfile.DisplayName,
                Role = userProfile.Role,
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                SubscriptionTier = tenant.SubscriptionTier,
                IsTrial = tenant.IsTrial,
                Features = features
            };

            return Ok(currentUser);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to profile");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current profile");
            return StatusCode(500, "An error occurred while retrieving profile");
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="request">Update request</param>
    /// <returns>Updated user profile</returns>
    [HttpPut]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateCurrentProfile([FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var entraUserId = _tenantService.GetCurrentUserId();
            var userProfile = await _userManagementService.GetUserByEntraIdAsync(entraUserId);

            if (userProfile == null)
            {
                return NotFound("User profile not found");
            }

            // Users can only update their own name, not role or active status
            var safeRequest = new UpdateUserRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = null, // Cannot change own role
                IsActive = null // Cannot deactivate self
            };

            var updatedProfile = await _userManagementService.UpdateUserAsync(userProfile.Id, safeRequest);
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current profile");
            return StatusCode(500, "An error occurred while updating profile");
        }
    }
}
