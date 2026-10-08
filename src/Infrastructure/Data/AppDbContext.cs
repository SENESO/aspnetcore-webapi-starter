using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(100).IsRequired();
            e.Property(p => p.Description).HasMaxLength(500);
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Name);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.Email).HasMaxLength(100).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Sample catalog so the API returns data right after the first migration.
        // Fixed dates keep the generated migration stable.
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Mechanical Keyboard", Description = "Hot-swappable, RGB", Price = 89.99m, Stock = 25, CreatedAt = seedDate },
            new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic, 2.4GHz", Price = 34.50m, Stock = 60, CreatedAt = seedDate },
            new Product { Id = 3, Name = "USB-C Hub", Description = "7-in-1 aluminum hub", Price = 45.00m, Stock = 40, CreatedAt = seedDate }
        );
    }
}
