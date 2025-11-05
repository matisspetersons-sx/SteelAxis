using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Directory.Models;
using SteelAxis.Shared.DTOs;

namespace SteelAxis.Directory.Services;

/// <summary>
/// User management service implementation
/// Handles user invitations and profile management
/// Works exclusively with Directory database
/// Email sending integrated with Azure Communication Services (TODO)
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly DirectoryDbContext _context;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        DirectoryDbContext context,
        ILogger<UserManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<UserProfileDto>> GetUsersAsync(Guid tenantId)
    {
        var users = await _context.UserProfiles
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Email)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? null : MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto?> GetUserByEntraIdAsync(string entraUserId)
    {
        var user = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EntraUserId == entraUserId);

        return user == null ? null : MapToDto(user);
    }

    /// <inheritdoc />
    public async Task<InvitationDto> InviteUserAsync(Guid tenantId, InviteUserRequest request, string invitedBy)
    {
        try
        {
            // Check if user already exists in this tenant
            var existingUser = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists in this tenant");
            }

            // Check for pending invitation
            var pendingInvitation = await _context.UserInvitations
                .FirstOrDefaultAsync(i => i.TenantId == tenantId 
                                          && i.Email == request.Email 
                                          && i.Status == InvitationStatus.Pending);

            if (pendingInvitation != null)
            {
                throw new InvalidOperationException($"Pending invitation already exists for {request.Email}");
            }

            // Get inviter's name
            var inviter = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.EntraUserId == invitedBy);

            // Create invitation
            var invitation = new UserInvitation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                InvitedBy = invitedBy,
                InvitedByName = inviter?.DisplayName,
                Status = InvitationStatus.Pending,
                InvitationToken = Guid.NewGuid().ToString(),
                Message = request.Message,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _context.UserInvitations.AddAsync(invitation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User invitation created for {Email} to tenant {TenantId}", request.Email, tenantId);

            // TODO: Send invitation email using Azure Communication Services
            // Email should include:
            // - Link to accept invitation: /accept-invitation?token={invitation.InvitationToken}
            // - Tenant name
            // - Inviter's name
            // - Custom message if provided

            return await MapToInvitationDtoAsync(invitation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invitation for {Email}", request.Email);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<InvitationDto>> GetInvitationsAsync(Guid tenantId, bool includeCancelled = false)
    {
        var query = _context.UserInvitations
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId);

        if (!includeCancelled)
        {
            query = query.Where(i => i.Status != InvitationStatus.Cancelled);
        }

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var dtos = new List<InvitationDto>();
        foreach (var invitation in invitations)
        {
            dtos.Add(await MapToInvitationDtoAsync(invitation));
        }

        return dtos;
    }

    /// <inheritdoc />
    public async Task<InvitationDto?> GetInvitationByTokenAsync(string token)
    {
        var invitation = await _context.UserInvitations
            .AsNoTracking()
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.InvitationToken == token);

        return invitation == null ? null : await MapToInvitationDtoAsync(invitation);
    }

    /// <inheritdoc />
    public async Task<AcceptInvitationResponse> AcceptInvitationAsync(AcceptInvitationRequest request)
    {
        try
        {
            var invitation = await _context.UserInvitations
                .Include(i => i.Tenant)
                .FirstOrDefaultAsync(i => i.InvitationToken == request.InvitationToken);

            if (invitation == null)
            {
                return new AcceptInvitationResponse
                {
                    Success = false,
                    ErrorMessage = "Invitation not found"
                };
            }

            if (invitation.Status != InvitationStatus.Pending)
            {
                return new AcceptInvitationResponse
                {
                    Success = false,
                    ErrorMessage = $"Invitation is {invitation.Status.ToLower()}"
                };
            }

            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                invitation.Status = InvitationStatus.Expired;
                await _context.SaveChangesAsync();

                return new AcceptInvitationResponse
                {
                    Success = false,
                    ErrorMessage = "Invitation has expired"
                };
            }

            // Check if user profile already exists (shouldn't happen, but safety check)
            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => u.EntraUserId == request.EntraUserId);

            if (existingProfile != null)
            {
                return new AcceptInvitationResponse
                {
                    Success = false,
                    ErrorMessage = "User profile already exists"
                };
            }

            // Create user profile
            var userProfile = new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = invitation.TenantId,
                EntraUserId = request.EntraUserId,
                Email = invitation.Email,
                FirstName = invitation.FirstName,
                LastName = invitation.LastName,
                Role = invitation.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = invitation.InvitedBy
            };

            await _context.UserProfiles.AddAsync(userProfile);

            // Update invitation status
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedBy = request.EntraUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {EntraUserId} accepted invitation to tenant {TenantId}", 
                request.EntraUserId, invitation.TenantId);

            return new AcceptInvitationResponse
            {
                Success = true,
                UserProfileId = userProfile.Id,
                TenantId = invitation.TenantId,
                TenantName = invitation.Tenant?.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token {Token}", request.InvitationToken);
            return new AcceptInvitationResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while accepting the invitation"
            };
        }
    }

    /// <inheritdoc />
    public async Task ResendInvitationAsync(Guid invitationId)
    {
        var invitation = await _context.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            throw new InvalidOperationException($"Invitation {invitationId} not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot resend invitation with status {invitation.Status}");
        }

        // Extend expiration
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation {InvitationId} resent to {Email}", invitationId, invitation.Email);

        // TODO: Resend invitation email
    }

    /// <inheritdoc />
    public async Task CancelInvitationAsync(Guid invitationId)
    {
        var invitation = await _context.UserInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            throw new InvalidOperationException($"Invitation {invitationId} not found");
        }

        invitation.Status = InvitationStatus.Cancelled;
        invitation.CancelledAt = DateTime.UtcNow;
        // TODO: Set CancelledBy to current user

        await _context.SaveChangesAsync();

        _logger.LogInformation("Invitation {InvitationId} cancelled", invitationId);
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.Role))
            user.Role = request.Role;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;
        // TODO: Set UpdatedBy to current user

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated", userId);

        return MapToDto(user);
    }

    /// <inheritdoc />
    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deactivated", userId);
    }

    /// <inheritdoc />
    public async Task UpdateLastLoginAsync(string entraUserId)
    {
        var user = await _context.UserProfiles
            .FirstOrDefaultAsync(u => u.EntraUserId == entraUserId);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private UserProfileDto MapToDto(UserProfile user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private async Task<InvitationDto> MapToInvitationDtoAsync(UserInvitation invitation)
    {
        var tenantName = invitation.Tenant?.Name;
        if (tenantName == null && invitation.TenantId != Guid.Empty)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == invitation.TenantId);
            tenantName = tenant?.Name ?? string.Empty;
        }

        return new InvitationDto
        {
            Id = invitation.Id,
            TenantId = invitation.TenantId,
            TenantName = tenantName ?? string.Empty,
            Email = invitation.Email,
            FirstName = invitation.FirstName,
            LastName = invitation.LastName,
            Role = invitation.Role,
            InvitedBy = invitation.InvitedBy,
            InvitedByName = invitation.InvitedByName,
            Status = invitation.Status,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            AcceptedAt = invitation.AcceptedAt,
            IsValid = invitation.IsValid,
            IsExpired = invitation.IsExpired
        };
    }
}
