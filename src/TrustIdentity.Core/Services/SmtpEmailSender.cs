using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrustIdentity.Abstractions.Services;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.Core.Services;

/// <summary>
/// SMTP implementation of IEmailSender for production use.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;
    /// <summary>
    /// SmtpEmailSender
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SmtpEmailSender(
        TrustIdentityOptions options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Smtp ?? new SmtpOptions();
        _logger = logger;
    }
    /// <summary>
    /// SendEmailAsync
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(_options.Host))
        {
            _logger.LogError("SMTP Host is not configured. Cannot send email to {To}", to);
            return;
        }

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // We do not throw here to avoid breaking the authentication flow if email server is down
        }
    }
}
