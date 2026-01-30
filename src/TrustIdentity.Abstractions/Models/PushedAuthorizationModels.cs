using System;
using System.Collections.Generic;

namespace TrustIdentity.Abstractions.Models;

/// <summary>
/// Represents a stored pushed authorization request (RFC 9126)
/// </summary>
public class PushedAuthorizationRequest
{
    /// <summary>The request URI</summary>
    public string RequestUri { get; set; } = string.Empty;
    
    /// <summary>The authorization request parameters</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    /// <summary>The client ID that made the request</summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>When the request was created</summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>When the request expires</summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Response from a pushed authorization request (RFC 9126)
/// </summary>
public class PushedAuthorizationResponse
{
    /// <summary>The request URI to use in the authorization request</summary>
    public string RequestUri { get; set; } = string.Empty;
    
    /// <summary>The lifetime of the request URI in seconds</summary>
    public int ExpiresIn { get; set; }
}
