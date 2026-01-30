using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace TrustIdentity.Core.Models;

/// <summary>
/// Represents a user in the TrustIdentity system
/// </summary>
public class TrustIdentityUser
{
    /// <summary>Unique subject identifier</summary>
    public string SubjectId { get; set; } = string.Empty;
    /// <summary>Tenant identifier for multi-tenancy</summary>
    public string? TenantId { get; set; }
    /// <summary>Username</summary>
    public string? Username { get; set; }
    /// <summary>Email address</summary>
    public string? Email { get; set; }
    /// <summary>Whether the email is verified</summary>
    public bool EmailVerified { get; set; }
    /// <summary>Phone number</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Whether the phone number is verified</summary>
    public bool PhoneNumberVerified { get; set; }
    /// <summary>Hashed password</summary>
    public string? Password { get; set; }
    /// <summary>List of user claims</summary>
    public List<UserClaim> Claims { get; set; } = new();
    /// <summary>Whether the user is active</summary>
    public bool IsActive { get; set; } = true;
    /// <summary>Creation time</summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;
    /// <summary>Last update time</summary>
    public DateTime? Updated { get; set; }
    /// <summary>Last login time</summary>
    public DateTime? LastLogin { get; set; }
    /// <summary>Number of failed login attempts</summary>
    public int FailedLoginAttempts { get; set; }
    /// <summary>End of lockout period</summary>
    public DateTime? LockoutEnd { get; set; }
    /// <summary>Whether two-factor authentication is enabled</summary>
    public bool TwoFactorEnabled { get; set; }
    /// <summary>List of user roles</summary>
    public List<string> Roles { get; set; } = new();
    /// <summary>AI security profile for the user</summary>
    public UserAIProfile? AIProfile { get; set; }
}

/// <summary>
/// Represents a user claim
/// </summary>
public class UserClaim
{
    /// <summary>Primary key</summary>
    public int Id { get; set; }
    /// <summary>Claim type</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Claim value</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Claim value type</summary>
    public string? ValueType { get; set; }
    /// <summary>Claim issuer</summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// Converts to a System.Security.Claims.Claim
    /// </summary>
    public Claim ToClaim()
    {
        return new Claim(Type, Value, ValueType, Issuer);
    }

    /// <summary>
    /// Creates a UserClaim from a System.Security.Claims.Claim
    /// </summary>
    public static UserClaim FromClaim(Claim claim)
    {
        return new UserClaim
        {
            Type = claim.Type,
            Value = claim.Value,
            ValueType = claim.ValueType,
            Issuer = claim.Issuer
        };
    }
}

/// <summary>
/// AI security profile containing risk scores and behavior analysis
/// </summary>
public class UserAIProfile
{
    /// <summary>Current risk score</summary>
    public double RiskScore { get; set; }
    /// <summary>Current trust score</summary>
    public double TrustScore { get; set; } = 0.5;
    /// <summary>Detected behavior patterns</summary>
    public List<BehaviorPattern> BehaviorPatterns { get; set; } = new();
    /// <summary>Known devices for the user</summary>
    public List<DeviceFingerprint> KnownDevices { get; set; } = new();
    /// <summary>Recent login attempts</summary>
    public List<LoginAttempt> RecentLogins { get; set; } = new();
    /// <summary>Last risk assessment time</summary>
    public DateTime LastRiskAssessment { get; set; }
}

/// <summary>
/// Represents a detected behavior pattern
/// </summary>
public class BehaviorPattern
{
    /// <summary>The type of pattern (e.g., "typing", "navigation")</summary>
    public string PatternType { get; set; } = string.Empty;
    /// <summary>Confidence score of the pattern detection</summary>
    public double Confidence { get; set; }
    /// <summary>Additional pattern metadata</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a device fingerprint for security tracking
/// </summary>
public class DeviceFingerprint
{
    /// <summary>Unique device identifier</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>The user agent string</summary>
    public string UserAgent { get; set; } = string.Empty;
    /// <summary>The IP address</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>Geographic location</summary>
    public string? Location { get; set; }
    /// <summary>First time this device was seen</summary>
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    /// <summary>Last time this device was seen</summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    /// <summary>Whether this device is trusted by the user</summary>
    public bool IsTrusted { get; set; }
}

/// <summary>
/// Represents a single login attempt
/// </summary>
public class LoginAttempt
{
    /// <summary>Timestamp of the attempt</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>The IP address of the attempt</summary>
    public string IpAddress { get; set; } = string.Empty;
    /// <summary>Geographic location of the attempt</summary>
    public string? Location { get; set; }
    /// <summary>Whether the attempt was successful</summary>
    public bool Success { get; set; }
    /// <summary>Failure reason if applicable</summary>
    public string? Reason { get; set; }
    /// <summary>Calculated risk score for this attempt</summary>
    public double RiskScore { get; set; }
}
