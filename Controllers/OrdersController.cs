using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

public class OrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // All orders view
    public async Task<IActionResult> Index(string search, string email, DateTime? orderDate)
    {
        var orders = _context.Orders
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            orders = orders.Where(o => o.userName.Contains(search));
            _logger.LogInformation("Search filter applied: userName contains '{Search}'", search);
        }

        if (!string.IsNullOrEmpty(email))
        {
            orders = orders.Where(o => o.userEmail.Contains(email));
            _logger.LogInformation("Email filter applied: userEmail contains '{Email}'", email);
        }

        if (orderDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate.Date == orderDate.Value.Date);
            _logger.LogInformation("Date filter applied: OrderDate = {OrderDate}", orderDate.Value.Date);
        }

        var results = await orders.ToListAsync();
        _logger.LogInformation("Returning {Count} orders.", results.Count);

        return View(results);
    }


    // Order details view
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            _logger.LogInformation("GET /Orders/Details/{Id} called at {Timestamp} by {User}", id, DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order ID {Id} not found at {Timestamp}", id, DateTime.UtcNow);
                return NotFound();
            }

            return View(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order details for ID {Id} at {Timestamp} by {User}", id, DateTime.UtcNow, User.Identity?.Name ?? "Anonymous");
            return View("~/Views/Errors/500.cshtml");
        }
    }

}

