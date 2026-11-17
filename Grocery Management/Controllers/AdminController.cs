using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grocery_Management.Models;
using Microsoft.AspNetCore.Identity;
using Grocery_Management.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Grocery_Management.Data;

namespace Grocery_Management.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var associateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Associate");
        var associateCount = associateRole != null
            ? await _context.UserRoles.CountAsync(ur => ur.RoleId == associateRole.Id)
            : 0;

        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.LowStockCount = await _context.Products.CountAsync(p => p.CurrentStock <= 10);
        ViewBag.TotalOrders = await _context.Purchases.CountAsync();
        ViewBag.TotalRevenue = await _context.Purchases.SumAsync(p => (decimal?)p.TotalPrice) ?? 0m;
        ViewBag.AssociateCount = associateCount;

        var recentPurchases = await _context.Purchases
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View(recentPurchases);
    }

    public async Task<IActionResult> StockOverview()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> CreateProduct()
    {
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Product '{product.Name}' added successfully.";
            return RedirectToAction(nameof(StockOverview));
        }

        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restock(int productId, int additionalStock)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null && additionalStock > 0)
        {
            product.CurrentStock += additionalStock;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Added {additionalStock} units to {product.Name}.";
        }
        return RedirectToAction(nameof(StockOverview));
    }

    public async Task<IActionResult> PurchaseHistory()
    {
        var purchases = await _context.Purchases
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(purchases);
    }
    
    // Add to Controllers/AdminController.cs

    [HttpGet]
    public async Task<IActionResult> ManageEmployees()
    {
        var associateRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Associate");
        if (associateRole == null) return View(new List<ApplicationUser>());

        var associateUserIds = await _context.UserRoles
            .Where(ur => ur.RoleId == associateRole.Id)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var associates = await _context.Users
            .Where(u => associateUserIds.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(associates);
    }

    [HttpGet]
    public IActionResult CreateAssociate()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssociate(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            // Note: You can inject UserManager<ApplicationUser> into AdminController constructor for this
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Associate");
                TempData["Success"] = $"Associate account for {model.FullName} created successfully.";
                return RedirectToAction(nameof(ManageEmployees));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }
}