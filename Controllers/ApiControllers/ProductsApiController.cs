using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsApiController> _logger;

    public ProductsApiController(AppDbContext context, ILogger<ProductsApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get all products with filters and sorting as selected
    [HttpGet]
    public async Task<IActionResult> GetProducts(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, bool? lowStockOnly)
    {
        _logger.LogInformation("GET /api/products called by {User} at {Time}", User.Identity?.Name ?? "Anonymous", DateTime.UtcNow);

        var products = _context.Products.Include(p => p.Category).AsQueryable();

        // Filters
        if (!string.IsNullOrEmpty(search))
            products = products.Where(p => p.Name.ToLower().Contains(search.ToLower()));

        if (categoryId.HasValue)
            products = products.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            products = products.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            products = products.Where(p => p.Price <= maxPrice);

        if (lowStockOnly.HasValue && lowStockOnly.Value == true)
            products = products.Where(p => p.ProductStockAmount < p.LowStockThreshold);

        // Sorting
        products = sortBy?.ToLower() switch
        {
            "price" => products.OrderBy(p => p.Price),
            "quantity" => products.OrderBy(p => p.ProductStockAmount),
            "name" => products.OrderBy(p => p.Name),
            _ => products
        };

        var result = await products.ToListAsync();
        _logger.LogInformation("Returned {Count} products", result.Count);
        return Ok(result);
    }


    // Get single product by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("GET /api/products/{Id} by {User}", id, User.Identity?.Name ?? "Anonymous");

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                _logger.LogWarning("Product {Id} not found", id);
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return StatusCode(500, new { message = "Error retrieving product.", details = ex.Message });
        }
    
    }

    // Get all categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            _logger.LogInformation("GET /api/products/categories called");

            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    c.CategoryDescription
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product categories");
            return StatusCode(500, new { message = "Error retrieving categories.", details = ex.Message });
        }
    }

    // Create new product
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        try
        {
            _logger.LogInformation("POST /api/products called by {User}", User.Identity?.Name ?? "Anonymous");

            if (!ModelState.IsValid || product.Price < 0 || product.ProductStockAmount < 0)
            {
                _logger.LogWarning("Invalid product creation attempt");
                return BadRequest("Invalid product. Price and stock must be positive.");
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Category ID {Id} not found for new product", product.CategoryId);
                return BadRequest("Category does not exist.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {Id} created successfully", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            return StatusCode(500, new { message = "Failed to create product.", details = ex.Message });

        }

    }

    // Update existing product
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            _logger.LogInformation("PUT /api/products/{Id} by {User}", id, User.Identity?.Name ?? "Anonymous");

            if (id != product.Id)
                return BadRequest("Product ID mismatch");

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product {Id} not found for update", id);
                return NotFound("Product not found");
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Price = product.Price;
            existingProduct.ProductStockAmount = product.ProductStockAmount;
            existingProduct.LowStockThreshold = product.LowStockThreshold;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product {Id} updated successfully", id);

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency issue while updating product {Id}", id);
            return NotFound("Product no longer exists (concurrency issue).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "Error updating product.", details = ex.Message });
        }
    }

    // Delete a product
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("DELETE /api/products/{Id} by {User}", id, User.Identity?.Name ?? "Anonymous");

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {Id} not found for deletion", id);
                return NotFound("Product not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product {Id} deleted successfully", id);
            return NoContent();
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException?.Message.Contains("violates foreign key constraint") == true)
        {
            _logger.LogWarning("Product {Id} cannot be deleted due to foreign key constraint", id);
            return BadRequest("Error: Product cannot be deleted because it is part of one or more orders.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { message = "Error deleting product.", details = ex.Message });
        }
    }
}

