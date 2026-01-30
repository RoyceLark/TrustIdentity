using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;

namespace TrustIdentity.Core.Services;

/// <summary>
/// A simple logger-based email sender for development/demo purposes.
/// In production, this should be replaced by a real SMTP or API implementation (e.g. SendGrid).
/// </summary>
public class LoggerEmailSender : IEmailSender
{
    private readonly ILogger<LoggerEmailSender> _logger;
    /// <summary>
    /// LoggerEmailSender
    /// </summary>
    /// <param name="logger"></param>
    public LoggerEmailSender(ILogger<LoggerEmailSender> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// SendEmailAsync
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // For demo purposes, we just log the email content.
        // In a real implementation, you would use SmtpClient or an API client here.
        _logger.LogWarning("================ EMAIL SIMULATION ================");
        _logger.LogWarning("To: {To}", to);
        _logger.LogWarning("Subject: {Subject}", subject);
        _logger.LogWarning("Body: {Body}", body);
        _logger.LogWarning("==================================================");

        return Task.CompletedTask;
    }
}
