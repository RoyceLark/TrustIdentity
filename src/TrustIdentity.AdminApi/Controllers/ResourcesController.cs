using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing Identity Resources, API Resources, and API Scopes
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceStore _identityResourceStore;
    private readonly IApiResourceStore _apiResourceStore;
    private readonly IApiScopeStore _apiScopeStore;

    /// <summary>
    /// Initializes a new instance of the ResourcesController
    /// </summary>
    public ResourcesController(
        IResourceStore identityResourceStore,
        IApiResourceStore apiResourceStore,
        IApiScopeStore apiScopeStore)
    {
        _identityResourceStore = identityResourceStore;
        _apiResourceStore = apiResourceStore;
        _apiScopeStore = apiScopeStore;
    }

    /// <summary>Retrieves all identity resources</summary>
    [HttpGet("identity")]
    public async Task<IActionResult> GetIdentityResources() => Ok(await _identityResourceStore.GetAllResourcesAsync());

    /// <summary>Retrieves an identity resource by name</summary>
    [HttpGet("identity/{name}")]
    public async Task<IActionResult> GetIdentityResource(string name) => Ok(await _identityResourceStore.FindIdentityResourceAsync(name));

    /// <summary>Creates a new identity resource</summary>
    [HttpPost("identity")]
    public async Task<IActionResult> CreateIdentityResource([FromBody] IdentityResource res)
    {
        await _identityResourceStore.AddResourceAsync(res);
        return Created($"/api/v1/admin/resources/identity/{res.Name}", res);
    }

    /// <summary>Updates an existing identity resource</summary>
    [HttpPut("identity/{name}")]
    public async Task<IActionResult> UpdateIdentityResource(string name, [FromBody] IdentityResource res)
    {
        if (name != res.Name) return BadRequest();
        await _identityResourceStore.UpdateResourceAsync(res);
        return NoContent();
    }

    /// <summary>Deletes an identity resource</summary>
    [HttpDelete("identity/{name}")]
    public async Task<IActionResult> DeleteIdentityResource(string name)
    {
        await _identityResourceStore.DeleteResourceAsync(name);
        return NoContent();
    }

    /// <summary>Retrieves all API resources</summary>
    [HttpGet("api-resources")]
    public async Task<IActionResult> GetApiResources() => Ok(await _apiResourceStore.GetAllResourcesAsync());

    /// <summary>Retrieves an API resource by name</summary>
    [HttpGet("api-resources/{name}")]
    public async Task<IActionResult> GetApiResource(string name) => Ok(await _apiResourceStore.FindApiResourceAsync(name));

    /// <summary>Creates a new API resource</summary>
    [HttpPost("api-resources")]
    public async Task<IActionResult> CreateApiResource([FromBody] ApiResource res)
    {
        await _apiResourceStore.AddResourceAsync(res);
        return Created($"/api/v1/admin/resources/api-resources/{res.Name}", res);
    }

    /// <summary>Updates an existing API resource</summary>
    [HttpPut("api-resources/{name}")]
    public async Task<IActionResult> UpdateApiResource(string name, [FromBody] ApiResource res)
    {
        if (name != res.Name) return BadRequest();
        await _apiResourceStore.UpdateResourceAsync(res);
        return NoContent();
    }

    /// <summary>Deletes an API resource</summary>
    [HttpDelete("api-resources/{name}")]
    public async Task<IActionResult> DeleteApiResource(string name)
    {
        await _apiResourceStore.DeleteResourceAsync(name);
        return NoContent();
    }

    /// <summary>Retrieves all API scopes</summary>
    [HttpGet("api-scopes")]
    public async Task<IActionResult> GetApiScopes() => Ok(await _apiScopeStore.GetAllScopesAsync());

    /// <summary>Retrieves an API scope by name</summary>
    [HttpGet("api-scopes/{name}")]
    public async Task<IActionResult> GetApiScope(string name) => Ok(await _apiScopeStore.FindApiScopeAsync(name));

    /// <summary>Creates a new API scope</summary>
    [HttpPost("api-scopes")]
    public async Task<IActionResult> CreateApiScope([FromBody] ApiScope scope)
    {
        await _apiScopeStore.AddScopeAsync(scope);
        return Created($"/api/v1/admin/resources/api-scopes/{scope.Name}", scope);
    }

    /// <summary>Updates an existing API scope</summary>
    [HttpPut("api-scopes/{name}")]
    public async Task<IActionResult> UpdateApiScope(string name, [FromBody] ApiScope scope)
    {
        if (name != scope.Name) return BadRequest();
        await _apiScopeStore.UpdateScopeAsync(scope);
        return NoContent();
    }

    /// <summary>Deletes an API scope</summary>
    [HttpDelete("api-scopes/{name}")]
    public async Task<IActionResult> DeleteApiScope(string name)
    {
        await _apiScopeStore.DeleteScopeAsync(name);
        return NoContent();
    }
}
