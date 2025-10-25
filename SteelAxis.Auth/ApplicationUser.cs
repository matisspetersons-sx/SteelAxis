using Microsoft.AspNetCore.Identity;

namespace SteelAxis.Auth;

/// <summary>
/// Extended Identity user for SteelAxis application
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the user's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets when the user was created
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
