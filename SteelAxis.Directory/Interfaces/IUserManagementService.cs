using SteelAxis.Shared.DTOs;

namespace SteelAxis.Directory.Interfaces;

/// <summary>
/// Service for managing user profiles and invitations
/// Works exclusively with Directory database
/// Handles user onboarding through invitation workflow
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Get all users for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>List of user profiles</returns>
    Task<List<UserProfileDto>> GetUsersAsync(Guid tenantId);

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    /// <param name="userId">User profile ID</param>
    /// <returns>User profile or null</returns>
    Task<UserProfileDto?> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Get user profile by Entra External ID
    /// </summary>
    /// <param name="entraUserId">Entra user object ID</param>
    /// <returns>User profile or null</returns>
    Task<UserProfileDto?> GetUserByEntraIdAsync(string entraUserId);

    /// <summary>
    /// Send invitation to join a tenant
    /// Creates invitation record and sends email
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Invitation request</param>
    /// <param name="invitedBy">Entra user ID of inviter</param>
    /// <returns>Created invitation</returns>
    Task<InvitationDto> InviteUserAsync(Guid tenantId, InviteUserRequest request, string invitedBy);

    /// <summary>
    /// Get all invitations for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="includeCancelled">Include cancelled invitations</param>
    /// <returns>List of invitations</returns>
    Task<List<InvitationDto>> GetInvitationsAsync(Guid tenantId, bool includeCancelled = false);

    /// <summary>
    /// Get invitation by token (for acceptance page)
    /// </summary>
    /// <param name="token">Invitation token</param>
    /// <returns>Invitation or null</returns>
    Task<InvitationDto?> GetInvitationByTokenAsync(string token);

    /// <summary>
    /// Accept an invitation and create user profile
    /// Called after user authenticates with Entra External ID
    /// </summary>
    /// <param name="request">Acceptance request with token and Entra user ID</param>
    /// <returns>Acceptance response with user profile ID</returns>
    Task<AcceptInvitationResponse> AcceptInvitationAsync(AcceptInvitationRequest request);

    /// <summary>
    /// Resend invitation email
    /// </summary>
    /// <param name="invitationId">Invitation ID</param>
    Task ResendInvitationAsync(Guid invitationId);

    /// <summary>
    /// Cancel a pending invitation
    /// </summary>
    /// <param name="invitationId">Invitation ID</param>
    Task CancelInvitationAsync(Guid invitationId);

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="userId">User profile ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user profile</returns>
    Task<UserProfileDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);

    /// <summary>
    /// Deactivate user (soft delete)
    /// </summary>
    /// <param name="userId">User profile ID</param>
    Task DeactivateUserAsync(Guid userId);

    /// <summary>
    /// Update user's last login timestamp
    /// Called during authentication
    /// </summary>
    /// <param name="entraUserId">Entra user object ID</param>
    Task UpdateLastLoginAsync(string entraUserId);
}
