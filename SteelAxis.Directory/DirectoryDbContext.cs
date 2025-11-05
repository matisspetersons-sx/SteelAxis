using Microsoft.EntityFrameworkCore;
using SteelAxis.Directory.Models;

namespace SteelAxis.Directory;

/// <summary>
/// Central directory database context for tenant management
/// </summary>
public class DirectoryDbContext : DbContext
{
    public DirectoryDbContext(DbContextOptions<DirectoryDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Tenants (companies/organizations) in the system
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// User profiles linked to Entra External ID accounts
    /// </summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    /// <summary>
    /// User invitations for tenant onboarding
    /// </summary>
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    /// <summary>
    /// Feature flags controlling tenant access to features
    /// </summary>
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant entity
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Domain).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Domain).HasMaxLength(100);
            entity.Property(e => e.SubscriptionTier).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(500);

            // Relationships
            entity.HasMany(e => e.Users)
                .WithOne(u => u.Tenant)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.FeatureFlags)
                .WithOne(f => f.Tenant)
                .HasForeignKey(f => f.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Invitations)
                .WithOne(i => i.Tenant)
                .HasForeignKey(i => i.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.EntraUserId).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

            entity.Property(e => e.EntraUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // Configure UserInvitation entity
        modelBuilder.Entity<UserInvitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.InvitationToken).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.TenantId, e.Email, e.Status });

            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InvitedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InvitedByName).HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InvitationToken).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).HasMaxLength(500);
        });

        // Configure FeatureFlag entity
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.FeatureName }).IsUnique();

            entity.Property(e => e.FeatureName).IsRequired().HasMaxLength(100);
        });
    }
}
