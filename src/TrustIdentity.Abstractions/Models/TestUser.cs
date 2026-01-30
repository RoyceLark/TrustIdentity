namespace TrustIdentity.Abstractions.Models;

using System.Collections.Generic;
/// <summary>
/// Represents a test user for non-persistent storage
/// </summary>
public class TestUser
{
    /// <summary>The subject ID</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>The username</summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>The password</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>The email address</summary>
    public string? Email { get; set; }
    /// <summary>The user claims</summary>
    public List<System.Security.Claims.Claim> Claims { get; set; } = new();
    /// <summary>Whether the user is active</summary>
    public bool IsActive { get; set; } = true;
}