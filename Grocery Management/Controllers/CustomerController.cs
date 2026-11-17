using Grocery_Management.Data;
using Grocery_Management.Models;
using Grocery_Management.Models.ViewModels;
//using Grocery_Management.Extensions; // For Session JSON extensions if needed, or standard Session helpers
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Grocery_Management.Controllers;

public class CustomerController(AppDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private const string CartSessionKey = "CustomerCartSession";

    // 1. Storefront Catalog & Category Filter
    [HttpGet]
    public async Task<IActionResult> Index(int? categoryId)
    {
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
            ViewBag.SelectedCategory = categoryId.Value;
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return View(products);
    }

    // 2. View Shopping Cart
    [HttpGet]
    public IActionResult Cart()
    {
        var cart = GetCartSession();
        return View(cart);
    }

    // 3. Add Item to Cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.CurrentStock < quantity)
        {
            TempData["Error"] = "Product is out of stock or requested quantity is unavailable.";
            return RedirectToAction(nameof(Index));
        }

        var cart = GetCartSession();
        var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);

        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Emoji = product.Emoji,
                UnitPrice = product.UnitPrice,
                Quantity = quantity
            });
        }

        SaveCartSession(cart);
        TempData["Success"] = $"Added {quantity}x {product.Emoji} {product.Name} to your cart.";
        return RedirectToAction(nameof(Index));
    }

    // 4. Remove Item from Cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    
    [Authorize(Roles = "Customer")]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCartSession();
        cart.RemoveAll(c => c.ProductId == productId);
        SaveCartSession(cart);
        return RedirectToAction(nameof(Cart));
    }

    // 5. Checkout / Place Order
    [HttpPost]
    [ValidateAntiForgeryToken]
    
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCartSession();
        if (!cart.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        var user = await _userManager.GetUserAsync(User);
        string customerName = user?.FullName ?? User.Identity?.Name ?? "Customer";

        // Create Header-Item Purchase Record
        var purchase = new Purchase
        {
            CreatedAt = DateTime.UtcNow,
            AssociateId = $"Customer: {customerName}",
            TotalPrice = cart.Sum(i => i.TotalPrice),
            Items = new List<PurchaseItem>()
        };

        foreach (var item in cart)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.CurrentStock < item.Quantity)
            {
                TempData["Error"] = $"Sorry, {item.ProductName} is out of stock.";
                return RedirectToAction(nameof(Cart));
            }

            // Deduct Inventory
            product.CurrentStock -= item.Quantity;

            purchase.Items.Add(new PurchaseItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        // Clear Cart Session
        HttpContext.Session.Remove(CartSessionKey);

        TempData["Success"] = "Order placed successfully! Thank you for shopping with us.";
        return RedirectToAction(nameof(OrderConfirmation), new { id = purchase.Id });
    }

    [HttpGet]
    
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> OrderConfirmation(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();

        return View(purchase);
    }

    // Session Helpers
    
    [Authorize(Roles = "Customer")]
    private List<CartItemViewModel> GetCartSession()
    {
        var sessionData = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(sessionData)) return new List<CartItemViewModel>();
        return JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData) ?? new List<CartItemViewModel>();
    }

    private void SaveCartSession(List<CartItemViewModel> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }
}