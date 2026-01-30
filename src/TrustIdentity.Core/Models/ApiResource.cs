namespace TrustIdentity.Core.Models;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Models an API resource
/// </summary>
public class ApiResource : Resource
{
    /// <summary>
    /// The API scopes
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Signing algorithm for access token
    /// </summary>
    public List<string> AllowedAccessTokenSigningAlgorithms { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether [show in discovery document]
    /// </summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this resource requires resource indicator
    /// </summary>
    public bool RequireResourceIndicator { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the ApiResource class
    /// </summary>
    public ApiResource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    public ApiResource(string name)
        : this(name, name, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="displayName">The display name of the resource</param>
    public ApiResource(string name, string displayName)
        : this(name, displayName, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="userClaims">The user claims associated with the resource</param>
    public ApiResource(string name, IEnumerable<string> userClaims)
        : this(name, name, userClaims)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiResource class
    /// </summary>
    /// <param name="name">The name of the resource</param>
    /// <param name="displayName">The display name of the resource</param>
    /// <param name="userClaims">The user claims associated with the resource</param>
    public ApiResource(string name, string displayName, IEnumerable<string>? userClaims)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DisplayName = displayName;

        Scopes.Add(name);

        if (userClaims != null)
        {
            UserClaims.AddRange(userClaims);
        }
    }
}

/// <summary>
/// Models an API scope
/// </summary>
public class ApiScope : Resource
{
    /// <summary>
    /// Specifies whether the user can de-select the scope on the consent screen
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// Specifies whether the consent screen will emphasize this scope
    /// </summary>
    public bool Emphasize { get; set; } = false;

    /// <summary>
    /// Specifies whether this scope is shown in the discovery document
    /// </summary>
    public bool ShowInDiscoveryDocument { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the ApiScope class
    /// </summary>
    public ApiScope()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiScope class
    /// </summary>
    /// <param name="name">The name of the scope</param>
    public ApiScope(string name)
        : this(name, name, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiScope class
    /// </summary>
    /// <param name="name">The name of the scope</param>
    /// <param name="displayName">The display name of the scope</param>
    public ApiScope(string name, string displayName)
        : this(name, displayName, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiScope class
    /// </summary>
    /// <param name="name">The name of the scope</param>
    /// <param name="userClaims">The user claims associated with the scope</param>
    public ApiScope(string name, IEnumerable<string> userClaims)
        : this(name, name, userClaims)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ApiScope class
    /// </summary>
    /// <param name="name">The name of the scope</param>
    /// <param name="displayName">The display name of the scope</param>
    /// <param name="userClaims">The user claims associated with the scope</param>
    public ApiScope(string name, string displayName, IEnumerable<string>? userClaims)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DisplayName = displayName;

        if (userClaims != null)
        {
            UserClaims.AddRange(userClaims);
        }
    }
}

/// <summary>
/// Models a scope claim
/// </summary>
public class ScopeClaim
{
    /// <summary>The scope</summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>The claim type</summary>
    public string Type { get; set; } = string.Empty;
}
