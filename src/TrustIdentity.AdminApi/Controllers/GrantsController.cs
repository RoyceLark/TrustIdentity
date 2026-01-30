using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing Persisted Grants (tokens, codes, etc.)
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class GrantsController : ControllerBase
{
    private readonly IPersistedGrantStore _grantStore;

    /// <summary>
    /// Initializes a new instance of the GrantsController
    /// </summary>
    public GrantsController(IPersistedGrantStore grantStore)
    {
        _grantStore = grantStore;
    }

    /// <summary>
    /// Retrieves all grants for a specific subject and/or client
    /// </summary>
    /// <param name="subjectId">The subject ID to filter by</param>
    /// <param name="clientId">Optional client ID to filter by</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? subjectId, [FromQuery] string? clientId)
    {
        // In a real implementation, the store would support filtering
        // For now, let's assume we can get all if no filter or filtered by subject
        if (!string.IsNullOrEmpty(subjectId))
        {
            var grants = await _grantStore.GetAllAsync(subjectId);
            return Ok(grants);
        }
        
        return BadRequest("SubjectId is required for listing grants in this version");
    }

    /// <summary>
    /// Retrieves a specific grant by its key
    /// </summary>
    /// <param name="key">The grant key</param>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var grant = await _grantStore.GetAsync(key);
        if (grant == null) return NotFound();
        return Ok(grant);
    }

    /// <summary>
    /// Revokes a specific grant by its key
    /// </summary>
    /// <param name="key">The grant key to revoke</param>
    [HttpDelete("{key}")]
    public async Task<IActionResult> Revoke(string key)
    {
        await _grantStore.RemoveAsync(key);
        return NoContent();
    }

    /// <summary>
    /// Revokes all grants matching the specified filter criteria
    /// </summary>
    /// <param name="subjectId">The subject ID</param>
    /// <param name="clientId">Optional client ID</param>
    /// <param name="type">Optional grant type (e.g., refresh_token)</param>
    [HttpDelete("all")]
    public async Task<IActionResult> RevokeAll([FromQuery] string subjectId, [FromQuery] string? clientId, [FromQuery] string? type)
    {
        if (string.IsNullOrEmpty(subjectId)) return BadRequest("SubjectId is required");
        
        var filter = new PersistedGrantFilter 
        { 
            SubjectId = subjectId, 
            ClientId = clientId, 
            Type = type 
        };
        await _grantStore.RemoveAllAsync(filter);
        return NoContent();
    }
}
