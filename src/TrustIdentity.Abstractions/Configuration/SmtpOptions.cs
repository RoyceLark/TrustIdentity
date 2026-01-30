namespace TrustIdentity.Abstractions.Configuration;

/// <summary>
/// Configuration options for SMTP Email Sender
/// </summary>
public class SmtpOptions
{
    /// <summary>SMTP Server Host</summary>
    public string Host { get; set; } = string.Empty;
    
    /// <summary>SMTP Server Port</summary>
    public int Port { get; set; } = 587;
    
    /// <summary>Enable SSL/TLS</summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>SMTP Username</summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>SMTP Password</summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>From Email Address</summary>
    public string FromAddress { get; set; } = string.Empty;
    
    /// <summary>From Name</summary>
    public string FromName { get; set; } = "TrustIdentity Security";
}
