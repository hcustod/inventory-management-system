using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context; 
        _logger = logger;

    }

    // All product list
    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("ProductsController.Index called at {Time} by {User}", DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");
            
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync(); 
        
            _logger.LogInformation("Fetched {Count} products successfully at {Time}", products.Count, DateTime.UtcNow);
            return View(products);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProductsController.Index at {Time}", DateTime.UtcNow);
            return RedirectToAction("Error500", "Error");
        }
     
    }

    // Load create product view
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        try
        {
            _logger.LogInformation("ProductsController.Create GET called at {Time} by {User}", DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");
            
            ViewBag.Categories = await _context.Categories.ToListAsync();
        
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProductsController.Create at {Time}", DateTime.UtcNow);
            return RedirectToAction("Error500", "Error");
        }
    }
    
}
