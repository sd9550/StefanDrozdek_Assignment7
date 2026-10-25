using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Part>()
            .HasIndex(part => part.SKU)
            .IsUnique();

        modelBuilder.Entity<Part>()
            .Property(part => part.UnitCost)
            .HasPrecision(10, 2);
    }
}
