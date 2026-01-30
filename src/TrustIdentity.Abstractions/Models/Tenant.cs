namespace TrustIdentity.Abstractions.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a tenant in a multi-tenant deployment
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tenant name (display name)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier/key for the tenant (used in URLs, headers, etc.)
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Host/domain associated with this tenant (for host-based resolution)
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Connection string for tenant-specific database (if using database-per-tenant)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Whether this tenant is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tenant creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tenant configuration as JSON
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Issuer URI for this tenant (overrides global issuer)
    /// </summary>
    public string? IssuerUri { get; set; }

    /// <summary>
    /// Custom properties for the tenant
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Maximum number of users allowed for this tenant
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Maximum number of clients allowed for this tenant
    /// </summary>
    public int? MaxClients { get; set; }

    /// <summary>
    /// Tenant subscription tier (e.g., "Free", "Pro", "Enterprise")
    /// </summary>
    public string? SubscriptionTier { get; set; }

    /// <summary>
    /// Subscription expiration date
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }
}
