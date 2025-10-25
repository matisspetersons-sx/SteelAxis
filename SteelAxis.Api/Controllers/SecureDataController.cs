using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace SteelAxis.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[RequiredScope("access_as_user")]
public class SecureDataController : ControllerBase
{
    private readonly ILogger<SecureDataController> _logger;

    public SecureDataController(ILogger<SecureDataController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets data for authenticated users only
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        var userName = User.Identity?.Name ?? "Unknown";
        var userId = User.FindFirst("oid")?.Value ?? "Unknown";
        
        _logger.LogInformation("User {UserName} (ID: {UserId}) accessed secure data", userName, userId);

        return Ok(new
        {
            Message = "This is secure data",
            User = userName,
            UserId = userId,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    /// <summary>
    /// Example of role-based authorization
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminData()
    {
        return Ok(new
        {
            Message = "This is admin-only data",
            User = User.Identity?.Name
        });
    }

    /// <summary>
    /// Example of policy-based authorization
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAuthenticatedUser")]
    public IActionResult Post([FromBody] object data)
    {
        return Ok(new
        {
            Message = "Data received",
            Data = data,
            User = User.Identity?.Name
        });
    }
}
