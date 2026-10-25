using Microsoft.EntityFrameworkCore;
using RepairService.Models;

namespace RepairService.Data;

public sealed class RepairDbContext(DbContextOptions<RepairDbContext> options) : DbContext(options)
{
    public DbSet<ConsoleUnit> ConsoleUnits => Set<ConsoleUnit>();

    public DbSet<RepairLog> RepairLogs => Set<RepairLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsoleUnit>()
            .HasIndex(unit => unit.SerialTag)
            .IsUnique();

        modelBuilder.Entity<RepairLog>()
            .HasOne<ConsoleUnit>()
            .WithMany()
            .HasForeignKey(log => log.ConsoleUnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
