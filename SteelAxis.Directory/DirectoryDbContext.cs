using Microsoft.EntityFrameworkCore;

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

    // Add DbSets for tenant directory entities here

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure directory entities here
    }
}
