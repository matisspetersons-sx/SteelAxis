using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for user management operations
/// Handles user profiles, invitations, and role management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserManagementService userManagementService,
        ITenantService tenantService,
        ILogger<UsersController> logger)
    {
        _userManagementService = userManagementService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users in current tenant
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<UserProfileDto>>> GetUsers()
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var users = await _userManagementService.GetUsersAsync(tenantId);
            return Ok(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to users list");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User profile ID</param>
    /// <returns>User profile</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetUserById(Guid id)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User {id} not found");
            }

            // Verify user belongs to current tenant
            var tenantId = _tenantService.GetCurrentTenantId();
            if (user.TenantId != tenantId)
            {
                return Forbid();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving user");
        }
    }

    /// <summary>
    /// Invite a new user to the tenant
    /// </summary>
    /// <param name="request">Invitation request</param>
    /// <returns>Created invitation</returns>
    [HttpPost("invite")]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvitationDto>> InviteUser([FromBody] InviteUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var invitedBy = _tenantService.GetCurrentUserId();

            var invitation = await _userManagementService.InviteUserAsync(tenantId, request, invitedBy);

            return CreatedAtAction(
                nameof(InvitationsController.GetInvitationById),
                "Invitations",
                new { id = invitation.Id },
                invitation);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid invitation request for {Email}", request.Email);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized invitation attempt");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting user {Email}", request.Email);
            return StatusCode(500, "An error occurred while inviting user");
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="id">User profile ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify user belongs to current tenant
            var existingUser = await _userManagementService.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound($"User {id} not found");
            }

            var tenantId = _tenantService.GetCurrentTenantId();
            if (existingUser.TenantId != tenantId)
            {
                return Forbid();
            }

            // TODO: Check if current user has permission to update (Admin or Manager)

            var updatedUser = await _userManagementService.UpdateUserAsync(id, request);
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found for update", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating user");
        }
    }

    /// <summary>
    /// Deactivate user (Soft delete)
    /// </summary>
    /// <param name="id">User profile ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            // Verify user belongs to current tenant
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User {id} not found");
            }

            var tenantId = _tenantService.GetCurrentTenantId();
            if (user.TenantId != tenantId)
            {
                return Forbid();
            }

            // TODO: Check if current user has permission to deactivate (Admin only)

            await _userManagementService.DeactivateUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User {UserId} not found for deactivation", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, "An error occurred while deactivating user");
        }
    }
}
