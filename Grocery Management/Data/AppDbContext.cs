using Grocery_Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Management.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PurchaseItem>()
            .HasOne(pi => pi.Purchase)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseItem>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.PurchaseItems)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>().HasData(
            
            new Category { Id = 1, Name = "Appliancies" },
            new Category { Id = 2, Name = "Bakery" },
            new Category { Id = 3, Name = "Beverages" },
            new Category { Id = 4, Name = "Canned and Jarred" },
            new Category { Id = 5, Name = "Dairy and Eggs" },
            new Category { Id = 6, Name = "Frozen fruits" },
            new Category { Id = 7, Name = "Household" },
            new Category { Id = 8, Name = "Meat and Seafood" },
            new Category { Id = 9, Name = "Other" },
            new Category { Id = 10, Name = "Pantry" },
            new Category { Id = 11, Name = "Personal Care" },
            new Category { Id = 12, Name = "Produce" },
            new Category { Id = 13, Name = "Snacks" }
        );
        
    }
}