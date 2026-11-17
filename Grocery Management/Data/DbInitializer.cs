using System.Globalization;
using Grocery_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Management.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        string[] roles = { "Admin", "Associate", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed default Admin account
        var adminEmail = "admin@grocery.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Store Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Categories & 100+ Products from CSV if Products table is empty
        await SeedFromCsvAsync(context);
    }

    public static async Task SeedFromCsvAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "products_seed.csv");
        if (!File.Exists(csvPath))
            return;

        var lines = await File.ReadAllLinesAsync(csvPath);
        var categoryCache = await context.Categories
            .ToDictionaryAsync(c => c.Name.ToLower(), c => c);

        // Skip header line (index 0)
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split(',');
            if (parts.Length < 5) continue;

            var categoryName = parts[0].Trim();
            var emoji = parts[1].Trim();
            var productName = parts[2].Trim();

            if (!decimal.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                continue;
            if (!int.TryParse(parts[4].Trim(), out var stock))
                continue;

            var catKey = categoryName.ToLower();
            if (!categoryCache.TryGetValue(catKey, out var category))
            {
                category = new Category { Name = categoryName };
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                categoryCache[catKey] = category;
            }

            context.Products.Add(new Product
            {
                CategoryId = category.Id,
                Emoji = string.IsNullOrEmpty(emoji) ? "🛒" : emoji,
                Name = productName,
                UnitPrice = price,
                CurrentStock = stock
            });
        }

        await context.SaveChangesAsync();
    }
}