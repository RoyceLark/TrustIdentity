using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustIdentity.Abstractions.Configuration;

namespace TrustIdentity.AspNetCore.Middleware;

/// <summary>
/// Middleware to handle IP whitelisting at the server level
/// </summary>
public class IpSafeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpSafeMiddleware> _logger;
    private readonly List<string> _safelist;

    /// <summary>
    /// Initializes a new instance of the IpSafeMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="safelist">Semicolon-separated list of allowed IP addresses</param>
    public IpSafeMiddleware(
        RequestDelegate next, 
        ILogger<IpSafeMiddleware> logger,
        string safelist)
    {
        _next = next;
        _logger = logger;
        _safelist = safelist.Split(';').Select(p => p.Trim()).ToList();
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task Invoke(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

        if (remoteIp != null && _safelist.Count > 0)
        {
            var bytes = remoteIp.GetAddressBytes();
            bool badIp = true;

            foreach (var address in _safelist)
            {
                if (address == "*")
                {
                    badIp = false;
                    break;
                }

                var testIp = IPAddress.Parse(address);
                if (testIp.GetAddressBytes().SequenceEqual(bytes))
                {
                    badIp = false;
                    break;
                }
            }

            if (badIp)
            {
                _logger.LogWarning("Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for IP Safelist middleware
/// </summary>
public static class IpSafeMiddlewareExtensions
{
    /// <summary>
    /// Adds IP Safelist middleware to the pipeline.
    /// Usage: app.UseIpSafelist("127.0.0.1;::1;192.168.1.100");
    /// </summary>
    public static IApplicationBuilder UseIpSafelist(this IApplicationBuilder builder, string safelist)
    {
        return builder.UseMiddleware<IpSafeMiddleware>(safelist);
    }
}
