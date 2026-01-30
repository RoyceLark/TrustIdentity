namespace TrustIdentity.Core.Constants;

using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// Supported OAuth 2.0 grant types
/// </summary>
public static class GrantTypes
{
    /// <summary>Implicit grant type</summary>
    public static ICollection<string> Implicit =>
        new[] { "implicit" };

    /// <summary>Implicit and Client Credentials grant types</summary>
    public static ICollection<string> ImplicitAndClientCredentials =>
        new[] { "implicit", "client_credentials" };

    /// <summary>Authorization Code grant type</summary>
    public static ICollection<string> Code =>
        new[] { "authorization_code" };

    /// <summary>Authorization Code and Client Credentials grant types</summary>
    public static ICollection<string> CodeAndClientCredentials =>
        new[] { "authorization_code", "client_credentials" };

    /// <summary>Hybrid grant type</summary>
    public static ICollection<string> Hybrid =>
        new[] { "hybrid" };

    /// <summary>Hybrid and Client Credentials grant types</summary>
    public static ICollection<string> HybridAndClientCredentials =>
        new[] { "hybrid", "client_credentials" };

    /// <summary>Client Credentials grant type</summary>
    public static ICollection<string> ClientCredentials =>
        new[] { "client_credentials" };

    /// <summary>Resource Owner Password grant type</summary>
    public static ICollection<string> ResourceOwnerPassword =>
        new[] { "password" };

    /// <summary>Resource Owner Password and Client Credentials grant types</summary>
    public static ICollection<string> ResourceOwnerPasswordAndClientCredentials =>
        new[] { "password", "client_credentials" };

    /// <summary>Device Flow grant type</summary>
    public static ICollection<string> DeviceFlow =>
        new[] { "urn:ietf:params:oauth:grant-type:device_code" };

    /// <summary>Authorization Code grant type string</summary>
    public const string AuthorizationCode = "authorization_code";
    /// <summary>Refresh Token grant type string</summary>
    public const string RefreshToken = "refresh_token";
    /// <summary>CIBA grant type string</summary>
    public const string Ciba = "urn:openid:params:grant-type:ciba";
    /// <summary>Token Exchange grant type string</summary>
    public const string TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
}