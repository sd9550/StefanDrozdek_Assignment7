using Microsoft.EntityFrameworkCore;
using SalesService.Models;

namespace SalesService.Data;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SaleListing> SaleListings => Set<SaleListing>();

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SaleListing>()
            .Property(listing => listing.ListingPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .Property(order => order.SoldPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .Property(order => order.PlatformFee)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .HasIndex(order => order.ListingId)
            .IsUnique();
    }
}
