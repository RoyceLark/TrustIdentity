using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.AdminApi.Models;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing OAuth2/OIDC Clients
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientStore _clientStore;

    /// <summary>
    /// Initializes a new instance of the ClientsController
    /// </summary>
    public ClientsController(IClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    /// <summary>
    /// Retrieves all registered clients
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientStore.GetAllClientsAsync();
        var response = clients.Select(c => c.ToResponse());
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a specific client by ID
    /// </summary>
    /// <param name="id">The Client ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var client = await _clientStore.FindClientByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(client.ToResponse());
    }

    /// <summary>
    /// Creates a new client
    /// </summary>
    /// <param name="client">The client configuration</param>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Client client)
    {
        if (string.IsNullOrEmpty(client.ClientId)) return BadRequest("ClientId is required");
        
        var existing = await _clientStore.FindClientByIdAsync(client.ClientId);
        if (existing != null) return Conflict("Client ID already exists");

        await _clientStore.AddClientAsync(client);
        return CreatedAtAction(nameof(GetById), new { id = client.ClientId }, client.ToResponse());
    }

    /// <summary>
    /// Updates an existing client
    /// </summary>
    /// <param name="id">The Client ID</param>
    /// <param name="client">The updated client configuration</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Client client)
    {
        if (id != client.ClientId) return BadRequest("ID mismatch");

        var existing = await _clientStore.FindClientByIdAsync(id);
        if (existing == null) return NotFound();

        await _clientStore.UpdateClientAsync(client);
        return NoContent();
    }

    /// <summary>
    /// Deletes a client
    /// </summary>
    /// <param name="id">The Client ID to delete</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _clientStore.FindClientByIdAsync(id);
        if (existing == null) return NotFound();

        await _clientStore.DeleteClientAsync(id);
        return NoContent();
    }
}
