using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Grocery_Management.Models;
using Grocery_Management.Data;
using Microsoft.EntityFrameworkCore;

namespace Grocery_Management.Controllers;

public class HomeController : Controller
{
    
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.LowStockCount = await _context.Products.CountAsync(p => p.CurrentStock < 10);
        ViewBag.RecentSalesCount = await _context.Purchases.CountAsync();

        return View();
    }

}