using Grocery_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Management.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed basic Categories & Products
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Bakery" },
            new Category { Id = 2, Name = "Dairy" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, CategoryId = 1, Name = "Bagel", UnitPrice = 1.50m, CurrentStock = 100 },
            new Product { Id = 2, CategoryId = 2, Name = "Milk (1L)", UnitPrice = 3.99m, CurrentStock = 50 }
        );
    }
}