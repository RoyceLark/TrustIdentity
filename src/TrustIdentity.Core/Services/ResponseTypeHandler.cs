using TrustIdentity.Core.Models;
using TrustIdentity.Core.Constants;
using System;


namespace TrustIdentity.Core.Services;

/// <summary>
/// Handles and validates OAuth 2.0 response types
/// </summary>
public class ResponseTypeHandler
{
    /// <summary>
    /// Validates if a response type is allowed for a given client
    /// </summary>
    /// <param name="responseType">The response type string (may be space-separated)</param>
    /// <param name="client">The client</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(string responseType, Client client)
    {
        var types = responseType.Split(' ');
        
        // Validate based on grant types
        if (types.Contains("code"))
        {
            return client.AllowedGrantTypes.Contains(GrantTypes.AuthorizationCode) ||
                   client.AllowedGrantTypes.Contains("authorization_code");
        }
        
        if (types.Contains("token") || types.Contains("id_token"))
        {
            return client.AllowedGrantTypes.Contains(GrantTypes.Implicit.ToString()!) ||
                   client.AllowedGrantTypes.Contains("implicit") ||
                   client.AllowedGrantTypes.Contains(GrantTypes.Hybrid.ToString()!) ||
                   client.AllowedGrantTypes.Contains("hybrid");
        }
        
        return false;
    }
}

/// <summary>
/// Standard OAuth 2.0 response modes
/// </summary>
public static class ResponseModes
{
    /// <summary>form_post response mode</summary>
    public const string FormPost = "form_post";
    /// <summary>query response mode</summary>
    public const string Query = "query";
    /// <summary>fragment response mode</summary>
    public const string Fragment = "fragment";
}