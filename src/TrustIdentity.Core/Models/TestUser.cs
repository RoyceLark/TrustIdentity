using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
namespace TrustIdentity.Core.Models;

/// <summary>
/// Represents a test user for development and testing purposes
/// </summary>
public class TestUser
{
    /// <summary>The unique subject identifier</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>The username</summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>The password</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>The email address</summary>
    public string? Email { get; set; }
    /// <summary>The user claims</summary>
    public List<Claim> Claims { get; set; } = new();
    /// <summary>Whether the user is active</summary>
    public bool IsActive { get; set; } = true;
}