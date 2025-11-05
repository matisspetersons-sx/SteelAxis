using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.DTOs;

/// <summary>
/// Data transfer object for user invitation
/// </summary>
public record InvitationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Role { get; init; } = string.Empty;
    public string InvitedBy { get; init; } = string.Empty;
    public string? InvitedByName { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? AcceptedAt { get; init; }
    public bool IsValid { get; init; }
    public bool IsExpired { get; init; }
}

/// <summary>
/// Request to accept an invitation
/// </summary>
public record AcceptInvitationRequest
{
    /// <summary>
    /// Invitation token from email link
    /// </summary>
    [Required(ErrorMessage = "Invitation token is required")]
    public string InvitationToken { get; init; } = string.Empty;

    /// <summary>
    /// Entra External ID user object ID (after authentication)
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public string EntraUserId { get; init; } = string.Empty;
}

/// <summary>
/// Response after accepting invitation
/// </summary>
public record AcceptInvitationResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? UserProfileId { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
}

/// <summary>
/// Request to resend an invitation
/// </summary>
public record ResendInvitationRequest
{
    /// <summary>
    /// Invitation ID to resend
    /// </summary>
    [Required]
    public Guid InvitationId { get; init; }
}

/// <summary>
/// Request to cancel an invitation
/// </summary>
public record CancelInvitationRequest
{
    /// <summary>
    /// Invitation ID to cancel
    /// </summary>
    [Required]
    public Guid InvitationId { get; init; }
}
