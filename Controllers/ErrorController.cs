using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace InventoryManagementSystem.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error/404")]
    public IActionResult Error404()
    {
        _logger.LogWarning("404 Error at {Time} by {User}. Path: {Path}", 
            DateTime.UtcNow, User.Identity?.Name ?? "Anonymous", HttpContext.Request.Path);
        return View("/Views/Errors/404.cshtml");
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        _logger.LogError("500 Error at {Time} by {User}. Path: {Path}", 
            DateTime.UtcNow, User.Identity?.Name ?? "Anonymous", HttpContext.Request.Path);
        return View("/Views/Errors/500.cshtml");
    }

    [Route("Error/{code}")]
    public IActionResult Generic(int code)
    {
        _logger.LogWarning("Generic Error {Code} at {Time} by {User}. Path: {Path}", 
            code, DateTime.UtcNow, User.Identity?.Name ?? "Anonymous", HttpContext.Request.Path);

        return code == 404
            ? View("/Views/Errors/404.cshtml")
            : View("/Views/Errors/500.cshtml");
    }
}