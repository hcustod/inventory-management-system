using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace InventoryManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("HomeController Index accessed at {Time} by {User}. Path: {Path}",
            DateTime.UtcNow, User.Identity?.Name ?? "Anonymous", HttpContext.Request.Path);

        return View();
    }
}