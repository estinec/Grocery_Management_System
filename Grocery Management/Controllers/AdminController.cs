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
        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        return View(products);
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
        }
        return RedirectToAction(nameof(StockOverview));
    }

    public async Task<IActionResult> PurchaseHistory()
    {
        var purchases = await _context.Purchases
            .Include(p => p.Product)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(purchases);
    }
}