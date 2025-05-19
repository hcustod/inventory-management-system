using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersApiController> _logger;

    public OrdersApiController(AppDbContext context, ILogger<OrdersApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Get all orders with product details
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            _logger.LogInformation("GET /api/orders called at {Timestamp} by {User}", DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");
            
            var orders = await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ToListAsync();
            
            _logger.LogInformation("Successfully fetched {OrderCount} orders at {Timestamp}.", orders.Count, DateTime.UtcNow);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders at {Timestamp}. User: {User}", DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");

            return StatusCode(500, new
            {
                message = "Failed to fetch orders.",
                details = ex.Message
            });        }
    }
    

    // Get a specific order with its products
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            _logger.LogInformation("GET /api/orders/{Id} triggered by {User}", id, User.Identity?.Name ?? "Anonymous");
            
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order {Id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Returned order {Id}", id);

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {Id}", id);
            return StatusCode(500, new { message = "Failed to fetch order.", details = ex.Message });        }
   
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        try
        {
            _logger.LogInformation("POST /api/orders triggered by {User} at {Time}", User.Identity?.Name ?? "Anonymous", DateTime.UtcNow);
           

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order creation");
                return BadRequest(ModelState);
            }

            if (order.OrderProducts == null || !order.OrderProducts.Any())
            {
                _logger.LogWarning("Attempted to create order without products");
                return BadRequest(new { message = "Order must contain at least one product." });
            }

            // Assign Order Date
            order.OrderDate = DateTime.UtcNow;
            
            // check stock and deduct before saving
            foreach (var op in order.OrderProducts)
            {
                var product = await _context.Products.FindAsync(op.ProductId);
        
                if (product == null)
                {
                    _logger.LogWarning("Product {Id} not found during order creation", op.ProductId);
                    return BadRequest(new { message = $"Product with ID {op.ProductId} not found." });
                }

                if (product.ProductStockAmount < op.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {Name}. Requested: {Qty}, Available: {Stock}", product.Name, op.Quantity, product.ProductStockAmount);
                    return BadRequest(new { message = $"Not enough stock for {product.Name}. Only {product.ProductStockAmount} left." });
                }

                product.ProductStockAmount -= op.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            // Save the order FIRST for an id. 
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Solution for duplicate product entries in `OrderProducts`?
            var existingOrderProducts = _context.OrderProducts
                .Where(op => op.OrderId == order.Id)
                .ToDictionary(op => op.ProductId);

            foreach (var op in order.OrderProducts)
            {
                // Assign correct id
                op.OrderId = order.Id;  

                if (existingOrderProducts.ContainsKey(op.ProductId))
                {
                    // If the product already exists in the order update its quantity
                    existingOrderProducts[op.ProductId].Quantity += op.Quantity;
                }
                else
                {
                    // else new
                    _context.OrderProducts.Add(op);
                }
            }

            // Update Total Price
            order.TotalPrice = order.OrderProducts.Sum(op =>
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == op.ProductId);
                return product != null ? product.Price * op.Quantity : 0;
            });

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {Id} created successfully", order.Id);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during order creation by {User}", User.Identity?.Name ?? "Anonymous");
            return StatusCode(500, new
            {
                message = "An unexpected server error occurred.",
                details = ex.Message
            });
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            _logger.LogInformation("DELETE /api/orders/{Id} called by {User}", id, User.Identity?.Name ?? "Anonymous");

            var order = await _context.Orders
                .Include(o => o.OrderProducts) // Include related OrderProducts
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Attempted to delete non-existent order {Id}", id);
                return NotFound(new { message = "Order not found." });
            }

            // Remove associated OrderProducts first to maintain referential integrity
            _context.OrderProducts.RemoveRange(order.OrderProducts);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {Id} deleted", id);
            return Ok(new { message = "Order deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete order {Id}", id);
            return StatusCode(500, new { message = "Failed to delete order.", details = ex.Message });
        }

    }
}
    

