using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Api.Controllers;

/// <summary>
/// API controller for user invitation management
/// Handles invitation lifecycle: create, accept, cancel, resend
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvitationsController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<InvitationsController> _logger;

    public InvitationsController(
        IUserManagementService userManagementService,
        ITenantService tenantService,
        ILogger<InvitationsController> logger)
    {
        _userManagementService = userManagementService;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all invitations for current tenant
    /// </summary>
    /// <param name="includeCancelled">Include cancelled invitations</param>
    /// <returns>List of invitations</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<InvitationDto>>> GetInvitations([FromQuery] bool includeCancelled = false)
    {
        try
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            var invitations = await _userManagementService.GetInvitationsAsync(tenantId, includeCancelled);
            return Ok(invitations);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to invitations");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitations");
            return StatusCode(500, "An error occurred while retrieving invitations");
        }
    }

    /// <summary>
    /// Get invitation by ID
    /// </summary>
    /// <param name="id">Invitation ID</param>
    /// <returns>Invitation details</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InvitationDto>> GetInvitationById(Guid id)
    {
        try
        {
            var invitations = await _userManagementService.GetInvitationsAsync(_tenantService.GetCurrentTenantId(), true);
            var invitation = invitations.FirstOrDefault(i => i.Id == id);

            if (invitation == null)
            {
                return NotFound($"Invitation {id} not found");
            }

            return Ok(invitation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation {InvitationId}", id);
            return StatusCode(500, "An error occurred while retrieving invitation");
        }
    }

    /// <summary>
    /// Get invitation by token (public endpoint for acceptance page)
    /// </summary>
    /// <param name="token">Invitation token</param>
    /// <returns>Invitation details</returns>
    [HttpGet("by-token/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvitationDto>> GetInvitationByToken(string token)
    {
        try
        {
            var invitation = await _userManagementService.GetInvitationByTokenAsync(token);

            if (invitation == null)
            {
                return NotFound("Invitation not found");
            }

            return Ok(invitation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invitation by token");
            return StatusCode(500, "An error occurred while retrieving invitation");
        }
    }

    /// <summary>
    /// Accept an invitation (called after user authenticates with Entra External ID)
    /// </summary>
    /// <param name="request">Acceptance request with token and Entra user ID</param>
    /// <returns>Acceptance response</returns>
    [HttpPost("accept")]
    [Authorize]
    [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AcceptInvitationResponse>> AcceptInvitation([FromBody] AcceptInvitationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Verify the Entra user ID matches the authenticated user
            var currentUserId = _tenantService.GetCurrentUserId();
            if (request.EntraUserId != currentUserId)
            {
                return BadRequest("User ID mismatch");
            }

            var response = await _userManagementService.AcceptInvitationAsync(request);

            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return StatusCode(500, "An error occurred while accepting invitation");
        }
    }

    /// <summary>
    /// Resend invitation email
    /// </summary>
    /// <param name="id">Invitation ID</param>
    /// <returns>Success response</returns>
    [HttpPost("{id:guid}/resend")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResendInvitation(Guid id)
    {
        try
        {
            await _userManagementService.ResendInvitationAsync(id);
            return Ok(new { message = "Invitation resent successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot resend invitation {InvitationId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {InvitationId}", id);
            return StatusCode(500, "An error occurred while resending invitation");
        }
    }

    /// <summary>
    /// Cancel a pending invitation
    /// </summary>
    /// <param name="id">Invitation ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        try
        {
            await _userManagementService.CancelInvitationAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invitation {InvitationId} not found for cancellation", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {InvitationId}", id);
            return StatusCode(500, "An error occurred while cancelling invitation");
        }
    }
}
