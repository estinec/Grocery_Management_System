using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Grocery_Management.Models;
using Grocery_Management.Data;

namespace Grocery_Management.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
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
}