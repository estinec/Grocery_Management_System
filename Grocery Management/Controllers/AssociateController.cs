using Microsoft.AspNetCore.Mvc;
using Grocery_Management.Models;
using Grocery_Management.Models.ViewModels;
using Grocery_Management.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Grocery_Management.Controllers;
[Authorize(Roles = "Associate")]
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
        ViewBag.Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
        return View(new PosCheckoutViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetails(int id)
    {
        var product = await _context.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        return Json(new
        {
            category = product.Category?.Name,
            unitPrice = product.UnitPrice,
            currentStock = product.CurrentStock
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePurchase(PosCheckoutViewModel model)
    {
        var validItems = model.Items?
            .Where(i => i.ProductId > 0 && i.Quantity > 0)
            .ToList();

        if (validItems == null || !validItems.Any())
        {
            ModelState.AddModelError("", "Please add at least one product to the sale.");
            ViewBag.Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
            return View(model);
        }

        // Merge duplicate product rows if selected twice
        var groupedItems = validItems
            .GroupBy(i => i.ProductId)
            .Select(g => new CartItemInput
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var purchase = new Purchase
            {
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0m,
                AssociateId = User.Identity?.Name ?? "Associate_Default"
            };

            foreach (var item in groupedItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", "Selected product was not found.");
                    ViewBag.Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                if (item.Quantity > product.CurrentStock)
                {
                    ModelState.AddModelError("", $"Insufficient stock for '{product.Name}'. Only {product.CurrentStock} left.");
                    ViewBag.Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
                    return View(model);
                }

                product.CurrentStock -= item.Quantity;

                var purchaseItem = new PurchaseItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice
                };

                purchase.TotalPrice += purchaseItem.Subtotal;
                purchase.Items.Add(purchaseItem);
            }

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Receipt #{purchase.Id} recorded! Total: ${purchase.TotalPrice:F2}";
            return RedirectToAction(nameof(CreatePurchase));
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "An error occurred while saving the sale.");
            ViewBag.Products = await _context.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();
            return View(model);
        }
    }
}