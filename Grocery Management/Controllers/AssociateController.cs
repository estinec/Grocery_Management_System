using Microsoft.AspNetCore.Mvc;
using Grocery_Management.Models;
using Grocery_Management.Data;
using Microsoft.EntityFrameworkCore;
namespace Grocery_Management.Controllers;

public class AssociateController : Controller
{
    private readonly AppDbContext _context;

    public AssociateController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> CreatePurchase()
    {
        ViewBag.Products = await _context.Products.Include(p => p.Category).ToListAsync();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetails(int id)
    {
        var product = await _context.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null) return NotFound();

        return Json(new { 
            category = product.Category?.Name, 
            unitPrice = product.UnitPrice,
            currentStock = product.CurrentStock 
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePurchase(Purchase purchase)
    {
        var product = await _context.Products.FindAsync(purchase.ProductId);
        if (product == null) return NotFound();

        if (purchase.Quantity > product.CurrentStock)
        {
            ModelState.AddModelError("Quantity", $"Insufficient stock. Only {product.CurrentStock} left.");
        }

        if (ModelState.IsValid)
        {
            purchase.TotalPrice = product.UnitPrice * purchase.Quantity;
            product.CurrentStock -= purchase.Quantity;

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CreatePurchase));
        }

        ViewBag.Products = await _context.Products.Include(p => p.Category).ToListAsync();
        return View(purchase);
    }
}