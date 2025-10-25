using System.ComponentModel.DataAnnotations;

namespace SteelAxis.Shared.Models;

/// <summary>
/// Base model placeholder for shared entities
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets when the entity was created
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
