using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.Extensions.Logging;


namespace InventoryManagementSystem.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesApiController> _logger;

    public CategoriesApiController(AppDbContext context, ILogger<CategoriesApiController> logger)
    {
        _context = context;
        _logger = logger;

    }

    // Get all categories 
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            _logger.LogInformation("GET /api/categories called by {User} at {Time}", User.Identity?.Name ?? "Anonymous", DateTime.UtcNow);
            var categories = await _context.Categories.ToListAsync();
            _logger.LogInformation("Returned {Count} categories", categories.Count);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve categories at {Time} by {User}", DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");
            return StatusCode(500, new { message = "Failed to retrieve categories.", details = ex.Message });
        }
    }

    // Get category by ID 
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("GET /api/categories/{Id} called by {User}", id, User.Identity?.Name ?? "Anonymous");

            var categories = await _context.Categories.FindAsync(id);
            if (categories == null)
            {
                _logger.LogWarning("Category {Id} not found", id);
                return NotFound();
            }
            
            _logger.LogInformation("Returned category {Id}", id);
            return Ok(categories);  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {Id}", id);
            return StatusCode(500, new { message = "Error fetching category.", details = ex.Message });
        }
    }


    // Create category
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        try
        {
            _logger.LogInformation("POST /api/categories called by {User}", User.Identity?.Name ?? "Anonymous");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state on category creation");
                return BadRequest(ModelState);
            }

            if (await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
            {
                _logger.LogWarning("Attempt to create duplicate category: {Name}", category.CategoryName);
                return BadRequest("Category already exists!");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created category {Id}", category.Id);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category");
            return StatusCode(500, new { message = "Failed to create category.", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    // update a category
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
    {
        try
        {
            _logger.LogInformation("PUT /api/categories/{Id} triggered by {User}", id, User.Identity?.Name ?? "Anonymous");

            if (id != category.Id) 
                return BadRequest("Category ID mismatch");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
            {
                _logger.LogWarning("Category {Id} not found for update", id);
                return NotFound("Category not found");
            }

            // Update fields
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.CategoryDescription = category.CategoryDescription;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated category {Id}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.Id == id))
                {
                    _logger.LogWarning("Concurrency conflict: Category {Id} no longer exists", id);
                    return NotFound("Category no longer exists");
                }
                throw;
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, new { message = "Failed to create category.", details = ex.Message });
        }
    }
    
    
    // Get products by category
    [HttpGet("/api/products/byCategory/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId)
    {
        try
        {
            _logger.LogInformation("GET /api/products/byCategory/{Id} called", categoryId);

            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
            
            _logger.LogInformation("Found {Count} products for category {Id}", products.Count, categoryId);

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {Id}", categoryId);
            return StatusCode(500, new { message = "Error getting products by category.", details = ex.Message });
        }
      
    }

// Delete category if no products exist
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            _logger.LogInformation("DELETE /api/categories/{Id} triggered by {User}", id, User.Identity?.Name ?? "Anonymous");

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {Id} not found for deletion", id);
                return NotFound();
            }

            // Check if products exist in the category
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                _logger.LogWarning("Category {Id} contains products. Deletion not allowed", id);
                return BadRequest("Cannot delete category with existing products.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted category {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new { message = "Error deleting category.", details = ex.Message });
        }
    }

}