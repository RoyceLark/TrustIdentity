using System.Threading.Tasks;

namespace TrustIdentity.Abstractions.Services;

/// <summary>
/// Service for sending emails (e.g., fraud alerts, account verification)
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    Task SendEmailAsync(string to, string subject, string body);
}
