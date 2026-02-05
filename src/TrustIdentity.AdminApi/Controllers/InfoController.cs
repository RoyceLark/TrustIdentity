using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for server information and connectivity checks
/// </summary>
[ApiController]
[Route("api/v1/admin/[controller]")]
public class InfoController : ControllerBase
{
    /// <summary>
    /// Returns basic server information
    /// </summary>
    [HttpGet]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
            Status = "Online",
            ServerDate = DateTime.UtcNow,
            IdentityServer = "TrustIdentity"
        });
    }
}
