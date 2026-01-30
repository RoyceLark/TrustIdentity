using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
namespace TrustIdentity.Core.Models;

/// <summary>
/// Models an OpenID Connect or OAuth 2.0 identity resource
/// </summary>
public class IdentityResource : Resource
{
    /// <summary>
    /// Specifies whether this scope is shown in the discovery document
    /// </summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;


    /// <summary>
    /// Gets or sets a value indicating whether this resource is required
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to emphasize this resource in consent screen
    /// </summary>
    public bool Emphasize { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the IdentityResource class
    /// </summary>
    public IdentityResource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the IdentityResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="userClaims">The user claims associated with the resource</param>
    public IdentityResource(string name, IEnumerable<string> userClaims)
        : this(name, name, userClaims)
    {
    }

    /// <summary>
    /// Initializes a new instance of the IdentityResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="displayName">The display name of the resource</param>
    /// <param name="userClaims">The user claims associated with the resource</param>
    public IdentityResource(string name, string displayName, IEnumerable<string> userClaims)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DisplayName = displayName;
        UserClaims = userClaims.ToList();
    }
}

/// <summary>
/// Models the common data of API and identity resources
/// </summary>
public abstract class Resource
{
    /// <summary>
    /// Indicates if this resource is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The unique name of the resource
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the resource
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Description of the resource
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The API secret is used for the introspection endpoint
    /// </summary>
    public List<Secret> ApiSecrets { get; set; } = new();

    /// <summary>
    /// List of associated user claims that should be included when this resource is requested
    /// </summary>
    public List<string> UserClaims { get; set; } = new();

    /// <summary>
    /// Gets or sets the custom properties for the resource
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Creation time
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update time
    /// </summary>
    public DateTime? Updated { get; set; }

    /// <summary>
    /// Last time resource was accessed
    /// </summary>
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// Non-editable flag
    /// </summary>
    public bool NonEditable { get; set; } = false;
}

/// <summary>
/// Standard OpenID Connect identity resources
/// </summary>
public static class IdentityResources
{
    /// <summary>openid scope</summary>
    public static IdentityResource OpenId =>
        new IdentityResource(
            "openid",
            "Your user identifier",
            new[] { ClaimTypes.NameIdentifier, "sub" })
        {
            Required = true
        };

    /// <summary>profile scope</summary>
    public static IdentityResource Profile =>
        new IdentityResource(
            "profile",
            "User profile",
            new[]
            {
                "name",
                "family_name",
                "given_name",
                "middle_name",
                "nickname",
                "preferred_username",
                "profile",
                "picture",
                "website",
                "gender",
                "birthdate",
                "zoneinfo",
                "locale",
                "updated_at"
            });

    /// <summary>email scope</summary>
    public static IdentityResource Email =>
        new IdentityResource(
            "email",
            "Your email address",
            new[]
            {
                "email",
                "email_verified"
            });

    /// <summary>phone scope</summary>
    public static IdentityResource Phone =>
        new IdentityResource(
            "phone",
            "Your phone number",
            new[]
            {
                "phone_number",
                "phone_number_verified"
            });

    /// <summary>address scope</summary>
    public static IdentityResource Address =>
        new IdentityResource(
            "address",
            "Your postal address",
            new[] { "address" });
}
