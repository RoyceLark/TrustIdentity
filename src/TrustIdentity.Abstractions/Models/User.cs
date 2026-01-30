namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;
/// <summary>
/// Represents a user in the identity system
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public string SubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Optional tenant identifier for multi-tenancy
    /// </summary>
    public string? TenantId { get; set; }
    
    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Indicates if the email has been verified
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Indicates if the phone number has been verified
    /// </summary>
    public bool PhoneNumberVerified { get; set; }
    
    /// <summary>
    /// Hashed password
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Additional user claims
    /// </summary>
    public List<System.Security.Claims.Claim> Claims { get; set; } = new();
    
    /// <summary>
    /// Date of last login
    /// </summary>
    public DateTime? LastLoginDate { get; set; }
    
    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Date when the lockout ends
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }
    
    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}