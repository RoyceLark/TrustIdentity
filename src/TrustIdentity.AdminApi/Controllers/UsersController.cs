using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustIdentity.Abstractions.Stores;
using TrustIdentity.Abstractions.Models;
using TrustIdentity.AdminApi.Models;

namespace TrustIdentity.AdminApi.Controllers;

/// <summary>
/// Controller for managing Users
/// </summary>
[Authorize(Policy = "AdminApiAccess")]
[ApiController]
[Route("api/v1/admin/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserStore _userStore;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(IUserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// Retrieves a paginated list of users
    /// </summary>
    /// <param name="search">Search string for filtering by username or email</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Number of items per page</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (users, total) = await _userStore.GetAllUsersAsync(search, (page - 1) * pageSize, pageSize);
        var response = users.Select(u => u.ToResponse());
        return Ok(new { data = response, total, page, pageSize });
    }

    /// <summary>
    /// Retrieves a user by their unique subject ID
    /// </summary>
    /// <param name="id">The user subject ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userStore.FindBySubjectIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user.ToResponse());
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = new User
        {
            SubjectId = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _userStore.AddUserAsync(user, request.Password);
        return CreatedAtAction(nameof(GetById), new { id = user.SubjectId }, user.ToResponse());
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">The user subject ID</param>
    /// <param name="user">The user data to update</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] User user)
    {
        if (id != user.SubjectId) return BadRequest("ID mismatch");

        var existing = await _userStore.FindBySubjectIdAsync(id);
        if (existing == null) return NotFound();

        await _userStore.UpdateUserAsync(user);
        return NoContent();
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">The user subject ID</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existing = await _userStore.FindBySubjectIdAsync(id);
        if (existing == null) return NotFound();

        await _userStore.DeleteUserAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Resets a user's password
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] string newPassword)
    {
        var existing = await _userStore.FindBySubjectIdAsync(id);
        if (existing == null) return NotFound();

        await _userStore.SetPasswordAsync(id, newPassword);
        return Ok(new { Message = "Password has been reset" });
    }

    /// <summary>
    /// Deactivates a user account
    /// </summary>
    [HttpPost("{id}/lock")]
    public async Task<IActionResult> Lock(string id)
    {
        var user = await _userStore.FindBySubjectIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _userStore.UpdateUserAsync(user);
        return NoContent();
    }

    /// <summary>
    /// Activates a user account
    /// </summary>
    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _userStore.FindBySubjectIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        await _userStore.UpdateUserAsync(user);
        return NoContent();
    }
}

/// <summary>
/// Request model for creating a new user
/// </summary>
public record CreateUserRequest(string Username, string Email, string Password);
