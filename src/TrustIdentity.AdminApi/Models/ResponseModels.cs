using TrustIdentity.Abstractions.Models;

namespace TrustIdentity.AdminApi.Models;

/// <summary>
/// Response model for user data
/// </summary>
public class UserResponse
{
    /// <summary>Subject ID</summary>
    public string SubjectId { get; set; } = string.Empty;
    
    /// <summary>Username</summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>Email</summary>
    public string? Email { get; set; }
    
    /// <summary>Email verified</summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>Phone number</summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>Is active</summary>
    public bool IsActive { get; set; }
    
    /// <summary>Created date</summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>Last login date</summary>
    public DateTime? LastLoginDate { get; set; }
}

/// <summary>
/// Response model for client data
/// </summary>
public class ClientResponse
{
    /// <summary>Client ID</summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>Client name</summary>
    public string? ClientName { get; set; }
    
    /// <summary>Description</summary>
    public string? Description { get; set; }
    
    /// <summary>Enabled</summary>
    public bool Enabled { get; set; }
    
    /// <summary>Allowed grant types</summary>
    public List<string> AllowedGrantTypes { get; set; } = new();
    
    /// <summary>Redirect URIs</summary>
    public List<string> RedirectUris { get; set; } = new();
    
    /// <summary>Allowed scopes</summary>
    public List<string> AllowedScopes { get; set; } = new();
    
    /// <summary>Created date</summary>
    public DateTime Created { get; set; }
}

/// <summary>
/// Extension methods for converting models to response models
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Converts a User to a UserResponse
    /// </summary>
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            SubjectId = user.SubjectId,
            Username = user.Username,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate
        };
    }
    
    /// <summary>
    /// Converts a Client to a ClientResponse
    /// </summary>
    public static ClientResponse ToResponse(this Client client)
    {
        return new ClientResponse
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            Description = client.Description,
            Enabled = client.Enabled,
            AllowedGrantTypes = client.AllowedGrantTypes,
            RedirectUris = client.RedirectUris,
            AllowedScopes = client.AllowedScopes,
            Created = client.Created
        };
    }
}
